using System;
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

namespace job_checker.TelegramUI;

/// <summary>
/// Класс, отвечающий за взаимодействие с пользователем
/// </summary>
public static class TelegramBot
{
    private static TelegramDBContext DB = new();

    public static void Start(string token)
    {
        var TBClient = new TelegramBotClient(token);

        TBClient.StartReceiving(HandleUpdate, HandleError);


    }


    private async static Task HandleUpdate(ITelegramBotClient TBClient, Update update, CancellationToken arg3)
    {
        if (!update.IsMessageType() && !update.IsCallbackType())
        {
            //await TBClient.SendTextMessageAsync(chatID, "Неподдерживаемый тип сообщений!");
            return;
        }

        var user = GetUserFromDB(update.GetChatID());
        var chatID = update.GetChatID();


        if (update.IsMessageType() && update.Message.Text == "/start" && !user.IsRegistered())
        {
            await TBClient.SendTextMessageAsync(chatID, "Этот бот служит для удобного получения расписания студентов ИФОИОТ.");
            await RequestCourse(TBClient, chatID);
            return;
        }

        if (update.IsMessageType())
        {
            HandeBugreport(TBClient, update, user);
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
                await RestartRegistration(TBClient, chatID, user);
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
        DB.Update(user);
        await DB.SaveChangesAsync();

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

        await TBClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Выбрана группа {user.GroupName}.");

        DB.Update(user);
        await DB.SaveChangesAsync();
    }

    private static async Task RestartRegistration(ITelegramBotClient TBClient, long chatID, TelegramUser user)
    {
        await TBClient.SendTextMessageAsync(chatID, "Настройки пользователя сброшены.");

        user.Course = null;
        user.GroupName = null;
        DB.Update(user);
        await DB.SaveChangesAsync();

        await RequestCourse(TBClient, chatID);
    }

    private static async Task HandeBugreport(ITelegramBotClient TBClient, Update update, TelegramUser user)
    {
        BugreportNote note = new(update.Message.From.FirstName,
                                update.Message.From.LastName,
                                update.Message.From.Username,
                                update.Message.Text, user);

        DB.Bugreports.Add(note);
        await DB.SaveChangesAsync();

        await TBClient.SendTextMessageAsync(update.GetChatID(), "Спасибо за отзыв! Постараемся решить в скорейшее время.");
    }

    private static async Task HandePrintCouplesSelection(ITelegramBotClient TBClient, Update update, int course) { }




    private static TelegramUser GetUserFromDB(long chatID)
    {
        TelegramUser user = null;
        if (DB.Users.Count() > 0)
            user = DB.Users.Where(x => x.ChatID == chatID)?.First();

        if (user is null)
        {
            DB.Users.Add(new TelegramUser(chatID));
            DB.SaveChanges();
            user = DB.Users.Where(x => x.ChatID == chatID).First();
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


    private static Task HandleError(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        throw new NotImplementedException();
    }
}
