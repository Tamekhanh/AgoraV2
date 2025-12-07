
using System.Threading.Tasks;
using Agora.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Agora.Application.Service;

public interface IImageService
{
    Task<int> Create(IFormFile file);
    Task Delete(int id);
    Task UpdateUserImage(int userId, IFormFile file);
    Task UpdateShopImage(int shopId, IFormFile file);

    Task<ImageDTO?> GetById(int id);
}
