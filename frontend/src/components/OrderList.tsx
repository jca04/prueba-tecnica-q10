import type { Order } from "../api/orders";

interface Props {
  orders: Order[];
  loading: boolean;
  error: string | null;
  connected: boolean;
}

export function OrderList({ orders, loading, error, connected }: Props) {
  if (loading) return <p>Cargando pedidos…</p>;

  return (
    <section>
      <h2>
        Pedidos{" "}
        <small>
          {connected ? "🟢 tiempo real" : "🟡 actualizando cada 5 s"}
        </small>
      </h2>

      {error && (
        <p role="alert" style={{ color: "crimson" }}>
          {error}
        </p>
      )}

      {orders.length === 0 ? (
        <p>Aún no hay pedidos.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Cliente</th>
              <th>SKU</th>
              <th>Cant.</th>
              <th>Estado</th>
              <th>Creado</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((o) => (
              <tr key={o.id}>
                <td>{o.clienteNombre}</td>
                <td>{o.sku}</td>
                <td>{o.cantidad}</td>
                <td>
                  <strong>{o.estado}</strong>
                  {o.motivoRechazo ? ` (${o.motivoRechazo})` : ""}
                </td>
                <td>{new Date(o.creadoEn).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
