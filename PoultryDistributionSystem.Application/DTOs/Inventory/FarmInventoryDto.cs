namespace PoultryDistributionSystem.Application.DTOs.Inventory;

/// <summary>
/// Farm inventory summary DTO
/// </summary>
public class FarmInventoryDto
{
    public Guid FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentStock { get; set; }
    public int AvailableStock { get; set; }
    public int StockIn { get; set; }
    public int StockOut { get; set; }
    public int StockLoss { get; set; }
    public List<ChickenStockDto> ChickenStocks { get; set; } = new();
}

/// <summary>
/// Chicken stock DTO
/// </summary>
public class ChickenStockDto
{
    public Guid ChickenId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int AvailableQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
}
