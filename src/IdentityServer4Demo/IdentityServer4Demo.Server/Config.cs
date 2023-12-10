using IdentityServer4.Models;

namespace IdentityServer4Demo.Server;

public class Config
{
    public static IEnumerable<Client> Clients = new List<Client>
    {
        new()
        {
            ClientId = "IdentityServer.Client",
            ClientSecrets = new List<Secret> { new("4rmIfl0CejC63ubVtl5QonqEUkrgooAi") },
            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = false,
            RequirePkce = true,
            RequireConsent = false,
            RedirectUris =
            {
                "http://localhost:5000/",
                "http://localhost:5000/signin-oidc",
            },
            PostLogoutRedirectUris = { "http://localhost:5000/index.html" },
            AllowedScopes = { "openid", "profile" },
            AllowedCorsOrigins = { "http://localhost:5000" }
        },
    };

    public static IEnumerable<IdentityResource> IdentityResources = new List<IdentityResource>
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email(),
    };
}