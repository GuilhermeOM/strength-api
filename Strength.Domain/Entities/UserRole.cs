namespace Strength.Domain.Entities;

using Base;

public class UserRole : BaseEntity
{
    public required User User { get; init; }
    public required Role Role { get; init; }
}
