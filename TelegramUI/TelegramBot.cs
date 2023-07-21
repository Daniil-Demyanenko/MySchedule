using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using System.Text.RegularExpressions;

namespace MySchedule.TelegramUI;

/// <summary>
/// Класс, отвечающий за взаимодействие с пользователем
/// </summary>
public static class TelegramBot
{
    private static readonly TelegramDBContext _db = new();
    private static ITelegramBotClient _tbClient;

    public static async void Start(string token)
    {
        bool needToRestart = true;

        while (true) // Небольшой костыль из-за странности работы либы. В документации решения не нашёл.
        {
            if (needToRestart)
            {
                needToRestart = false;
                
                _tbClient = new TelegramBotClient(token);
                _tbClient.StartReceiving(async (_, u, _) =>
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

        var user = GetUserFromDB(update.GetChatId());


        if (update.IsMessageType() && update.Message.Text == "/start")
        {
            await _tbClient.SendTextMessageAsync(user.ChatID,
                "Этот бот служит для удобного получения расписания студентов ИФОИОТ.");
            await RequestCourse(user.ChatID);
            return;
        }


        if (update.IsMessageType())
        {
            await HandleBugreport(update, user);
            return;
        }

        // Далее мы точно уверены, что тип сообщения -- CallbackQuery

        Match matches = Regex.Match(update.CallbackQuery.Data, "^[A-Z]+");
        string command = matches.Value;

        switch (command)
        {
            case "COURSE":
                await HandleCourseSelection(update, user);
                return;
            case "GROUP":
                await HandleGroupSelection(update, user);
                return;
            case "RESET":
                await RestartRegistration(user);
                return;
            case "PRINT":
                await HandlePrintCouplesSelection(user);
                return;
        }
    }


    private static async Task RequestCourse(long chatId)
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

        await _tbClient.SendTextMessageAsync(chatId, "Выберите курс:", replyMarkup: ikm);
    }

    private static async Task HandleCourseSelection(Update update, TelegramUser user)
    {
        int course = int.Parse(update.CallbackQuery.Data.Split('_', StringSplitOptions.RemoveEmptyEntries)[1]);

        await _tbClient.SendTextMessageAsync(user.ChatID, $"Выбран {course} курс.");

        user.Course = course;
        _db.Update(user);
        await _db.SaveChangesAsync();

        await RequestPossibleGroup(user.ChatID, course);
    }

    private static async Task RequestPossibleGroup(long chatId, int course)
    {
        var groups = Schedule.StudyGroups.Where(x => x.Course == course).Select(x => x.GroupName).Order().ToArray();
        var buttons = new InlineKeyboardButton[groups.Count() + 1][];

        for (int i = 0; i < groups.Count(); i++)
            buttons[i] = new[] { InlineKeyboardButton.WithCallbackData(groups[i], $"GROUP_{groups[i]}") };

        buttons[^1] = new[] { InlineKeyboardButton.WithCallbackData("Вернуться к выбору курса", "RESET") };

        var keyboard = new InlineKeyboardMarkup(buttons);

        await _tbClient.SendTextMessageAsync(chatId, "Выберите группу:", replyMarkup: keyboard);
    }

    private static async Task HandleGroupSelection(Update update, TelegramUser user)
    {
        Match matches = Regex.Match(update.CallbackQuery.Data, "^GROUP_(.*)");
        user.GroupName = matches.Groups[1].ToString();

        await _tbClient.SendTextMessageAsync(user.ChatID, $"Выбрана группа {user.GroupName}.");

        _db.Update(user);
        await _db.SaveChangesAsync();

        await HandlePrintCouplesSelection(user);
    }

    private static async Task RestartRegistration(TelegramUser user)
    {
        await _tbClient.SendTextMessageAsync(user.ChatID, "Настройки пользователя сброшены.");

        user.Course = null;
        user.GroupName = null;
        _db.Update(user);
        await _db.SaveChangesAsync();

        await RequestCourse(user.ChatID);
    }

    private static async Task HandleBugreport(Update update, TelegramUser user)
    {
        var from = update.Message.From;
        BugreportNote note = new(from.FirstName, from.LastName, from.Username, update.Message.Text, user);

        _db.Bugreports.Add(note);
        await _db.SaveChangesAsync();

        await _tbClient.SendTextMessageAsync(update.GetChatId(),
            "Спасибо за ваш отсчёт об ошибке! Постараемся решить в скорейшее время.");
    }

    private static async Task HandlePrintCouplesSelection(TelegramUser user)
    {
        if (user.GroupName is null ||
            !Schedule.StudyGroups.Any(x => x.Course == user.Course && x.GroupName == user.GroupName))
        {
            await _tbClient.SendTextMessageAsync(user.ChatID,
                "Ваша группа не найдена в расписании. Вероятно, в обновлённом рассписании её назвали как-то подругому (с маленькой буквы, без скобок, т.д.). " +
                "Выберите её заново.");
            await RestartRegistration(user);
            return;
        }

        await _tbClient.SendTextMessageAsync(user.ChatID, $"Расписание для {user.Course} - {user.GroupName}");

        var days = Schedule.Couples.Where(x => x.Course == user.Course && x.Group == user.GroupName && x.Date >= DateTime.Now - TimeSpan.FromDays(3))
            .GroupBy(x => x.Date);

        foreach (var day in days)
        {
            StringBuilder message = new($"{day.First().Day.ToUpper()} ({day.First().Date: yy-MM-dd.})\n\n");

            var sortedCouples = day.OrderBy(x => GetTimeOfCouple(x.Time));
            foreach (var c in sortedCouples)
                message.Append($"{c.Time} ||  {c.Title}\n\n");

            await _tbClient.SendTextMessageAsync(user.ChatID, message.ToString());
        }

        var buttons = new InlineKeyboardButton[2][];
        buttons[0] = new[] { InlineKeyboardButton.WithCallbackData("Выбрать другую группу", "RESET") };
        buttons[1] = new[] { InlineKeyboardButton.WithCallbackData("Обновить расписание", "PRINT") };
        var keyboard = new InlineKeyboardMarkup(buttons);

        await _tbClient.SendTextMessageAsync(user.ChatID,
            "<i>Если вы заметили какую-то ошибку в работе этого бота, пожалуйста, сообщите о ней разработчикам. " +
            "Это можно сделать просто подробно описав и отправив её в сообщении этому боту.</i>",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: keyboard);
    }

    private static TimeOnly GetTimeOfCouple(string time)
    {
        string startTime = time.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];
        var separatedTime = startTime.Split(":.;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        int.TryParse(separatedTime[0], out int hours);
        int.TryParse(separatedTime[1], out int minutes);

        var res = new TimeOnly(hours, minutes);
        return res;
    }
    
    private static TelegramUser GetUserFromDB(long chatId)
    {
        TelegramUser user = _db.Users.Find(chatId);

        if (user is null)
        {
            _db.Users.Add(new TelegramUser(chatId));
            _db.SaveChanges();
            user = _db.Users.Find(chatId);
        }

        return user;
    }

    private static long GetChatId(this Update update) =>
        update.IsCallbackType() ? update.CallbackQuery.Message.Chat.Id : update.Message.Chat.Id;


    // Да, если тип апдейта -- Message, то не факт, что у него будет поле Message, 
    // и если есть поле, не факт, что у него будет поле Text. Очень крутая либа...
    private static bool IsMessageType(this Update update)
        => update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message?.Text is not null;

    private static bool IsCallbackType(this Update update) =>
        update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery;


    private static Task HandleError(ITelegramBotClient tbc, Exception e, CancellationToken ct)
    {
        throw e;
    }
}