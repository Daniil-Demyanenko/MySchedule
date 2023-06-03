using System;
using System.Threading;
using MySchedule.TelegramUI;

namespace MySchedule;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1) throw new Exception("Не указан токен для бота.");

        SetUpdateScheduleTimer(); //Запускаем таймер, проверяющий обновление расписаний каждые 4 часа
        while (CouplesSchedule.Couples is null) Thread.Sleep(100);
        TelegramBot.Start(args[0]);


        Console.WriteLine("Press 'q' to stop program and exit...");
        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Q) return;
        }
    }


    static async void SetUpdateScheduleTimer()
    {
        await ScheduleDownloader.CheckUpdate();
        CouplesSchedule.Update(ScheduleDownloader.CacheDir);

        var updateInterval = new TimeSpan(hours: 4, minutes: 5, seconds: 0);
        var updateTimer = new System.Timers.Timer(updateInterval);
        updateTimer.Elapsed += (s, e) => FullUpdate();

        updateTimer.AutoReset = true;
        updateTimer.Enabled = true;
        updateTimer.Start();
    }

    static async void FullUpdate()
    {
        try
        {
            if (await ScheduleDownloader.CheckUpdate())
                CouplesSchedule.Update(ScheduleDownloader.CacheDir);
        }
        catch
        {
            Console.WriteLine("Error >> Не удалось обновить расписание.");
        }
    }
}