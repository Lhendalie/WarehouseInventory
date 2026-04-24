using WarehouseInventory.Application.DTOs;

namespace WarehouseInventory.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task UpdateUserRoleAsync(Guid userId, string role);
    Task DeactivateUserAsync(Guid id);
}
