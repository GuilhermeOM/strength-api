namespace Strength.Application.Users.Commands.LoginUser;

using Abstractions.Messaging;
using Domain.Shared;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<AuthToken>;
