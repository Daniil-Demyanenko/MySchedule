
using System;
using System.Collections.Generic;
using job_checker.InstituteParsers;

namespace job_checker;

/// <summary>
/// Глобальное расписание пар университета
/// </summary>
public static class CoupleSchedule
{
    private static List<ClassInfo> _Jobs;
    /// <summary>
    /// Список пар всех институтов
    /// </summary>
    /// <value></value>
    public static IEnumerable<ClassInfo> Jobs => _Jobs;


    /// <summary>
    /// Заново распарсить и заполнить расписание
    /// </summary>
    public static void Update()
    {
        using var IFMOIOT = new IFMOIOTParser(AppDomain.CurrentDomain.BaseDirectory + "/ras.xls");
        _Jobs = IFMOIOT.Parse();
    }

}