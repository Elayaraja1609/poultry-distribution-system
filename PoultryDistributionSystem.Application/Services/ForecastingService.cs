using PoultryDistributionSystem.Application.DTOs.Forecasting;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Forecasting service implementation
/// Uses simple moving average and trend analysis
/// </summary>
public class ForecastingService : IForecastingService
{
    private readonly IUnitOfWork _unitOfWork;

    public ForecastingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<List<DemandForecastDto>> ForecastDemandAsync(DateTime startDate, DateTime endDate, Guid? shopId, CancellationToken cancellationToken = default)
    {
        // Get historical sales data
        var historicalStart = startDate.AddMonths(-6); // Use 6 months of historical data
        var sales = await _unitOfWork.Sales.FindAsync(
            s => !s.IsDeleted && 
                 s.SaleDate >= historicalStart && 
                 s.SaleDate <= DateTime.UtcNow &&
                 (!shopId.HasValue || s.ShopId == shopId.Value),
            cancellationToken);
        var salesList = sales.ToList();

        if (salesList.Count == 0)
        {
            return new List<DemandForecastDto>();
        }

        // Calculate average daily demand
        var dailyDemands = salesList
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new { Date = g.Key, Quantity = g.Sum(s => GetSaleQuantity(s)) })
            .OrderBy(x => x.Date)
            .ToList();

        var averageDemand = dailyDemands.Count > 0 ? dailyDemands.Average(d => d.Quantity) : 0;
        var trendData = dailyDemands.Select(d => (d.Date, d.Quantity)).ToList();
        var trend = CalculateTrend(trendData);

        // Generate forecasts
        var forecasts = new List<DemandForecastDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            // Simple linear forecast: base demand + trend
            var daysFromStart = (currentDate - startDate).Days;
            var predictedQuantity = (int)(averageDemand + (trend * daysFromStart));
            predictedQuantity = Math.Max(0, predictedQuantity); // Ensure non-negative

            // Confidence decreases as we forecast further into the future
            var daysAhead = (currentDate - DateTime.UtcNow).Days;
            var confidenceLevel = Math.Max(0.5, 1.0 - (daysAhead / 30.0)); // 50% minimum confidence

            forecasts.Add(new DemandForecastDto
            {
                ForecastDate = currentDate,
                PredictedQuantity = predictedQuantity,
                ConfidenceLevel = confidenceLevel,
                ForecastMethod = "Moving Average with Trend"
            });

            currentDate = currentDate.AddDays(1);
        }

        return forecasts;
    }

    public async Task<List<SeasonalTrendDto>> GetSeasonalTrendsAsync(int periodType, CancellationToken cancellationToken = default)
    {
        var historicalStart = DateTime.UtcNow.AddMonths(-12);
        var sales = await _unitOfWork.Sales.FindAsync(
            s => !s.IsDeleted && s.SaleDate >= historicalStart,
            cancellationToken);
        var salesList = sales.ToList();

        var trends = new List<SeasonalTrendDto>();

        if (periodType == 1) // Monthly
        {
            var monthlyData = salesList
                .GroupBy(s => s.SaleDate.Month)
                .Select(g => new
                {
                    Period = new DateTime(2000, g.Key, 1).ToString("MMMM"),
                    AverageDemand = g.Average(s => GetSaleQuantity(s)),
                    Count = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToList();

            var monthlyDataTuples = monthlyData.Select(m => (m.Period, m.AverageDemand)).ToList();
            foreach (var month in monthlyData)
            {
                trends.Add(new SeasonalTrendDto
                {
                    Period = month.Period,
                    AverageDemand = month.AverageDemand,
                    Trend = CalculateMonthlyTrend(month.Period, monthlyDataTuples)
                });
            }
        }
        else if (periodType == 2) // Quarterly
        {
            var quarterlyData = salesList
                .GroupBy(s => (s.SaleDate.Month - 1) / 3 + 1)
                .Select(g => new
                {
                    Period = $"Q{g.Key}",
                    AverageDemand = g.Average(s => GetSaleQuantity(s)),
                    Count = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToList();

            foreach (var quarter in quarterlyData)
            {
                trends.Add(new SeasonalTrendDto
                {
                    Period = quarter.Period,
                    AverageDemand = quarter.AverageDemand,
                    Trend = CalculateQuarterlyTrend(quarter.Period, quarterlyData.Select(q => (q.Period, q.AverageDemand)).ToList())
                });
            }
        }
        else if (periodType == 3) // Weekly
        {
            var weeklyData = salesList
                .GroupBy(s => s.SaleDate.DayOfWeek)
                .Select(g => new
                {
                    Period = g.Key.ToString(),
                    AverageDemand = g.Average(s => GetSaleQuantity(s)),
                    Count = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToList();

            foreach (var day in weeklyData)
            {
                trends.Add(new SeasonalTrendDto
                {
                    Period = day.Period,
                    AverageDemand = day.AverageDemand,
                    Trend = 0 // No trend calculation for weekly
                });
            }
        }

        return trends;
    }

    public async Task<List<StockLevelRecommendationDto>> RecommendStockLevelsAsync(CancellationToken cancellationToken = default)
    {
        var farms = await _unitOfWork.Farms.FindAsync(f => !f.IsDeleted, cancellationToken);
        var recommendations = new List<StockLevelRecommendationDto>();

        // Get historical demand
        var historicalStart = DateTime.UtcNow.AddMonths(-3);
        var sales = await _unitOfWork.Sales.FindAsync(
            s => !s.IsDeleted && s.SaleDate >= historicalStart,
            cancellationToken);
        var salesList = sales.ToList();

        var averageDailyDemand = salesList.Count > 0 
            ? salesList.Average(s => GetSaleQuantity(s)) / 30.0 // Average per day
            : 10.0; // Default

        foreach (var farm in farms)
        {
            var stockMovements = await _unitOfWork.StockMovements.FindAsync(
                sm => sm.FarmId == farm.Id && !sm.IsDeleted,
                cancellationToken);
            var movements = stockMovements.ToList();

            var stockOut = movements
                .Where(m => m.MovementType == Domain.Enums.StockMovementType.Out)
                .Sum(m => m.Quantity);

            var averageMonthlyOut = movements.Count > 0 
                ? stockOut / Math.Max(1, (DateTime.UtcNow - movements.Min(m => m.MovementDate)).Days / 30.0)
                : averageDailyDemand * 30;

            // Recommendation: Keep 1.5x monthly demand as safety stock
            var recommendedStock = (int)(averageMonthlyOut * 1.5);
            var minimumStock = (int)(averageMonthlyOut * 0.5); // Minimum safety stock
            var maximumStock = farm.Capacity; // Don't exceed capacity

            var reason = farm.CurrentCount < minimumStock
                ? "Stock level is below minimum recommended level"
                : farm.CurrentCount > maximumStock * 0.9
                    ? "Stock level is near capacity"
                    : "Stock level is within recommended range";

            recommendations.Add(new StockLevelRecommendationDto
            {
                FarmId = farm.Id,
                FarmName = farm.Name,
                CurrentStock = farm.CurrentCount,
                RecommendedStock = Math.Min(recommendedStock, maximumStock),
                MinimumStock = minimumStock,
                MaximumStock = maximumStock,
                RecommendationReason = reason
            });
        }

        return recommendations;
    }

    private int GetSaleQuantity(Domain.Entities.Sale sale)
    {
        // In a real implementation, sum up delivery items
        // For now, return a default quantity
        return 10; // Placeholder
    }

    private double CalculateTrend(List<(DateTime Date, int Quantity)> dailyDemands)
    {
        if (dailyDemands.Count < 2)
        {
            return 0;
        }

        // Simple linear regression slope
        var n = dailyDemands.Count;
        var sumX = 0.0;
        var sumY = 0.0;
        var sumXY = 0.0;
        var sumX2 = 0.0;

        for (int i = 0; i < n; i++)
        {
            var x = i;
            var y = (double)dailyDemands[i].Quantity;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        return slope;
    }

    private double CalculateMonthlyTrend(string month, List<(string Period, double AverageDemand)> monthlyData)
    {
        var index = monthlyData.FindIndex(m => m.Period == month);
        if (index == -1 || index == monthlyData.Count - 1)
        {
            return 0;
        }

        var current = monthlyData[index].AverageDemand;
        var next = monthlyData[index + 1].AverageDemand;
        
        return current > 0 ? ((next - current) / current) * 100 : 0;
    }

    private double CalculateQuarterlyTrend(string quarter, List<(string Period, double AverageDemand)> quarterlyData)
    {
        var index = quarterlyData.FindIndex(q => q.Period == quarter);
        if (index == -1 || index == quarterlyData.Count - 1)
        {
            return 0;
        }

        var current = quarterlyData[index].AverageDemand;
        var next = quarterlyData[index + 1].AverageDemand;
        
        return current > 0 ? ((next - current) / current) * 100 : 0;
    }
}
