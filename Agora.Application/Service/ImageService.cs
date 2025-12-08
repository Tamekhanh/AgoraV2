using Agora.Application.Service;
using Agora.Domain.Entities;
using Agora.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Agora.Application.DTOs;
using Microsoft.Extensions.Logging;
namespace Agora.Application.Service;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

public class ImageService : IImageService
{
    private readonly AgoraDbContext _db;
    private readonly ILogger<ImageService> _logger;

    public ImageService(AgoraDbContext db, ILogger<ImageService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> Create(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var image = new ImageFile
        {
            Data = memoryStream.ToArray()
        };

        _db.ImageFiles.Add(image);
        await _db.SaveChangesAsync();

        return image.Id;
    }

    public async Task Delete(int id)
    {
        var image = await _db.ImageFiles.FindAsync(id);
        if (image != null)
        {
            _db.ImageFiles.Remove(image);
            await _db.SaveChangesAsync();
        }
    }

    public async Task UpdateUserImage(int userId, IFormFile file)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");

        var oldImageId = user.ImageId;
        var newImageId = await Create(file);

        user.ImageId = newImageId;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        if (oldImageId.HasValue)
        {
            await Delete(oldImageId.Value);
        }
    }

    public async Task UpdateShopImage(int shopId, IFormFile file)
    {
        var shop = await _db.Shops.FindAsync(shopId);
        if (shop == null) throw new Exception("Shop not found");

        var oldImageId = shop.ImageId;
        var newImageId = await Create(file);

        shop.ImageId = newImageId;
        _db.Shops.Update(shop);
        await _db.SaveChangesAsync();

        if (oldImageId.HasValue)
        {
            await Delete(oldImageId.Value);
        }
    }

    public async Task UpdateProductImage(int productId, IFormFile file)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) throw new Exception("Product not found");

        var oldImageId = product.ImageId;
        var newImageId = await Create(file);

        product.ImageId = newImageId;
        _db.Products.Update(product);
        await _db.SaveChangesAsync();

        if (oldImageId.HasValue)
        {
            await Delete(oldImageId.Value);
        }
    }

    public async Task<ImageDTO?> GetById(int id, bool? ReSize = false, bool? isSmall = null, int? width = null, int? height = null)
    {
        var image = await _db.ImageFiles.FindAsync(id);
        if (image == null) return null;

        if (image.Data == null || image.Data.Length == 0)
        {
            _logger.LogWarning("Image with ID {ImageId} has no data.", id);
            return null;
        }

        if(ReSize == true)
        {
            return new ImageDTO
            {
                Id = image.Id,
                Data = ResizeIfSmall(image.Data, image.Id, isSmall, width, height)
            };
        }
        else
        {
            return new ImageDTO
            {
                Id = image.Id,
                Data = image.Data
            };
        }
    }

    ///
    ///BẢN QUYỀN THUỘC VỀ SAFxTHP :V
    ///     
    private byte[] ResizeIfSmall(byte[] imageData, int id, bool? isSmall, int? width, int? height)
    {
        _logger.LogInformation($"\u001b[46mTrigger Resize ID = {id}\u001b[0m");
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageData);
        if (isSmall == true)
        {
            int targetWidth = width ?? 150;
            if (image.Width != targetWidth) // chỉ resize nếu khác
            {
                double scale = (double)targetWidth / image.Width;
                int targetHeight = (int)(image.Height * scale);
                _logger.LogInformation($"\u001b[46mResize SMALL ID={id}:\u001b[0m {image.Width}x{image.Height} to {targetWidth}x{targetHeight}");
                image.Mutate(x => x.Resize(targetWidth, targetHeight));
            }
        }
        else if (width.HasValue || height.HasValue)
        {
            double scale = 1.0;
            int newWidth = image.Width;
            int newHeight = image.Height;

            // Case 1: Có cả width và height
            if (width.HasValue && height.HasValue)
            {
                if (width.Value >= 1500 || height.Value >= 1500)
                {
                    _logger.LogInformation($"\u001b[46mResize LARGE ID={id}:\u001b[0m {image.Width}x{image.Height} → {width}x{height}");
                    return imageData; // Không resize nếu là ảnh lớn
                }
            }

            // Case 2: Chỉ có width
            else if (width.HasValue && width.Value >= 1500)
            {
                _logger.LogInformation($"\u001b[46mResize LARGE-WIDTH ID={id}:\u001b[0m {image.Width}x{image.Height} → Width={width}");
                return imageData;
            }

            // Case 3: Chỉ có height
            else if (height.HasValue && height.Value >= 1500)
            {
                _logger.LogInformation($"\u001b[46mResize LARGE-HEIGHT ID={id}:\u001b[0m {image.Width}x{image.Height} → Height={height}");
                return imageData;
            }


            if (width.HasValue && height.HasValue)
            {
                newWidth = width.Value;
                newHeight = height.Value;
            }
            else if (width.HasValue || height.HasValue)
            {
                if (width.HasValue)
                {
                    scale = (double)width.Value / image.Width;
                }
                else
                {
                    scale = (double)height!.Value / image.Height;
                }
                newWidth = (int)(image.Width * scale);
                newHeight = (int)(image.Height * scale);
            }

            newWidth = (int)(image.Width * scale);
            newHeight = (int)(image.Height * scale);

            if (newWidth != image.Width || newHeight != image.Height)
            {
                _logger.LogInformation($"\u001b[46mResize CUSTOM ID={id}:\u001b[0m {image.Width}x{image.Height} → {newWidth}x{newHeight}");
                image.Mutate(x => x.Resize(newWidth, newHeight));
            }
        }
        else
        {
            int minWidth = 850, minHeight = 850;
            if (image.Width < minWidth || image.Height < minHeight)
            {
                double widthRatio = (double)minWidth / image.Width;
                double heightRatio = (double)minHeight / image.Height;
                double scale = Math.Max(widthRatio, heightRatio);
                int newWidth = (int)(image.Width * scale);
                int newHeight = (int)(image.Height * scale);

                _logger.LogInformation($"\u001b[46mResize DEFAULT ID={id}:\u001b[0m {image.Width}x{image.Height} → {newWidth}x{newHeight}");
                image.Mutate(x => x.Resize(newWidth, newHeight));
            }
            else
            {
                return imageData;
            }
        }

        using var ms = new MemoryStream();
        image.Save(ms, new WebpEncoder { Quality = 100 });
        return ms.ToArray();
    }
    ///
    ///BẢN QUYỀN THUỘC VỀ SAFxTHP :V
    ///     
}
