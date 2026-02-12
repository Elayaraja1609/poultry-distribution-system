using PoultryDistributionSystem.Application.DTOs.Route;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Route optimization service interface
/// </summary>
public interface IRouteOptimizationService
{
    Task<RouteOptimizationDto> OptimizeDeliveryRouteAsync(RouteOptimizationRequestDto request, CancellationToken cancellationToken = default);
    Task<double> CalculateDistanceAsync(string address1, string address2, CancellationToken cancellationToken = default);
    Task<TimeSpan> EstimateDeliveryTimeAsync(double distance, CancellationToken cancellationToken = default);
}
