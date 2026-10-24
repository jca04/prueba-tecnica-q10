

namespace OrderFlow.InventoryWorker.Domain;

public class Stock
{
    public string Sku { get; set; } = string.Empty;
    public int Disponible { get; set; }
}