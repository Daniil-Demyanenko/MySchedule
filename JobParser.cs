
using System.Collections.Generic;
using job_checker.InstituteParsers;

namespace job_checker
{
    public static class JobParser
    {
        private static List<JobInfo> _Jobs;
        /// <summary>
        /// Список пар со всех расписаний
        /// </summary>
        /// <value></value>
        public static IEnumerable<JobInfo> Jobs => _Jobs;



        public static void Parse()
        {
            var IFMOIOT = new IFMOIOTParser();
            _Jobs = IFMOIOT.Parse("ras.xls");
        }

    }
}