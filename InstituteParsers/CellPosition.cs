using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MySchedule.InstituteParsers
{
    public record CellPosition(int Col, int Row)
    {
        public int Col = Col;
        public int Row = Row;
    }
}