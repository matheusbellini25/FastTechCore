using FastTechKitchen.Domain.Entities;

namespace FastTechKitchen.Domain.Interfaces.Security;

public interface ITokenService
{
    string GenerateToken(User user, bool force = false);
}