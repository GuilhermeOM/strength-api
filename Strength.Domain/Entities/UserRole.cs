namespace Strength.Domain.Entities;

using Base;

public class UserRole : BaseEntity
{
    public User User { get; init; } = new();
    public Role Role { get; init; } = new();
}
