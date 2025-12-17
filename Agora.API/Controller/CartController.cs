using Agora.Application.DTOs;
using Agora.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agora.API.Controllers //  Admin/Staff không thể truy cập giỏ hàng của user khác mà chỉ có thể thao tác trên giỏ hàng của chính mình
{
    [Authorize] 
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _logger = logger;
            _cartService = cartService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return int.Parse(userIdClaim.Value);
        }

        [HttpGet]
        public async Task<ActionResult<CartDTO>> GetCart()
        {
            try
            {
                _logger.LogInformation("\u001b[44;97m[Cart]\u001b[0mGetting cart for user.");
                var userId = GetUserId();
                var cart = await _cartService.GetCart(userId);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError getting cart for user.");
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogError("\u001b[44;97m[Cart]\u001b[0mUnauthorized access when getting cart for user.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError getting cart for user.");
                return StatusCode(500, "An error occurred while retrieving the cart. " + ex.Message);
            }
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("\u001b[44;97m[Cart]\u001b[0mAdding item {ProductId} to cart for user {UserId}.", request.ProductId, userId);
                await _cartService.AddToCart(userId, request);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError adding item {ProductId} to cart for user {UserId}.", request.ProductId, GetUserId());
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogError("\u001b[44;97m[Cart]\u001b[0mUnauthorized access when adding item {ProductId} to cart for user {UserId}.", request.ProductId, GetUserId());
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError adding item {ProductId} to cart for user {UserId}.", request.ProductId, GetUserId());
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving the cart. " + ex.Message);
            }
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("\u001b[44;97m[Cart]\u001b[0mUpdating item {ProductId} in cart for user {UserId}.", request.ProductId, userId);
                await _cartService.UpdateCartItem(userId, request);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError updating item {ProductId} in cart for user {UserId}.", request.ProductId, GetUserId());
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogError("\u001b[44;97m[Cart]\u001b[0mUnauthorized access when updating item {ProductId} in cart for user {UserId}.", request.ProductId, GetUserId());
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError updating item {ProductId} in cart for user {UserId}.", request.ProductId, GetUserId());
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving the cart. " + ex.Message);
            }
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("\u001b[44;97m[Cart]\u001b[0mRemoving item {ProductId} from cart for user {UserId}.", productId, userId);
                await _cartService.RemoveFromCart(userId, productId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError removing item {ProductId} from cart for user {UserId}.", productId, GetUserId());
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogError("\u001b[44;97m[Cart]\u001b[0mUnauthorized access when removing item {ProductId} from cart for user {UserId}.", productId, GetUserId());
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError removing item {ProductId} from cart for user {UserId}.", productId, GetUserId());
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving the cart. " + ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("\u001b[44;97m[Cart]\u001b[0mClearing cart for user {UserId}.", userId);
                await _cartService.ClearCart(userId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError clearing cart for user {UserId}.", GetUserId());
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogError("\u001b[44;97m[Cart]\u001b[0mUnauthorized access when clearing cart for user {UserId}.", GetUserId());
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError clearing cart for user {UserId}.", GetUserId());
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving the cart. " + ex.Message);
            }
        }

        [HttpPost("items/{productId}/increase")]
        public async Task<IActionResult> IncreaseCartItem(int productId)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("\u001b[44;97m[Cart]\u001b[0mIncreasing item {ProductId} in cart for user {UserId}.", productId, userId);
                await _cartService.IncreaseCartItem(userId, productId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError increasing item {ProductId} in cart for user {UserId}.", productId, GetUserId());
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogError("\u001b[44;97m[Cart]\u001b[0mUnauthorized access when increasing item {ProductId} in cart for user {UserId}.", productId, GetUserId());
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError increasing item {ProductId} in cart for user {UserId}.", productId, GetUserId());
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving the cart. " + ex.Message);
            }
        }

        [HttpPost("items/{productId}/decrease")]
        public async Task<IActionResult> DecreaseCartItem(int productId)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("\u001b[44;97m[Cart]\u001b[0mDecreasing item {ProductId} in cart for user {UserId}.", productId, userId);
                await _cartService.DecreaseCartItem(userId, productId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError decreasing item {ProductId} in cart for user {UserId}.", productId, GetUserId());
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogError("\u001b[44;97m[Cart]\u001b[0mUnauthorized access when decreasing item {ProductId} in cart for user {UserId}.", productId, GetUserId());
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError decreasing item {ProductId} in cart for user {UserId}.", productId, GetUserId());
                _logger.LogError(ex, "\u001b[44;97m[Cart]\u001b[0mError: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving the cart. " + ex.Message);
            }
        }
    }
}
