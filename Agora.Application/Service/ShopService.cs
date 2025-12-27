using Agora.Application.Common;
using Agora.Domain.Entities;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Agora.Auth;
using Agora.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Agora.Application.Service;

public class ShopService : IShopService
{
    private readonly AgoraDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ILogger<ShopService> _logger;

    public ShopService(AgoraDbContext db, ITokenService tokenService, ILogger<ShopService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<PagedResult<ShopRequest>> GetPaged(PagedRequest req)
    {
        try
        {
            var query = _db.Shops.AsQueryable();
    
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(x => x.Name!.Contains(req.Search));
    
            var total = await query.CountAsync();
    
            var items = await query
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();
    
            var shopRequests = items.Select(shop => new ShopRequest
            {
                Id = shop.Id,
                Name = shop.Name,
                ContactName = shop.ContactName,
                Phone = shop.Phone,
                Email = shop.Email,
                TaxCode = shop.TaxCode,
                Address = shop.Address,
                UserId = shop.UserId,
                ImageId = shop.ImageId,
                Status = shop.Status
            }).ToList();
            return new PagedResult<ShopRequest>
            {
                Items = shopRequests,
                Total = total,
            };
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task<ShopRequest?> GetById(int id)
    {
        try
        {
            var shop = await _db.Shops.FirstOrDefaultAsync(x => x.Id == id);
            if (shop == null) return null;
    
            return new ShopRequest
            {
                Id = shop.Id,
                Name = shop.Name,
                ContactName = shop.ContactName,
                Phone = shop.Phone,
                Email = shop.Email,
                TaxCode = shop.TaxCode,
                Address = shop.Address,
                UserId = shop.UserId,
                ImageId = shop.ImageId,
                Status = shop.Status
            };
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task<ShopRequest> Create(CreateShopRequest request)
    {
        try
        {
            if(await _db.Shops.AnyAsync(s => s.UserId == request.UserId))
            {
                throw new ArgumentException("User already has a shop.");
            }
            var shop = new Shop
            {
                Name = request.Name,
                ContactName = request.ContactName,
                Phone = request.Phone,
                Email = request.Email,
                TaxCode = request.TaxCode,
                Address = request.Address,
                UserId = request.UserId,           
            };
            shop.CreatedAt = DateTime.UtcNow;
            shop.Status = 0;
    
            _db.Shops.Add(shop);
            await _db.SaveChangesAsync();
    
            var result = new ShopRequest
            {
                Id = shop.Id,
                Name = shop.Name,
                ContactName = shop.ContactName,
                Phone = shop.Phone,
                Email = shop.Email,
                TaxCode = shop.TaxCode,
                Address = shop.Address,
                UserId = shop.UserId,
                ImageId = shop.ImageId,
                Status = shop.Status
            };
            return result;
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task Update(ShopUpdateRequest req)
    {
        try
        {
            var shop = await _db.Shops.FindAsync(req.Id);
            if (shop == null) return;
    
            _logger.LogInformation("Updating shop with name: {ShopName}", shop.Name);
    
            if (!string.IsNullOrEmpty(req.Name))
            {
                _logger.LogInformation("Updating shop name to: {ShopName}", req.Name);
                shop.Name = req.Name;
            }
    
            if (!string.IsNullOrEmpty(req.ContactName))
            {
                shop.ContactName = req.ContactName;
            }
    
            if (!string.IsNullOrEmpty(req.Phone))
            {
                shop.Phone = req.Phone;
            }
    
            if (!string.IsNullOrEmpty(req.Email))
            {
                shop.Email = req.Email;
            }
    
            if (!string.IsNullOrEmpty(req.TaxCode))
            {
                shop.TaxCode = req.TaxCode;
            }
    
            if (!string.IsNullOrEmpty(req.Address))
            {
                shop.Address = req.Address;
            }
    
            if (req.UserId.HasValue)
            {
                shop.UserId = req.UserId;
            }
    
    
            shop.ImageId = req.ImageId ?? shop.ImageId;
            _db.Shops.Update(shop);
            await _db.SaveChangesAsync();
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task Delete(int id)
    {
        try
        {
            var shop = await _db.Shops.FindAsync(id);
            if (shop == null) return;
    
            _db.Shops.Remove(shop);
            await _db.SaveChangesAsync();
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task<ShopRequest?> GetByUserId(int userId)
    {
        try
        {
            var shop = await _db.Shops.FirstOrDefaultAsync(x => x.UserId == userId);
            if (shop == null) return null;
    
            return new ShopRequest
            {
                Id = shop.Id,
                Name = shop.Name,
                ContactName = shop.ContactName,
                Phone = shop.Phone,
                Email = shop.Email,
                TaxCode = shop.TaxCode,
                Address = shop.Address,
                UserId = shop.UserId,
                ImageId = shop.ImageId,
                Status = shop.Status
            };
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task ChangeStatus(int shopId, int status)
    {
       try
       {
         var shop = await _db.Shops.FindAsync(shopId);
         if (shop == null) return;
 
         if (status < 0 || status > 2)
         {
             throw new ArgumentException("Invalid status value.");
         }
 
         shop.Status = status;
         _db.Shops.Update(shop);
         await _db.SaveChangesAsync();
       }
       catch (System.Exception)
       {
        
        throw;
       }
    }

    public async Task UpdateImage(int shopId, int imageId)
    {
        try
        {
            var shop = await _db.Shops.FindAsync(shopId);
            if (shop == null) return;
    
            shop.ImageId = imageId;
            _db.Shops.Update(shop);
            await _db.SaveChangesAsync();
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task<ShopRequest?> GetByProductId(int productId)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
            if (product == null) return null;

            var shop = await _db.Shops.FirstOrDefaultAsync(x => x.Id == product.ShopId);
            if (shop == null) return null;

            return new ShopRequest
            {
                Id = shop.Id,
                Name = shop.Name,
                ContactName = shop.ContactName,
                Phone = shop.Phone,
                Email = shop.Email,
                TaxCode = shop.TaxCode,
                Address = shop.Address,
                UserId = shop.UserId,
                ImageId = shop.ImageId,
                Status = shop.Status
            };
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }
}
