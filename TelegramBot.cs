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

namespace job_checker
{
    /// <summary>
    /// Класс, отвечающий за взаимодействие с пользователем
    /// </summary>
    public static class TelegramBot
    {
        private static Dictionary<ChatId, TelegramUser> _Users = new();
        public static void Start(string token)
        {
            var TBClient = new TelegramBotClient(token);

            TBClient.StartReceiving(HandleUpdate, HandleError);


        }

        private static Task HandleError(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        private async static Task HandleUpdate(ITelegramBotClient TBClient, Update update, CancellationToken arg3)
        {
            var ikm = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("1 курс", "1"),
                    InlineKeyboardButton.WithCallbackData("2 курс", "2"),
                    InlineKeyboardButton.WithCallbackData("3 курс", "3"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("4 курс", "4"),
                    InlineKeyboardButton.WithCallbackData("5 курс", "5"),
                },
            });


            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message.Text == "/start")
            {
                await TBClient.SendTextMessageAsync(update.Message.Chat.Id, "Здарова меченый, ты на старт нажал и в благородство я играть не буду. Выберешь курс свой, потом группу и мы в расчёте.", replyMarkup: ikm);
            }

            int course;
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery && int.TryParse(update.CallbackQuery.Data, out course))
            {
                _Users.TryAdd(update.CallbackQuery.Message.Chat.Id, new TelegramUser() { Course = course, RegStatus = 1 });
                var a = update.CallbackQuery.Data;

                await PrintPosibleGroupsForUser(TBClient, update, course);
                if (update.CallbackQuery.Data == "start")
                {
                    await TBClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Успешно", replyMarkup: ikm);
                }
                await TBClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Выбран {a} курс");

            }

        }

        private static async Task PrintPosibleGroupsForUser(ITelegramBotClient TBClient, Update update, int course)
        {

            var groups = CoupleSchedule.StudyGroups.Where(x => x.Course == course).Select(x => x.GroupName).Order().ToArray();
            var buttons = new InlineKeyboardButton[groups.Count() + 1][];
            for (int i = 0; i < groups.Count(); i++)
                buttons[i] = new[] { InlineKeyboardButton.WithCallbackData(groups[i], groups[i]) };

            buttons[^1] = new[] { InlineKeyboardButton.WithCallbackData("Вернуться к выбору курса", "start") };

            var keyboard = new InlineKeyboardMarkup(buttons);

            await TBClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Выберите группу:", replyMarkup: keyboard);
        }


    }


    public struct TelegramUser
    {
        public int Course;
        public string Group;
        public int RegStatus = 0;
        public TelegramUser() { }
        public TelegramUser(int course, string group, int regStatus)
        => (Course, Group, RegStatus) = (course, group, regStatus);
    }
}