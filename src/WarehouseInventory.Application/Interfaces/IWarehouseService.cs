using WarehouseInventory.Application.DTOs;

namespace WarehouseInventory.Application.Interfaces;

public interface IWarehouseService
{
    Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync();
    Task<WarehouseDto?> GetWarehouseByIdAsync(Guid id);
    Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto dto);
    Task<WarehouseDto> UpdateWarehouseAsync(UpdateWarehouseDto dto);
    Task DeleteWarehouseAsync(Guid id);
}
