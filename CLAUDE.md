# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```powershell
# Build
dotnet build

# Run the worker service
dotnet run --project src\SystemCustomerEngagement.Worker\SystemCustomerEngagement.Worker.csproj

# Run tests (if added)
dotnet test
```

## Architecture

This is a **.NET 10 background worker service** following **Domain-Driven Design (DDD)** and **Clean Architecture** with the **CQRS pattern**. The solution has four layers with strict dependency direction (outer layers depend on inner layers):

### Layers

**Domain** (`SystemCustomerEngagement.Domain`) — no dependencies on other layers
- `CustomerEngagement` aggregate root with `CustomerId` value object
- `EngagementChannel` (Email, Sms, Push, InApp) and `EngagementStatus` (Pending, Processed, Failed) enums
- Domain events raised on state transitions: Created, Processed, Failed
- `ICustomerEngagementRepository` repository interface

**Application** (`SystemCustomerEngagement.Application`) — depends on Domain only
- CQRS via `ICommandHandler<TCommand>` and `IQueryHandler<TQuery, TResult>` interfaces
- Commands: `CreateEngagementCommand`, `ProcessEngagementCommand`
- Queries: `GetPendingEngagementsQuery` → returns `IEnumerable<EngagementDto>`
- `IDomainEventDispatcher` interface consumed by handlers

**Infrastructure** (`SystemCustomerEngagement.Infrastructure`) — implements Application/Domain interfaces
- `InMemoryCustomerEngagementRepository` using `ConcurrentDictionary` (no persistent storage yet)
- `DomainEventDispatcher` — currently only logs events, does not publish to any bus
- `ServiceCollectionExtensions` registers all infrastructure services

**Worker** (`SystemCustomerEngagement.Worker`) — entry point, depends on all layers
- `CustomerEngagementWorker`: hosted service that polls every 10 seconds, fetches up to 10 pending engagements, and dispatches `ProcessEngagementCommand` for each
- `Program.cs` wires up DI using extension methods from Infrastructure and Application layers

### Adding new use cases

1. Add command/query class in `Application/Commands` or `Application/Queries`
2. Add handler in `Application/Handlers` implementing `ICommandHandler` or `IQueryHandler`
3. Register handler in `ApplicationServiceExtensions.cs` (Worker project)
4. If new infrastructure is needed (e.g., external messaging, persistence), implement in Infrastructure and register in `ServiceCollectionExtensions.cs`
