using FastTechKitchen.Domain.Entities;
using FastTechKitchen.Domain.Interfaces.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FastTechKitchen.Infraestructure.Data.Repositories;

public class UserRepository(ApplicationDBContext context) : BaseRepository<User>(context), IUserRepository
{
    public override async Task<User> GetById(Guid id, bool include = false, bool tracking = false)
    {
        var query = BaseQuery(tracking);

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User> GetByEmail(string email, bool include = false, bool tracking = false)
    {
        var user = await BaseQuery(tracking)
            .FirstOrDefaultAsync(x => x.Email == email);

        return user;
    }
}