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
            var client = new TelegramBotClient(token);

            client.StartReceiving(HandleUpdate, HandleError);



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

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                // update.CallbackQuery.Data;
            }
        }

        private record TelegramUser
        {
            public int Course { get; init; }
            public string Group { get; init; }
            public int RegStatus { get; init; }
            public TelegramUser(int course, string group, int regStatus)
            => (Course, Group, RegStatus) = (course, group, regStatus);
        }
    }
}