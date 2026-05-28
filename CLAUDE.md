# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```powershell
# Build
dotnet build

# Run the worker service (requires RabbitMQ running locally)
dotnet run --project src\SystemCustomerEngagement.Worker\app.microservice.customer.engagement.worker.csproj

# Run tests (if added)
dotnet test
```

## Environment setup

Copy `.env.example` to `.env` in the Worker project and fill in the values. The app loads it automatically via DotNetEnv at startup.

```
src\SystemCustomerEngagement.Worker\.env.example → src\SystemCustomerEngagement.Worker\.env
```

Key config values:
- `RabbitMq__Host/Port/VirtualHost/Username/Password/UseSsl` — RabbitMQ connection
- `HubSpot__BaseUrl` / `HubSpot__AccessToken` — HubSpot Private App Token (Bearer)
- `Otlp__Enabled` / `Otlp__Endpoint` — OTLP gRPC for Datadog Agent

## Architecture

**.NET 10 background worker service** using **MassTransit + RabbitMQ**. Root namespace is `AppMicroserviceCustomerEngagement`.

### Layer overview

**Domain** (`app.microservice.customer.engagement.domain`)
- `Exceptions/TransientException` — retryable error; MassTransit applies retry/redelivery
- `Exceptions/PermanentException` — non-retryable; goes directly to DLQ

**Infrastructure** (`app.microservice.customer.engagement.infrastructure`)
- `HubSpot/HubSpotServiceProvider` — calls `POST /crm/v3/objects/contacts/batch/upsert` with `idProperty: "email"` to upsert contacts and set `paso_actual`; throws `TransientException` on `429`/`5xx`, `PermanentException` on other `4xx`
- `Messaging/LoggingFilter<T>` — MassTransit consume filter; enriches every log with `MessageId`, `CorrelationId`, `MessageType`, and processing duration

**Worker** (`app.microservice.customer.engagement.worker`) — entry point
- `Contracts/` — local message contract records (one per queue)
- `Consumers/` — handler + mapper pair per queue (see below)
- `Extensions/MassTransitExtensions.cs` — all queue/consumer registration

### Active queues and consumers

Each integration flow has three files: a **contract** record, a **mapper** (static, validates and projects to `(Email, CurrentStep)` tuples), and a **handler** (`IConsumer<Batch<T>>`).

| Queue name | Contract | Handler | Mapper |
|---|---|---|---|
| `customer_engagement_upsert_credit_origination_integration_event` | `CreditFlowStepIntegrationEvent` | `CreditOriginationIntegrationEventHandler` | `CreditOriginationIntegrationEventMapper` |
| `customer_engagement_upsert_smart_origination_integration_event` | `SmartOriginationIntegrationEvent` | `SmartOriginationIntegrationEventHandler` | `SmartOriginationIntegrationEventMapper` |
| `customer_engagement_upsert_user_registration_integration_event` | `UpdateUserIntegrationEvent` | `UpdateUserIntegrationEventHandler` | `UpdateUserIntegrationEventMapper` |

All queues are **quorum queues**. Batch settings: `MessageLimit=10`, `TimeLimit=30s`, `ConcurrencyLimit=4`, `PrefetchCount=40`.

Retry policy (identical across all queues):
1. **Immediate retry** — 3 attempts, exponential backoff (100ms → 2s); ignores `PermanentException`
2. **Delayed redelivery** — 3 redeliveries at 30s / 2min / 10min (requires `rabbitmq_delayed_message_exchange` plugin)
3. All attempts exhausted → MassTransit moves message to `{queue_name}_error` (DLQ)

### Adding a new integration flow

1. Add a contract record in `Worker/Contracts/`
2. Add a static mapper in `Worker/Consumers/` implementing `ToHubSpotContacts(IEnumerable<TEvent>, ILogger)`
3. Add a handler in `Worker/Consumers/` implementing `IConsumer<Batch<TEvent>>`; silently skip invalid messages (no exception), use `TransientException`/`PermanentException` for HubSpot errors
4. Register in `MassTransitExtensions.cs`: `x.AddConsumer<THandler>` with batch options + `cfg.ReceiveEndpoint` with quorum queue, retry, redelivery, and `LoggingFilter`
