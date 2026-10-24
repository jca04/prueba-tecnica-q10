using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderFlow.InventoryWorker.Domain;


public enum ReservationOutcome { Reserved, Rejected }

public class ProcessedEvent
{
    public Guid EventId { get; set; } // PK para asegurar idempotencia
    public Guid OrderId { get; set; }
    public ReservationOutcome Outcome { get; set; }
    public string? Motivo { get; set; }
    public DateTime ProcesadoEn { get; set; }
}