using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySchedule.InstituteParsers;

namespace MySchedule;

/// <summary>
/// Глобальное расписание пар университета
/// </summary>
public static class Schedule
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

        files.AsParallel().ForAll((file)=>{
            try
            {
                using var IFMOIOT = new TemplateScheduleParser(file);
                tempCouples.AddRange(IFMOIOT.Parse());
                tempStudyGroups.AddRange(IFMOIOT.StudyGroups);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error >> Не получилось спарсить файл: {file}. Ошибка: {e.Message}");
            }
        });

        _couples = tempCouples;
        _studyGroups = tempStudyGroups;
        Console.WriteLine($"Info >> Обновлено {_couples.Count} пар.");
    }

}