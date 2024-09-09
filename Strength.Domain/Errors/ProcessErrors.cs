namespace Strength.Domain.Errors;

using Shared;

public static class ProcessErrors
{
    public static readonly CustomError InternalError = new("ProcessErrors.InternalError", "Some operation failed");
}
