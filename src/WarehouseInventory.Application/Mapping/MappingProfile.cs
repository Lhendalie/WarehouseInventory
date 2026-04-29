using AutoMapper;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Domain.Entities;

namespace WarehouseInventory.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();

        CreateMap<ProductStock, ProductStockDto>();

        CreateMap<StockTransaction, StockTransactionDto>();

        CreateMap<Warehouse, WarehouseDto>();

        CreateMap<CreateWarehouseDto, Warehouse>();

        CreateMap<UpdateWarehouseDto, Warehouse>();
    }
}
