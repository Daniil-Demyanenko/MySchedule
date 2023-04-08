using System;
using System.Linq;
using System.Timers;

namespace job_checker;
class Program
{
    static void Main(string[] args)
    {
        TelegramBot bot = new TelegramBot(args[0]);
        CoupleSchedule.Update();
        // SetUpdateScheduleTimer(); //Запускаем таймер, проверяющий обновление расписаний каждые 4 часа
        bot.WaitMessages();




        //////////////// Вывод спаршеных групп, перед релизом удалить ////////////////
        //var a = CoupleSchedule.Couples.Select(x => x.Course + " " + x.Group).Distinct();
        var a = CoupleSchedule.Couples.Where(x => x.Course == 3 && x.Group.ToLower() == "по (инф)").Select(x=> $"{x.Date} | {x.Title}");

        foreach (var i in a)
        {
            Console.WriteLine($"{i}");
        }
        Console.WriteLine($"кол-во пар {CoupleSchedule.Couples.Count()}");
        Console.ReadKey();
    }


    static void SetUpdateScheduleTimer()
    {
        var UpdateInterval = new TimeSpan(hours: 4, minutes: 0, seconds: 0);
        var UpdateTimer = new Timer(UpdateInterval);
        UpdateTimer.Elapsed += (s, e) => ScheduleDownloader.CheckUpdate();
        UpdateTimer.AutoReset = true;
        UpdateTimer.Enabled = true;
    }
}

