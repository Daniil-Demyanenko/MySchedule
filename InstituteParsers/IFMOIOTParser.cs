using System;
using Aspose.Cells;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using System.Text;
#nullable enable

namespace job_checker.InstituteParsers;

/// <summary>
/// Парсер для расписаний института ИФМОИОТ
/// </summary>
public class IFMOIOTParser : AbstractParser, IDisposable
{
    //private int _VisibleLinesStartIndex = 38 - 1; // Видимая строка, с которой начинается расписание пар
    private int _GroupNameRow = 5; // Строка с названиями групп

    // Индекс столбца, с которого начинаются названия групп
    private int _firstColWithCouple = 3;

    // Позиции групп в расписании не по порядку, скрыты удалённые специализации и т.д. 
    private List<int>? _GroupNamePositions;

    //TODO: заменить числовые константы на DateCol, TimeCol, DayCol, GroupNameRow и т.д.

    public IFMOIOTParser(string path) : base(path) { }
    ~IFMOIOTParser() { base.Dispose(); }


    public override List<ClassInfo> Parse()
    {
        List<ClassInfo> result = new();

        _GroupNamePositions = GetGroupNamePositions();
        var dayPos = GetDaysRowInformation();

       foreach (var pos in _GroupNamePositions)
           result.AddRange(GetGroupClasses(pos, dayPos));

        foreach (var i in result)
            Console.WriteLine("{0, -30} {1} \n{2, -2} {3} \n{4, -2} {5}\n\n", i.Date, i.Day, i.Course, i.Group, i.Number, i.Title);

        foreach (var i in _GroupNamePositions)
            Console.Write($"{i} ");

        Console.WriteLine(_Sheet.Cells[_GroupNameRow, 63].Value.ToString()?.Trim());


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
        int course;
        string groupName;

        if (_Sheet.Cells[_GroupNameRow, col].IsMerged)
            (course, groupName) = SplitGroupNameForMerged(colWithGroup: col);
        else (course, groupName) = SplitGroupName(colWithGroup: col);



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
    /// Разделяет название группы на Курс и Направление подготовки
    /// </summary>
    /// <param name="colWithGroup"></param>
    /// <returns>(Курс, Направление подготовки)</returns>
    private (int, string) SplitGroupName(int colWithGroup)
    {
        string[] groupTitle = _Sheet.Cells[_GroupNameRow, colWithGroup].Value.ToString().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries); // Полное название группы
        string groupName = String.Join(' ', groupTitle[1..]); // Только название группы
        int course = int.Parse(groupTitle[0]); // Только курс группы

        return (course, groupName);
    }

    /// <summary>
    /// Разделяет название группы на Курс и Направление подготовки.
    /// Добавляет уточнение для объеденённых групп в название
    /// </summary>
    /// <param name="colWithGroup"></param>
    /// <returns>(Курс, Направление подготовки)</returns>
    private (int, string) SplitGroupNameForMerged(int colWithGroup)
    {
        bool isSecondCell = _Sheet.Cells[_GroupNameRow, colWithGroup].Value == null; // Является второй ячейкай в объединении?

        if (isSecondCell) colWithGroup--;

        (int course, string groupName) = SplitGroupName(colWithGroup);

        string appendName = _Sheet.Cells[_GroupNameRow + 1, isSecondCell ? colWithGroup + 1 : colWithGroup].Value.ToString().Trim()[0..^1].Trim();
        groupName += $" [{appendName}]";
        

        return (course, groupName);
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
