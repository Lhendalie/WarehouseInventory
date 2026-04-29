using AutoMapper;
using FluentValidation;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Application.Interfaces;
using WarehouseInventory.Application.Validators;
using WarehouseInventory.Domain.Entities;
using WarehouseInventory.Domain.Interfaces;

namespace WarehouseInventory.Application.Services;

public class StockService : IStockService
{
    private readonly IRepository<ProductStock> _stockRepository;
    private readonly IRepository<StockTransaction> _transactionRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CreateStockTransactionValidator _validator;

    public StockService(
        IRepository<ProductStock> stockRepository,
        IRepository<StockTransaction> transactionRepository,
        IRepository<Product> productRepository,
        IRepository<Warehouse> warehouseRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CreateStockTransactionValidator validator)
    {
        _stockRepository = stockRepository;
        _transactionRepository = transactionRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<IEnumerable<ProductStockDto>> GetStockByWarehouseAsync(Guid warehouseId)
    {
        var allStock = await _stockRepository.GetAllAsync();
        var stock = allStock.Where(s => s.WarehouseId == warehouseId);
        return _mapper.Map<IEnumerable<ProductStockDto>>(stock);
    }

    public async Task<IEnumerable<ProductStockDto>> GetStockByProductAsync(Guid productId)
    {
        var allStock = await _stockRepository.GetAllAsync();
        var stock = allStock.Where(s => s.ProductId == productId);
        return _mapper.Map<IEnumerable<ProductStockDto>>(stock);
    }

    public async Task<ProductStockDto?> GetStockAsync(Guid productId, Guid warehouseId, string? batchNumber = null)
    {
        var allStock = await _stockRepository.GetAllAsync();
        var stock = allStock.FirstOrDefault(s => 
            s.ProductId == productId && 
            s.WarehouseId == warehouseId && 
            (batchNumber == null || s.BatchNumber == batchNumber));
        return stock == null ? null : _mapper.Map<ProductStockDto>(stock);
    }

    public async Task<StockTransactionDto> ProcessStockTransactionAsync(CreateStockTransactionDto dto, Guid userId)
    {
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException($"Validation failed: {errors}");
        }

        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {dto.ProductId} not found");

        var warehouse = await _warehouseRepository.GetByIdAsync(dto.WarehouseId);
        if (warehouse == null)
            throw new KeyNotFoundException($"Warehouse with ID {dto.WarehouseId} not found");

        var allStock = await _stockRepository.GetAllAsync();
        var stock = allStock.FirstOrDefault(s => 
            s.ProductId == dto.ProductId && 
            s.WarehouseId == dto.WarehouseId && 
            (dto.BatchNumber == null || s.BatchNumber == dto.BatchNumber));

        decimal previousQuantity = stock?.Quantity ?? 0;
        decimal newQuantity;

        switch (dto.TransactionType)
        {
            case TransactionType.StockIn:
                newQuantity = previousQuantity + dto.Quantity;
                break;
            case TransactionType.StockOut:
                newQuantity = previousQuantity - dto.Quantity;
                if (newQuantity < 0)
                    throw new InvalidOperationException("Insufficient stock for this operation");
                break;
            default:
                throw new NotImplementedException($"Transaction type {dto.TransactionType} not yet implemented");
        }

        if (stock == null)
        {
            stock = new ProductStock
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                Quantity = newQuantity,
                ExpiryDate = dto.ExpiryDate,
                BatchNumber = dto.BatchNumber,
                LastUpdated = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            await _stockRepository.AddAsync(stock);
        }
        else
        {
            stock.Quantity = newQuantity;
            stock.LastUpdated = DateTime.UtcNow;
            stock.UpdatedAt = DateTime.UtcNow;
            stock.UpdatedBy = userId;
            if (dto.ExpiryDate.HasValue)
                stock.ExpiryDate = dto.ExpiryDate;
            await _stockRepository.UpdateAsync(stock);
        }

        var transaction = new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            TransactionType = dto.TransactionType,
            Quantity = dto.Quantity,
            PreviousQuantity = previousQuantity,
            NewQuantity = newQuantity,
            Reason = dto.Reason,
            ReferenceNumber = dto.ReferenceNumber,
            PerformedBy = userId,
            PerformedAt = DateTime.UtcNow,
            BatchNumber = dto.BatchNumber,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<StockTransactionDto>(transaction);
    }

    public async Task<IEnumerable<StockTransactionDto>> GetTransactionHistoryAsync(Guid? productId = null, Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var transactions = await _transactionRepository.GetAllAsync();

        if (productId.HasValue)
            transactions = transactions.Where(t => t.ProductId == productId.Value);
        if (warehouseId.HasValue)
            transactions = transactions.Where(t => t.WarehouseId == warehouseId.Value);
        if (startDate.HasValue)
            transactions = transactions.Where(t => t.PerformedAt >= startDate.Value);
        if (endDate.HasValue)
            transactions = transactions.Where(t => t.PerformedAt <= endDate.Value);

        return _mapper.Map<IEnumerable<StockTransactionDto>>(transactions);
    }
}
