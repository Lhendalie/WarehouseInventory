using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WarehouseInventory.Web.Pages.Reports;

namespace WarehouseInventory.UnitTests.PageModels;

public class ReportsModelTests
{
    [Fact]
    public async Task OnGet_ShouldKeepKpiCardsGlobal_WhenFiltersAreApplied()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var model = new ReportsModel(context);

        await model.OnGetAsync("StockLevels", filterWarehouse: "West Storage Facility", filterType: "Tools");

        model.TotalProducts.Should().Be(5);
        model.TotalStock.Should().Be(600);
        model.LowStockItems.Should().Be(1);
        model.ExpiringItems.Should().Be(2);
        model.StockLevels.Should().HaveCount(1);
        model.StockLevels[0].Warehouse.Should().Be("West Storage Facility");
    }

    [Fact]
    public async Task OnGet_ShouldFilterStockMovementsByDateRangeAndType()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var model = new ReportsModel(context);

        await model.OnGetAsync("StockMovements", filterDateRange: "7", filterType: "Tools");

        model.StockMovements.Should().ContainSingle();
        model.StockMovements[0].ProductCode.Should().Be("TOOL001");
    }

    [Fact]
    public async Task OnPostExport_ShouldReturnCsvFile_ForSelectedReport()
    {
        await using var context = await DbContextTestHelper.CreateContextAsync();
        var model = new ReportsModel(context);

        var result = await model.OnPostExportAsync("LowStock") as FileContentResult;

        result.Should().NotBeNull();
        result!.ContentType.Should().Be("text/csv");
        result.FileDownloadName.Should().StartWith("LowStock_Report_");
        System.Text.Encoding.UTF8.GetString(result.FileContents).Should().Contain("Product Code,Product Name,Warehouse,Current Quantity,Status");
    }
}
