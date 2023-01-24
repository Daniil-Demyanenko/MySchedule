using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Cells;

namespace job_checker.InstituteParsers;
public abstract class Parser
{
    protected CellPosition _FirstVisibleCell;
    protected Workbook _Workbook ;
    protected Worksheet _Sheet;

    protected Parser(string path)
    {
        _Workbook = new Workbook(path);//new Workbook(AppDomain.CurrentDomain.BaseDirectory + "/r.xls");
        _Sheet = _Workbook.Worksheets[0];
        _FirstVisibleCell = GetFirstVisibleCell();
    }


    public abstract List<JobInfo> Parse();

    protected CellPosition GetFirstVisibleCell()
    {
        int colCount = 1000;

        for (int row = 7; row < 200; row++)
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
}

