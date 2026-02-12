using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Analytics;
using PoultryDistributionSystem.Application.DTOs.Report;
using PoultryDistributionSystem.Application.Interfaces;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Reports controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    /// <summary>
    /// Get dashboard summary
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var result = await _reportService.GetDashboardSummaryAsync(cancellationToken);
        return Ok(ApiResponse<DashboardSummaryDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get sales report
    /// </summary>
    [HttpGet("sales")]
    [ProducesResponseType(typeof(ApiResponse<SalesReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SalesReportDto>>> GetSalesReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetSalesReportAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<SalesReportDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get profit & loss report
    /// </summary>
    [HttpGet("profit-loss")]
    [ProducesResponseType(typeof(ApiResponse<ProfitLossReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProfitLossReportDto>>> GetProfitLossReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetProfitLossReportAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<ProfitLossReportDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get revenue breakdown
    /// </summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(ApiResponse<RevenueBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RevenueBreakdownDto>>> GetRevenueBreakdown(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetRevenueByPeriodAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<RevenueBreakdownDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get expense breakdown
    /// </summary>
    [HttpGet("expenses")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ExpenseBreakdownDto>>> GetExpenseBreakdown(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetExpenseBreakdownAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<ExpenseBreakdownDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get sales trends
    /// </summary>
    [HttpGet("sales-trends")]
    [ProducesResponseType(typeof(ApiResponse<List<SalesTrendDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<SalesTrendDto>>>> GetSalesTrends(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetSalesTrendsAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<List<SalesTrendDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get customer analytics
    /// </summary>
    [HttpGet("customer-analytics")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerAnalyticsDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CustomerAnalyticsDto>>>> GetCustomerAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetCustomerAnalyticsAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<List<CustomerAnalyticsDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get inventory analytics
    /// </summary>
    [HttpGet("inventory-analytics")]
    [ProducesResponseType(typeof(ApiResponse<List<InventoryAnalyticsDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<InventoryAnalyticsDto>>>> GetInventoryAnalytics(CancellationToken cancellationToken)
    {
        var result = await _reportService.GetInventoryAnalyticsAsync(cancellationToken);
        return Ok(ApiResponse<List<InventoryAnalyticsDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get performance metrics
    /// </summary>
    [HttpGet("performance-metrics")]
    [ProducesResponseType(typeof(ApiResponse<PerformanceMetricsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PerformanceMetricsDto>>> GetPerformanceMetrics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetPerformanceMetricsAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<PerformanceMetricsDto>.SuccessResponse(result));
    }
}
