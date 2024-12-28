namespace Strength.Application.User.Commands.SendUserVerificationEmail;

using Abstractions.Messaging;

public sealed record SendUserVerificationEmailCommand(string Email) : ICommand<string>;
