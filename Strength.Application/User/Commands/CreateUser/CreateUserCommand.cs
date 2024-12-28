namespace Strength.Application.User.Commands.CreateUser;

using Abstractions.Messaging;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string ConfirmPassword) : ICommand;
