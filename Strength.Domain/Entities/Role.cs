namespace Strength.Domain.Entities;

using Base;
using Enums;

public class Role : BaseEntity
{
    public RoleName Name { get; init; }
}
