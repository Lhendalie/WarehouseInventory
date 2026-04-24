using WarehouseInventory.Application.DTOs;

namespace WarehouseInventory.Application.Interfaces;

public interface IStockService
{
    Task<IEnumerable<ProductStockDto>> GetStockByWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<ProductStockDto>> GetStockByProductAsync(Guid productId);
    Task<ProductStockDto?> GetStockAsync(Guid productId, Guid warehouseId, string? batchNumber = null);
    Task<StockTransactionDto> ProcessStockTransactionAsync(CreateStockTransactionDto dto, Guid userId);
    Task<IEnumerable<StockTransactionDto>> GetTransactionHistoryAsync(Guid? productId = null, Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null);
}
