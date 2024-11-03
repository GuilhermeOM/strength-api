namespace Strength.Domain.Entities.Base;

public class BaseEntity
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
