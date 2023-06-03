using System;
using System.Collections.Generic;
using System.IO;
using MySchedule.InstituteParsers;
using System.Threading.Tasks;
using System.Threading;

namespace MySchedule;

/// <summary>
/// Глобальное расписание пар университета
/// </summary>
public static class CouplesSchedule
{
    /// <summary>
    /// Список пар всех институтов
    /// </summary>
    public static IEnumerable<ClassInfo> Couples => _couples;
    /// <summary>
    /// Список всех учебных групп
    /// </summary>
    public static IEnumerable<StudyGroup> StudyGroups => _studyGroups;

    private static List<StudyGroup> _studyGroups;
    private static List<ClassInfo> _couples;



    /// <summary>
    /// Заново распарсить и заполнить данные о расписании
    /// </summary>
    public static void Update(string cachePath)
    {
        var files = Directory.GetFiles(cachePath);

        var tempCouples = new List<ClassInfo>();
        var tempStudyGroups = new List<StudyGroup>();

        var parsing = Parallel.ForEach(files, (file)=>{
            try
            {
                using var IFMOIOT = new TemplateScheduleParser(file);
                tempCouples.AddRange(IFMOIOT.Parse());
                tempStudyGroups.AddRange(IFMOIOT.StudyGroups);
            }
            catch
            {
                Console.WriteLine($"Error >> Не получилось спарсить файл: {file}");
            }
        });

        // foreach (var file in files)
        // {
        //     try
        //     {
        //         using var IFMOIOT = new TemplateScheduleParser(file);
        //         tempCouples.AddRange(IFMOIOT.Parse());
        //         tempStudyGroups.AddRange(IFMOIOT.StudyGroups);
        //     }
        //     catch
        //     {
        //         Console.WriteLine($"Error >> Не получилось спарсить файл: {file}");
        //     }
        // }


        while (!parsing.IsCompleted)
            Thread.Sleep(20);

        _couples = tempCouples;
        _studyGroups = tempStudyGroups;
        Console.WriteLine($"Info >> Обновлено {_couples.Count} пар.");
    }

}