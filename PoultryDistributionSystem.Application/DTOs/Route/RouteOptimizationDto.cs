namespace PoultryDistributionSystem.Application.DTOs.Route;

/// <summary>
/// Route optimization result DTO
/// </summary>
public class RouteOptimizationDto
{
    public List<RouteStopDto> Stops { get; set; } = new();
    public double TotalDistance { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public decimal EstimatedCost { get; set; }
}

/// <summary>
/// Route stop DTO
/// </summary>
public class RouteStopDto
{
    public int Order { get; set; }
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double DistanceFromPrevious { get; set; }
    public TimeSpan EstimatedArrival { get; set; }
}

/// <summary>
/// Route optimization request DTO
/// </summary>
public class RouteOptimizationRequestDto
{
    public List<Guid> ShopIds { get; set; } = new();
    public Guid? VehicleId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
}
