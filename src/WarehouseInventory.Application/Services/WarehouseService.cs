using AutoMapper;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Application.Interfaces;
using WarehouseInventory.Domain.Entities;
using WarehouseInventory.Domain.Interfaces;

namespace WarehouseInventory.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WarehouseService(IRepository<Warehouse> warehouseRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
    {
        var warehouses = await _warehouseRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }

    public async Task<WarehouseDto?> GetWarehouseByIdAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        return warehouse == null ? null : _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto dto)
    {
        var warehouse = _mapper.Map<Warehouse>(dto);
        warehouse.Id = Guid.NewGuid();
        warehouse.CreatedAt = DateTime.UtcNow;
        await _warehouseRepository.AddAsync(warehouse);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> UpdateWarehouseAsync(UpdateWarehouseDto dto)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(dto.Id);
        if (warehouse == null)
            throw new KeyNotFoundException($"Warehouse with ID {dto.Id} not found");

        _mapper.Map(dto, warehouse);
        warehouse.UpdatedAt = DateTime.UtcNow;
        await _warehouseRepository.UpdateAsync(warehouse);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task DeleteWarehouseAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null)
            throw new KeyNotFoundException($"Warehouse with ID {id} not found");

        await _warehouseRepository.DeleteAsync(warehouse);
        await _unitOfWork.SaveChangesAsync();
    }
}
