using Agora.Application.Common;
using Agora.Application.Service;
using Agora.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Agora.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public Task<PagedResult<Category>> GetPaged([FromQuery] PagedRequest req)
        => _service.GetPaged(req);

    [HttpGet("{id}")]
    public Task<Category?> GetById(int id)
        => _service.GetById(id);

    [HttpPost]
    public Task<Category> Create(Category category)
        => _service.Create(category);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Category category)
    {
        category.Id = id;
        await _service.Update(category);
        return Ok();
    }

    [HttpDelete("{id}")]
    public Task Delete(int id)
        => _service.Delete(id);
}
