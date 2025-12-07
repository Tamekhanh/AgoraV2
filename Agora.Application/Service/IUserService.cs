using Agora.Application.Common;
using Agora.Application.DTOs;
using Agora.Domain.Entities;

namespace Agora.Application.Service;

public interface IUserService
{
    Task<PagedResult<UserDTO>> GetPaged(PagedRequest req);
    Task<UserDTO?> GetById(int id);
    Task<UserCreateDTO> Create(UserCreateDTO user);
    Task Update(int id, UserUpdateDTO user);
    Task<UserDTO?> UpdateSelf(int userId, UserUpdateDTO req);
    Task Delete(int id);
    Task<LoginResponse> Login(LoginRequest req);
    Task UpdateRole(int userId, int newRoleId);
    Task UpdateAccount(int userId, UserUpdateAccount req);
    Task UpdateImage(int userId, int imageId);
}
