using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Cells;

namespace job_checker.InstituteParsers;
public abstract class AbstractParser : IDisposable
{
    protected CellPosition _FirstVisibleCell;
    protected Workbook _Workbook;
    protected Worksheet _Sheet;
    protected int _MaxDataCol;
    protected int _MaxDataRow;

    protected AbstractParser(string path) // TODO: Добавить поиск листа с данными, например у OFO_MAG_IFMOIOT только на третем листе есть расписание
    {
        _Workbook = new Workbook(path);//new Workbook(AppDomain.CurrentDomain.BaseDirectory + "/r.xls");
        _Sheet = FindPageWithSchedule();
        _FirstVisibleCell = GetFirstVisibleCell();
        _MaxDataCol = _Sheet.Cells.MaxDataColumn;
        _MaxDataRow = _Sheet.Cells.MaxDataRow;
    }
    public void Dispose()
    {
        _Sheet.Dispose();
        _Workbook.Dispose();
    }
    
    public abstract List<ClassInfo> Parse();

    protected CellPosition GetFirstVisibleCell()
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
    public static bool IsContainDay(string str)
    {
        var days = new string[] { "понедельник", "вторник", "среда", "четверг", "пятница", "суббота" };
        foreach (var day in days)
            if (str.Contains(day, StringComparison.InvariantCultureIgnoreCase)) return true;

        return false;
    }


    /// <summary>
    /// Положение и название дня недели
    /// </summary>
    protected record DayData
    {
        /// <summary>
        /// Столбец группы
        /// </summary>
        public int Pos;
        public string Name;
        public string Date;
        public DayData(int pos, string name, string date) => (Pos, Name, Date) = (pos, name, date);

    }
}

