﻿using System;
using System.Linq;
using System.Timers;

namespace job_checker;
class Program
{
    static void Main(string[] args)
    {
        CoupleSchedule.Update();
        // SetUpdateScheduleTimer(); //Запускаем таймер, проверяющий обновление расписаний каждые 6 часов
        TelegramBot.Start(args[0]);


        //////////////// Вывод спаршеных групп, перед релизом удалить ////////////////
        //var a = CoupleSchedule.Couples.Select(x => x.Course + " " + x.Group).Distinct();
        var a = CoupleSchedule.Couples.Where(x => x.Course == 3 && x.Group.ToLower() == "по (инф)") // Выбираем пары у конкретной группы
                                      .Select(x => $"{x.Date.ToString("d"),10} \t {x.Time,10} \t {x.Title}");     // Формируем строку для вывода

        foreach (var i in a)
        {
            Console.WriteLine($"{i}");
        }

        Console.WriteLine($"\nкол-во пар {CoupleSchedule.Couples.Count()}");

        Console.WriteLine("Press 'q' to stop program and exit...");
        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Q) return;
        }
    }


    static void SetUpdateScheduleTimer()
    {
        var UpdateInterval = new TimeSpan(hours: 6, minutes: 5, seconds: 0);
        var UpdateTimer = new Timer(UpdateInterval);
        UpdateTimer.Elapsed += (s, e) => ScheduleDownloader.CheckUpdate();
        UpdateTimer.AutoReset = true;
        UpdateTimer.Enabled = true;
    }
}

