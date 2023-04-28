using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MySchedule.TelegramUI;
public class BugreportNote
{
    [Key]
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Message { get; set; }

    public TelegramUser User { get; set; }

    public BugreportNote(string firstName, string lastName, string userName, string message, TelegramUser user) =>
    (FirstName, LastName, UserName, Message, User) = (firstName, lastName, userName, message, user);

    public BugreportNote() { }
}
