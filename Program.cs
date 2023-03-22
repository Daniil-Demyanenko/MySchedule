using System;


namespace job_checker;
class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine(args[0]);
        CoupleSchedule.Update();
        // scheduleDownloader.CheckUpdate();
        Console.ReadKey();
    }
}

