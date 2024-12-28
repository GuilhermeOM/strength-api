namespace Strength.Application.User.Commands.VerifyUser;

using Abstractions.Messaging;

public sealed record VerifyUserCommand(string VerificationToken) : ICommand;
