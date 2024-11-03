namespace Strength.Domain.Errors;

using Shared;

public static class RoleErrors
{
    public static readonly CustomError NotFound = new("RoleErrors.NotFound", "Role not found");
}
