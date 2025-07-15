using FastTech.Domain.Entities;

namespace FastTech.Domain.Interfaces.Security;

public interface ITokenService
{
    string GenerateToken(User user, bool force = false);
}