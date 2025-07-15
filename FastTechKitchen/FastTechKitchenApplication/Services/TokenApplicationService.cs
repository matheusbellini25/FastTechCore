using FastTechKitchen.Application.DataTransferObjects;
using FastTechKitchen.Application.Interfaces;
using FastTechKitchen.Domain.Interfaces;
using FastTechKitchen.Domain.Interfaces.Security;

namespace FastTechKitchen.Application.Services;

public class TokenApplicationService(
    IUserService userService,
    ITokenService tokenService) : ITokenApplicationService
{
    private readonly IUserService _userService = userService;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<string> GetToken(UserLogin userLogin)
    {
        var user = await _userService.GetByEmail(userLogin.Email);

        if (user == null)
            throw new Exception("Usuário não encontrado");

        if (user.Password != userLogin.Password)
            throw new Exception("Senha inválida");

        return _tokenService.GenerateToken(user);
    }

    public async Task<string> GetTokenByAutorization(string email)
    {
        var user = await _userService.GetByEmail(email);

        if (user == null)
            throw new Exception("Usuário não encontrado");

        return _tokenService.GenerateToken(user, force: true);
    }
}