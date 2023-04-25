using System;
using Microsoft.EntityFrameworkCore;

namespace job_checker.TelegramUI;
public class UserDBContext : DbContext
{
    private static string DBPath = AppDomain.CurrentDomain.BaseDirectory + "Users.db";

    public DbSet<TelegramUser> Users => Set<TelegramUser>();
    public UserDBContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DBPath}");
    }
}
