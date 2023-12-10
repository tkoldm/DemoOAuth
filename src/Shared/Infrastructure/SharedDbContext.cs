using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public record DbContextSchema(string Schema);

public class SharedDbContext : DbContext
{
    private readonly string? _schema;

    public SharedDbContext(DbContextOptions<SharedDbContext> options, DbContextSchema schema)
        : base(options)
    {
        _schema = schema.Schema ?? "public";
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_schema);
        base.OnModelCreating(modelBuilder);
    }
}