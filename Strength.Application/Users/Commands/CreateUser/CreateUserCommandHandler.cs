namespace Strength.Application.Users.Commands.CreateUser;

using System.Net;
using System.Security.Cryptography;
using System.Text;
using Abstractions.Messaging;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Errors;
using Domain.Repositories;
using Domain.Repositories.Base;
using Domain.Services.Email;
using Domain.Shared;

internal sealed class CreateUserCommandHandler(
    IEmailService emailService,
    IUserRepository userRepository,
    IUserRoleRepository userRoleRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateUserCommand>
{
    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

        var user = new User { Email = request.Email, PasswordHash = passwordHash, PasswordSalt = passwordSalt };

        return await unitOfWork.BeginTransactionAsync(async () =>
        {
            var newUserId = await userRepository.CreateAsync(user, cancellationToken);
            if (newUserId == Guid.Empty)
            {
                return ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [UserErrors.NotCreated]);
            }

            var role = await roleRepository.GetByNameAsync(RoleName.User, cancellationToken);
            if (role is null)
            {
                return ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [RoleErrors.NotFound]);
            }

            var newUserRole = await userRoleRepository.CreateAsync(new UserRole { User = user, Role = role }, cancellationToken);
            if (newUserRole == Guid.Empty)
            {
                return ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [UserRoleErrors.NotCreated]);
            }

            return await emailService.SendVerificationEmail(user.Email, user.VerificationToken, cancellationToken)
                ? Result.Success()
                : ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [UserErrors.VerificationEmailNotSent]);
        }, cancellationToken);
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
}
