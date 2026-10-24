

using System.ComponentModel.DataAnnotations;

namespace OrderFlow.InventoryWorker.Messaging
{
    public sealed class RabbitMqOptions
    {

        public const string SectionName = "RabbitMq";

        [Required] public string Host { get; init; } = string.Empty;
        [Required] public string Username { get; init; } = string.Empty;
        [Required] public string Password { get; init; } = string.Empty;

    }
}