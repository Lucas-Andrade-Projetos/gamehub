using gameHubBack.Entities;
using Microsoft.EntityFrameworkCore;

namespace gameHubBack.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
}
