using OrderFlow.OrdersApi.Models;

namespace OrderFlow.OrdersApi.Validation
{
    public static class OrderValidator
    {
        public static Dictionary<string, string[]> Validate(CreateOrderRequest r, bool skuExists)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(r.ClienteNombre))
                errors["clienteNombre"] = ["El nombre del cliente es obligatorio."];

            if (string.IsNullOrWhiteSpace(r.Sku))
                errors["sku"] = ["El SKU es obligatorio."];

            else if (!skuExists)
                errors["sku"] = [$"El SKU '{r.Sku}' no existe en el catálogo."];

            if (r.Cantidad is < 1 or > 100)
                errors["cantidad"] = ["La cantidad debe estar entre 1 y 100."];

            return errors;
        }
    }
}