import { OrderForm } from "./components/OrderForm";
import { OrderList } from "./components/OrderList";
import { useOrders } from "./hooks/useOrders";

export default function App() {
  const { orders, loading, error, connected, upsert } = useOrders();

  return (
    <main style={{ maxWidth: 900, margin: "0 auto", padding: 16 }}>
      <h1>OrderFlow</h1>
      <OrderForm onCreated={upsert} />
      <OrderList
        orders={orders}
        loading={loading}
        error={error}
        connected={connected}
      />
    </main>
  );
}
