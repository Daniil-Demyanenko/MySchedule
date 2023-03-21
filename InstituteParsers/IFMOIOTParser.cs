using System;
using Aspose.Cells;
using System.Collections.Generic;
using static System.Console;
#nullable enable

namespace job_checker.InstituteParsers;

/// <summary>
/// Парсер для расписаний института ИФМОИОТ
/// </summary>
public class IFMOIOTParser : AbstractParser, IDisposable
{
    //private int _VisibleLinesStartIndex = 38 - 1; // Видимая строка, с которой начинается расписание пар
    private int _GroupNameRow = 5; // Строка с названиями групп

    private int[] _GroupPosition = { }; // Позиции групп в расписании не по порядку, скрыты удалённые специализации и т.д. 

            //TODO: заменить числовые константы на DateCol, TimeCol, DayCol, GroupNameRow и т.д.

    public IFMOIOTParser(string path) : base(path) { }
    ~IFMOIOTParser() { _Workbook.Dispose(); }


    public override List<ClassInfo> Parse()
    {
        List<ClassInfo> result = new();

        var dayPos = GetDaysRowInformation();

        result.AddRange(GetGroupClasses(4, dayPos));

        WriteLine();
        foreach (var i in result)
        {
            WriteLine("{0} {1} \n{2} {3} {4} {5}\n\n", i.Date, i.Day, i.Course, i.Group, i.Title, i.Number);
        }

        return result;
    }

    /// <summary>
    /// Возвращает информацию о не скрытых днях недели в расписании
    /// </summary>
    /// <returns></returns>
    private List<DayData> GetDaysRowInformation()
    {
        List<DayData> dayPosition = new();

        for (int i = _FirstVisibleCell.Row; i < _Sheet.Cells.MaxDataRow; i++)
        {
            var cellValue = _Sheet.Cells[i, 0].Value?.ToString()?.Trim();

            if (cellValue is not null &&        //Ячейка имеет значение, содержит день недели, не является скрытой
                cellValue.ContainsDay() && 
                !_Sheet.Cells.Rows[i].IsHidden
                )
            {
                dayPosition.Add(new DayData(pos: i, name: cellValue, date: _Sheet.Cells[i, 1].Value?.ToString()?.Trim()));
            }
        }


        return dayPosition;
    }

    /// <summary>
    /// Возвращает информацию о парах для определённой группы
    /// </summary>
    /// <param name="col">колонка с группой</param>
    /// <param name="dayPos">Позиции дней недели</param>
    /// <returns></returns>
    private List<ClassInfo> GetGroupClasses(int col, List<DayData> dayPos)
    {
        var result = new List<ClassInfo>();

        string[] groupTitle = _Sheet.Cells[_GroupNameRow, col].Value.ToString().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries); // Полное название группы
        string groupName = String.Join(' ', groupTitle[1..]); // Только название группы
        int course = int.Parse(groupTitle[0]); // Только курс группы

        foreach (var day in dayPos)
            for (int i = 0; i < 4; i++)
            {
                string? className = _Sheet.Cells[day.Pos + i, col].Value?.ToString() ?? null;
                if (className is null) continue;

                var date = day.Date + " (" + _Sheet.Cells[day.Pos + i, 2].Value.ToString().Trim() +")";
                var classItem = new ClassInfo(className, date , 
                                    day.Name, cabinet:"", groupName, "ИФМОИОТ", number: i + 1, course);
                result.Add(classItem);
            }

        return result;
    }



}
