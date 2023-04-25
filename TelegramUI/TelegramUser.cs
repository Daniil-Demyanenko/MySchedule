using System.ComponentModel.DataAnnotations;

namespace job_checker.TelegramUI;
public class TelegramUser
{
    [Key]
    public long ChatID { get; set; }
    public int Course { get; set; }
    public string GroupName { get; set; }
    public int RegistrationStatus { get; set; }

    public TelegramUser(long chatID, int course, string group, int regStatus)
    => (ChatID, Course, GroupName, RegistrationStatus) = (chatID, course, group, regStatus);
    public TelegramUser() { }
}