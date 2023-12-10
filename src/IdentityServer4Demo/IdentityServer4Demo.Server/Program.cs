using System.Text;
using AspNet.Security.OAuth.Yandex;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4Demo.Server;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true)
    .AddEnvironmentVariables()
    .Build();

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine(configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddSingleton(new DbContextSchema("identity_server"));

builder.Services.AddIdentityServer()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b => b.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
            sql => sql.MigrationsAssembly("IdentityServer4Demo.Server"));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b => b.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
            sql => sql.MigrationsAssembly("IdentityServer4Demo.Server"));
    });

builder.Services.AddAuthentication(YandexAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddYandex(options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.ClientId = "feed1db9ccd948b19df6123bd3b9026b";
        options.ClientSecret = "980b03ff50d445a8addace1b6acfa96b";
        options.Scope.Add("login:email");
        options.Scope.Add("login:info");
        options.SaveTokens = true;
        options.ReturnUrlParameter = "http://localhost:5071/signin-yandex";
    });;
builder.Services.AddAuthorization();

var app = builder.Build();

await InitializeDatabase(app);

app.UseAuthentication();
app.UseAuthorization();
app.UseIdentityServer();

app.MapGet("/", () => "Hello World!");
app.MapGet("/requiredAuth", (HttpContext context) =>
    {
        var response = new StringBuilder();
        foreach (var claim in context.User.Claims)
        {
            response.AppendLine($"{claim.Type} = {claim.Value}");
        }

        return response.ToString();
    })
    .RequireAuthorization();

app.Run();
return;

async Task InitializeDatabase(IApplicationBuilder appBuilder)
{
    using var serviceScope = appBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
    serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

    var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    await context.Database.MigrateAsync();
    if (!await context.Clients.AnyAsync())
    {
        foreach (var client in Config.Clients)
        {
            await context.Clients.AddAsync(client.ToEntity());
        }
        await context.SaveChangesAsync();
    }

    if (!await context.IdentityResources.AnyAsync())
    {
        foreach (var resource in Config.IdentityResources)
        {
            await context.IdentityResources.AddAsync(resource.ToEntity());
        }
        await context.SaveChangesAsync();
    }
}