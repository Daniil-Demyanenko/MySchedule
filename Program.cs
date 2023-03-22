using System;


namespace job_checker;
class Program
{
    static void Main(string[] args)
    {
        // настраиваем уровень логирования
        LogSingleton.Instance.LogLevel = LogLevels.Debug;

        System.Console.WriteLine(args[0]);
        CoupleSchedule.Update();
        // scheduleDownloader.CheckUpdate();
        Console.ReadKey();
    }
}

