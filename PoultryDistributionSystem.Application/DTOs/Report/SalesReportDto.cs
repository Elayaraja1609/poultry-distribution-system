namespace PoultryDistributionSystem.Application.DTOs.Report;

/// <summary>
/// Sales report DTO
/// </summary>
public class SalesReportDto
{
    public DateTime ReportDate { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public List<SalesReportItemDto> SalesByShop { get; set; } = new();
    public List<SalesReportItemDto> SalesByDate { get; set; } = new();
}

/// <summary>
/// Sales report item DTO
/// </summary>
public class SalesReportItemDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}
