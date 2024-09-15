namespace Strength.Infrastructure.Security;

using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Domain.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

internal sealed class TokenProvider(IConfiguration configuration) : ITokenProvider
{
    public AuthToken Create(User userWithRoles)
    {
        if (userWithRoles.UserRoles.Count == 0)
        {
            throw new InvalidOperationException("User must contain at least one role in order to generate a token");
        }

        var secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwtExpirationInMinutesIsConfigured = int.TryParse(configuration["Jwt:ExpirationInMinutes"],
            out var expirationInMinutes);
        expirationInMinutes = jwtExpirationInMinutesIsConfigured ? expirationInMinutes : 60; // default 1h

        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, userWithRoles.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, userWithRoles.Email),
            new("verified", userWithRoles.VerifiedAt.ToString() ?? string.Empty),
        };

        foreach (var userRole in userWithRoles.UserRoles)
        {
            claims.Add(new(ClaimTypes.Role, Enum.GetName(userRole.Role.Name)!));
        }

        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationInMinutes),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            TokenType = "Bearer"
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(tokenDescription);

        return new AuthToken
        {
            TokenType = tokenDescription.TokenType,
            Token = token,
            ExpiresAt = tokenDescription.Expires.Value,
        };
    }
}
