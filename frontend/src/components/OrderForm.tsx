"use client";

import { useState, type FormEvent } from "react";
import { ApiValidationError, createOrder, type Order } from "../api/orders";

type FieldErrors = Record<string, string[]>;

function validateClient(f: {
  clienteNombre: string;
  sku: string;
  cantidad: number;
}): FieldErrors {
  const errors: FieldErrors = {};
  if (!f.clienteNombre.trim())
    errors.clienteNombre = ["El nombre del cliente es obligatorio."];
  if (!f.sku.trim()) errors.sku = ["El SKU es obligatorio."];
  if (!Number.isInteger(f.cantidad) || f.cantidad < 1 || f.cantidad > 100)
    errors.cantidad = ["La cantidad debe estar entre 1 y 100."];
  return errors;
}

function FieldError({ messages }: { messages?: string[] }) {
  if (!messages?.length) return null;
  return (
    <p role="alert" style={{ color: "crimson", margin: "2px 0" }}>
      {messages.join(" ")}
    </p>
  );
}

export function OrderForm({
  onCreated,
}: {
  onCreated: (order: Order) => void;
}) {
  const [form, setForm] = useState({ clienteNombre: "", sku: "", cantidad: 1 });
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setFormError(null);

    const clientErrors = validateClient(form);
    setFieldErrors(clientErrors);
    if (Object.keys(clientErrors).length > 0) return;

    setSubmitting(true);
    try {
      const order = await createOrder(form);
      onCreated(order);
      setForm((f) => ({ ...f, clienteNombre: "", cantidad: 1 }));
    } catch (err) {
      if (err instanceof ApiValidationError) {
        setFieldErrors(err.fieldErrors);
        if (Object.keys(err.fieldErrors).length === 0)
          setFormError(err.message);
      } else {
        setFormError(err instanceof Error ? err.message : "Error inesperado.");
      }
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} noValidate>
      <h2>Nuevo pedido</h2>

      <label>
        Cliente
        <input
          value={form.clienteNombre}
          onChange={(e) => setForm({ ...form, clienteNombre: e.target.value })}
        />
      </label>
      <FieldError messages={fieldErrors.clienteNombre} />

      <label>
        SKU
        <input
          value={form.sku}
          placeholder="ABC-01"
          onChange={(e) => setForm({ ...form, sku: e.target.value })}
        />
      </label>
      <FieldError messages={fieldErrors.sku} />

      <label>
        Cantidad
        <input
          type="number"
          value={form.cantidad}
          onChange={(e) =>
            setForm({ ...form, cantidad: Number(e.target.value) })
          }
        />
      </label>
      <FieldError messages={fieldErrors.cantidad} />

      {formError && (
        <p role="alert" style={{ color: "crimson" }}>
          {formError}
        </p>
      )}

      <button type="submit" disabled={submitting}>
        {submitting ? "Enviando…" : "Crear pedido"}
      </button>
    </form>
  );
}
