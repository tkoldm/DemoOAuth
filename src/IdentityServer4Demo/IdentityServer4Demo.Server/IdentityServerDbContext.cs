using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer4Demo.Server;

public record DbContextSchema(string Schema);

public class IdentityServerDbContext : DbContext, IConfigurationDbContext, IPersistedGrantDbContext
{
    private readonly string? _schema;

    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }
    public DbSet<IdentityResource> IdentityResources { get; set; }
    public DbSet<ApiResource> ApiResources { get; set; }
    public DbSet<ApiScope> ApiScopes { get; set; }
    public DbSet<PersistedGrant> PersistedGrants { get; set; }
    public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }

    public IdentityServerDbContext(DbContextOptions<IdentityServerDbContext> options, DbContextSchema schema)
        : base(options)
    {
        _schema = schema.Schema ?? "public";
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceFlowCodes>()
            .HasNoKey();
        base.OnModelCreating(modelBuilder);
    }

    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();
}