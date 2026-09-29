using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();


// ======================================================
// RSA Key
// ======================================================

var rsa = RSA.Create(2048);


var keyId = "key-001";

var signingKey = new RsaSecurityKey(rsa)
{
    KeyId = keyId
};

// ======================================================
// 2. Discovery Endpoint
// ======================================================

app.MapGet(
    "/.well-known/openid-configuration",
    () =>
    {
        return Results.Json(new
        {
            issuer = "http://localhost:5001",

            jwks_uri =
                "http://localhost:5001/.well-known/jwks.json",

            token_endpoint =
                "http://localhost:5001/connect/token"
        });
    });


// ======================================================
// 3. JWKS Endpoint
// ======================================================

app.MapGet(
    "/.well-known/jwks.json",
    () =>
    {
        var parameters = rsa.ExportParameters(false);

        var jwk = new
        {
            kty = "RSA",

            use = "sig",

            alg = "RS256",

            kid = keyId,

            n = Base64UrlEncoder.Encode(parameters.Modulus!),

            e = Base64UrlEncoder.Encode(parameters.Exponent!)
        };

        return Results.Json(new
        {
            keys = new[] { jwk }
        });
    });


// ======================================================
// 4. Token Endpoint
// ======================================================

app.MapPost(
    "/connect/token",
    () =>
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "123"),
            new Claim(JwtRegisteredClaimNames.Email,"ali@example.com"),

            new Claim("scope", "orders.read")
        };

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.RsaSha256
        );

        var token = new JwtSecurityToken(
            issuer: "http://localhost:5001",

            audience: "orders-api",

            claims: claims,

            expires: DateTime.UtcNow.AddMinutes(30),

            signingCredentials: credentials
        );

        var tokenString =
            new JwtSecurityTokenHandler()
                .WriteToken(token);



        return Results.Json(new
        {
            access_token = tokenString,

            token_type = "Bearer",

            expires_in = 1800
        });
    });


app.Run();