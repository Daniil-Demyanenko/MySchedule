
using System;
using System.Collections.Generic;
using job_checker.InstituteParsers;

namespace job_checker;

/// <summary>
/// Глобальное расписание пар университета
/// </summary>
public static class CoupleSchedule
{
    /// <summary>
    /// Список пар всех институтов
    /// </summary>
    public static IEnumerable<ClassInfo> Couples => _Couples;
    /// <summary>
    /// Список всех учебных групп
    /// </summary>
    public static IEnumerable<StudyGroup> StudyGroups => _StudyGroups;

    private static string _cdir = AppDomain.CurrentDomain.BaseDirectory + "/Cache";
    private static List<StudyGroup> _StudyGroups;
    private static List<ClassInfo> _Couples;



    /// <summary>
    /// Заново распарсить и заполнить расписание
    /// </summary>
    public static void Update()
    {
        using var IFMOIOT = new TemplateScheduleParser(AppDomain.CurrentDomain.BaseDirectory + "/ras.xls");
        _Couples = IFMOIOT.Parse();
        _StudyGroups = IFMOIOT.StudyGroups;
    }

}