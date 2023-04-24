using System;
using System.Collections.Generic;
using System.Linq;
using job_checker.InstituteParsers;
using Xunit;

namespace job_checker.UnitTests;

public class ParserCouplesUnitTests
{
    [Fact]
    public void ParsingTestOFOBAK1_GroupCount()
    {
        var couples = GetCouples(Paths.OFOBAK1);
        var groups = couples.Select(x => x.Course + " " + x.Group).Distinct();

        Assert.Equal(40, groups.Count());
    }

    [Fact]
    public void ParsingTestOFOBAK1_CoupleCount()
    {
        var couples = GetCouples(Paths.OFOBAK1);
        var POMII1 = couples.Where(x => x.Course == 1 && x.Group == "ПО (МиИ)");

        Assert.Equal(18, POMII1.Count());
    }

    [Fact]
    public void ParsingTestOFOBAK2_GroupCount()
    {
        var couples = GetCouples(Paths.OFOBAK2);
        var groups = couples.Select(x => x.Course + " " + x.Group).Distinct();

        Assert.Equal(43, groups.Count());
    }

    [Fact]
    public void ParsingTestOFOBAK2_CoupleCount()
    {
        var couples = GetCouples(Paths.OFOBAK2);
        var POMII1 = couples.Where(x => x.Course == 1 && x.Group == "ПО (МиИ)");

        Assert.Equal(19, POMII1.Count());
    }

    [Fact]
    public void ParsingTestOFOBAK3_GroupCount()
    {
        var couples = GetCouples(Paths.OFOBAK3);
        var groups = couples.Select(x => x.Course + " " + x.Group).Distinct();

        Assert.Equal(43, groups.Count());
    }

    [Fact]
    public void ParsingTestOFOBAK3_CoupleCount()
    {
        var couples = GetCouples(Paths.OFOBAK3);
        var POMII1 = couples.Where(x => x.Course == 1 && x.Group == "ПО (МиИ)");

        Assert.Equal(21, POMII1.Count());
    }

    [Fact]
    public void ParsingTestOFOMAG_GroupCount()
    {
        var couples = GetCouples(Paths.OFOMAG);
        var groups = couples.Select(x => x.Course + " " + x.Group).Distinct();

        int count = groups.Count();
        Assert.Equal(17, count);
    }

    [Fact]
    public void ParsingTestOFOMAG_CoupleCount()
    {
        var couples = GetCouples(Paths.OFOMAG);
        var POMII1 = couples.Where(x => x.Course == 1 && x.Group == "маг ПО (ТО) []");

        int count = POMII1.Count();
        Assert.Equal(18, count);
    }

    [Fact]
    public void ParsingTestZFOMAG_GroupCount()
    {
        var couples = GetCouples(Paths.ZFOMAG);
        var groups = couples.Select(x => x.Course + " " + x.Group).Distinct();

        int count = groups.Count();
        Assert.Equal(4, count);
    }

    [Fact]
    public void ParsingTestZFOMAG_CoupleCount()
    {
        var couples = GetCouples(Paths.ZFOMAG);
        var POPT1 = couples.Where(x => x.Course == 1 && x.Group == "маг ПО (ПТ)");

        Assert.Equal(18, POPT1.Count());
    }

    // [Fact]
    // public void ParsingTestZFOBAK_GroupCount()
    // {
    //     var couples = GetZFOCouples(_ZFOBAK);
    //     var groups = couples.Select(x => x.Course + " " + x.Group).Distinct();

    //     Assert.Equal(11, groups.Count());
    // }

    // [Fact]
    // public void ParsingTestZFOBAK_CoupleCount()
    // {
    //     var couples = GetZFOCouples("UnitTests/TESTSDATA/ZFOBAK.xls");
    //     var POPT1 = couples.Where(x => x.Course == 3 && x.Group == "ПО (ТО)");

    //     Assert.Equal(25, POPT1.Count());
    // }

    public static List<ClassInfo> GetCouples(string path)
    {
        using var IFMOIOT = new TemplateScheduleParser(path);
        return IFMOIOT.Parse();
    }
}

