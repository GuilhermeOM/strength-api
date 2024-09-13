namespace Strength.Domain.Errors;

using Shared;

public static class UserErrors
{
    public static readonly CustomError InvalidEmail = new("User.InvalidEmail", "Invalid email");
    public static readonly CustomError InvalidPassword = new("User.InvalidPassword", "Password is incorrect");
    public static readonly CustomError InvalidConfirmPassword = new("User.InvalidConfirmPassword", "Confirm password is not equal to password");
    public static readonly CustomError InvalidVerificationToken = new("User.InvalidVerificationToken", "Invalid verification token");
    public static readonly CustomError NotFound = new("User.NotFound", "User not found");
    public static readonly CustomError AlreadyExists = new("User.AlreadyExists", "User already exists");
    public static readonly CustomError NotVerified = new("User.NotVerified", "User not verified");
    public static readonly CustomError NotCreated = new("User.NotCreated", "User not created");
}
