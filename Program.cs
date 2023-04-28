using System;
using System.Threading;
using System.Linq;
using job_checker.TelegramUI;

namespace job_checker;
class Program
{
    static void Main(string[] args)
    {
        SetUpdateScheduleTimer(); //Запускаем таймер, проверяющий обновление расписаний каждые 4 часа
        while (CoupleSchedule.Couples is null) Thread.Sleep(100);
        TelegramBot.Start(args[0]);

        Console.WriteLine("Press 'q' to stop program and exit...");
        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Q) return;
        }
    }


    static void SetUpdateScheduleTimer()
    {
        FullUpdate();

        var UpdateInterval = new TimeSpan(hours: 4, minutes: 5, seconds: 0);
        var UpdateTimer = new System.Timers.Timer(UpdateInterval);
        UpdateTimer.Elapsed += (s, e) => FullUpdate();

        UpdateTimer.AutoReset = true;
        UpdateTimer.Enabled = true;
        UpdateTimer.Start();
    }

    static void FullUpdate()
    {
        try
        {
            if (ScheduleDownloader.CheckUpdate())
                CoupleSchedule.Update(ScheduleDownloader.CacheDir);
        }
        catch
        {
            Console.WriteLine("Error >> Не удалось обновить расписание.");
        }
    }
}

