using PoultryDistributionSystem.Application.DTOs.Route;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Route optimization service implementation
/// Uses simple nearest-neighbor algorithm for route optimization
/// </summary>
public class RouteOptimizationService : IRouteOptimizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private const double AverageSpeedKmh = 50.0; // Average vehicle speed in km/h
    private const decimal CostPerKm = 0.5m; // Cost per kilometer

    public RouteOptimizationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<RouteOptimizationDto> OptimizeDeliveryRouteAsync(RouteOptimizationRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.ShopIds.Count == 0)
        {
            return new RouteOptimizationDto();
        }

        // Get shop details
        var shops = new List<(Guid Id, string Name, string Address)>();
        foreach (var shopId in request.ShopIds)
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(shopId, cancellationToken);
            if (shop != null)
            {
                shops.Add((shop.Id, shop.Name, shop.Address ?? ""));
            }
        }

        if (shops.Count == 0)
        {
            return new RouteOptimizationDto();
        }

        // Simple nearest-neighbor algorithm
        var optimizedRoute = OptimizeRoute(shops);
        var stops = new List<RouteStopDto>();
        double totalDistance = 0;
        var currentTime = request.StartTime;

        for (int i = 0; i < optimizedRoute.Count; i++)
        {
            var shop = optimizedRoute[i];
            double distanceFromPrevious = 0;

            if (i > 0)
            {
                var previousShop = optimizedRoute[i - 1];
                distanceFromPrevious = await CalculateDistanceAsync(previousShop.Address, shop.Address, cancellationToken);
                totalDistance += distanceFromPrevious;
            }

            var duration = await EstimateDeliveryTimeAsync(distanceFromPrevious, cancellationToken);
            currentTime = currentTime.Add(duration);

            stops.Add(new RouteStopDto
            {
                Order = i + 1,
                ShopId = shop.Id,
                ShopName = shop.Name,
                Address = shop.Address,
                DistanceFromPrevious = distanceFromPrevious,
                EstimatedArrival = currentTime.TimeOfDay
            });
        }

        var totalDuration = currentTime - request.StartTime;
        var estimatedCost = (decimal)totalDistance * CostPerKm;

        return new RouteOptimizationDto
        {
            Stops = stops,
            TotalDistance = totalDistance,
            EstimatedDuration = totalDuration,
            EstimatedCost = estimatedCost
        };
    }

    public async Task<double> CalculateDistanceAsync(string address1, string address2, CancellationToken cancellationToken = default)
    {
        // Simplified distance calculation using Haversine formula
        // In a real implementation, you would use Google Maps API or similar
        // For now, return a random distance between 5-50 km for demonstration
        await Task.CompletedTask;
        
        if (string.IsNullOrEmpty(address1) || string.IsNullOrEmpty(address2))
        {
            return 10.0; // Default distance
        }

        // Simple hash-based distance approximation
        var hash1 = address1.GetHashCode();
        var hash2 = address2.GetHashCode();
        var distance = Math.Abs(hash1 - hash2) % 45 + 5; // 5-50 km range
        
        return distance;
    }

    public async Task<TimeSpan> EstimateDeliveryTimeAsync(double distance, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        
        // Estimate time: distance / speed + 15 minutes per stop
        var travelTime = TimeSpan.FromHours(distance / AverageSpeedKmh);
        var stopTime = TimeSpan.FromMinutes(15);
        
        return travelTime.Add(stopTime);
    }

    private List<(Guid Id, string Name, string Address)> OptimizeRoute(List<(Guid Id, string Name, string Address)> shops)
    {
        if (shops.Count <= 1)
        {
            return shops;
        }

        // Nearest-neighbor algorithm
        var route = new List<(Guid Id, string Name, string Address)>();
        var remaining = new List<(Guid Id, string Name, string Address)>(shops);
        
        // Start with first shop (could be optimized to start from depot/warehouse)
        var current = remaining[0];
        route.Add(current);
        remaining.RemoveAt(0);

        while (remaining.Count > 0)
        {
            // Find nearest unvisited shop
            var nearest = remaining
                .OrderBy(s => CalculateSimpleDistance(current.Address, s.Address))
                .First();

            route.Add(nearest);
            remaining.Remove(nearest);
            current = nearest;
        }

        return route;
    }

    private double CalculateSimpleDistance(string address1, string address2)
    {
        // Simple distance approximation based on address hash
        var hash1 = address1.GetHashCode();
        var hash2 = address2.GetHashCode();
        return Math.Abs(hash1 - hash2) % 50;
    }
}
