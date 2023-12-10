using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "OpenIddictIdentity";
    })
    .AddCookie()
    .AddOpenIdConnect("OpenIddictIdentity", options =>
    {
        options.RequireHttpsMetadata = false;
        options.ClientId = "OpenIdDict.Client";
        options.Authority = "http://localhost:5102/";
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