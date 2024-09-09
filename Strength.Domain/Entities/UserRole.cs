namespace Strength.Domain.Entities;

public class UserRole
{
    public Guid Id { get; init; }

    public User User { get; init; } = new();
    public Role Role { get; init; } = new();
}
