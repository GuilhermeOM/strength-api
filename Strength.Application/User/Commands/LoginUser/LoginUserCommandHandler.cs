namespace Strength.Application.User.Commands.LoginUser;

using System.Net;
using System.Security.Cryptography;
using System.Text;
using Abstractions.Messaging;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services.Token;
using Domain.Shared;

internal sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService) : ICommandHandler<LoginUserCommand, AuthToken>
{
    public async Task<Result<AuthToken>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var userWithRoles = await userRepository.GetWithRolesByEmailAsync(request.Email, cancellationToken);

        if (userWithRoles is null)
        {
            return ResponseResult.WithErrors<AuthToken>(HttpStatusCode.NotFound, [UserErrors.NotFound]);
        }

        if (!VerifyPasswordHash(request.Password, userWithRoles.PasswordHash, userWithRoles.PasswordSalt))
        {
            return ResponseResult.WithErrors<AuthToken>(HttpStatusCode.Unauthorized, [UserErrors.InvalidPassword]);
        }

        return userWithRoles.VerifiedAt is null
            ? ResponseResult.WithErrors<AuthToken>(HttpStatusCode.Unauthorized, [UserErrors.NotVerified])
            : Result.Success(tokenService.Create(userWithRoles));
    }

    private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return computedHash.SequenceEqual(passwordHash);
    }
}
