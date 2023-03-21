using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace job_checker;

/// <summary>
/// Описание пары
/// </summary>
/// <value></value>
public record ClassInfo
{
    /// <summary>
    /// Название предмета
    /// </summary>
    public string Title { get; init; }
    /// <summary>
    /// День недели
    /// </summary>
    public string Day { get; init; }
    /// <summary>
    /// День недели
    /// </summary>
    public string Date { get; init; }
    /// <summary>
    /// Номер аудитории
    /// </summary>
    public string Cabinet { get; init; }
    /// <summary>
    /// Название группы
    /// </summary>
    public string Group { get; init; }
    /// <summary>
    /// Название института
    /// </summary>
    public string Institute { get; init; }
    /// <summary>
    /// Номер пары в расписании
    /// </summary>
    public int Number { get; init; }
    /// <summary>
    /// Номер курса
    /// </summary>
    public int Course { get; init; }



    public ClassInfo(string title, string date, string day, string cabinet, string group, string institute, int number, int course) =>
        (Title, Date, Day, Cabinet, Group, Institute, Number, Course) = (title, date, day, cabinet, group, institute, number, course);
}
