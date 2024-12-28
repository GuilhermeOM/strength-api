namespace Strength.Infrastructure.Services;

using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Domain.Services.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

internal sealed class TokenService(IConfiguration configuration) : ITokenService
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

        var jwtExpirationInMinutesIsConfigured = int.TryParse(configuration["Jwt:ExpirationInMinutes"], out var expirationInMinutes);
        expirationInMinutes = jwtExpirationInMinutesIsConfigured ? expirationInMinutes : 60; // default 1h

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userWithRoles.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, userWithRoles.Email),
            new("verified", userWithRoles.VerifiedAt.ToString() ?? string.Empty)
        };

        claims.AddRange(
            userWithRoles.UserRoles.Select(userRole => new Claim(ClaimTypes.Role, Enum.GetName(userRole.Role.Name)!)));

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
            TokenType = tokenDescription.TokenType, Token = token, ExpiresAt = tokenDescription.Expires.Value
        };
    }
}
