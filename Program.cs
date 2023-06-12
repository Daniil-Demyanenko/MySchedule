using System;
using System.Threading;
using System.Threading.Tasks;
using MySchedule.TelegramUI;

namespace MySchedule;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length != 1) throw new Exception("Не указан токен для бота.");

        SetUpdateScheduleTimer(); //Запускаем таймер, проверяющий обновление расписаний каждые 4 часа
        while (Schedule.Couples is null) await Task.Delay(100);
        await TelegramBot.Start(args[0]);


        Console.WriteLine("Press 'q' to stop program and exit...");
        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Q) return;
        }
    }


    private static async void SetUpdateScheduleTimer()
    {
        await ScheduleDownloader.CheckUpdate();
        Schedule.Update(ScheduleDownloader.CacheDir);

        var updateInterval = new TimeSpan(hours: 4, minutes: 5, seconds: 0);
        var updateTimer = new System.Timers.Timer(updateInterval);
        updateTimer.Elapsed += (s, e) => FullUpdate();

        updateTimer.AutoReset = true;
        updateTimer.Enabled = true;
        updateTimer.Start();
    }

    private static async void FullUpdate()
    {
        try
        {
            if (await ScheduleDownloader.CheckUpdate())
                Schedule.Update(ScheduleDownloader.CacheDir);
        }
        catch
        {
            Console.WriteLine("Error >> Не удалось обновить расписание.");
        }
    }
}