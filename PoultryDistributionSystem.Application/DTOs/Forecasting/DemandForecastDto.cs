namespace PoultryDistributionSystem.Application.DTOs.Forecasting;

/// <summary>
/// Demand forecast DTO
/// </summary>
public class DemandForecastDto
{
    public DateTime ForecastDate { get; set; }
    public int PredictedQuantity { get; set; }
    public double ConfidenceLevel { get; set; }
    public string ForecastMethod { get; set; } = string.Empty;
}

/// <summary>
/// Seasonal trend DTO
/// </summary>
public class SeasonalTrendDto
{
    public string Period { get; set; } = string.Empty; // e.g., "January", "Q1", "Monday"
    public double AverageDemand { get; set; }
    public double Trend { get; set; } // Positive = increasing, Negative = decreasing
}

/// <summary>
/// Stock level recommendation DTO
/// </summary>
public class StockLevelRecommendationDto
{
    public Guid FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int RecommendedStock { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public string RecommendationReason { get; set; } = string.Empty;
}
