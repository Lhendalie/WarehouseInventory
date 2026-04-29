using AutoMapper;
using FluentValidation;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Application.Interfaces;
using WarehouseInventory.Application.Validators;
using WarehouseInventory.Domain.Entities;
using WarehouseInventory.Domain.Interfaces;

namespace WarehouseInventory.Application.Services;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CreateProductValidator _validator;
    private readonly IRepository<Warehouse> _warehouseRepository;

    public ProductService(
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CreateProductValidator validator,
        IRepository<Warehouse> warehouseRepository)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
        _warehouseRepository = warehouseRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto?> GetProductByCodeAsync(string productCode)
    {
        var products = await _productRepository.GetAllAsync();
        var product = products.FirstOrDefault(p => p.ProductCode == productCode);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException($"Validation failed: {errors}");
        }

        // Resolve warehouse name to ID
        Guid? warehouseId = null;
        if (!string.IsNullOrWhiteSpace(dto.Warehouse))
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            var warehouse = warehouses.FirstOrDefault(w => w.Name == dto.Warehouse);
            warehouseId = warehouse?.Id;
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            ProductCode = dto.ProductCode,
            Name = dto.ProductName,
            Category = dto.Group,
            Group = dto.Group,
            Barcode = dto.Barcode,
            Catalogue = dto.Catalogue,
            WarehouseId = warehouseId,
            Type = dto.Type,
            Condition = dto.Condition,
            Date = dto.Date == default ? DateTime.UtcNow : dto.Date,
            ExpiryDate = dto.ExpiryDate,
            UnitOfMeasure = "pcs",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateProductAsync(UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(dto.Id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {dto.Id} not found");

        _mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} not found");

        await _productRepository.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();
    }
}
