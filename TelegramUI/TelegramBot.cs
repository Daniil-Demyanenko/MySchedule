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
    private static ITelegramBotClient _TBClient;

    public static async Task Start(string token)
    {
        bool needToRestart = true;

        while (true) // Небольшой костыль из-за странности работы либы. В документации решения не нашёл.
        {
            if (needToRestart)
            {
                needToRestart = false;

                _TBClient = new TelegramBotClient(token);
                _TBClient.StartReceiving(async (tbc, u, ct) =>
                {
                    try
                    {
                        await HandleUpdate(u);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error >> Ошибка обработки сообщения: {e.Message}");
                        needToRestart = true;
                    }
                }, HandleError);
            }
            else await Task.Delay(3000);
        }
    }


    private static async Task HandleUpdate(Update update)
    {
        if (!update.IsMessageType() && !update.IsCallbackType()) return;

        var user = GetUserFromDB(update.GetChatID());


        if (update.IsMessageType() && update.Message.Text == "/start")
        {
            await _TBClient.SendTextMessageAsync(user.ChatID,
                "Этот бот служит для удобного получения расписания студентов ИФОИОТ.");
            await RequestCourse(user.ChatID);
            return;
        }


        if (update.IsMessageType())
        {
            await HandeBugreport(update, user);
            return;
        }

        // Далее мы точно уверены, что тип сообщения -- CallbackQuery

        Match matches = Regex.Match(update.CallbackQuery.Data, "^[A-Z]+");
        string command = matches.Value;

        switch (command)
        {
            case "COURSE":
                await HandeCourseSelection(update, user);
                return;
            case "GROUP":
                await HandeGroupSelection(update, user);
                return;
            case "RESET":
                await RestartRegistration(user);
                return;
            case "PRINT":
                await HandePrintCouplesSelection(user);
                return;
        }
    }


    private static async Task RequestCourse(long chatID)
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

        await _TBClient.SendTextMessageAsync(chatID, "Выберите курс:", replyMarkup: ikm);
    }

    private static async Task HandeCourseSelection(Update update, TelegramUser user)
    {
        int course = int.Parse(update.CallbackQuery.Data.Split('_', StringSplitOptions.RemoveEmptyEntries)[1]);

        await _TBClient.SendTextMessageAsync(user.ChatID, $"Выбран {course} курс.");

        user.Course = course;
        _DB.Update(user);
        await _DB.SaveChangesAsync();

        await RequestPosibleGroup(user.ChatID, course);
    }

    private static async Task RequestPosibleGroup(long chatID, int course)
    {
        var groups = Schedule.StudyGroups.Where(x => x.Course == course).Select(x => x.GroupName).Order().ToArray();
        var buttons = new InlineKeyboardButton[groups.Count() + 1][];

        for (int i = 0; i < groups.Count(); i++)
            buttons[i] = new[] { InlineKeyboardButton.WithCallbackData(groups[i], $"GROUP_{groups[i]}") };

        buttons[^1] = new[] { InlineKeyboardButton.WithCallbackData("Вернуться к выбору курса", "RESET") };

        var keyboard = new InlineKeyboardMarkup(buttons);

        await _TBClient.SendTextMessageAsync(chatID, "Выберите группу:", replyMarkup: keyboard);
    }

    private static async Task HandeGroupSelection(Update update, TelegramUser user)
    {
        Match matches = Regex.Match(update.CallbackQuery.Data, "^GROUP_(.*)");
        user.GroupName = matches.Groups[1].ToString();

        await _TBClient.SendTextMessageAsync(user.ChatID, $"Выбрана группа {user.GroupName}.");

        _DB.Update(user);
        await _DB.SaveChangesAsync();

        await HandePrintCouplesSelection(user);
    }

    private static async Task RestartRegistration(TelegramUser user)
    {
        await _TBClient.SendTextMessageAsync(user.ChatID, "Настройки пользователя сброшены.");

        user.Course = null;
        user.GroupName = null;
        _DB.Update(user);
        await _DB.SaveChangesAsync();

        await RequestCourse(user.ChatID);
    }

    private static async Task HandeBugreport(Update update, TelegramUser user)
    {
        var from = update.Message.From;
        BugreportNote note = new(from.FirstName, from.LastName, from.Username, update.Message.Text, user);

        _DB.Bugreports.Add(note);
        await _DB.SaveChangesAsync();

        await _TBClient.SendTextMessageAsync(update.GetChatID(),
            "Спасибо за ваш отсчёт об ошибке! Постараемся решить в скорейшее время.");
    }

    private static async Task HandePrintCouplesSelection(TelegramUser user)
    {
        if (user.GroupName is null ||
            !Schedule.StudyGroups.Any(x => x.Course == user.Course && x.GroupName == user.GroupName))
        {
            await _TBClient.SendTextMessageAsync(user.ChatID,
                "Ваша группа не найдена в расписании. Вероятно, в обновлённом рассписании её назвали как-то подругому. Выберите её заново.");
            await RestartRegistration(user);
            return;
        }

        await _TBClient.SendTextMessageAsync(user.ChatID, $"Расписание для {user.Course} - {user.GroupName}");

        var days = Schedule.Couples.Where(x => x.Course == user.Course && x.Group == user.GroupName)
            .GroupBy(x => x.Date);

        foreach (var day in days)
        {
            StringBuilder message = new($"{day.First().Day.ToUpper()} ({day.First().Date.ToString("dd.MM.yyyy")})\n\n");

            var sortedCouples = day.OrderBy(x => GetTimeOfCouple(x.Time));
            foreach (var c in sortedCouples)
                message.Append($"{c.Time} ||  {c.Title}\n\n");

            await _TBClient.SendTextMessageAsync(user.ChatID, message.ToString());
        }

        var buttons = new InlineKeyboardButton[2][];
        buttons[0] = new[] { InlineKeyboardButton.WithCallbackData("Выбрать другую группу", "RESET") };
        buttons[1] = new[] { InlineKeyboardButton.WithCallbackData("Обновить расписание", "PRINT") };
        var keyboard = new InlineKeyboardMarkup(buttons);

        await _TBClient.SendTextMessageAsync(user.ChatID,
            "<i>Если вы заметили какую-то ошибку в работе этого бота, пожалуйста, сообщите о ней разработчикам. Это можно сделать просто подробно описав и отправив её в сообщении этому боту.</i>",
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
        TelegramUser user = _DB.Users.Find(chatID);

        if (user is null)
        {
            _DB.Users.Add(new TelegramUser(chatID));
            _DB.SaveChanges();
            user = _DB.Users.Find(chatID);
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

    
    // Да, если тип апдейта -- Message, то не факт, что у него будет поле Message, 
    // и если есть поле, не факт, что у него будет поле Text. Очень крутая либа...
    private static bool IsMessageType(this Update update)
        => update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update?.Message?.Text is not null; 
    
    
    private static bool IsCallbackType(this Update update) =>
        update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery;


    private static Task HandleError(ITelegramBotClient tbc, Exception e, CancellationToken ct)
    {
        throw e;
    }
}