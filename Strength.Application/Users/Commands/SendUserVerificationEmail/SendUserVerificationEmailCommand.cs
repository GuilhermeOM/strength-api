namespace Strength.Application.Users.Commands.SendUserVerificationEmail;

using Abstractions.Messaging;

public sealed record SendUserVerificationEmailCommand(string Email) : ICommand;
