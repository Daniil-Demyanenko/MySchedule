using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace job_checker
{
    /// <summary>
    /// Класс, отвечающий за взаимодействие с пользователем
    /// </summary>
    public class TelegramBot
    {
        private string _token;
        
        public TelegramBot(string token)
        {
            _token = token;
        }

        public void WaitMessages()
        {

        }
    }
}