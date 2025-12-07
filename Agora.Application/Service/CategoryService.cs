using Agora.Application.Common;
using Agora.Domain.Entities;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Agora.Auth;
using Agora.Application.DTOs;

namespace Agora.Application.Service;

public class CategoryService : ICategoryService
{
    private readonly AgoraDbContext _db;
    private readonly ITokenService _tokenService;

    public CategoryService(AgoraDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<PagedResult<CategoryDTO>> GetPaged(PagedRequest req)
    {
        try
        {
            var query = _db.Categories.AsQueryable();
    
            if (!string.IsNullOrEmpty(req.Search))
                query = query.Where(x => x.Name!.Contains(req.Search));
    
            var total = await query.CountAsync();
    
            var items = await query
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();
    
            var itemsDto = items.Select(item => new CategoryDTO
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            }).ToList();
    
            return new PagedResult<CategoryDTO> { Total = total, Items = itemsDto };
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<CategoryDTO?> GetById(int id)
    {
        try
        {
            var category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null) return null;
    
            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<CategoryDTO> Create(CategoryDTO category)
    {
        try
        {
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
    
            var entity = new Category
            {
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
    
            _db.Categories.Add(entity);
            await _db.SaveChangesAsync();
            return category;
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task Update(int id, CategoryDTO category)
    {
        try
        {
            category.UpdatedAt = DateTime.UtcNow;
    
            var entity = await _db.Categories.FindAsync(id);
            if (entity == null) return;
    
            entity.Name = category.Name;
            entity.Description = category.Description;
            entity.UpdatedAt = DateTime.UtcNow;
    
            _db.Categories.Update(entity);
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
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return;
    
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }
}
