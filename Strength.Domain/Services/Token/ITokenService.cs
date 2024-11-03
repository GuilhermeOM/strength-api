namespace Strength.Domain.Services.Token;

using Entities;

public interface ITokenService
{
    AuthToken Create(User user);
}
