namespace PoultryDistributionSystem.Application.DTOs.Shop;

/// <summary>
/// Create shop request DTO
/// </summary>
public class CreateShopDto
{
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
