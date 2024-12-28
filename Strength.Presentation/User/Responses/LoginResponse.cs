namespace Strength.Presentation.User.Responses;

internal sealed record LoginResponse(string TokenType, string Token, DateTime ExpiresAt);
