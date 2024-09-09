namespace Strength.Domain.Shared;

using Entities;

public interface ITokenProvider
{
    AuthToken Create(User user);
}
