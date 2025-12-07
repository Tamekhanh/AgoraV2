using Agora.Application.Common;
using Agora.Application.Service;
using Agora.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Agora.Application.DTOs;

namespace Agora.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService service, ILogger<CategoryController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryDTO>>> GetPaged([FromQuery] PagedRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[44;97m[Category]\u001b[0m Fetching paged categories.");
            var result = await _service.GetPaged(req);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[Category]\u001b[0m Error occurred while retrieving paged categories.");
            return BadRequest("An error occurred while retrieving categories: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDTO?>> GetById(int id)
        {try
        {
            _logger.LogInformation("\u001b[44;97m[Category]\u001b[0m Fetching category with ID {CategoryId}.", id);
            var category = await _service.GetById(id);
            if (category == null)
            {
                _logger.LogWarning("\u001b[44;97m[Category]\u001b[0m Category with ID {CategoryId} not found.", id);
                return NotFound();
            }
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[Category]\u001b[0m Error occurred while retrieving category with ID {CategoryId}.", id);
            return BadRequest("An error occurred while retrieving the category: " + ex.Message);
        }}

    [HttpPost]
    [Authorize(Roles = "1")]
    public async Task<ActionResult<CategoryDTO>> Create(CategoryDTO category)
    {
        try
        {
            _logger.LogInformation("\u001b[44;97m[Category]\u001b[0m Creating a new category with name {CategoryName}.", category.Name);
            var createdCategory = await _service.Create(category);
            return Ok(createdCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[Category]\u001b[0m Error occurred while creating a new category.");
            return BadRequest("An error occurred while creating the category: " + ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "1")]
    public async Task<IActionResult> Update(int id, CategoryDTO category)
    {
        try
        {
            _logger.LogInformation("\u001b[44;97m[Category]\u001b[0m Updating category with ID {CategoryId}.", id);
            category.Id = id;
            await _service.Update(id,category);
            return Ok();
        }
        catch (Exception ex)
        {      
            _logger.LogError(ex, "\u001b[44;97m[Category]\u001b[0m Error occurred while updating category with ID {CategoryId}.", id);
            return BadRequest("An error occurred while updating the category: " + ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "1")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            _logger.LogInformation("\u001b[44;97m[Category]\u001b[0m Deleting category with ID {CategoryId}.", id);
            await _service.Delete(id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[Category]\u001b[0m Error occurred while deleting category with ID {CategoryId}.", id);
            return BadRequest("An error occurred while deleting the category: " + ex.Message);
        }
    }
}
