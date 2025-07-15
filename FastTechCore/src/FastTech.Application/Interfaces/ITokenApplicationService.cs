using FastTech.Application.DataTransferObjects;

namespace FastTech.Application.Interfaces
{
    public interface ITokenApplicationService
    {
        Task<string> GetToken(UserLogin userLogin);
        Task<string> GetTokenByAutorization(string email);
    }
}