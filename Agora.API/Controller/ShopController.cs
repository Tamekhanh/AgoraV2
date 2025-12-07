using Agora.Application.Common;
using Agora.Application.DTOs;
using Agora.Application.Service;
using Agora.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agora.API.Controllers;

[ApiController]
[Route("api/shops")]
public class ShopController : ControllerBase
{
    private readonly IShopService _service;
    private readonly ILogger<ShopController> _logger;

    public ShopController(IShopService service, ILogger<ShopController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ShopRequest>>> GetPaged([FromQuery] PagedRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mRetrieving paged shops with request: {@PagedRequest}", req);
            var result = await _service.GetPaged(req);
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mPaged shops retrieved successfully.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[SHOP]\u001b[0mAn error occurred while retrieving paged shops.");
            return StatusCode(500, "An error occurred while retrieving shops: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShopRequest?>> GetById(int id)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mRetrieving shop with ID: {ShopId}", id);
            var shop = await _service.GetById(id);
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mShop with ID: {ShopId} retrieved successfully.", id);
            return Ok(shop);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[SHOP]\u001b[0mAn error occurred while retrieving the shop with ID: {ShopId}", id);
            return StatusCode(500, "An error occurred while retrieving the shop: " + ex.Message);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CreateShopRequest>> Create(CreateShopRequest req) // Only authenticated user can create their shop
    {
        try
        {
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mCreating a new shop with name: {ShopName}", req.Name);
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("\u001b[32m[SHOP]\u001b[0mUnauthorized attempt to create a shop.");
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mUser ID extracted from token: {UserId}", userId);
            req.UserId = userId;
            var createdShop = await _service.Create(req);
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mShop created successfully with ID: {ShopId}", createdShop.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdShop.Id }, createdShop);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[SHOP]\u001b[0mAn error occurred while creating a new shop.");
            return StatusCode(500, "An error occurred while creating the shop: " + ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "1, 2")]
    public async Task<IActionResult> Update(int id, ShopUpdateRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mUpdating shop with ID: {ShopId}", id);
            req.Id = id;
            await _service.Update(req);
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mShop with ID: {ShopId} updated successfully.", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[SHOP]\u001b[0mAn error occurred while updating the shop.");
            return StatusCode(500, "An error occurred while updating the shop: " + ex.Message);
        }
    }

    [Authorize]
    [HttpPut("self")]
    public async Task<IActionResult> UpdateSelf([FromBody] ShopUpdateRequest req)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mUpdating self shop information.");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("\u001b[32m[SHOP]\u001b[0mUnauthorized attempt to update shop.");
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            var shop = await _service.GetByUserId(userId);
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mUpdating shop ID: {ShopId} for user ID: {UserId}", shop?.Id, userId);

            if (shop == null)
            {
                return NotFound();
            }

            // Allow update if user is owner OR if shop has no owner (legacy data fix)
            if (shop.UserId != null && shop.UserId != userId)
            {
                _logger.LogWarning("\u001b[32m[SHOP]\u001b[0mUser {UserId} attempted to update a shop they do not own.", userId);
                // _logger.LogInformation("Shop owner ID: {ShopOwnerId}, Requesting user ID: {UserId}", shop?.UserId, userId);
                return Forbid();
            }

            // Pass userId to service to claim ownership if needed
            // req.UserId = userId;
            req.Id = shop.Id;

            await _service.Update(req);
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mShop {ShopId} updated successfully by user {UserId}.", shop.Id, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[SHOP]\u001b[0mAn error occurred while updating the shop.");
            return StatusCode(500, "An error occurred while updating the shop: " + ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "1, 2")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mDeleting shop with ID: {ShopId}", id);
            await _service.Delete(id);
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mShop with ID: {ShopId} deleted successfully.", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[SHOP]\u001b[0mAn error occurred while deleting the shop.");
            return StatusCode(500, "An error occurred while deleting the shop: " + ex.Message);
        }
    }

    [HttpPut("{shopId}/status/{status}")]
    [Authorize(Roles = "1, 2")]
    public async Task<IActionResult> ChangeStatus(int shopId, int status) // -1: banned, 0: normal, 1: verified , 2: closed
    {
        try
        {
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mChanging status of shop ID: {ShopId} to status: {Status}", shopId, status);
            await _service.ChangeStatus(shopId, status);
            _logger.LogInformation("\u001b[32m[SHOP]\u001b[0mStatus of shop ID: {ShopId} changed successfully to status: {Status}", shopId, status);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "\u001b[32m[SHOP]\u001b[0mInvalid status value provided for shop ID: {ShopId}", shopId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u001b[32m[SHOP]\u001b[0mAn error occurred while changing the status of the shop.");
            return StatusCode(500, "An error occurred while changing the shop status: " + ex.Message);
        }
    }
}