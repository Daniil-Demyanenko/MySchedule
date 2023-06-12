using System.ComponentModel.DataAnnotations;

namespace MySchedule.TelegramUI;
public class TelegramUser
{
        // public long Id { get; set; }
        [Key]
        public long ChatID { get; set; }
        public int? Course { get; set; }
        public string GroupName { get; set; }
        //public ICollection<BugreportNote> BugreportNotes { get; set; }

        public TelegramUser(long chatID, int course, string group)
        => (ChatID, Course, GroupName) = (chatID, course, group);
        public TelegramUser(long chatID) => ChatID = chatID;
}