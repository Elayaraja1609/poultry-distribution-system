using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Forecasting;
using PoultryDistributionSystem.Application.Interfaces;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Forecasting controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin")]
public class ForecastingController : ControllerBase
{
    private readonly IForecastingService _forecastingService;

    public ForecastingController(IForecastingService forecastingService)
    {
        _forecastingService = forecastingService ?? throw new ArgumentNullException(nameof(forecastingService));
    }

    [HttpGet("demand")]
    [ProducesResponseType(typeof(ApiResponse<List<DemandForecastDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<DemandForecastDto>>>> ForecastDemand(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? shopId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _forecastingService.ForecastDemandAsync(startDate, endDate, shopId, cancellationToken);
            return Ok(ApiResponse<List<DemandForecastDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("seasonal-trends")]
    [ProducesResponseType(typeof(ApiResponse<List<SeasonalTrendDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<SeasonalTrendDto>>>> GetSeasonalTrends(
        [FromQuery] int periodType = 1, // 1=Monthly, 2=Quarterly, 3=Weekly
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _forecastingService.GetSeasonalTrendsAsync(periodType, cancellationToken);
            return Ok(ApiResponse<List<SeasonalTrendDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("stock-recommendations")]
    [ProducesResponseType(typeof(ApiResponse<List<StockLevelRecommendationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<StockLevelRecommendationDto>>>> GetStockRecommendations(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _forecastingService.RecommendStockLevelsAsync(cancellationToken);
            return Ok(ApiResponse<List<StockLevelRecommendationDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
