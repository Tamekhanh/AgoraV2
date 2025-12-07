using Agora.Domain.Entities;
using Agora.Application.Common;
using Agora.Application.DTOs;
using System.Threading.Tasks;

namespace Agora.Application.Service;

public interface ICategoryService
{
    Task<PagedResult<CategoryDTO>> GetPaged(PagedRequest req);
    Task<CategoryDTO?> GetById(int id);
    Task<CategoryDTO> Create(CategoryDTO category);
    Task Update(int id, CategoryDTO category);
    Task Delete(int id);
}
