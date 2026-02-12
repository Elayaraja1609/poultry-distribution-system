using PoultryDistributionSystem.Application.DTOs.Forecasting;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Forecasting service interface
/// </summary>
public interface IForecastingService
{
    Task<List<DemandForecastDto>> ForecastDemandAsync(DateTime startDate, DateTime endDate, Guid? shopId, CancellationToken cancellationToken = default);
    Task<List<SeasonalTrendDto>> GetSeasonalTrendsAsync(int periodType, CancellationToken cancellationToken = default); // 1=Monthly, 2=Quarterly, 3=Weekly
    Task<List<StockLevelRecommendationDto>> RecommendStockLevelsAsync(CancellationToken cancellationToken = default);
}
