namespace Strength.Application.Users.Commands.LoginUser;

using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Messaging;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;

internal sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, AuthToken>
{
    public async Task<Result<AuthToken>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var userWithRoles = await userRepository.GetUserWithRolesByEmailAsync(request.Email, cancellationToken);

        if (userWithRoles is null)
        {
            return ResponseResult.WithErrors<AuthToken>(HttpStatusCode.NotFound, [UserErrors.NotFound]);
        }

        if (userWithRoles.VerifiedAt is null)
        {
            return ResponseResult.WithErrors<AuthToken>(HttpStatusCode.BadRequest, [UserErrors.NotVerified]);
        }

        return !VerifyPasswordHash(request.Password, userWithRoles.PasswordHash, userWithRoles.PasswordSalt)
            ? ResponseResult.WithErrors<AuthToken>(HttpStatusCode.BadRequest, [UserErrors.InvalidPassword])
            : Result.Success(tokenProvider.Create(userWithRoles));
    }

    private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return computedHash.SequenceEqual(passwordHash);
    }
}
