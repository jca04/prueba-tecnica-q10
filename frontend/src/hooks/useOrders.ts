import { useCallback, useEffect, useState } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";
import { getOrders, type Order } from "../api/orders";

export function useOrders() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [connected, setConnected] = useState(false);

  const refresh = useCallback(async () => {
    try {
      setOrders(await getOrders());
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Error desconocido");
    } finally {
      setLoading(false);
    }
  }, []);

  const upsert = useCallback((incoming: Order) => {
    setOrders((prev) => {
      const i = prev.findIndex((o) => o.id === incoming.id);
      if (i === -1) return [incoming, ...prev];
      if (prev[i].estado !== "Pending" && incoming.estado === "Pending")
        return prev;
      const next = [...prev];
      next[i] = incoming;
      return next;
    });
  }, []);

  useEffect(() => {
    const loadOrders = async () => {
      await refresh();
    };

    void loadOrders();

    const connection = new HubConnectionBuilder()
      .withUrl("/hubs/orders")
      .withAutomaticReconnect()
      .build();

    connection.on("OrderUpdated", upsert);
    connection.onreconnecting(() => setConnected(false));
    connection.onreconnected(() => {
      setConnected(true);
      refresh();
    });
    connection.onclose(() => setConnected(false));

    connection
      .start()
      .then(() => {
        setConnected(true);
        refresh();
      })
      .catch(() => setConnected(false));

    return () => {
      connection.stop();
    };
  }, [refresh, upsert]);

  useEffect(() => {
    if (connected) return;
    const id = setInterval(refresh, 5000);
    return () => clearInterval(id);
  }, [connected, refresh]);

  return { orders, loading, error, connected, upsert };
}
