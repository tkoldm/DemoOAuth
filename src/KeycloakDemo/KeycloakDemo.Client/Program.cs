using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "KeycloackIdentity";
    })
    .AddCookie()
    .AddOpenIdConnect("KeycloackIdentity", options =>
    {
        options.RequireHttpsMetadata = false;
        options.ClientId = "Keycloack.Client";
        options.ClientSecret = "4rmIfl0CejC63ubVtl5QonqEUkrgooAi";
        options.Authority = "http://localhost:8080/realms/master";
        options.SaveTokens = true;
        options.ResponseType = "code";
        options.UsePkce = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

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