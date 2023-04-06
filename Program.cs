using System;
using System.Linq;


namespace job_checker;
class Program
{
    static void Main(string[] args)
    {
        TelegramBot bot = new TelegramBot(args[0]);
        CoupleSchedule.Update();
        // scheduleDownloader.CheckUpdate();
        bot.WaitMessages();




        //////////////// Вывод спаршеных групп, перед релизом удалить ////////////////
        var a = CoupleSchedule.Couples.Select(x => x.Course + " " + x.Group).Distinct();

        foreach (var i in a)
        {
            Console.WriteLine($"{i}");
        }
        Console.WriteLine($"кол-во пар {CoupleSchedule.Couples.Count()}");
        Console.ReadKey();
    }
}

