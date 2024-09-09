namespace Strength.Domain.Errors;

using Strength.Domain.Shared;

public static class UserRoleErrors
{
    public static readonly CustomError NotCreated = new("UserRoleErrors.NotCreated", "UserRole not created");
}
