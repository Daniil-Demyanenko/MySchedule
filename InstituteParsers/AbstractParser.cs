using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Cells;

namespace job_checker.InstituteParsers;
public abstract class AbstractParser: IDisposable
{
    protected CellPosition _FirstVisibleCell;
    protected Workbook _Workbook;
    protected Worksheet _Sheet;

    protected AbstractParser(string path)
    {
        _Workbook = new Workbook(path);//new Workbook(AppDomain.CurrentDomain.BaseDirectory + "/r.xls");
        _Sheet = _Workbook.Worksheets[0];
        _FirstVisibleCell = GetFirstVisibleCell();
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



    /// <summary>
    /// Положение и название дня недели
    /// </summary>
    protected struct DayData
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

