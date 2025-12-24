using Agora.Application.Common;
using Agora.Application.DTOs;
using Agora.Domain.Entities;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Agora.Application.Service;

public class ProductService : IProductService
{
    private readonly AgoraDbContext _db;
    private readonly ILogger<ProductService> _logger;

    public ProductService(AgoraDbContext db, ILogger<ProductService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDTO>> GetPaged(PagedRequest req)
    {
        try
        {
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(x => x.Name!.Contains(req.Search));

            var total = await query.CountAsync();

            var items = await query
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();



            // Hide cost price for non-admin users
            foreach (var product in items)
            {
                product.CostPrice = 0;
                product.CreatedAt = null;
                product.UpdatedAt = null;
            }


            var productDTOs = items.Select(product => new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                Description = product.Description,
                CategoryId = product.CategoryId,
                ShopId = product.ShopId,
                CostPrice = product.CostPrice,
                RetailPrice = product.RetailPrice,
                StockQty = product.StockQty,
                DiscountPercent = product.DiscountPercent,
                GuaranteeMonths = product.GuaranteeMonths,
                ImageId = product.ImageId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Note = product.Note,
                SoldQty = product.SoldQty,
                Status = product.Status
            }).ToList();

            return new PagedResult<ProductDTO>
            {
                Items = productDTOs,
                Total = total,
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ProductDTO?> GetById(int id, int? userId = null)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null) return null;

            var isAdmin = false;
            if (userId != null)
            {
                isAdmin = await CheckAdminOfProduct(id, userId.Value);
            }

            if (isAdmin != true)
            {
                // Hide cost price for non-admin users
                product.CostPrice = 0;
                product.CreatedAt = null;
                product.UpdatedAt = null;
            }

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                Description = product.Description,
                CategoryId = product.CategoryId,
                ShopId = product.ShopId,
                CostPrice = product.CostPrice,
                RetailPrice = product.RetailPrice,
                StockQty = product.StockQty,
                DiscountPercent = product.DiscountPercent,
                GuaranteeMonths = product.GuaranteeMonths,
                ImageId = product.ImageId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Note = product.Note,
                SoldQty = product.SoldQty,
                Status = product.Status
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ProductDTO> Create(CreateProductRequest request)
    {
        try
        {
            var product = new Product
            {
                Name = request.Name,
                Barcode = request.Barcode,
                Description = request.Description,
                CategoryId = request.CategoryId,
                ShopId = request.ShopId,
                CostPrice = request.CostPrice,
                RetailPrice = request.RetailPrice,
                StockQty = request.StockQty,
                DiscountPercent = request.DiscountPercent,
                GuaranteeMonths = request.GuaranteeMonths,
                Note = request.Note,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = 0 // Default status
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                Description = product.Description,
                CategoryId = product.CategoryId,
                ShopId = product.ShopId,
                CostPrice = product.CostPrice,
                RetailPrice = product.RetailPrice,
                StockQty = product.StockQty,
                DiscountPercent = product.DiscountPercent,
                GuaranteeMonths = product.GuaranteeMonths,
                ImageId = product.ImageId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Note = product.Note,
                SoldQty = product.SoldQty,
                Status = product.Status
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task Update(ProductUpdateRequest request)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (product == null) throw new Exception("Product not found");

            product.Name = request.Name ?? product.Name;
            product.Barcode = request.Barcode ?? product.Barcode;
            product.Description = request.Description ?? product.Description;
            product.CategoryId = request.CategoryId ?? product.CategoryId;
            // product.ShopId = request.ShopId ?? product.ShopId;
            product.CostPrice = request.CostPrice ?? product.CostPrice;
            product.RetailPrice = request.RetailPrice ?? product.RetailPrice;
            product.StockQty = request.StockQty ?? product.StockQty;
            product.DiscountPercent = request.DiscountPercent ?? product.DiscountPercent;
            product.GuaranteeMonths = request.GuaranteeMonths ?? product.GuaranteeMonths;
            product.ImageId = request.ImageId ?? product.ImageId;
            product.Note = request.Note ?? product.Note;
            product.Status = request.Status ?? product.Status;
            product.UpdatedAt = DateTime.Now;

            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task Delete(int id)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null) throw new Exception("Product not found");

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task ChangeStatus(int productId, int status)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
            if (product == null) throw new Exception("Product not found");

            if (status < -1 || status > 2) // -1: banned, 0: Active, 1: Out of Stock, 2: Discontinued
            {
                throw new ArgumentException("Invalid status value.");
            }

            product.Status = status;
            product.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task UpdateImage(int productId, int imageId)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
            if (product == null) throw new Exception("Product not found");

            product.ImageId = imageId;
            product.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ProductDTO?> GetByShopId(int shopId)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.ShopId == shopId);
            if (product == null) return null;

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                Description = product.Description,
                CategoryId = product.CategoryId,
                ShopId = product.ShopId,
                CostPrice = product.CostPrice,
                RetailPrice = product.RetailPrice,
                StockQty = product.StockQty,
                DiscountPercent = product.DiscountPercent,
                GuaranteeMonths = product.GuaranteeMonths,
                ImageId = product.ImageId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Note = product.Note,
                SoldQty = product.SoldQty,
                Status = product.Status
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> CheckAdminOfProduct(int productId, int userId)
    {
        try
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                if (user.Role == 1) // Admin role
                    return true;
            }

            var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
            if (product == null) return false;

            var shop = await _db.Shops.FirstOrDefaultAsync(x => x.Id == product.ShopId);
            if (shop == null) return false;

            return shop.UserId == userId;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> GetProductStock(int productId)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null)
        {
            throw new ArgumentException("Product not found.");
        }
        return product.StockQty ?? 0;
    }

    public async Task<bool> ReserveStock(int orderId)
    {
        try
        {
            var orderItems = await _db.OrderItems.Where(x => x.OrderId == orderId).ToListAsync();
            foreach (var item in orderItems)
            {
                if (item.ProductId == null || item.Quantity == null) continue;

                var product = await _db.Products.FindAsync(item.ProductId);
                if (product == null || (product.StockQty ?? 0) < item.Quantity)
                {
                    return false;
                }
                product.StockQty = (product.StockQty ?? 0) - item.Quantity;
            }
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for order {OrderId}", orderId);
            return false;
        }
    }

    public async Task ReleaseStock(int orderId)
    {
        try
        {
            var orderItems = await _db.OrderItems.Where(x => x.OrderId == orderId).ToListAsync();
            foreach (var item in orderItems)
            {
                if (item.ProductId == null || item.Quantity == null) continue;

                var product = await _db.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQty = (product.StockQty ?? 0) + item.Quantity;
                }
            }
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing stock for order {OrderId}", orderId);
        }
    }
}
