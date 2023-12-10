using Microsoft.EntityFrameworkCore;

namespace OpenIddictDemo.Server;

public record DbContextSchema(string Schema);

public class OpenIddictDbContext : DbContext
{
    private readonly string? _schema;

    public OpenIddictDbContext(DbContextOptions<OpenIddictDbContext> options, DbContextSchema schema)
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