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

        //////////////// Вывод спаршеных групп, перед релизом удалить ////////////////
        //var a = CoupleSchedule.Couples.Select(x => x.Course + " " + x.Group).Distinct();
        // var a = CoupleSchedule.Couples.Where(x => x.Course == 3 && x.Group.ToLower() == "по (инф)") // Выбираем пары у конкретной группы
        //                               .Select(x => $"{x.Date,10} \t {x.Time,10} \t {x.Title}");    // Формируем строку для вывода

        // foreach (var i in a)
        // {
        //     Console.WriteLine($"{i}");
        // }

        Console.WriteLine($"\nСпаршено пар: {CoupleSchedule.Couples.Count()}");

        Console.WriteLine("Press 'q' to stop program and exit...");
        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Q) return;
        }
    }


    static void SetUpdateScheduleTimer()
    {
        ScheduleDownloader.CheckUpdate();
        CoupleSchedule.Update(ScheduleDownloader.CacheDir);

        var UpdateInterval = new TimeSpan(hours: 4, minutes: 5, seconds: 0);
        var UpdateTimer = new System.Timers.Timer(UpdateInterval);
        UpdateTimer.Elapsed += (s, e) =>
        {
            if (ScheduleDownloader.CheckUpdate())
                CoupleSchedule.Update(ScheduleDownloader.CacheDir);
        };

        UpdateTimer.AutoReset = true;
        UpdateTimer.Enabled = true;
        UpdateTimer.Start();
    }
}

