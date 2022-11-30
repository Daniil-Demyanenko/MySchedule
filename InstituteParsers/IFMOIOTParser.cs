using System;
using Aspose.Cells;
using System.Collections.Generic;
using static System.Console;

namespace job_checker.InstituteParsers;

/// <summary>
/// Парсер для расписаний института ИФМОИОТ
/// </summary>
public class IFMOIOTParser : IParser
{
    private List<string> _ParsedDay = new();

    public List<JobInfo> Parse(string path)
    {
        List<JobInfo> result = new();
        Workbook wb = new Workbook(path);
        var sheet = wb.Worksheets[0];

        GetDayRowPositions(sheet);

        wb.Dispose();
        sheet.Dispose();

        return new List<JobInfo>();
    }

    private List<int> GetDayRowPositions(Worksheet sheet)
    {
        List<int> dayPosition = new();

        for (int i = 0; i < sheet.Cells.MaxDataRow; i++)
        {
            var cellValue = sheet.Cells[i, 0].Value?.ToString();
            WriteLine($"{i}) {cellValue}");
            if (cellValue is not null &&        //Ячейка имеет значение, содержит день недели, который встречается только в первый раз
                cellValue.ContainsDay() && 
                !_ParsedDay.Contains(cellValue))
            {
                dayPosition.Add(i);
                _ParsedDay.Add(cellValue);
            }
        }

        WriteLine();
        foreach (var i in dayPosition)
            Write($"{i}\t ");
        return dayPosition;
    }
}
