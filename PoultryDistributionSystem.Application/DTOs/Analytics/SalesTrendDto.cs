namespace PoultryDistributionSystem.Application.DTOs.Analytics;

/// <summary>
/// Sales trend DTO
/// </summary>
public class SalesTrendDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Customer analytics DTO
/// </summary>
public class CustomerAnalyticsDto
{
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime LastOrderDate { get; set; }
    public int DaysSinceLastOrder { get; set; }
}

/// <summary>
/// Inventory analytics DTO
/// </summary>
public class InventoryAnalyticsDto
{
    public Guid FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int Capacity { get; set; }
    public double UtilizationRate { get; set; }
    public int StockIn { get; set; }
    public int StockOut { get; set; }
    public int StockLoss { get; set; }
    public double TurnoverRate { get; set; }
}

/// <summary>
/// Performance metrics DTO
/// </summary>
public class PerformanceMetricsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public double ProfitMargin { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedDeliveries { get; set; }
    public double DeliverySuccessRate { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int ActiveCustomers { get; set; }
    public double CustomerRetentionRate { get; set; }
    public Dictionary<string, decimal> RevenueByCategory { get; set; } = new();
    public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
}
