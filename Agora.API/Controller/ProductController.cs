using Agora.Application.Common;
using Agora.Application.DTOs;
using Agora.Application.Service;
using Agora.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agora.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IShopService _shopService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService service, IShopService shopService, ILogger<ProductController> logger)
    {
        _service = service;
        _shopService = shopService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDTO>>> GetPaged([FromQuery] PagedRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mRetrieving paged products with request: {@PagedRequest}", req);
            var result = await _service.GetPaged(req);
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mPaged products retrieved successfully.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[PRODUCT]\u001b[0mAn error occurred while retrieving paged products.");
            return StatusCode(500, "An error occurred while retrieving products: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDTO?>> GetById(int id)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mRetrieving product with ID: {ProductId}", id);
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
            var product = await _service.GetById(id, userId);
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mProduct with ID: {ProductId} retrieved successfully.", id);
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[PRODUCT]\u001b[0mAn error occurred while retrieving the product with ID: {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product: " + ex.Message);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductDTO>> Create(CreateProductRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mCreating a new product with name: {ProductName}", req.Name);
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("\u001b[32m[SHOP]\u001b[0mUnauthorized attempt to create a shop.");
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            var shop = await _shopService.GetByUserId(userId);
            if (shop == null)
            {
                _logger.LogWarning("\u001b[32m[PRODUCT]\u001b[0mUser ID: {UserId} attempted to create a product without a shop.", userId);
                return BadRequest("User does not have a shop to add products to.");
            }
            req.ShopId = shop.Id;
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mUser ID and Shop ID extracted from token: {UserId}, {ShopId}", userId, shop.Id);
            var createdProduct = await _service.Create(req);
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mProduct created successfully with ID: {ProductId}", createdProduct.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[PRODUCT]\u001b[0mAn error occurred while creating a new product.");
            return StatusCode(500, "An error occurred while creating the product: " + ex.Message);
        }
    }

    [HttpPut("controller/{id}")]
    [Authorize(Roles = "1, 2")]
    public async Task<IActionResult> Update(int id, ProductUpdateRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mUpdating product with ID: {ProductId}", id);
            req.Id = id;
            await _service.Update(req);
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mProduct with ID: {ProductId} updated successfully.", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[PRODUCT]\u001b[0mAn error occurred while updating the product.");
            return StatusCode(500, "An error occurred while updating the product: " + ex.Message);
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSelf(int id, [FromBody] ProductUpdateRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mUpdating self product information.");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("\u001b[32m[PRODUCT]\u001b[0mUnauthorized attempt to update product.");
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            var shop = await _shopService.GetByUserId(userId);
            if (shop == null)
            {
                _logger.LogWarning("\u001b[32m[PRODUCT]\u001b[0mUser ID: {UserId} attempted to create a product without a shop.", userId);
                return BadRequest("User does not have a shop to add products to.");
            }
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mUser ID and Shop ID extracted from token: {UserId}, {ShopId}", userId, shop.Id);
            var product = await _service.GetById(id, userId);

            if (product == null)
            {
                _logger.LogWarning("\u001b[32m[PRODUCT]\u001b[0mNo product found for shop ID: {ShopId} when user ID: {UserId} attempted update.", shop.Id, userId);
                return NotFound("Product not found for this shop");
            }

            // Allow update if user is owner of the shop
            if (shop.Id != product.ShopId)
            {
                _logger.LogWarning("\u001b[32m[PRODUCT]\u001b[0mUser {UserId} attempted to update a product they do not own.", userId);
                // _logger.LogInformation("Shop owner ID: {ShopOwnerId}, Requesting user ID: {UserId}", shop?.UserId, userId);
                return Forbid();
            }

            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mUpdating product ID: {ProductId} for shop ID: {ShopId}", req.Id, shop.Id);


            // Pass userId to service to claim ownership if needed
            // req.UserId = userId;
            req.Id = product.Id;

            if (req.Id <= 0)
                return BadRequest("Invalid product ID.");

            await _service.Update(req);
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mProduct {ProductId} updated successfully by user {UserId}.", product.Id, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[PRODUCT]\u001b[0mAn error occurred while updating the product.");
            return StatusCode(500, "An error occurred while updating the product: " + ex.Message);
        }
    }

    [HttpDelete("controller/{id}")]
    [Authorize(Roles = "1, 2")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mDeleting product with ID: {ProductId}", id);
            await _service.Delete(id);
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mProduct with ID: {ProductId} deleted successfully.", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[PRODUCT]\u001b[0mAn error occurred while deleting the product.");
            return StatusCode(500, "An error occurred while deleting the product: " + ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteSelf(int id)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mDeleting self product with ID: {ProductId}", id);
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("\u001b[32m[PRODUCT]\u001b[0mUnauthorized attempt to delete product.");
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            var shop = await _shopService.GetByUserId(userId);
            if (shop == null)
            {
                _logger.LogWarning("\u001b[32m[PRODUCT]\u001b[0mUser ID: {UserId} attempted to delete a product without a shop.", userId);
                return BadRequest("User does not have a shop.");
            }

            var product = await _service.GetById(id, userId);
            if (product == null || product.ShopId != shop.Id)
            {
                _logger.LogWarning("\u001b[32m[PRODUCT]\u001b[0mNo product found for shop ID: {ShopId} when user ID: {UserId} attempted delete.", shop.Id, userId);
                return NotFound("Product not found for this shop");
            }

            await _service.Delete(id);
            _logger.LogInformation("\u001b[32m[PRODUCT]\u001b[0mProduct with ID: {ProductId} deleted successfully by user {UserId}.", id, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[PRODUCT]\u001b[0mAn error occurred while deleting the product.");
            return StatusCode(500, "An error occurred while deleting the product: " + ex.Message);
        }
    }

    [HttpGet("stock/{productId}")]
    public async Task<IActionResult> GetProductStock(int productId)
    {
        try
        {
            var stock = await _service.GetProductStock(productId);
            return Ok(stock);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("\u001b[32m[PRODUCT]\u001b[0mProduct with ID: {ProductId} not found when getting stock.", productId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[PRODUCT]\u001b[0mAn error occurred while getting the product stock.");
            _logger.LogError(ex, "\u001b[32m[PRODUCT]\u001b[0mError: {Message}", ex.Message);
            return StatusCode(500, "An error occurred while getting the product stock: " + ex.Message);
        }
    }
}