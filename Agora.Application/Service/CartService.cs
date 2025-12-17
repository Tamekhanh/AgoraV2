using Agora.Application.DTOs;
using Agora.Domain.Entities;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Agora.Application.Service
{
    public class CartService : ICartService
    {
        private readonly AgoraDbContext _db;
        private readonly ILogger<CartService> _logger;
        private readonly IProductService _productService;

        public CartService(AgoraDbContext db, ILogger<CartService> logger, IProductService productService)
        {
            _db = db;
            _logger = logger;
            _productService = productService;
        }

        public async Task<CartDTO> GetCartAsync(int userId)
        {
            try
            {
                var cart = await _db.Carts
                    .Include(c => c.CartItems!)
                    .ThenInclude(ci => ci.Product)
                    // .ThenInclude(p => p.ImageFile)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    return new CartDTO
                    {
                        UserId = userId,
                        Items = new List<CartItemDTO>()
                    };
                }

                var cartDto = new CartDTO
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    Items = cart.CartItems?.Select(ci =>
                    {
                        var product = ci.Product;
                        var price = product?.RetailPrice ?? 0;
                        var discountPercent = product?.DiscountPercent ?? 0;
                        var discountAmount = price * discountPercent / 100m;
                        var quantity = ci.Quantity ?? 0;

                        return new CartItemDTO
                        {
                            Id = ci.Id,
                            ProductId = ci.ProductId ?? 0,
                            ProductName = product?.Name ?? "Unknown",
                            Quantity = quantity,
                            Price = price,
                            Discount = discountAmount,
                            TotalPrice = (price - discountAmount) * quantity,
                            ImageUrl = (product?.ImageFile != null && product.ImageFile.Data != null) ? Convert.ToBase64String(product.ImageFile.Data) : null
                        };
                    }).ToList() ?? new List<CartItemDTO>()
                };

                cartDto.TotalItems = cartDto.Items.Sum(i => i.Quantity);
                cartDto.TotalPrice = cartDto.Items.Sum(i => i.Price * i.Quantity);
                cartDto.TotalDiscount = cartDto.Items.Sum(i => i.Discount * i.Quantity);
                cartDto.FinalPrice = cartDto.Items.Sum(i => i.TotalPrice);

                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}.", userId);
                throw;
            }
        }

        public async Task AddToCartAsync(int userId, AddToCartRequest request)
        {
            try
            {
                var cart = await _db.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _db.Carts.Add(cart);
                    await _db.SaveChangesAsync();
                }

                var cartItem = await _db.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == request.ProductId);

                if (cartItem != null)
                {
                    if (await _productService.GetProductStock(request.ProductId) < ((cartItem.Quantity ?? 0) + request.Quantity))
                    {
                        throw new InvalidOperationException("Insufficient stock for the requested product quantity.");
                    }
                    cartItem.Quantity = (cartItem.Quantity ?? 0) + request.Quantity;
                }
                else
                {
                    if (await _productService.GetProductStock(request.ProductId) < request.Quantity)
                    {
                        throw new InvalidOperationException("Insufficient stock for the requested product quantity.");
                    }
                    cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        Status = 1 // Active
                    };
                    _db.CartItems.Add(cartItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}.", userId);
                throw;
            }
        }

        public async Task UpdateCartItemAsync(int userId, UpdateCartItemRequest request)
        {
            try
            {
                var cart = await _db.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null) return;

                var cartItem = cart.CartItems?.FirstOrDefault(ci => ci.ProductId == request.ProductId);

                if (cartItem != null)
                {
                    if (request.Quantity <= 0)
                    {
                        _db.CartItems.Remove(cartItem);
                    }
                    else
                    {
                        if (await _productService.GetProductStock(request.ProductId) < request.Quantity)
                        {
                            throw new InvalidOperationException("Insufficient stock for the requested product quantity.");
                        }
                        cartItem.Quantity = request.Quantity;
                    }
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}.", userId);
                throw;
            }
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            try
            {
                var cart = await _db.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null) return;

                var cartItem = cart.CartItems?.FirstOrDefault(ci => ci.ProductId == productId);

                if (cartItem != null)
                {
                    _db.CartItems.Remove(cartItem);
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}.", userId);
                throw;
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            try
            {
                var cart = await _db.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.CartItems == null) return;

                _db.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}.", userId);
                throw;
            }
        }

        public async Task IncreaseCartItem(int userId, int productId)
        {
            try
            {
                var cart = await _db.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null) return;

                var cartItem = cart.CartItems?.FirstOrDefault(ci => ci.ProductId == productId);

                if (cartItem != null)
                {
                    if (await _productService.GetProductStock(productId) < ((cartItem.Quantity ?? 0) + 1))
                    {
                        throw new InvalidOperationException("Insufficient stock for the requested product quantity.");
                    }
                    cartItem.Quantity = (cartItem.Quantity ?? 0) + 1;
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error increasing cart item {ProductId} for user {UserId}.", productId, userId);
                throw;
            }
        }

        public async Task DecreaseCartItem(int userId, int productId)
        {
            try
            {
                var cart = await _db.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null) return;

                var cartItem = cart.CartItems?.FirstOrDefault(ci => ci.ProductId == productId);

                if (cartItem != null)
                {
                    if ((cartItem.Quantity ?? 0) - 1 <= 0)
                    {
                        _db.CartItems.Remove(cartItem);
                    }
                    else
                    {
                        cartItem.Quantity = (cartItem.Quantity ?? 0) - 1;
                    }
                    cart.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decreasing cart item {ProductId} for user {UserId}.", productId, userId);
                throw;
            }
        }
    }
}
