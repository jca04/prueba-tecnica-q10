


namespace OrderFlow.OrdersApi.Domain;

public enum OrderStatus { Pending, Confirmed, Rejected };

public class Order
{

    private Order() { }

    public Guid Id { get; private set; }
    public string ClienteNombre { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public int Cantidad { get; private set; }
    public OrderStatus Estado { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public string? MotivoRechazo { get; private set; }

    public static Order Create(string clienteNombre, string sku, int cantidad) => new()
    {

        Id = Guid.NewGuid(),
        ClienteNombre = clienteNombre,
        Sku = sku,
        Cantidad = cantidad,
        Estado = OrderStatus.Pending,
        CreadoEn = DateTime.UtcNow

    };

    public bool TryConfirm()
    {
        if (Estado != OrderStatus.Pending) return false;
        Estado = OrderStatus.Confirmed;
        return true;
    }

    public bool TryReject(string motivo)
    {
        if (Estado != OrderStatus.Pending) return false;
        Estado = OrderStatus.Rejected;
        MotivoRechazo = motivo;
        return true;
    }
}
