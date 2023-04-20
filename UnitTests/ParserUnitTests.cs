using System;
using System.Collections.Generic;
using System.Linq;
using job_checker.InstituteParsers;
using Xunit;

namespace job_checker.UnitTests
{
    public class ParserUnitTests
    {
        static string _OFOBAK1 = "../../../UnitTests/TESTSDATA/OFOBAK1.xlsx";
        static string _OFOBAK2 = "../../../UnitTests/TESTSDATA/OFOBAK2.xlsx";
        static string _OFOBAK3 = "../../../UnitTests/TESTSDATA/OFOBAK3.xlsx";
        static string _OFOMAG = "../../../UnitTests/TESTSDATA/OFOMAG.xlsx";
        static string _ZFOBAK = "../../../UnitTests/TESTSDATA/ZFOBAK.xls";
        static string _ZFOMAG = "../../../UnitTests/TESTSDATA/ZFOMAG.xls";

        [Fact]
        public void ParsingTestOFOBAK1_GroupCount()
        {
            var couples = GetCouples(_OFOBAK1);
            var groups = couples.Select(x => x.Course + " " + x.Group).Distinct();

            Assert.Equal(40, groups.Count());
        }

        [Fact]
        public void ParsingTestOFOBAK1_CoupleCount()
        {
            var couples = GetCouples(_OFOBAK1);
            var POMII1 = couples.Where(x => x.Course == 1 && x.Group == "ПО (МиИ)");

            Assert.Equal(18, POMII1.Count());
        }

        [Fact]
        public void ParsingTestOFOBAK2_GroupCount()
        {
            var couples = GetCouples(_OFOBAK2);
            var groups = couples.Select(x => x.Course + " " + x.Group).Distinct();

            Assert.Equal(43, groups.Count());
        }

        [Fact]
        public void ParsingTestOFOBAK2_CoupleCount()
        {
            var couples = GetCouples(_OFOBAK2);
            var POMII1 = couples.Where(x => x.Course == 1 && x.Group == "ПО (МиИ)");

            Assert.Equal(19, POMII1.Count());
        }

        [Fact]
        public void ParsingTestOFOBAK3_GroupCount()
        {
            var couples = GetCouples(_OFOBAK3);
            var groups = couples.Select(x => x.Course + " " + x.Group).Distinct();

            Assert.Equal(43, groups.Count());
        }

        [Fact]
        public void ParsingTestOFOBAK3_CoupleCount()
        {
            var couples = GetCouples(_OFOBAK3);
            var POMII1 = couples.Where(x => x.Course == 1 && x.Group == "ПО (МиИ)");

            Assert.Equal(21, POMII1.Count());
        }

        [Fact]
        public void ParsingTestOFOMAG_GroupCount()
        {
            var couples = GetCouples(_OFOMAG);
            var groups = couples.Select(x => x.Course + " " + x.Group).Distinct();

            Assert.Equal(17, groups.Count());
        }

        [Fact]
        public void ParsingTestOFOMAG_CoupleCount()
        {
            var couples = GetCouples(_OFOMAG);
            var POMII1 = couples.Where(x => x.Course == 1 && x.Group == "маг ПО (ТО) []");
            var a = couples.Select(x => x.Group).Distinct();

            foreach (var i in a) Console.WriteLine(i);

            Assert.Equal(18, POMII1.Count());
        }

        public static List<ClassInfo> GetCouples(string path)
        {
            using var IFMOIOT = new TemplateScheduleParser(path);
            return IFMOIOT.Parse();
        }
    }
}
