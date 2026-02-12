namespace PoultryDistributionSystem.Application.DTOs.Supplier;

/// <summary>
/// Update supplier request DTO
/// </summary>
public class UpdateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
