

namespace OrderFlow.Contracts.Events
{
    public record OrderCreated(Guid EventId, Guid OrderId, string Sku, int Cantidad, DateTime OcurridoEn);
    public record StockReserved(Guid EventId, Guid OrderId, DateTime OcurridoEn);
    public record StockRejected(Guid EventId, Guid OrderId, string Motivo, DateTime OcurridoEn);
}