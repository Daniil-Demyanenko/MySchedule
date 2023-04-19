using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace job_checker
{
    /// <summary>
    /// Класс, отвечающий за взаимодействие с пользователем
    /// </summary>
    public static class TelegramBot
    {
        private static string _token;
        
        public static void Start(string token)
        {
            _token = token;
        }

        public static void WaitMessages()
        {

        }
    }
}