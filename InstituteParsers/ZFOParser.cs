using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#nullable enable

namespace job_checker.InstituteParsers;
public class ZFOParser : TemplateScheduleParser, IDisposable
{
    public ZFOParser(string path) : base(path) { }
    ~ZFOParser() { base.Dispose(); }

    /// <summary>
    /// Возвращает позиции столбцов с названиями групп
    /// </summary>
    protected override List<int> GetGroupNamePositions()
    {
        List<int> result = new();

        for (int i = _FirstVisibleCell.Col; i < _MaxDataCol; i++)
        {
            var cellValue = _Sheet.Cells[_GroupNameRow + 1, i].Value?.ToString()?.Trim();

            if (cellValue is not null && !_Sheet.Cells.Columns[i].IsHidden)       //Ячейка имеет значение, не является скрытой
            {
                if(_Sheet.Cells[_GroupNameRow, i].Value?.ToString().Trim() == "группа") break;
                result.Add(i);
            }
        }

        return result;
    }

    /// <summary>
    /// Возвращает информацию о парах для определённой группы
    /// </summary>
    /// <param name="col">колонка с группой</param>
    /// <param name="dayPos">Позиции дней недели</param>
    protected override List<ClassInfo> GetGroupClasses(int col, List<DayData> dayPos)
    {
        var result = new List<ClassInfo>();
        int course;
        string groupName;

        if (_Sheet.Cells[_GroupNameRow, col].IsMerged)
            (course, groupName) = SplitGroupNameForMerged(colWithGroup: col);
        else (course, groupName) = SplitGroupName(colWithGroup: col);

        foreach (var day in dayPos)
            for (int i = 0; i < 3; i++)
            {
                string? className = _Sheet.Cells[day.Pos + i, col].Value?.ToString() ?? null;
                if (className is null) continue;

                var time = _Sheet.Cells[day.Pos + i, 2].Value.ToString()?.Trim();
                var classItem = new ClassInfo(className, day.Date, day.Name, time, groupName, course);
                result.Add(classItem);
            }

        return result;
    }
}
