using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderFlow.OrdersApi.Domain;
using OrderFlow.OrdersApi.Models;
using OrderFlow.OrdersApi.Validation;

namespace OrderFlow.Tests
{
    public class OrderRulesTests
    {
        [Fact]
        public void Pending_order_confirms_once_and_repeats_are_ignored()
        {
            var order = Order.Create("Ana", "ABC-01", 2);

            Assert.True(order.TryConfirm());
            Assert.False(order.TryConfirm());
            Assert.Equal(OrderStatus.Confirmed, order.Estado);
        }

        [Fact]
        public void Confirmed_order_cannot_be_rejected()
        {
            var order = Order.Create("Ana", "ABC-01", 2);
            order.TryConfirm();

            Assert.False(order.TryReject("Stock insuficiente"));
            Assert.Equal(OrderStatus.Confirmed, order.Estado);
            Assert.Null(order.MotivoRechazo);
        }


        [Theory]
        [InlineData("", "ABC-01", 1, true, "clienteNombre")]
        [InlineData("Ana", "ABC-01", 0, true, "cantidad")]
        [InlineData("Ana", "ABC-01", 101, true, "cantidad")]
        [InlineData("Ana", "NOPE", 1, false, "sku")]
        public void Invalid_request_reports_the_failing_field(
        string cliente, string sku, int cantidad, bool skuExists, string campo)
        {
            var errors = OrderValidator.Validate(new CreateOrderRequest(cliente, sku, cantidad), skuExists);

            Assert.Contains(campo, errors.Keys);
        }
    }
}