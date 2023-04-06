using System;
using System.IO;

namespace job_checker
{
    public static class ScheduleDownloader
    {
        private static string dir = AppDomain.CurrentDomain.BaseDirectory + "/Cache"; //Путь к папке Cache в директории программы
        public static void CheckUpdate()
        {
            if (!CacheIsRelevant()) Download();
        }

        private static bool CacheIsRelevant()
        {

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                return false;
            }

            if (!File.Exists(dir + "/ras.xls"))
            {
                return false;
            }

            var fileWriteDate = File.GetLastWriteTime(dir + "/ras.xls");
            if ((DateTime.Now - fileWriteDate).TotalHours > 6) return false;

            return true;

        }

        private static void Download()
        {
            try
            {
                //скачиваем
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"###### Ошибка при скачивании расписания ({ex.Message})");
            }
            finally
            {
                //удалить IDisposable объекты
                
            }
        }
    }
}