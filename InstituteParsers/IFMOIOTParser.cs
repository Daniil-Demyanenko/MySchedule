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
    private int _GroupNameRow; // Строка с названиями групп

    // Индекс столбца, с которого начинаются названия групп
    private int _firstColWithCouple = 3;

    // Позиции групп в расписании не по порядку, скрыты удалённые специализации и т.д. 
    private List<int>? _GroupNamePositions;

    //TODO: заменить числовые константы на DateCol, TimeCol, DayCol, GroupNameRow и т.д.

    public IFMOIOTParser(string path) : base(path) { }
    ~IFMOIOTParser() {base.Dispose(); }


    public override List<ClassInfo> Parse()
    {
        List<ClassInfo> result = new();
        var lvl = LogLevels.Debug; //уровень логирования

        _GroupNameRow = 5;
        _GroupNamePositions = GetGroupNamePositions();
        var dayPos = GetDaysRowInformation();

        foreach (var pos in _GroupNamePositions)
            result.AddRange(GetGroupClasses(4, dayPos));


        foreach (var i in result)
            LogSingleton.Instance.LogLine(lvl, "{0} {1} \n{2} {3} {4} {5}\n\n", i.Date, i.Day, i.Course, i.Group, i.Title, i.Number);

        foreach (var i in _GroupNamePositions)
            LogSingleton.Instance.Log(lvl, $"{i} ");

        LogSingleton.Instance.Log(lvl, _Sheet.Cells[_GroupNameRow, 63].Value.ToString()?.Trim());


        return result;
    }

    /// <summary>
    /// Возвращает информацию о не скрытых днях недели в расписании
    /// </summary>
    private List<DayData> GetDaysRowInformation()
    {
        List<DayData> dayPosition = new();

        for (int i = _FirstVisibleCell.Row; i < _Sheet.Cells.MaxDataRow; i++)
        {
            var cellValue = _Sheet.Cells[i, 0].Value?.ToString()?.Trim();

            if (cellValue is not null &&        //Ячейка имеет значение, содержит день недели, не является скрытой
                isContainsDay(cellValue) &&
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
    private List<ClassInfo> GetGroupClasses(int col, List<DayData> dayPos) //TODO: Реализовать для двухъячеечных групп корректное добавление пар и имени группы
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

                var date = day.Date + " (" + _Sheet.Cells[day.Pos + i, 2].Value.ToString()?.Trim() + ")";
                var classItem = new ClassInfo(className, date,
                                    day.Name, cabinet: "", groupName, "ИФМОИОТ", number: i + 1, course);
                result.Add(classItem);
            }

        return result;
    }

    /// <summary>
    /// Возвращает позиции столбцов с названиями групп
    /// </summary>
    private List<int> GetGroupNamePositions()
    {
        List<int> result = new();

        for (int i = _firstColWithCouple; i < _Sheet.Cells.MaxDataRow; i++)
        {
            var cellValue = _Sheet.Cells[_GroupNameRow + 1, i].Value?.ToString()?.Trim();

            if (cellValue is not null && !_Sheet.Cells.Columns[i].IsHidden)       //Ячейка имеет значение, не является скрытой
                result.Add(i);
        }

        return result;
    }

}
