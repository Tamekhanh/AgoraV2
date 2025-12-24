using Agora.Application.Common;
using Agora.Application.DTOs;
using System.Threading.Tasks;

namespace Agora.Application.Service;

public interface IProductService
{
    Task<PagedResult<ProductDTO>> GetPaged(PagedRequest req);
    Task<ProductDTO?> GetById(int id, int? userId = null);
    Task<ProductDTO> Create(CreateProductRequest product);
    Task Update(ProductUpdateRequest product);
    Task Delete(int id);
    Task ChangeStatus(int productId, int status);
    Task UpdateImage(int productId, int imageId);

    Task<ProductDTO?> GetByShopId(int shopId);

    Task<int> GetProductStock(int productId);
    
    Task<bool> ReserveStock(int orderId);
    Task ReleaseStock(int orderId);
}
