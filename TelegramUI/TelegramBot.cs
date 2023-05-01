using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using System.Threading;
using System.Text.RegularExpressions;

namespace MySchedule.TelegramUI;

/// <summary>
/// Класс, отвечающий за взаимодействие с пользователем
/// </summary>
public static class TelegramBot
{
    private static TelegramDBContext _DB = new();

    public static void Start(string token)
    {
        var TBClient = new TelegramBotClient(token);

        TBClient.StartReceiving(async (tbc, u, ct) =>
        {
            try { await HandleUpdate(tbc, u, ct); }
            catch (Exception e)
            {
                Console.WriteLine($"Error >> Ошибка обработки сообщения: {e.Message}");
                return;
            }
        }, HandleError);
    }


    private async static Task HandleUpdate(ITelegramBotClient TBClient, Update update, CancellationToken ct)
    {
        if (!update.IsMessageType() && !update.IsCallbackType()) return;

        var user = GetUserFromDB(update.GetChatID());
        var chatID = update.GetChatID();


        if (update.IsMessageType() && update.Message.Text == "/start" && !user.IsRegistered())
        {
            await TBClient.SendTextMessageAsync(update.Message.Chat.Id, "Этот бот служит для удобного получения расписания студентов ИФОИОТ.");
            await RequestCourse(TBClient, chatID);
            return;
        }

        if (update.IsMessageType())
        {
            await HandeBugreport(TBClient, update, user);
            return;
        }

        // Далее мы точно уверены, что тип сообщения -- CallbackQuery

        Match matches = Regex.Match(update.CallbackQuery.Data, "^[A-Z]+");
        string command = matches.Value;

        switch (command)
        {
            case "COURSE":
                await HandeCourseSelection(TBClient, update, user);
                return;
            case "GROUP":
                await HandeGroupSelection(TBClient, update, user);
                return;
            case "RESET":
                await RestartRegistration(TBClient, user);
                return;
            case "PRINT":
                await HandePrintCouplesSelection(TBClient, user);
                return;
        }

    }


    private static async Task RequestCourse(ITelegramBotClient TBClient, long chatID)
    {
        var ikm = new InlineKeyboardMarkup(new[]
       {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(" 1 курс ", "COURSE_1"),
                    InlineKeyboardButton.WithCallbackData(" 2 курс ", "COURSE_2"),
                    InlineKeyboardButton.WithCallbackData(" 3 курс ", "COURSE_3"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(" 4 курс ", "COURSE_4"),
                    InlineKeyboardButton.WithCallbackData(" 5 курс ", "COURSE_5"),
                },
        });

        await TBClient.SendTextMessageAsync(chatID, "Выберите курс:", replyMarkup: ikm);
    }

    private static async Task HandeCourseSelection(ITelegramBotClient TBClient, Update update, TelegramUser user)
    {
        int course = int.Parse(update.CallbackQuery.Data.Split('_', StringSplitOptions.RemoveEmptyEntries)[1]);

        await TBClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Выбран {course} курс.");

        user.Course = course;
        _DB.Update(user);
        await _DB.SaveChangesAsync();

        await RequestPosibleGroup(TBClient, update.GetChatID(), course);
    }

    private static async Task RequestPosibleGroup(ITelegramBotClient TBClient, long chatID, int course)
    {
        var groups = CoupleSchedule.StudyGroups.Where(x => x.Course == course).Select(x => x.GroupName).Order().ToArray();
        var buttons = new InlineKeyboardButton[groups.Count() + 1][];

        for (int i = 0; i < groups.Count(); i++)
            buttons[i] = new[] { InlineKeyboardButton.WithCallbackData(groups[i], $"GROUP_{groups[i]}") };

        buttons[^1] = new[] { InlineKeyboardButton.WithCallbackData("Вернуться к выбору курса", "RESET") };

        var keyboard = new InlineKeyboardMarkup(buttons);

        await TBClient.SendTextMessageAsync(chatID, "Выберите группу:", replyMarkup: keyboard);
    }

    private static async Task HandeGroupSelection(ITelegramBotClient TBClient, Update update, TelegramUser user)
    {
        Match matches = Regex.Match(update.CallbackQuery.Data, "^GROUP_(.*)");
        user.GroupName = matches.Groups[1].ToString();

        await TBClient.SendTextMessageAsync(user.ChatID, $"Выбрана группа {user.GroupName}.");

        _DB.Update(user);
        await _DB.SaveChangesAsync();

        await HandePrintCouplesSelection(TBClient, user);
    }

    private static async Task RestartRegistration(ITelegramBotClient TBClient, TelegramUser user)
    {
        await TBClient.SendTextMessageAsync(user.ChatID, "Настройки пользователя сброшены.");

        user.Course = null;
        user.GroupName = null;
        _DB.Update(user);
        await _DB.SaveChangesAsync();

        await RequestCourse(TBClient, user.ChatID);
    }

    private static async Task HandeBugreport(ITelegramBotClient TBClient, Update update, TelegramUser user)
    {
        BugreportNote note = new(update.Message.From.FirstName,
                                update.Message.From.LastName,
                                update.Message.From.Username,
                                update.Message.Text, user);

        _DB.Bugreports.Add(note);
        await _DB.SaveChangesAsync();

        await TBClient.SendTextMessageAsync(update.GetChatID(), "Спасибо за ваш отсчёт об ошибке! Постараемся решить в скорейшее время.");
    }

    private static async Task HandePrintCouplesSelection(ITelegramBotClient TBClient, TelegramUser user)
    {
        if (user.GroupName is null || !CoupleSchedule.StudyGroups.Any(x => x.Course == user.Course && x.GroupName == user.GroupName))
        {
            await TBClient.SendTextMessageAsync(user.ChatID, "Ваша группа не найдена в расписании. Вероятно, в обновлённом рассписании её назвали как-то подругому. Выберите её заново.");
            await RestartRegistration(TBClient, user);
            return;
        }

        await TBClient.SendTextMessageAsync(user.ChatID, $"Расписание для {user.Course} - {user.GroupName}");

        var days = CoupleSchedule.Couples.Where(x => x.Course == user.Course && x.Group == user.GroupName).GroupBy(x => x.Date);

        foreach (var day in days)
        {
            StringBuilder message = new($"{day.First().Day.ToUpper()} ({day.First().Date.ToString("dd.MM.yyyy")})\n\n");

            var sortedCouples = day.OrderBy(x => GetTimeOfCouple(x.Time));
            foreach (var c in sortedCouples)
                message.Append($"{c.Time} ||  {c.Title}\n\n");

            await TBClient.SendTextMessageAsync(user.ChatID, message.ToString());
        }

        var buttons = new InlineKeyboardButton[2][];
        buttons[0] = new[] { InlineKeyboardButton.WithCallbackData("Выбрать другую группу", "RESET") };
        buttons[1] = new[] { InlineKeyboardButton.WithCallbackData("Обновить расписание", "PRINT") };
        var keyboard = new InlineKeyboardMarkup(buttons);

        await TBClient.SendTextMessageAsync(user.ChatID, "<i>Если вы заметили какую-то ошибку в работе этого бота, пожалуйста, сообщите о ней разработчикам. Это можно сделать просто подробно описав и отправив её в сообщении этому боту.</i>",
                                             parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: keyboard);
    }

    private static TimeOnly GetTimeOfCouple(string time)
    {
        string startTime = time.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];
        var splitedTime = startTime.Split(":.;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        int hours, minutes;

        int.TryParse(splitedTime[0], out hours);
        int.TryParse(splitedTime[1], out minutes);

        var res = new TimeOnly(hours, minutes);
        return res;
    }



    private static TelegramUser GetUserFromDB(long chatID)
    {
        TelegramUser user = null;
        if (_DB.Users.Count() > 0 && _DB.Users.Where(x => x.ChatID == chatID).Count() > 0)
            user = _DB.Users.Where(x => x.ChatID == chatID)?.First();

        if (user is null)
        {
            _DB.Users.Add(new TelegramUser(chatID));
            _DB.SaveChanges();
            user = _DB.Users.Where(x => x.ChatID == chatID).First();
        }

        return user;
    }

    private static long GetChatID(this Update update)
    {
        if (update.IsCallbackType())
            return update.CallbackQuery.Message.Chat.Id;
        return update.Message.Chat.Id;
    }

    private static bool IsRegistered(this TelegramUser user) => user.Course != null && user.GroupName != null;
    private static bool IsMessageType(this Update update) => update.Type == Telegram.Bot.Types.Enums.UpdateType.Message;
    private static bool IsCallbackType(this Update update) => update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery;


    private static Task HandleError(ITelegramBotClient tbc, Exception e, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
