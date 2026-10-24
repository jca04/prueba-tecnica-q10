## Cómo ejecutar el proyecto

Requisitos: Docker con Compose v2.

```bash
docker compose up --build
```

Servicios disponibles:

- Panel web: http://localhost:3000
- Orders API: http://localhost:8080
- RabbitMQ: http://localhost:15672

Las migraciones y los datos iniciales se cargan al arrancar. Para reiniciar el proyecto desde cero, eliminando también los volúmenes:

```bash
docker compose down -v
```

### Tests

Se necesita el SDK de .NET 8+ para ejecutar las pruebas:

```bash
dotnet test
```

Los tests utilizan SQLite en memoria, por lo que no necesitan levantar Docker ni RabbitMQ.

### Pruebas manuales

- Crear un pedido de `ABC-01` con cantidad 2. Debe pasar a `Confirmed`.
- Crear un pedido de `ABC-03` con cantidad 10. Debe rechazarse por falta de stock.
- Probar la validación con un cliente vacío, un SKU inexistente o una cantidad fuera del rango permitido.
- Ejecutar la prueba de idempotencia descrita en la sección correspondiente del código o los tests.

## Arquitectura

La API recibe y valida los pedidos, los guarda como `Pending` y publica el evento `OrderCreated` mediante un outbox transaccional. RabbitMQ entrega el evento al Inventory Worker, que reserva el stock y publica el resultado. Finalmente, la API actualiza el estado del pedido y notifica al frontend mediante SignalR.

### Decisiones de arquitectura

- **Coreografía de eventos:** cada servicio reacciona a los mensajes y publica sus resultados. Para este flujo no consideré necesario añadir un orquestador.
- **Separación de datos:** Orders e Inventory tienen bases de datos independientes dentro del mismo servidor PostgreSQL.
- **RabbitMQ y MassTransit:** permiten comunicar los servicios de forma asíncrona y manejar reintentos y errores de procesamiento.
- **Outbox en la API:** el pedido y el evento se guardan en la misma transacción para evitar que un pedido quede registrado sin que su evento pueda publicarse.
- **Idempotencia en el Worker:** se registran los eventos procesados para evitar descontar el stock más de una vez ante mensajes duplicados.
- **Lógica de negocio separada del consumer:** el procesamiento de reservas se encuentra en `ReserveStockHandler`, lo que facilita probarlo sin levantar el broker.
- **SignalR con respaldo:** el frontend recibe actualizaciones en tiempo real y utiliza consultas periódicas cada cinco segundos cuando pierde la conexión.

## Trade-offs asumidos

| Decisión                            | Motivo                                                                  | Limitación                                                                   |
| ----------------------------------- | ----------------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| Catálogo duplicado en la API        | Validar los SKU sin depender de una consulta al servicio de inventario. | Los datos pueden desactualizarse.                                            |
| Outbox solo en la API               | Simplificar la publicación inicial del pedido.                          | El Worker requiere controlar los fallos y la duplicación de sus resultados.  |
| Idempotencia manual                 | Facilitar su comprensión y las pruebas.                                 | Hay que mantener la lógica de deduplicación.                                 |
| Migraciones al arrancar             | Poder levantar todo con un solo comando.                                | No es lo ideal cuando hay varias réplicas.                                   |
| Una réplica de la API               | Es suficiente para el alcance actual.                                   | Para escalar SignalR entre instancias se necesita infraestructura adicional. |
| Sin paginación en `GET /orders`     | Simplificar la consulta y el panel.                                     | El rendimiento puede degradarse al crecer los datos.                         |
| Sin timeout para pedidos pendientes | Evitar añadir otro proceso de recuperación.                             | Un pedido puede quedar en `Pending` indefinidamente.                         |
| Notificaciones a todos los clientes | El alcance no exige autenticación ni permisos por usuario.              | Todos los clientes conectados reciben las actualizaciones.                   |
| Estructura de código sencilla       | Evitar capas que no aportan mucho al tamaño actual del proyecto.        | Si aumenta la complejidad, habrá que separar mejor las responsabilidades.    |

## Manejo de fallos e idempotencia

El sistema utiliza reintentos para fallos temporales y colas durables para conservar los mensajes pendientes. Si un evento se procesa más de una vez, el Worker utiliza su registro de eventos procesados para evitar descontar nuevamente el stock.

Los cambios de estado solo se permiten desde `Pending`, por lo que un pedido ya resuelto no debería cambiar de `Confirmed` a `Rejected` ni al contrario.

La base de datos es la fuente de verdad del estado de los pedidos. SignalR se utiliza para notificar los cambios, y el frontend puede consultar nuevamente la API si pierde la conexión.

Con el test Same_event_processed_twice_deducts_stock_only_once procesa dos veces el mismo eventId y comprueba que el stock bajó una sola vez.

## Qué haría distinto con más tiempo

- Añadir un timeout y un mecanismo de recuperación para pedidos que permanezcan demasiado tiempo en `Pending`.
- Implementar un outbox también en el Worker para gestionar de forma más robusta la publicación de los resultados.
- Ejecutar las migraciones como un paso independiente del despliegue.
- Sincronizar el catálogo entre servicios mediante eventos para evitar mantener datos duplicados.
- Añadir paginación y concurrencia optimista para manejar un mayor volumen de pedidos.
- Incorporar trazabilidad entre servicios con OpenTelemetry y pruebas de integración con PostgreSQL y RabbitMQ reales.
- Preparar el sistema para varias réplicas, incluyendo un backplane de SignalR y una estrategia de despliegue con Kubernetes si el entorno lo requiere.

## Estructura del repositorio

```text
src/
  OrderFlow.Contracts/
  OrderFlow.OrdersApi/
  OrderFlow.InventoryWorker/
tests/
  OrderFlow.Tests/
frontend/
docker-compose.yml
```
