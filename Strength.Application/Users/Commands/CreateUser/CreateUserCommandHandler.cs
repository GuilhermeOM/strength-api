namespace Strength.Application.Users.Commands.CreateUser;

using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using Abstractions.Messaging;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Strength.Domain.Entities.Enums;
using System.Net;

internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IUserRoleRepository userRoleRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IFluentEmail fluentEmail,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<CreateUserCommand>
{
    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            VerificationToken = CreateRandomToken(),
        };

        return await unitOfWork.BeginTransactionAsync(async () =>
        {
            var newUserId = await userRepository.CreateUserAsync(user, cancellationToken);
            if (newUserId == Guid.Empty)
            {
                return ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [UserErrors.NotCreated]);
            }

            var role = await roleRepository.GetRoleByNameAsync(RoleName.User, cancellationToken);
            if (role is null)
            {
                return ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [RoleErrors.NotFound]);
            }

            var newUserRole = await userRoleRepository.CreateUserRoleAsync(new UserRole
            {
                User = user,
                Role = role
            }, cancellationToken);

            if (newUserRole is null)
            {
                return ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [UserRoleErrors.NotCreated]);
            }

            var httpContextRequest = httpContextAccessor.HttpContext?.Request;
            var verificationLink =
                $"{httpContextRequest?.Scheme}://{httpContextRequest?.Host}/api/user/verify?verificationToken={user.VerificationToken}";

            _ = await fluentEmail
                .To(user.Email)
                .Subject("Email verification for Strength")
                .Body($"To verify your email address <a href='{verificationLink}'>click here</a>", isHtml: true)
                .SendAsync(cancellationToken);

            return Result.Success();
        }, cancellationToken);
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private static string CreateRandomToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
}
