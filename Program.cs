using System;


namespace job_checker;
class Program
{
    static void Main(string[] args)
    {
        CoupleSchedule.Update();
        //ScheduleDownloader.CheckUpdate();
        Console.ReadKey();
    }
}

