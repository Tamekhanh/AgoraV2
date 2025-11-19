using Agora.Application.Common;
using Agora.Domain.Entities;

namespace Agora.Application.Service;

public interface IUserService
{
    Task<PagedResult<User>> GetPaged(PagedRequest req);
    Task<User?> GetById(int id);
    Task<User> Create(User user);
    Task Update(User user);
    Task Delete(int id);
}
