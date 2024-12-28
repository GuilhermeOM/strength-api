namespace Strength.Application.User.Commands.LoginUser;

using Abstractions.Messaging;
using Domain.Services.Token;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<AuthToken>;
