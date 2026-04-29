using FluentValidation.TestHelper;
using WarehouseInventory.Application.DTOs;
using WarehouseInventory.Application.Validators;
using WarehouseInventory.Domain.Entities;

namespace WarehouseInventory.UnitTests.Validators;

public class CreateStockTransactionValidatorTests
{
    private readonly CreateStockTransactionValidator _validator;

    public CreateStockTransactionValidatorTests()
    {
        _validator = new CreateStockTransactionValidator();
    }

    [Fact]
    public void Should_Have_Error_When_ProductId_Is_Empty()
    {
        var dto = new CreateStockTransactionDto { ProductId = Guid.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ProductId_Is_Valid()
    {
        var dto = new CreateStockTransactionDto { ProductId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Should_Have_Error_When_WarehouseId_Is_Empty()
    {
        var dto = new CreateStockTransactionDto { WarehouseId = Guid.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.WarehouseId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_WarehouseId_Is_Valid()
    {
        var dto = new CreateStockTransactionDto { WarehouseId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.WarehouseId);
    }

    [Fact]
    public void Should_Have_Error_When_TransactionType_Is_Invalid()
    {
        var dto = new CreateStockTransactionDto { TransactionType = (TransactionType)99 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.TransactionType);
    }

    [Fact]
    public void Should_Not_Have_Error_When_TransactionType_Is_Valid()
    {
        var dto = new CreateStockTransactionDto { TransactionType = TransactionType.StockIn };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.TransactionType);
    }

    [Fact]
    public void Should_Have_Error_When_Quantity_Is_Zero()
    {
        var dto = new CreateStockTransactionDto { Quantity = 0 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Should_Have_Error_When_Quantity_Is_Negative()
    {
        var dto = new CreateStockTransactionDto { Quantity = -1 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Quantity_Is_Positive()
    {
        var dto = new CreateStockTransactionDto { Quantity = 10 };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }
}
