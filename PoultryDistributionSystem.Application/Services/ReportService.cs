using PoultryDistributionSystem.Application.DTOs.Analytics;
using PoultryDistributionSystem.Application.DTOs.Report;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Enums;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Report service implementation
/// </summary>
public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var suppliers = await _unitOfWork.Suppliers.FindAsync(s => !s.IsDeleted, cancellationToken);
        var farms = await _unitOfWork.Farms.FindAsync(f => !f.IsDeleted, cancellationToken);
        var chickens = await _unitOfWork.Chickens.FindAsync(c => !c.IsDeleted, cancellationToken);
        var vehicles = await _unitOfWork.Vehicles.FindAsync(v => !v.IsDeleted, cancellationToken);
        var distributions = await _unitOfWork.Distributions.FindAsync(d => !d.IsDeleted, cancellationToken);
        var deliveries = await _unitOfWork.Deliveries.FindAsync(d => !d.IsDeleted, cancellationToken);
        var sales = await _unitOfWork.Sales.FindAsync(s => !s.IsDeleted, cancellationToken);
        var shops = await _unitOfWork.Shops.FindAsync(s => !s.IsDeleted, cancellationToken);

        var summary = new DashboardSummaryDto
        {
            TotalSuppliers = suppliers.Count(),
            TotalFarms = farms.Count(),
            TotalChickens = chickens.Count(),
            ChickensInFarms = chickens.Count(c => c.Status == ChickenStatus.InFarm),
            ChickensReadyForDistribution = chickens.Count(c => c.Status == ChickenStatus.ReadyForDistribution),
            TotalVehicles = vehicles.Count(),
            ActiveDistributions = distributions.Count(d => d.Status == DistributionStatus.Scheduled || d.Status == DistributionStatus.InTransit),
            PendingDeliveries = deliveries.Count(d => d.DeliveryStatus == DeliveryStatus.Pending),
            CompletedDeliveries = deliveries.Count(d => d.DeliveryStatus == DeliveryStatus.Completed),
            TotalShops = shops.Count()
        };

        // Calculate sales totals
        summary.TotalSales = sales.Sum(s => s.TotalAmount);

        var pendingSales = sales.Where(s => s.PaymentStatus == PaymentStatus.Pending || s.PaymentStatus == PaymentStatus.Partial).ToList();
        foreach (var sale in pendingSales)
        {
            var payments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == sale.Id, cancellationToken);
            summary.PendingPayments += sale.TotalAmount - payments.Sum(p => p.Amount);
        }

        return summary;
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var allSales = await _unitOfWork.Sales.FindAsync(s => !s.IsDeleted && s.SaleDate >= start && s.SaleDate <= end, cancellationToken);
        var salesList = allSales.ToList();

        var report = new SalesReportDto
        {
            ReportDate = DateTime.UtcNow,
            TotalSales = salesList.Count,
            TotalAmount = salesList.Sum(s => s.TotalAmount)
        };

        // Calculate paid and pending amounts
        decimal totalPaid = 0;
        foreach (var sale in salesList)
        {
            var payments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == sale.Id, cancellationToken);
            totalPaid += payments.Sum(p => p.Amount);
        }

        report.PaidAmount = totalPaid;
        report.PendingAmount = report.TotalAmount - totalPaid;

        // Sales by shop
        var shopGroups = salesList.GroupBy(s => s.ShopId).ToList();
        report.SalesByShop = new List<SalesReportItemDto>();

        foreach (var group in shopGroups)
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(group.Key, cancellationToken);
            report.SalesByShop.Add(new SalesReportItemDto
            {
                Label = shop?.Name ?? "Unknown",
                Count = group.Count(),
                Amount = group.Sum(s => s.TotalAmount)
            });
        }

        report.SalesByShop = report.SalesByShop.OrderByDescending(x => x.Amount).ToList();

        // Sales by date
        report.SalesByDate = salesList
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new SalesReportItemDto
            {
                Label = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count(),
                Amount = g.Sum(s => s.TotalAmount)
            })
            .OrderBy(x => x.Label)
            .ToList();

        return report;
    }

    public async Task<ProfitLossReportDto> GetProfitLossReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        // Get revenue (from sales)
        var sales = await _unitOfWork.Sales.FindAsync(
            s => !s.IsDeleted && s.SaleDate >= startDate && s.SaleDate <= endDate,
            cancellationToken);
        var salesList = sales.ToList();
        var totalRevenue = salesList.Sum(s => s.TotalAmount);

        // Get expenses
        var expenses = await _unitOfWork.Expenses.FindAsync(
            e => !e.IsDeleted && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate,
            cancellationToken);
        var expensesList = expenses.ToList();
        var totalExpenses = expensesList.Sum(e => e.Amount);

        var grossProfit = totalRevenue - totalExpenses;
        var netProfit = grossProfit; // Assuming no other costs for now
        var profitMargin = totalRevenue > 0 ? (decimal)((double)(netProfit / totalRevenue) * 100) : 0m;

        // Revenue breakdown
        var revenueBreakdown = new RevenueBreakdownDto
        {
            TotalSales = totalRevenue,
            TotalSalesCount = salesList.Count
        };

        // Sales by shop
        var shopGroups = salesList.GroupBy(s => s.ShopId).ToList();
        foreach (var group in shopGroups)
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(group.Key, cancellationToken);
            if (shop != null)
            {
                revenueBreakdown.SalesByShop[shop.Name] = group.Sum(s => s.TotalAmount);
            }
        }

        // Sales by month
        var monthGroups = salesList.GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month }).ToList();
        foreach (var group in monthGroups)
        {
            var monthKey = $"{group.Key.Year}-{group.Key.Month:D2}";
            revenueBreakdown.SalesByMonth[monthKey] = group.Sum(s => s.TotalAmount);
        }

        // Expense breakdown
        var expenseBreakdown = new ExpenseBreakdownDto
        {
            TotalExpenses = totalExpenses
        };

        // Expenses by category
        var categoryGroups = expensesList.GroupBy(e => e.Category).ToList();
        foreach (var group in categoryGroups)
        {
            expenseBreakdown.ExpensesByCategory[group.Key] = group.Sum(e => e.Amount);
        }

        // Expenses by month
        var expenseMonthGroups = expensesList.GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month }).ToList();
        foreach (var group in expenseMonthGroups)
        {
            var monthKey = $"{group.Key.Year}-{group.Key.Month:D2}";
            expenseBreakdown.ExpensesByMonth[monthKey] = group.Sum(e => e.Amount);
        }

        return new ProfitLossReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            GrossProfit = grossProfit,
            NetProfit = netProfit,
            ProfitMargin = profitMargin,
            RevenueBreakdown = revenueBreakdown,
            ExpenseBreakdown = expenseBreakdown
        };
    }

    public async Task<RevenueBreakdownDto> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var sales = await _unitOfWork.Sales.FindAsync(
            s => !s.IsDeleted && s.SaleDate >= startDate && s.SaleDate <= endDate,
            cancellationToken);
        var salesList = sales.ToList();

        var breakdown = new RevenueBreakdownDto
        {
            TotalSales = salesList.Sum(s => s.TotalAmount),
            TotalSalesCount = salesList.Count
        };

        // Sales by shop
        var shopGroups = salesList.GroupBy(s => s.ShopId).ToList();
        foreach (var group in shopGroups)
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(group.Key, cancellationToken);
            if (shop != null)
            {
                breakdown.SalesByShop[shop.Name] = group.Sum(s => s.TotalAmount);
            }
        }

        // Sales by month
        var monthGroups = salesList.GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month }).ToList();
        foreach (var group in monthGroups)
        {
            var monthKey = $"{group.Key.Year}-{group.Key.Month:D2}";
            breakdown.SalesByMonth[monthKey] = group.Sum(s => s.TotalAmount);
        }

        return breakdown;
    }

    public async Task<ExpenseBreakdownDto> GetExpenseBreakdownAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var expenses = await _unitOfWork.Expenses.FindAsync(
            e => !e.IsDeleted && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate,
            cancellationToken);
        var expensesList = expenses.ToList();

        var breakdown = new ExpenseBreakdownDto
        {
            TotalExpenses = expensesList.Sum(e => e.Amount)
        };

        // Expenses by category
        var categoryGroups = expensesList.GroupBy(e => e.Category).ToList();
        foreach (var group in categoryGroups)
        {
            breakdown.ExpensesByCategory[group.Key] = group.Sum(e => e.Amount);
        }

        // Expenses by month
        var monthGroups = expensesList.GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month }).ToList();
        foreach (var group in monthGroups)
        {
            var monthKey = $"{group.Key.Year}-{group.Key.Month:D2}";
            breakdown.ExpensesByMonth[monthKey] = group.Sum(e => e.Amount);
        }

        return breakdown;
    }

    public async Task<List<SalesTrendDto>> GetSalesTrendsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var sales = await _unitOfWork.Sales.FindAsync(
            s => !s.IsDeleted && s.SaleDate >= startDate && s.SaleDate <= endDate,
            cancellationToken);
        var salesList = sales.ToList();

        var trends = salesList
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new SalesTrendDto
            {
                Date = g.Key,
                Amount = g.Sum(s => s.TotalAmount),
                Count = g.Count()
            })
            .OrderBy(t => t.Date)
            .ToList();

        return trends;
    }

    public async Task<List<CustomerAnalyticsDto>> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var sales = await _unitOfWork.Sales.FindAsync(
            s => !s.IsDeleted && s.SaleDate >= startDate && s.SaleDate <= endDate,
            cancellationToken);
        var salesList = sales.ToList();

        var shopGroups = salesList.GroupBy(s => s.ShopId).ToList();
        var analytics = new List<CustomerAnalyticsDto>();

        foreach (var group in shopGroups)
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(group.Key, cancellationToken);
            if (shop == null) continue;

            var shopSales = group.ToList();
            var totalSpent = shopSales.Sum(s => s.TotalAmount);
            var totalOrders = shopSales.Count;
            var averageOrderValue = totalOrders > 0 ? totalSpent / totalOrders : 0;
            var lastOrderDate = shopSales.Max(s => s.SaleDate);
            var daysSinceLastOrder = (DateTime.UtcNow - lastOrderDate).Days;

            analytics.Add(new CustomerAnalyticsDto
            {
                ShopId = shop.Id,
                ShopName = shop.Name,
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                AverageOrderValue = averageOrderValue,
                LastOrderDate = lastOrderDate,
                DaysSinceLastOrder = daysSinceLastOrder
            });
        }

        return analytics.OrderByDescending(a => a.TotalSpent).ToList();
    }

    public async Task<List<InventoryAnalyticsDto>> GetInventoryAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var farms = await _unitOfWork.Farms.FindAsync(f => !f.IsDeleted, cancellationToken);
        var analytics = new List<InventoryAnalyticsDto>();

        foreach (var farm in farms)
        {
            var stockMovements = await _unitOfWork.StockMovements.FindAsync(
                sm => sm.FarmId == farm.Id && !sm.IsDeleted,
                cancellationToken);
            var movements = stockMovements.ToList();

            var stockIn = movements.Where(m => m.MovementType == Domain.Enums.StockMovementType.In).Sum(m => m.Quantity);
            var stockOut = movements.Where(m => m.MovementType == Domain.Enums.StockMovementType.Out).Sum(m => m.Quantity);
            var stockLoss = movements.Where(m => m.MovementType == Domain.Enums.StockMovementType.Loss).Sum(m => m.Quantity);

            var utilizationRate = farm.Capacity > 0 ? (farm.CurrentCount / (double)farm.Capacity) * 100 : 0;
            var turnoverRate = farm.Capacity > 0 ? (stockOut / (double)farm.Capacity) * 100 : 0;

            analytics.Add(new InventoryAnalyticsDto
            {
                FarmId = farm.Id,
                FarmName = farm.Name,
                CurrentStock = farm.CurrentCount,
                Capacity = farm.Capacity,
                UtilizationRate = utilizationRate,
                StockIn = stockIn,
                StockOut = stockOut,
                StockLoss = stockLoss,
                TurnoverRate = turnoverRate
            });
        }

        return analytics;
    }

    public async Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var sales = await _unitOfWork.Sales.FindAsync(
            s => !s.IsDeleted && s.SaleDate >= startDate && s.SaleDate <= endDate,
            cancellationToken);
        var salesList = sales.ToList();

        var expenses = await _unitOfWork.Expenses.FindAsync(
            e => !e.IsDeleted && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate,
            cancellationToken);
        var expensesList = expenses.ToList();

        var deliveries = await _unitOfWork.Deliveries.FindAsync(
            d => !d.IsDeleted && d.DeliveryDate >= startDate && d.DeliveryDate <= endDate,
            cancellationToken);
        var deliveriesList = deliveries.ToList();

        var orders = await _unitOfWork.Orders.FindAsync(
            o => !o.IsDeleted && o.OrderDate >= startDate && o.OrderDate <= endDate,
            cancellationToken);
        var ordersList = orders.ToList();

        var totalRevenue = salesList.Sum(s => s.TotalAmount);
        var totalExpenses = expensesList.Sum(e => e.Amount);
        var netProfit = totalRevenue - totalExpenses;
        var profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;

        var completedDeliveries = deliveriesList.Count(d => d.DeliveryStatus == Domain.Enums.DeliveryStatus.Completed);
        var deliverySuccessRate = deliveriesList.Count > 0 ? (completedDeliveries / (double)deliveriesList.Count) * 100 : 0;

            var averageOrderValue = ordersList.Count > 0 ? (decimal)ordersList.Average(o => (double)o.TotalAmount) : 0;

        var uniqueShops = salesList.Select(s => s.ShopId).Distinct().Count();
        var customerRetentionRate = 0.0; // Would need historical data to calculate properly

        var revenueByCategory = new Dictionary<string, decimal>();
        var expensesByCategory = expensesList.GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        return new PerformanceMetricsDto
        {
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            NetProfit = netProfit,
            ProfitMargin = (double)profitMargin,
            TotalOrders = ordersList.Count,
            CompletedDeliveries = completedDeliveries,
            DeliverySuccessRate = deliverySuccessRate,
            AverageOrderValue = (decimal)averageOrderValue,
            ActiveCustomers = uniqueShops,
            CustomerRetentionRate = customerRetentionRate,
            RevenueByCategory = revenueByCategory,
            ExpensesByCategory = expensesByCategory
        };
    }
}
