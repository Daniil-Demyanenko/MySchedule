using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace job_checker
{
    public class LogSingleton
    {

        /// <summary>
        /// Уровень логирования
        /// </summary>
        public LogLevels LogLevel = LogLevels.Debug;
        private static LogSingleton _log = null;
        private static object sync = new object();


        /// <summary>
        /// Ссылка на объект
        /// </summary>
        /// <value></value>
        public static LogSingleton Instance
        {
            get
            {
                if (_log == null)
                    lock (sync)
                    {
                        if (_log == null)
                            _log = new LogSingleton();
                    }
                return _log;
            }
        }

        protected LogSingleton() { }

        /// <summary>
        /// Вывести лог
        /// </summary>
        /// <param name="level">Уровень логирования</param>
        public void LogLine(LogLevels level, string str, params object[] args)
        {
            var a = args.Length > 1 ? args[1..] : null;
            if (level <= LogLevel)
                Console.WriteLine(String.Format(args[0].ToString(), args.Length > 1 ? args[1..] : new object[0]));
        }
        public void Log(LogLevels level, params object[] args)
        {
            if (level <= LogLevel)
                Console.Write(String.Format(args[0].ToString(), args.Length > 1 ? args[1..] : new object[0
                ]));
        }

    }

    public enum LogLevels
    {
        Error = 0,
        Warning,
        Info,
        Debug
    }
}