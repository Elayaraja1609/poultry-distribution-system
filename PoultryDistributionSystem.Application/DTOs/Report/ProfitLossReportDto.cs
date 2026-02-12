namespace PoultryDistributionSystem.Application.DTOs.Report;

/// <summary>
/// Profit & Loss report DTO
/// </summary>
public class ProfitLossReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public RevenueBreakdownDto RevenueBreakdown { get; set; } = new();
    public ExpenseBreakdownDto ExpenseBreakdown { get; set; } = new();
}

/// <summary>
/// Revenue breakdown DTO
/// </summary>
public class RevenueBreakdownDto
{
    public decimal TotalSales { get; set; }
    public int TotalSalesCount { get; set; }
    public Dictionary<string, decimal> SalesByShop { get; set; } = new();
    public Dictionary<string, decimal> SalesByMonth { get; set; } = new();
}

/// <summary>
/// Expense breakdown DTO
/// </summary>
public class ExpenseBreakdownDto
{
    public decimal TotalExpenses { get; set; }
    public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
    public Dictionary<string, decimal> ExpensesByMonth { get; set; } = new();
}
