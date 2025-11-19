using Agora.Domain.Entities;
using Agora.Application.Common;
using System.Threading.Tasks;

namespace Agora.Application.Service;

public interface ICategoryService
{
    Task<PagedResult<Category>> GetPaged(PagedRequest req);
    Task<Category?> GetById(int id);
    Task<Category> Create(Category category);
    Task Update(Category category);
    Task Delete(int id);
}
