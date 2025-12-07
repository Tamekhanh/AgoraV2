using Agora.Application.Service;
using Agora.Domain.Entities;
using Agora.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Agora.Application.DTOs;

namespace Agora.Application.Service;

public class ImageService : IImageService
{
    private readonly AgoraDbContext _db;

    public ImageService(AgoraDbContext db)
    {
        _db = db;
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

    public async Task<ImageDTO?> GetById(int id)
    {
        var image = await _db.ImageFiles.FindAsync(id);
        if (image == null) return null;

        return new ImageDTO
        {
            Id = image.Id,
            Data = image.Data
        };
    }
}
