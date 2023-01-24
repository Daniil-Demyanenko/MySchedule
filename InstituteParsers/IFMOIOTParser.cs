using System;
using Aspose.Cells;
using System.Collections.Generic;
using static System.Console;
#nullable enable

namespace job_checker.InstituteParsers;

/// <summary>
/// Парсер для расписаний института ИФМОИОТ
/// </summary>
public class IFMOIOTParser : Parser
{
    private List<string> _ParsedDay = new();
    private int _VisibleLinesStartIndex = 38 - 1; // Видимая строка, с которой начинается расписание пар
    private int _GroupNameRow = 5;

    private int[] _GroupPosition = { }; // Позиции групп в расписании не по порядку, скрыты удалённые специализации и т.д. 


    public IFMOIOTParser(string path) : base(path) { }
    ~IFMOIOTParser() { _Workbook.Dispose(); }


    public override List<JobInfo> Parse()
    {
        List<JobInfo> result = new();

        var dayPos = GetDayRowPositions(_Sheet);

        result.AddRange(GetGroupClasses(4, dayPos, _Sheet));

        WriteLine();
        foreach (var i in result)
        {
            WriteLine("{0} {1} {2} {3} {4}", i.Course, i.Group, i.Day, i.Title, i.Number);
        }

        return result;
    }

    private List<DayData> GetDayRowPositions(Worksheet sheet) //BUG: Переписать, определяя, не скрыта ли ячейка
    {
        List<DayData> dayPosition = new();

        for (int i = _VisibleLinesStartIndex; i < sheet.Cells.MaxDataRow; i++)
        {
            var cellValue = sheet.Cells[i, 0].Value?.ToString()?.Trim();

            if (cellValue is not null &&        //Ячейка имеет значение, содержит день недели, который встречается только в первый раз
                cellValue.ContainsDay() &&
                !_ParsedDay.Contains(cellValue))
            {
                dayPosition.Add(new DayData(i, cellValue));
                _ParsedDay.Add(cellValue);
            }
        }


        return dayPosition;
    }

    private List<JobInfo> GetGroupClasses(int col, List<DayData> dayPos, Worksheet sheet) //BUG: Переписать, определяя, не скрыта ли ячейка
    {
        var result = new List<JobInfo>();

        string[] groupTitle = sheet.Cells[_GroupNameRow, col].Value.ToString().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string groupName = String.Join(' ', groupTitle[1..]);
        int course = int.Parse(groupTitle[0]);

        foreach (var day in dayPos)
            for (int i = 0; i < 4; i++)
            {
                string? className = sheet.Cells[day.Pos + i, col].Value?.ToString() ?? null;
                if (className is null) continue;

                var classItem = new JobInfo(className, day.Name, "", groupName, "ИФМОИОТ", i + 1, course);
                result.Add(classItem);
            }

        return result;
    }


    private struct DayData
    {
        public int Pos;
        public string Name;

        public DayData(int pos, string name) => (Pos, Name) = (pos, name);
    }
}
