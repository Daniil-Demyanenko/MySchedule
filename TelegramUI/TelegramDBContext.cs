using System;
using Microsoft.EntityFrameworkCore;

namespace MySchedule.TelegramUI;
public class TelegramDBContext : DbContext
{
    private static string _DBPath = AppDomain.CurrentDomain.BaseDirectory + "Users.db";

    public DbSet<TelegramUser> Users { get; set; }
    public DbSet<BugreportNote> Bugreports { get; set; }
    public TelegramDBContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_DBPath}");
    }
}
