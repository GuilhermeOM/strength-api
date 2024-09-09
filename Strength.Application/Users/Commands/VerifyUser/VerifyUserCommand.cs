namespace Strength.Application.Users.Commands.VerifyUser;

using Abstractions.Messaging;

public sealed record VerifyUserCommand(string VerificationToken) : ICommand;
