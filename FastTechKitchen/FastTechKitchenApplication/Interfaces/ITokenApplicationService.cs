using FastTechKitchen.Application.DataTransferObjects;

namespace FastTechKitchen.Application.Interfaces
{
    public interface ITokenApplicationService
    {
        Task<string> GetToken(UserLogin userLogin);
        Task<string> GetTokenByAutorization(string email);
    }
}