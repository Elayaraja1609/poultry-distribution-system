namespace PoultryDistributionSystem.Application.DTOs.Report;

/// <summary>
/// Dashboard summary DTO
/// </summary>
public class DashboardSummaryDto
{
    public int TotalSuppliers { get; set; }
    public int TotalFarms { get; set; }
    public int TotalChickens { get; set; }
    public int ChickensInFarms { get; set; }
    public int ChickensReadyForDistribution { get; set; }
    public int TotalVehicles { get; set; }
    public int ActiveDistributions { get; set; }
    public int PendingDeliveries { get; set; }
    public int CompletedDeliveries { get; set; }
    public decimal TotalSales { get; set; }
    public decimal PendingPayments { get; set; }
    public int TotalShops { get; set; }
}
