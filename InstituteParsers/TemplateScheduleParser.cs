using System;
using System.Collections.Generic;
using System.Globalization;
using Aspose.Cells;
#nullable enable

namespace job_checker.InstituteParsers;

// TODO:
// Поиск имён преподавателей, регулярка: \b[\w]{4,}\b\s+\w\.\s*\w\.*
// Поиск номера аудитории, регулярка: \b\d{1,}-{0,1}\d{2,}\w{0,1}

/// <summary>
/// Парсер шаблона ИФМОИОТ
/// </summary>
public class TemplateScheduleParser : IDisposable
{
    // Позиции групп в расписании, не по порядку, исключая удалённые специализации и т.д. 
    private List<int>? _GroupNamePositions;
    private int _GroupNameRow = 5; // Строка с названиями групп
    private CellPosition _FirstVisibleCell;
    private Workbook _Workbook;
    private Worksheet _Sheet;
    private int _MaxDataCol;
    private int _MaxDataRow;

    public TemplateScheduleParser(string path)
    {
        _Workbook = new Workbook(path);
        _Sheet = FindPageWithSchedule();
        _FirstVisibleCell = GetFirstVisibleCell();
        _MaxDataCol = _Sheet.Cells.MaxDataColumn;
        _MaxDataRow = _Sheet.Cells.MaxDataRow;
    }
    ~TemplateScheduleParser()
    {
        this.Dispose();
    }
    public void Dispose()
    {
        _Sheet.Dispose();
        _Workbook.Dispose();
    }

    public List<ClassInfo> Parse()
    {
        List<ClassInfo> result = new();

        _GroupNamePositions = GetGroupNamePositions();
        var dayPos = GetDaysRowInformation();

        foreach (var pos in _GroupNamePositions)
            result.AddRange(GetGroupClasses(pos, dayPos));

        return result;
    }

    /// <summary>
    /// Возвращает позиции столбцов с названиями групп
    /// </summary>
    private List<int> GetGroupNamePositions()
    {
        List<int> result = new();

        for (int i = _FirstVisibleCell.Col; i < _MaxDataCol; i++)
        {
            var cellValue = _Sheet.Cells[_GroupNameRow + 1, i].Value?.ToString()?.Trim();

            if (cellValue is not null && !_Sheet.Cells.Columns[i].IsHidden)       //Ячейка имеет значение, не является скрытой
                result.Add(i);
        }

        return result;
    }

    /// <summary>
    /// Возвращает информацию о не скрытых днях недели в расписании
    /// </summary>
    private List<DayData> GetDaysRowInformation()
    {
        List<DayData> dayPosition = new();

        for (int i = _FirstVisibleCell.Row; i < _MaxDataRow; i++)
        {
            var cellValue = _Sheet.Cells[i, 0].Value?.ToString()?.Trim();

            if (cellValue is not null &&        //Ячейка имеет значение, содержит день недели, не является скрытой
                IsContainDayOfWeek(cellValue) &&
                !_Sheet.Cells.Rows[i].IsHidden)
            {
                CultureInfo culture = new("ru-RU");
                string dateStr = _Sheet.Cells[i, 1].Value?.ToString()?.Trim();
                DateTime date = DateTime.Parse(dateStr, culture);
                dayPosition.Add(new DayData(pos: i, name: cellValue, date: date));
            }
        }

        return dayPosition;
    }

    /// <summary>
    /// Возвращает информацию о парах для определённой группы
    /// </summary>
    /// <param name="col">колонка с группой</param>
    /// <param name="dayPos">Позиции дней недели</param>
    private List<ClassInfo> GetGroupClasses(int col, List<DayData> dayPos)
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

                var time = _Sheet.Cells[day.Pos + i, 2].Value.ToString()?.Trim();
                var classItem = new ClassInfo(className, day.Date, day.Name, time, groupName, course);
                result.Add(classItem);
            }

        return result;
    }

    /// <summary>
    /// Разделяет название группы на Курс и Направление подготовки
    /// </summary>
    /// <param name="colWithGroup">индекс колонки с названием группы</param>
    /// <returns>(Курс, Направление подготовки)</returns>
    private (int, string) SplitGroupName(int colWithGroup)
    {
        string[] groupTitle = _Sheet.Cells[_GroupNameRow, colWithGroup].Value.ToString().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries); // Полное название группы
        string groupName = String.Join(' ', groupTitle[1..]).Trim(); // Только название группы
        int course = int.Parse(groupTitle[0]); // Только курс группы

        return (course, groupName);
    }

    /// <summary>
    /// Разделяет название группы на Курс и Направление подготовки.
    /// Добавляет уточнение для объеденённых групп в название
    /// </summary>
    /// <param name="colWithGroup">индекс колонки с названием группы</param>
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


    // Возвращает ячейку, с которой начинается само расписание (без шапки, скрытых строк/столбцов)
    private CellPosition GetFirstVisibleCell()
    {
        int colCount = 1000;

        for (int row = 7; row < 1000; row++)
        {
            if (_Sheet.Cells.Rows[row].IsHidden) continue;
            for (int col = 3; col < colCount; col++)
            {
                if (_Sheet.Cells.Columns[col].IsHidden) continue;
                Cell cell = _Sheet.Cells[row, col];

                int visibleRow = row;
                int visibleCol = col;

                return new CellPosition(visibleCol, visibleRow);
            }
        }

        throw new Exception("Bad Exel file");
    }


    // Ищем нужную страницу в файле расписания
    private Worksheet FindPageWithSchedule()
    {
        for (int i = 0; i < _Workbook.Worksheets.Count; i++)
        {
            var sheet = _Workbook.Worksheets[i];
            if (sheet.Cells.MaxDataColumn >= 10 && sheet.Cells.MaxDataRow >= 7) return sheet;
            sheet.Dispose();
        }
        throw new Exception($"Не найдено страницы с расписанием в файле {_Workbook.AbsolutePath}");
    }

    /// <summary>
    /// Строка является названием дня недели?
    /// </summary>
    public static bool IsContainDayOfWeek(string str)
    {
        var days = new string[] { "понедельник", "вторник", "среда", "четверг", "пятница", "суббота" };
        foreach (var day in days)
            if (str.Contains(day, StringComparison.InvariantCultureIgnoreCase)) return true;

        return false;
    }


    /// <summary>
    /// Положение и название дня недели
    /// </summary>
    private record DayData
    {
        /// <summary>
        /// Столбец группы
        /// </summary>
        public int Pos;
        public string Name;
        public DateTime Date;
        public DayData(int pos, string name, DateTime date) => (Pos, Name, Date) = (pos, name, date);

    }
}

