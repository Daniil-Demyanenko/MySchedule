using System.Collections.Generic;
using MySchedule.InstituteParsers;
using Xunit;

namespace MySchedule.UnitTests;

public class ParserMultipageUnitTests
{
    [Fact]
    public void ParsingTestZFOMAG_Multipage()
    {
        Assert.True(GetStudyGroups(Paths.ZFOMAG).Count > 0);
    }
    
    private static List<StudyGroup> GetStudyGroups(string path)
    {
        using var parser = new TemplateScheduleParser(path);
        return parser.StudyGroups;
    }
}