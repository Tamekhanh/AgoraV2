using Agora.Application.Common;
using Agora.Domain.Entities;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agora.Application.Service;

public class CategoryService : ICategoryService
{
    private readonly AgoraDbContext _db;

    public CategoryService(AgoraDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<Category>> GetPaged(PagedRequest req)
    {
        var query = _db.Categories.AsQueryable();

        if (!string.IsNullOrEmpty(req.Search))
            query = query.Where(x => x.Name!.Contains(req.Search));

        var total = await query.CountAsync();

        var items = await query
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();

        return new PagedResult<Category> { Total = total, Items = items };
    }

    public Task<Category?> GetById(int id) =>
        _db.Categories.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Category> Create(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task Update(Category category)
    {
        category.UpdatedAt = DateTime.UtcNow;
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return;

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }
}
