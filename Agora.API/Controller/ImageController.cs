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
    private readonly IProductService _productService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(IImageService service, IShopService shopService, IProductService productService, ILogger<ImageController> logger)
    {
        _service = service;
        _shopService = shopService;
        _productService = productService;
        _logger = logger;
    }

    [Authorize]
    [HttpPut("users")]
    public async Task<IActionResult> UpdateUserImageSelf(IFormFile file)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mUpdating self user image.");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            await _service.UpdateUserImage(userId, file);
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mSelf user image updated successfully for user ID {UserId}.", userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[IMAGE]\u001b[0mError occurred while updating self user image.");
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpPut("controller/users/{userId}")]
    public async Task<IActionResult> UpdateUserImage(int userId, IFormFile file)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mUpdating image for user ID: {UserId}", userId);
            await _service.UpdateUserImage(userId, file);
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mImage for user ID: {UserId} updated successfully.", userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[IMAGE]\u001b[0mAn error occurred while updating the image of the user.");
            return StatusCode(500, "An error occurred while updating the user image: " + ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpPut("controller/shops/{shopId}")]
    public async Task<IActionResult> UpdateShopImage(int shopId, IFormFile file)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mUpdating image for shop ID: {ShopId}", shopId);
            await _service.UpdateShopImage(shopId, file);
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mImage for shop ID: {ShopId} updated successfully.", shopId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[IMAGE]\u001b[0mAn error occurred while updating the image of the shop.");
            return StatusCode(500, "An error occurred while updating the shop image: " + ex.Message);
        }
    }

    [Authorize]
    [HttpPut("shops")]
    public async Task<IActionResult> UpdateShopImageSelf(IFormFile file)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mUpdating self shop image.");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("\u001b[32m[IMAGE]\u001b[0mUnauthorized attempt to update shop image.");
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            
            var shop = await _shopService.GetByUserId(userId);

            if (shop == null)
            {
                return NotFound("Shop not found for this user");
            }

            await _service.UpdateShopImage(shop.Id, file);
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mShop image for shop ID: {ShopId} updated successfully by user ID: {UserId}.", shop.Id, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[IMAGE]\u001b[0mAn error occurred while updating the shop image.");
            return StatusCode(500, "An error occurred while updating the shop image: " + ex.Message);
        }
    }

    [Authorize(Roles = "1, 2")]
    [HttpPut("controller/products/{productId}")]
    public async Task<IActionResult> UpdateProductImage(int productId, IFormFile file)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mUpdating image for product ID: {ProductId}", productId);
            await _service.UpdateProductImage(productId, file);
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mImage for product ID: {ProductId} updated successfully.", productId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[IMAGE]\u001b[0mAn error occurred while updating the image of the product.");
            return StatusCode(500, "An error occurred while updating the product image: " + ex.Message);
        }
    }

    [Authorize]
    [HttpPut("products/{productId}")]
    public async Task<IActionResult> UpdateProductImageSelf(int productId, IFormFile file)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mUpdating self product image for product ID: {ProductId}", productId);
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("\u001b[32m[IMAGE]\u001b[0mUnauthorized attempt to update product.");
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            var shop = await _shopService.GetByUserId(userId);
            if (shop == null)
            {
                _logger.LogWarning("\u001b[32m[IMAGE]\u001b[0mUser ID: {UserId} attempted to create a product without a shop.", userId);
                return BadRequest("User does not have a shop to add products to.");
            }
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mUser ID and Shop ID extracted from token: {UserId}, {ShopId}", userId, shop.Id);
            var product = await _productService.GetById(productId, userId);

            if (product == null)
            {
                _logger.LogWarning("\u001b[32m[IMAGE]\u001b[0mNo product found for shop ID: {ShopId} when user ID: {UserId} attempted update.", shop.Id, userId);
                return NotFound("Product not found for this shop");
            }

            // Allow update if user is owner of the shop
            if (shop.Id != product.ShopId)
            {
                _logger.LogWarning("\u001b[32m[IMAGE]\u001b[0mUser {UserId} attempted to update a product they do not own.", userId);
                // _logger.LogInformation("Shop owner ID: {ShopOwnerId}, Requesting user ID: {UserId}", shop?.UserId, userId);
                return Forbid();
            }

            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mUpdating product ID: {ProductId} for shop ID: {ShopId}", product.Id, shop.Id);


            // Pass userId to service to claim ownership if needed
            // req.UserId = userId;
            if (product.Id <= 0)
                return BadRequest("Invalid product ID.");

            await _service.UpdateProductImage(product.Id, file);
            _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mProduct image for product ID: {ProductId} updated successfully by user ID: {UserId}.", product.Id, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[IMAGE]\u001b[0mAn error occurred while updating the product image.");
            return StatusCode(500, "An error occurred while updating the product image: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ImageDTO?>> GetById(int id, bool? ReSize = false, bool? isSmall = null, int? width = null, int? height = null)
    {
    try
    {
        //TODO: LÀM CACHE và HTTP ETAG ở dưới 
        _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mFetching image with ID {ImageId}.", id);
        
        bool reSize = ReSize ?? false;

        // Nếu isSmall = true hoặc width > 0 hoặc height > 0 → resize
        if ((isSmall ?? false) || (width.HasValue && width.Value > 0) || (height.HasValue && height.Value > 0))
        {
            reSize = true;
        }

        if (reSize)
        {
            if (width >= 1500 || height >= 1500)
            {
                _logger.LogWarning($"\u001b[45mResize LARGE ID={id}:\u001b[0m Cannot resize to large dimensions (>= 1500px). Please use smaller dimensions.");
                return StatusCode(400, "Cannot resize to large dimensions (>= 1500px). Please use smaller dimensions.");
            }
        }

        var image = await _service.GetById(id, ReSize, isSmall, width, height);
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

        //TODO: LÀM CACHE và HTTP ETAG ở trên 

        // Trả file ra đúng chuẩn HTTP
        return File(image.Data, "image/webp");   // đổi thành loại file của bạn
    }
    catch (ArgumentException ex)
    {
        _logger.LogError(ex, "Invalid argument while fetching image {ImageId}", id);
        return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error while fetching image {ImageId}", id);
        return StatusCode(500, "Internal Server Error");
    }
    }
}
