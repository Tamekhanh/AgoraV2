using Agora.Application.Common;
using Agora.Domain.Entities;
using System.Threading.Tasks;
using Agora.Application.DTOs;

namespace Agora.Application.Service;

public interface IShopService
{
    Task<PagedResult<ShopRequest>> GetPaged(PagedRequest req);
    Task<ShopRequest?> GetById(int id);
    Task<ShopRequest> Create(CreateShopRequest shop);
    Task Update(ShopUpdateRequest shop);
    Task Delete(int id);

    Task<ShopRequest?> GetByUserId(int userId);

    Task ChangeStatus(int shopId, int status);

    Task UpdateImage(int shopId, int imageId);
}
