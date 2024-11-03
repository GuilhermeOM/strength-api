namespace Strength.Domain.Services.Token;

public class AuthToken
{
    public required string TokenType { get; init; }
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
