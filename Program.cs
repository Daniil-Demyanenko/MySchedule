using System;
using System.Linq;
using System.Timers;

namespace job_checker;
class Program
{
    static void Main(string[] args)
    {
        TelegramBot.Start(args[0]);
        ScheduleDownloader.CheckUpdate();
        SetUpdateScheduleTimer(); //Запускаем таймер, проверяющий обновление расписаний каждые 4 часа
        CoupleSchedule.Update();

        //////////////// Вывод спаршеных групп, перед релизом удалить ////////////////
        //var a = CoupleSchedule.Couples.Select(x => x.Course + " " + x.Group).Distinct();
        // var a = CoupleSchedule.Couples.Where(x => x.Course == 3 && x.Group.ToLower() == "по (инф)") // Выбираем пары у конкретной группы
        //                               .Select(x => $"{x.Date,10} \t {x.Time,10} \t {x.Title}");    // Формируем строку для вывода

        // foreach (var i in a)
        // {
        //     Console.WriteLine($"{i}");
        // }

        Console.WriteLine($"\nкол-во пар {CoupleSchedule.Couples.Count()}");
        // var tmp = new UnitTests.ParserUnitTests();
        // tmp.ParsingTestOFOBAK1_CoupleCount();
        // tmp.ParsingTestOFOBAK1_GroupCount();
        // tmp.ParsingTestOFOBAK2_CoupleCount();
        // tmp.ParsingTestOFOBAK2_GroupCount();
        // tmp.ParsingTestOFOBAK3_CoupleCount();
        // tmp.ParsingTestOFOBAK3_GroupCount();
        // tmp.ParsingTestOFOMAG_CoupleCount();
        // tmp.ParsingTestOFOMAG_GroupCount();
        // tmp.ParsingTestZFOMAG_CoupleCount();
        // tmp.ParsingTestZFOMAG_GroupCount();
    

        Console.WriteLine("Press 'q' to stop program and exit...");
        while (true)
        {
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Q) return;
        }
    }


    static void SetUpdateScheduleTimer() //TODO: разобраться с запуском таймера при старте проги
    {
        var UpdateInterval = new TimeSpan(hours: 4, minutes: 5, seconds: 0);
        var UpdateTimer = new Timer(UpdateInterval);
        UpdateTimer.Elapsed += (s, e) => ScheduleDownloader.CheckUpdate();
        UpdateTimer.AutoReset = true;
        UpdateTimer.Enabled = true;
    }
}

