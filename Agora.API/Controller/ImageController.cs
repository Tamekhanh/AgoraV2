using Agora.Application.Common;
using Agora.Application.DTOs;
using Agora.Application.Service;
using Agora.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace Agora.API.Controllers;

[ApiController]
[Route("api/images")]
public class ImageController : ControllerBase
{
    private readonly IImageService _service;
    private readonly IShopService _shopService;
    private readonly IProductService _productService;
    private readonly ILogger<ImageController> _logger;
    private readonly IMemoryCache _cache;

    public ImageController(IImageService service, IShopService shopService, IProductService productService, ILogger<ImageController> logger, IMemoryCache cache)
    {
        _service = service;
        _shopService = shopService;
        _productService = productService;
        _logger = logger;
        _cache = cache;
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
            var oldImageId = await _service.UpdateUserImage(userId, file);
            if (oldImageId.HasValue)
            {
                _cache.Remove($"ImagesImage{oldImageId}");
            }
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
            var oldImageId = await _service.UpdateUserImage(userId, file);
            if (oldImageId.HasValue)
            {
                _cache.Remove($"ImagesImage{oldImageId}");
            }
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
            var oldImageId = await _service.UpdateShopImage(shopId, file);
            if (oldImageId.HasValue)
            {
                _cache.Remove($"ImagesImage{oldImageId}");
            }
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

            var oldImageId = await _service.UpdateShopImage(shop.Id, file);
            if (oldImageId.HasValue)
            {
                _cache.Remove($"ImagesImage{oldImageId}");
            }
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
            var oldImageId = await _service.UpdateProductImage(productId, file);
            if (oldImageId.HasValue)
            {
                _cache.Remove($"ImagesImage{oldImageId}");
            }
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

            var oldImageId = await _service.UpdateProductImage(product.Id, file);
            if (oldImageId.HasValue)
            {
                _cache.Remove($"ImagesImage{oldImageId}");
            }
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
        _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mFetching image with ID {ImageId}.", id);
        try
        {
            string cacheKey = $"ImagesImage{id}";

            bool reSize = ReSize ?? false;

            if(isSmall == null)
            {
                isSmall = false;
            }

            // Nếu isSmall = true hoặc width > 0 hoặc height > 0 → resize
            if ((isSmall ?? false) || (width.HasValue && width.Value > 0) || (height.HasValue && height.Value > 0))
            {
                reSize = true;
            }

            // Lấy toàn bộ phiên bản ảnh của id này từ cache
            if (!_cache.TryGetValue(cacheKey, out Dictionary<(bool?, bool?, int?, int?), (byte[] ImageData, string ETag)>? allVersions))
            {
                allVersions = new Dictionary<(bool?, bool?, int?, int?), (byte[], string)>();
            }

            var versionKey = (reSize, isSmall, width, height);

            // Nếu phiên bản này chưa có trong cache thì tạo mới
            if (!allVersions!.TryGetValue(versionKey, out var cacheEntry))
            {
                if (reSize)
                {
                    if (width >= 1500 || height >= 1500)
                    {
                        _logger.LogWarning($"\u001b[45mResize LARGE ID={id}:\u001b[0m Cannot resize to large dimensions (>= 1500px). Please use smaller dimensions.");
                        return StatusCode(400, "Cannot resize to large dimensions (>= 1500px). Please use smaller dimensions.");
                    }
                }

                _logger.LogInformation("\u001b[32m[IMAGE]\u001b[0mFetching image with ID {ImageId} from service.", id);
                var image = await _service.GetById(id, ReSize, isSmall, width, height);
                
                if (image == null)
                {
                    _logger.LogWarning("Image with ID {ImageId} not found.", id);
                    return NotFound();
                }

                if (image.Data == null)
                {
                    _logger.LogWarning("\u001b[32m[IMAGE]\u001b[0mImage data is null for ID {ImageId}.", id);
                    return BadRequest("Image data is empty.");
                }

                byte[] imageData = image.Data;
                string hash = ComputeHash(imageData);

                string eTag = $"\"{id}-{hash}-ReSize={reSize}-isSmall={isSmall}-Width={width}-Height={height}\"";
                _logger.LogInformation($"\u001b[45mGenerated ETag:\u001b[0m {eTag}");

                cacheEntry = (imageData, eTag);
                allVersions[versionKey] = cacheEntry;

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSize(1) // each entry size is 1
                    .SetAbsoluteExpiration(TimeSpan.FromDays(2)) // expire after 2 day
                    .SetSlidingExpiration(TimeSpan.FromHours(24)); // expire if not accessed for 24 hours

                _cache.Set(cacheKey, allVersions, cacheOptions);
            }

            // Nếu client gửi ETag khớp thì trả 304
            if (Request.Headers.TryGetValue("If-None-Match", out var incomingETag) && incomingETag == cacheEntry.ETag)
            {
                _logger.LogInformation($"\u001b[45m304 Not Modified, ETag:\u001b[0m {cacheEntry.ETag}");
                return StatusCode(304);
            }

            // Thiết lập header
            Response.Headers["Cache-Control"] = "public, must-revalidate, max-age=0";
            Response.Headers["Expires"] = DateTime.UtcNow.AddDays(1).ToString("R");
            Response.Headers["ETag"] = cacheEntry.ETag;

            return File(cacheEntry.ImageData, "image/webp");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "\u001b[32m[IMAGE]\u001b[0mInvalid argument while fetching image {ImageId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[IMAGE]\u001b[0mError while fetching image {ImageId}", id);
            return StatusCode(500, "Internal Server Error");
        }
    }

    private string ComputeHash(byte[] data)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }
    }
}
