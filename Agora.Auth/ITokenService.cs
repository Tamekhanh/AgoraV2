using Agora.Domain.Entities;

namespace Agora.Auth;

public interface ITokenService
{
    string GenerateToken(User user);
}
