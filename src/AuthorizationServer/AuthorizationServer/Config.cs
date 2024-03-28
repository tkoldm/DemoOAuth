﻿using IdentityServer4.Models;
using System.Collections.Generic;

namespace AuthorizationServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new("client_credentials"),
            new("owner_password"),
            new("implicit"),
            new("authorization_code"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new()
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                AllowedScopes = { "client_credentials" }
            },
            new()
            {
                ClientId = "console.client",
                ClientName = "Resource Owner Password Client",

                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                AllowedScopes = { "owner_password" }
            },
            new()
            {
                ClientId = "api.client",
                ClientName = "Implicit Client",

                AllowedGrantTypes = GrantTypes.Implicit,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                RedirectUris = { "https://localhost:5001/signin-oidc", "https://localhost:5001/callback", },

                AllowedScopes = { "openid", "implicit" },

                AllowAccessTokensViaBrowser = true
            },
            new()
            {
                ClientId = "interactive.client",
                ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:44300/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "authorization_code" }
            },
        };
}