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
[Route("api/images")]
public class ImageController : ControllerBase
{
    private readonly IImageService _service;
    private readonly IShopService _shopService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(IImageService service, IShopService shopService, ILogger<ImageController> logger)
    {
        _service = service;
        _shopService = shopService;
        _logger = logger;
    }

    [Authorize]
    [HttpPut("users/self")]
    public async Task<IActionResult> UpdateUserImageSelf(IFormFile file)
    {
        try
        {
            _logger.LogInformation("Updating self user image.");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            await _service.UpdateUserImage(userId, file);
            _logger.LogInformation("Self user image updated successfully for user ID {UserId}.", userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating self user image.");
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpPut("users/{userId}")]
    public async Task<IActionResult> UpdateUserImage(int userId, IFormFile file)
    {
        try
        {
            _logger.LogInformation("Updating image for user ID: {UserId}", userId);
            await _service.UpdateUserImage(userId, file);
            _logger.LogInformation("Image for user ID: {UserId} updated successfully.", userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the image of the user.");
            return StatusCode(500, "An error occurred while updating the user image: " + ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpPut("shops/{shopId}")]
    public async Task<IActionResult> UpdateShopImage(int shopId, IFormFile file)
    {
        try
        {
            _logger.LogInformation("Updating image for shop ID: {ShopId}", shopId);
            await _service.UpdateShopImage(shopId, file);
            _logger.LogInformation("Image for shop ID: {ShopId} updated successfully.", shopId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the image of the shop.");
            return StatusCode(500, "An error occurred while updating the shop image: " + ex.Message);
        }
    }

    [Authorize]
    [HttpPut("shops/self")]
    public async Task<IActionResult> UpdateShopImageSelf(IFormFile file)
    {
        try
        {
            _logger.LogInformation("Updating self shop image.");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("Unauthorized attempt to update shop image.");
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            
            var shop = await _shopService.GetByUserId(userId);

            if (shop == null)
            {
                return NotFound("Shop not found for this user");
            }

            await _service.UpdateShopImage(shop.Id, file);
            _logger.LogInformation("Shop image for shop ID: {ShopId} updated successfully by user ID: {UserId}.", shop.Id, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the shop image.");
            return StatusCode(500, "An error occurred while updating the shop image: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ImageDTO?>> GetById(int id)
    {
    try
    {
        _logger.LogInformation("Fetching image with ID {ImageId}.", id);

        var image = await _service.GetById(id);
        if (image == null)
        {
            _logger.LogWarning("Image with ID {ImageId} not found.", id);
            return NotFound();
        }

        if (image.Data == null)
        {
            _logger.LogWarning("Image data is null for ID {ImageId}.", id);
            return BadRequest("Image data is empty.");
        }

        // Trả file ra đúng chuẩn HTTP
        return File(image.Data, "image/webp");   // đổi thành loại file của bạn
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error while fetching image {ImageId}", id);
        return StatusCode(500, "Internal Server Error");
    }
    }
}
