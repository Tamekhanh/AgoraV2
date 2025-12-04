using Agora.Application.Common;
using Agora.Application.DTOs;
using Agora.Application.Service;
using Agora.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Agora.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService service, ILogger<UserController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [Authorize(Roles = "1, 2")]
    [HttpGet]
    public Task<PagedResult<User>> GetPaged([FromQuery] PagedRequest req)
    {
        _logger.LogInformation("\u001b[44;97m[User]\u001b[0m Fetching paged users.");
        return _service.GetPaged(req);
    }

    [Authorize(Roles = "1, 2")]
    [HttpGet("{id}")]
    public Task<User?> GetById(int id)
    {
        _logger.LogInformation("\u001b[44;97m[User]\u001b[0m Fetching user with ID {UserId}.", id);
        return _service.GetById(id);
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        try
        {
            _logger.LogInformation("\u001b[44;97m[User]\u001b[0m Creating a new user.");
            var createdUser = await _service.Create(user);
            _logger.LogInformation("\u001b[44;97m[User]\u001b[0m User created successfully with ID {UserId}.", createdUser.Id);
            return Ok(createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[User]\u001b[0m Error occurred while creating a new user.");
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, User user)
    {
        try
        {
            _logger.LogInformation("\u001b[44;97m[User]\u001b[0m Updating user with ID {UserId}.", id);
            user.Id = id;
            await _service.Update(user);
            _logger.LogInformation("\u001b[44;97m[User]\u001b[0m User with ID {UserId} updated successfully.", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[User]\u001b[0m Error occurred while updating user with ID {UserId}.", id);
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpPut("self")]
    public async Task<IActionResult> UpdateSelf([FromBody] UserUpdateRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[44;97m[User]\u001b[0m Updating self user information.");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var updatedUser = await _service.UpdateSelf(userId, req);

            if (updatedUser == null) return NotFound();
            _logger.LogInformation("\u001b[44;97m[User]\u001b[0m Self user information updated successfully for user ID {UserId}.", userId);
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[User]\u001b[0m Error occurred while updating self user information for user.");
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpDelete("{id}")]
    public Task Delete(int id)
    {
        _logger.LogInformation("\u001b[44;97m[User]\u001b[0m Deleting user with ID {UserId}.", id);
        try
        {
            return _service.Delete(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[User]\u001b[0m Error occurred while deleting user with ID {UserId}.", id);
            throw;
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[44;97m[User]\u001b[0m User login attempt.");
            var response = await _service.Login(req);
            _logger.LogInformation("\u001b[44;97m[User]\u001b[0m User login successful.");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[User]\u001b[0m User login failed.");
            return Unauthorized(ex.Message);
        }
    }

    [Authorize(Roles = "1")]
    [HttpPut("role/{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpdateRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[44;97m[User]\u001b[0m Updating role for user with ID {UserId}.", id);
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("User ID claim not found.");

            

            if (int.TryParse(userIdString, out int currentUserId) && currentUserId == id)
            {
                _logger.LogWarning("\u001b[44;97m[User]\u001b[0m User with ID {UserId} attempted to change their own role.", id);
                return BadRequest("Users cannot change their own roles.");
            }

            _logger.LogInformation("\u001b[44;97m[User]\u001b[0m Proceeding to update role for user with ID {UserId} to new role ID {NewRoleId} by user ID {CurrentUserId}.", id, req.NewRoleId, currentUserId);
            await _service.UpdateRole(id, req.NewRoleId);
            return Ok();
        }
        // Bắt các lỗi liên quan đến dữ liệu (như "Invalid role ID" hoặc "User not found")
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[User]\u001b[0m Invalid argument while updating role for user with ID {UserId}.", id);
            // Trả về 400 Bad Request cho lỗi dữ liệu đầu vào
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[User]\u001b[0m Invalid operation while updating role for user with ID {UserId}.", id);
            // Trả về 400 Bad Request cho lỗi không tìm thấy đối tượng (nếu bạn muốn)
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[44;97m[User]\u001b[0m Unexpected error while updating role for user with ID {UserId}.", id);
            // Bắt các lỗi không mong muốn khác
            // Ghi log ex
            return StatusCode(500, "An unexpected error occurred: " + ex.Message); // Trả về 500 Internal Server Error
        }
    }
}
