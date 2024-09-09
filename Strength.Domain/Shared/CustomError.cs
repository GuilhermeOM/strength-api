namespace Strength.Domain.Shared;

public sealed record CustomError(string Code, string? Description = null)
{
    public static readonly CustomError None = new(string.Empty);
}
