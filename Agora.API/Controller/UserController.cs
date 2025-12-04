using Agora.Application.Common;
using Agora.Application.DTOs;
using Agora.Application.Service;
using Agora.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agora.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [Authorize(Roles = "1, 2")]
    [HttpGet]
    public Task<PagedResult<User>> GetPaged([FromQuery] PagedRequest req)
        => _service.GetPaged(req);

    [Authorize(Roles = "1, 2")]
    [HttpGet("{id}")]
    public Task<User?> GetById(int id)
        => _service.GetById(id);

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        try
        {
            var createdUser = await _service.Create(user);
            return Ok(createdUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, User user)
    {
        try
        {
            user.Id = id;
            await _service.Update(user);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpPut("self")]
    public async Task<IActionResult> UpdateSelf([FromBody] UserUpdateRequest req)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            var userId = int.Parse(userIdClaim.Value);
            var updatedUser = await _service.UpdateSelf(userId, req);
            
            if (updatedUser == null) return NotFound();
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpDelete("{id}")]
    public Task Delete(int id)
        => _service.Delete(id);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        try
        {
            var response = await _service.Login(req);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [Authorize(Roles = "1")]
    [HttpPut("role/{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpdateRequest req)
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("User ID claim not found.");

            if (int.TryParse(userIdString, out int currentUserId) && currentUserId == id)
            {
                 return BadRequest("Users cannot change their own roles.");
            }

            await _service.UpdateRole(id, req.NewRoleId);
            return Ok();
        }
        // Bắt các lỗi liên quan đến dữ liệu (như "Invalid role ID" hoặc "User not found")
        catch (ArgumentException ex) 
        {
            // Trả về 400 Bad Request cho lỗi dữ liệu đầu vào
            return BadRequest(ex.Message); 
        }
        catch (InvalidOperationException ex) 
        {
            // Trả về 400 Bad Request cho lỗi không tìm thấy đối tượng (nếu bạn muốn)
            return BadRequest(ex.Message); 
        }
        catch (Exception ex)
        {
            // Bắt các lỗi không mong muốn khác
            // Ghi log ex
            return StatusCode(500, "An unexpected error occurred: " + ex.Message); // Trả về 500 Internal Server Error
        }
    }
}
