
using System;
using System.Collections.Generic;
using job_checker.InstituteParsers;

namespace job_checker;

/// <summary>
/// Глобальное расписание пар университета
/// </summary>
public static class CoupleSchedule
{
    private static List<ClassInfo> _Couples;
    /// <summary>
    /// Список пар всех институтов
    /// </summary>
    /// <value></value>
    public static IEnumerable<ClassInfo> Couples => _Couples;


    /// <summary>
    /// Заново распарсить и заполнить расписание
    /// </summary>
    public static void Update()
    {
        using var IFMOIOT = new IFMOIOTParser(AppDomain.CurrentDomain.BaseDirectory + "/ras.xls");
        _Couples = IFMOIOT.Parse();
    }

}