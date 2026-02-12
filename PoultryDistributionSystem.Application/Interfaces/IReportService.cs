using PoultryDistributionSystem.Application.DTOs.Analytics;
using PoultryDistributionSystem.Application.DTOs.Report;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Report service interface
/// </summary>
public interface IReportService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<SalesReportDto> GetSalesReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<ProfitLossReportDto> GetProfitLossReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<RevenueBreakdownDto> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ExpenseBreakdownDto> GetExpenseBreakdownAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<SalesTrendDto>> GetSalesTrendsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<CustomerAnalyticsDto>> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<InventoryAnalyticsDto>> GetInventoryAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
