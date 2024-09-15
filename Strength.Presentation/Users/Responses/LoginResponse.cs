namespace Strength.Presentation.Users.Responses;

internal sealed record LoginResponse(string TokenType, string Token, DateTime ExpiresAt);
