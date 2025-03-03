using Microsoft.EntityFrameworkCore;
public class AppDbContext : DbContext
{
    public DbSet<Output> Outputs { get; set; }
    public AppDbContext()
    {

    }
    public AppDbContext(DbContextOptions options) : base(options)
    {

    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Here you can define the provider (e.g., SQL Server) and connection string
            optionsBuilder.UseSqlServer("Server=localhost;Database=Snapshot;Trusted_Connection=True;");
        }
    }
}