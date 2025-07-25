using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

public class BackendContext : DbContext
{
    public BackendContext(DbContextOptions<BackendContext> options)
        : base(options)
    {
    }

    public DbSet<BackendItem> BackendItem { get; set; } = null!;
}