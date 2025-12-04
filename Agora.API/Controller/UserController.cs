using Agora.Application.Common;
using Agora.Application.DTOs;
using Agora.Application.Service;
using Agora.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    public Task<PagedResult<User>> GetPaged([FromQuery] PagedRequest req)
        => _service.GetPaged(req);

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
}
