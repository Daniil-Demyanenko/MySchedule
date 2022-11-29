using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace job_checker
{
    public record JobInfo
    {
        /// <summary>
        /// Название предмета
        /// </summary>
        public string Title { get; init; }
        /// <summary>
        /// День недели
        /// </summary>
        public string Day { get; init; }
        /// <summary>
        /// Номер аудитории
        /// </summary>
        public string Cabinet { get; init; }
        /// <summary>
        /// Название группы
        /// </summary>
        public string Group { get; init; }
        /// <summary>
        /// Название института
        /// </summary>
        public string Institute { get; init; }
        /// <summary>
        /// Номер пары в расписании
        /// </summary>
        public int Number { get; init; }
        /// <summary>
        /// Номер курса
        /// </summary>
        public int Course { get; init; }



        public JobInfo(string title, string day, string cabinet, string group, string institute, int number, int course) =>
            (Title, Day, Cabinet, Group, Institute, Number, Course) = (title, day, cabinet, group, institute, number, course);
    }
}