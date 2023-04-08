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
    /// Полная дата
    /// </summary>
    public string Date { get; init; }
    /// <summary>
    /// Название группы
    /// </summary>
    public string Group { get; init; }
    /// <summary>
    /// Номер курса
    /// </summary>
    public int Course { get; init; }



    public ClassInfo(string title, string date, string day, string group, int course) =>
        (Title, Date, Day, Group, Course) = (title, date, day, group, course);
}
