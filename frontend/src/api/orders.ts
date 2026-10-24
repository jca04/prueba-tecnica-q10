export type OrderStatus = "Pending" | "Confirmed" | "Rejected";

export interface Order {
  id: string;
  clienteNombre: string;
  sku: string;
  cantidad: number;
  estado: OrderStatus;
  creadoEn: string;
  motivoRechazo?: string;
}

export interface CreateOrderInput {
  clienteNombre: string;
  sku: string;
  cantidad: number;
}

export class ApiValidationError extends Error {
  constructor(
    public fieldErrors: Record<string, string[]>,
    message = "Los datos enviados no son válidos.",
  ) {
    super(message);
  }
}

const BASE = "/api";

export async function getOrders(): Promise<Order[]> {
  const res = await fetch(`${BASE}/orders`);
  if (!res.ok)
    throw new Error(
      `No se pudo cargar la lista de pedidos (HTTP ${res.status}).`,
    );
  return res.json();
}

export async function createOrder(input: CreateOrderInput): Promise<Order> {
  let res: Response;
  try {
    res = await fetch(`${BASE}/orders`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(input),
    });
  } catch {
    throw new Error("No se pudo conectar con el servidor. Intenta de nuevo.");
  }

  if (res.status === 400) {
    const problem = await res.json().catch(() => null);
    throw new ApiValidationError(problem?.errors ?? {}, problem?.title);
  }
  if (!res.ok) throw new Error(`Error del servidor (HTTP ${res.status}).`);
  return res.json();
}
