using System.Security.Claims;
using AspNet.Security.OAuth.Yandex;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddictDemo.Server;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true)
    .AddEnvironmentVariables()
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new DbContextSchema("open_id_dict"));
builder.Services.AddDbContext<OpenIddictDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("OpenIddictDemo.Server"));
    options.UseOpenIddict();
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
    });

builder.Services.AddAuthorization();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<OpenIddictDbContext>();
    })
    .AddServer(options =>
    {
        options.RegisterScopes(OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Roles);

        options.DisableAccessTokenEncryption();
        options.SetAuthorizationEndpointUris("/authorize")
            .SetTokenEndpointUris("/token");

        // Enable the authorization code flow.
        options.AllowAuthorizationCodeFlow()
            .RequireProofKeyForCodeExchange();

        // Register the signing and encryption credentials.
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .DisableTransportSecurityRequirement()
            .EnableAuthorizationEndpointPassthrough();
    })
    // Register the OpenIddict validation components.
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    if (await manager.FindByClientIdAsync("OpenIdDict.Client") is null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "OpenIdDict.Client",
            DisplayName = "My client application",
            RedirectUris =
            {
                new Uri("http://localhost:5103/"),
                new Uri("http://localhost:5103/signin-oidc"),
            },
            PostLogoutRedirectUris = { new Uri("http://localhost:5103/signout-callback-oidc") },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,

                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,

                OpenIddictConstants.Permissions.Scopes.Profile,
                
                OpenIddictConstants.Permissions.ResponseTypes.Code
            },
            Type = OpenIddictConstants.ClientTypes.Public
        });
    }
}

app.MapGet("/authorize", async (HttpContext context, [FromServices] DbContextSchema schema) =>
{
    var principal = (await context.AuthenticateAsync(YandexAuthenticationDefaults.AuthenticationScheme))?.Principal;
    if (principal is null)
    {
        return Results.Challenge(properties: null, new[] { YandexAuthenticationDefaults.AuthenticationScheme });
    }

    var identifier = principal.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);
    identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, identifier));
    identity.AddClaim(
        new Claim(OpenIddictConstants.Claims.Name, identifier).SetDestinations(OpenIddictConstants.Destinations
            .AccessToken));

    foreach (var claim in principal.Claims)
    {
        new Claim(claim.Type, claim.Value).SetDestinations(OpenIddictConstants.Destinations
            .AccessToken);
    }

    return Results.SignIn(new ClaimsPrincipal(identity), properties: null,
        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
});

app.Run();