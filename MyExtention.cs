using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace job_checker;
public static class MyExtentions
{
    private static bool ContainsDay(this string str)
    {
        var days = new string[] { "понедельник", "вторник", "среда", "четверг", "пятница", "суббота" };
        foreach (var day in days)
            if (str.Contains(day, StringComparison.InvariantCultureIgnoreCase)) return true;

        return false;
    }
}