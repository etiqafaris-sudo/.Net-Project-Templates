# Backend Foundation of Project Templates

## .NET Framework 4.8 platform notes (CleanArchiecture.WebApi.NetF)

This template follows the architecture below with these intentional deltas:

| Area | NetCore template | This template (.NET 4.8) |
|------|------------------|-----------------------------|
| Runtime | .NET 10 | .NET Framework 4.8 |
| HTTP host | ASP.NET Core Minimal API | ASP.NET Web API 2 on IIS / IIS Express |
| Persistence | EF Core 10 | EF6 (Code First migrations) |
| Hosting worker | Generic host in `Program.cs` | `Microsoft.Extensions.Hosting` via `HostedServiceRunner` in `Global.asax` |
| Docker | Linux API image | **SQL Server only** (`docker-compose.yml`); API runs on IIS, not in a Linux container |
| OpenTelemetry | ASP.NET Core instrumentation | Not included (use Serilog; add OTel later if needed) |

Layers, Mediator CQRS, FluentValidation, transactional outbox, UAM sample APIs, health routes, optimistic concurrency, and logging goals remain the same.

---

## Objective

Create the production-quality project template .NET 10 Minimal API foundation.

---

# Technology

Use:

* .NET 10
* ASP.NET Core Minimal API
* MartinThamar/Mediator
* CQRS
* Clean Architecture
* EF Core
* SQLServer
* FluentValidation
* Serilog
* OpenTelemetry

*(For NetF, substitute: .NET 4.8, Web API 2, EF6; OpenTelemetry optional.)*

---

# Projects

Create sample:

```text
Sample.Api
Sample.Application
Sample.Domain
Sample.Infrastructure
Sample.Shared
```

---

# Domain

Domain contains:

* Entities
* Value objects
* Domain events
* Domain rules
* Enums

Domain must not depend on infrastructure.

---

# Application

Application contains:

* Commands
* Queries
* Handlers
* Validators
* DTOs
* Interfaces
* Application services

Organize by feature.

---

# Infrastructure

Infrastructure contains:

* EF Core
* MariaDB configuration
* DbContext
* Persistence
* External services
* Authentication infrastructure

---

# API

API contains:

* Minimal API endpoints
* Dependency injection
* Middleware
* Configuration
* Authentication setup
* Health checks

Endpoints must be thin.

---

# Database

Create the initial DbContext.

Configure:

* UTC timestamps
* Primary keys
* Foreign keys
* Indexes
* Constraints
* **RowVersion** optimistic concurrency token on every auditable entity

Do not implement the complete schema yet.

Create only the foundation required for the application.

```text
Application/Features/Uam/
  CreateStaff/
  ListUsers/
  UpdateUserStatus/
```

Expose UAM endpoints under `/api/v1/uam`.

---

# Optimistic Concurrency

Every auditable domain entity must include a `RowVersion` concurrency token.

Requirements:

* Map `RowVersion` with EF Core `IsRowVersion()`
* Accept `RowVersion` on update commands
* Return `409 Conflict` when a concurrent modification is detected
* Never expose internal exception details

*(NetF: EF6 `IsRowVersion()` in fluent configuration.)*

---

# Error Handling

Implement centralized ProblemDetails handling.

Never expose internal exceptions.

---

# Validation

Configure FluentValidation.

Validation errors must produce consistent API responses.

---

---

# Code Best practice

1. for dto use sealed record 
2. for normal class alwasy have `sealed` if it`s no inherit.
3. implementation of handler must mark with `internal sealed` possible. as long not exposed to any.

---
# Cancellation

Use CancellationToken throughout:

```text
Endpoint
 ↓
Mediator
 ↓
Handler
 ↓
Database
```

---

# Health Checks

Implement:

```text
/health
/health/live
/health/ready
```

---

# Logging

Use structured logging.

Include:

* Correlation ID
* Request ID
* Trace ID
* User ID where available
* Duration

Never log sensitive information.

---

# Completion Criteria

The API must:

* Build
* Start
* Connect to SQLServer
* Respond to health checks
* Return consistent errors
* Have clean architecture boundaries
* Use Mediator
* Support CQRS
* Support EF Core migrations
* Run successfully in Docker
* Enforce optimistic concurrency via RowVersion

*(NetF: EF6 migrations; Docker for SQL only; API on IIS.)*