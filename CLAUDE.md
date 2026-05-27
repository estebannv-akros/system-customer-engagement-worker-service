# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```powershell
# Build
dotnet build

# Run the worker service (requires RabbitMQ running locally)
dotnet run --project src\SystemCustomerEngagement.Worker\SystemCustomerEngagement.Worker.csproj

# Run tests (if added)
dotnet test
```

## Architecture

This is a **.NET 10 background worker service** following **Domain-Driven Design (DDD)** and **Clean Architecture** with the **CQRS pattern**. The solution has five layers with strict dependency direction (outer layers depend on inner layers):

### Layers

**Domain** (`SystemCustomerEngagement.Domain`) — no dependencies on other layers
- `CustomerEngagement` aggregate root with `CustomerId` value object
- `EngagementChannel` (Email, Sms, Push) and `EngagementStatus` (Pending, Processed, Failed) enums
- Domain events raised on state transitions: Created, Processed, Failed
- `ICustomerEngagementRepository` repository interface
- `TransientException` — signals a retryable error; MassTransit will apply retry/redelivery
- `PermanentException` — signals a non-retryable error; MassTransit sends directly to DLQ

**Contracts** (`SystemCustomerEngagement.Contracts`) — no dependencies on other layers
- Immutable message contracts shared across producers and consumers
- The contract `CustomerEngagementRequested` also lives in `Worker/Contracts/` (local copy used by the consumer)
- `V1/CustomerEngagementRequested` — incoming command with `CorrelationId`, `Timestamp`, `CustomerId`, `Email`, `Channel`, `PasoActual`, `Message`; `Email` is the HubSpot contact identifier

**Application** (`SystemCustomerEngagement.Application`) — depends on Domain only
- CQRS via `ICommandHandler<TCommand>` and `IQueryHandler<TQuery, TResult>` interfaces
- Commands: `CreateEngagementCommand`, `ProcessEngagementCommand`
- Queries: `GetPendingEngagementsQuery` → returns `IEnumerable<EngagementDto>`
- `IDomainEventDispatcher` and `IHubSpotClient` interfaces

**Infrastructure** (`SystemCustomerEngagement.Infrastructure`) — implements Application/Domain interfaces
- `InMemoryCustomerEngagementRepository` using `ConcurrentDictionary` — repository calls are **commented out** in handlers while persistence is not yet integrated
- `DomainEventDispatcher` — publishes domain events to RabbitMQ via MassTransit `IPublishEndpoint`; registered as **scoped** (inherits the active consumer context to propagate `CorrelationId`)
- `LoggingFilter<T>` — MassTransit consume filter that enriches every log with `MessageId`, `CorrelationId`, `MessageType`, and processing duration
- `HubSpotClient` — calls `POST /crm/v3/objects/contacts/batch/upsert` with `idProperty: "email"` to upsert the contact and set `paso_actual`; `429`/`5xx` → `TransientException`, `4xx` → `PermanentException`; registered via `AddHttpClient` with `Authorization: Bearer` from `HubSpot:AccessToken`

**Worker** (`SystemCustomerEngagement.Worker`) — entry point, depends on all layers
- `Consumers/CustomerEngagementConsumer` — `IConsumer<Batch<CustomerEngagementRequested>>`; receives batches from RabbitMQ, **silently skips** messages with invalid `Email`, `PasoActual`, or `Channel` (no exception thrown), and calls `IHubSpotClient.UpsertContactsBatchAsync` for the valid subset
- `Extensions/MassTransitExtensions.cs` — configures MassTransit + RabbitMQ (see Messaging section below)
- `Program.cs` — loads `.env` via DotNetEnv, wires DI, MassTransit, and OpenTelemetry

### Messaging (MassTransit + RabbitMQ)

Three active quorum queues (pattern: `{service}.{purpose}`):
- `customer-engagement.engagements`
- `customer-engagement.notifications`
- `customer-engagement.interactions`

All three use `CustomerEngagementConsumer` with identical configuration.

Batch settings per queue: `MessageLimit = 10`, `TimeLimit = 30s`, `ConcurrencyLimit = 4`, `PrefetchCount = 40`.

Retry policy (applied in `MassTransitExtensions.cs`):
1. **Immediate retry** — 3 attempts, exponential backoff (100ms → 2s); ignores `PermanentException`
2. **Delayed redelivery** — 3 redeliveries at 30s / 2min / 10min; requires the `rabbitmq_delayed_message_exchange` plugin
3. If all attempts fail → MassTransit moves the message to the automatic `_error` queue (DLQ)

Config keys (`appsettings.json`):
- `RabbitMq:Host`, `RabbitMq:Port`, `RabbitMq:VirtualHost`, `RabbitMq:Username`, `RabbitMq:Password`, `RabbitMq:UseSsl`
- `HubSpot:BaseUrl` — API base URL (`https://api.hubapi.com`)
- `HubSpot:AccessToken` — Private App Token; use secrets/env vars in production
- `Otlp:Enabled`, `Otlp:Endpoint` — OTLP gRPC endpoint for Datadog Agent

Development defaults (`appsettings.Development.json`): `localhost:5672`, `guest/guest`, SSL off. Override via `.env` (see `.env.example`).

### Observability (OpenTelemetry)

Configured in `Program.cs`:
- Traces: `AddSource("MassTransit")` + HTTP client instrumentation → OTLP exporter (only when `Otlp:Enabled = true`)
- Metrics: `AddMeter("MassTransit")` + runtime instrumentation → OTLP exporter
- Every consume log automatically includes `MessageId`, `CorrelationId`, `MessageType` via `LoggingFilter<T>`

### Adding new consumers

1. Add a contract record in `Contracts/V{n}/` (and optionally a local copy in `Worker/Contracts/`)
2. Add a consumer class in `Worker/Consumers/` implementing `IConsumer<Batch<TMessage>>` or `IConsumer<TMessage>`
3. Register with `x.AddConsumer<TConsumer>()` in `MassTransitExtensions.cs`
4. Add a `cfg.ReceiveEndpoint(...)` block with quorum queue, batch settings, retry, redelivery, and `LoggingFilter`
5. Silently skip invalid messages (no exception); throw `PermanentException` for non-retryable processing errors, `TransientException` for retryable ones

### Adding new use cases (CQRS)

1. Add command/query class in `Application/Commands` or `Application/Queries`
2. Add handler in `Application/Handlers` implementing `ICommandHandler` or `IQueryHandler`
3. Register handler in `Worker/Extensions/ApplicationServiceExtensions.cs`
