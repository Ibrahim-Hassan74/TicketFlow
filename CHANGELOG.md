# Changelog

All notable changes to the **TicketFlow** project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [2.0.0] - 2026-10-17

### Added
- **Clean 4-Layer Architecture**: Restructured solution into `TicketFlow.Domain`, `TicketFlow.Application`, `TicketFlow.Infrastructure`, and `TicketFlow.API` with strict inward-pointing dependencies.
- **CQRS via MediatR**: Segregated Command mutations and Query reads:
  - Write commands handled with Entity Framework Core 10 and Unit of Work / Repository pattern.
  - Read queries handled with Dapper micro-ORM querying optimized PostgreSQL stored functions with zero EF tracking overhead.
- **PostgreSQL 17 Integration**: Migrated persistence from SQL Server to PostgreSQL using `Npgsql.EntityFrameworkCore.PostgreSQL` with native `UUID`, `TIMESTAMPTZ`, and `BYTEA` column mappings.
- **PostgreSQL Stored Functions**: Implemented 9 idempotent, high-performance table-valued functions:
  - `fn_get_dashboard_stats`: Single-pass conditional aggregations for dashboard metrics.
  - `fn_get_agent_workload`: Workload distribution and logged time per agent.
  - `fn_get_ticket_trends`: Dynamic date series trend aggregation using `generate_series`.
  - `fn_get_tickets`: Multi-criteria search, filtering, and dynamic pagination with `COUNT(*) OVER()`.
  - `fn_get_users` & `fn_get_agents`: Paginated user directory with role joins.
  - `fn_get_ticket_comments`, `fn_get_ticket_activities`, `fn_get_ticket_time_entries`: Sub-resource projections.
- **Optimistic Concurrency Control**:
  - Configured PostgreSQL trigger `trg_tickets_rowversion` calling `update_ticket_rowversion()` to recalculate `NEW."RowVersion" = md5(random()::text)::bytea` on every row update.
  - Centralized mapping of EF Core `DbUpdateConcurrencyException` to HTTP 409 Conflict via `GlobalExceptionHandler`.
- **E2E Integration Test Suite**:
  - Created `TicketFlow.IntegrationTests` project using `CustomWebApplicationFactory<Program>` pointing to the `"Testing"` environment.
  - 13 full-stack tests validating authentication, Dapper functions, ticket lifecycles, and IDOR customer data isolation against live PostgreSQL.
- **Cross-Cutting Pipeline Behaviors**:
  - `ValidationBehavior`: Automatic FluentValidation pipeline execution before handlers.
  - `LoggingBehavior`: Structured telemetry, diagnostics, and duration logging.
- **Observability**: Added `Serilog.Sinks.Seq` package for real-time structured telemetry ingestion alongside console and rolling file sinks.
- **Automated Docker Database Initialization**:
  - Generated `000_FullPostgreSQLInit.sql` and `001_CreateReadStoredFunctions.sql` mounted to `/docker-entrypoint-initdb.d:ro`.
  - Multi-container `docker-compose.yml` with healthchecks and parameterization via `.env.example`.
- **API Request Suite**: Created comprehensive `TicketFlow.API/TicketFlow.API.http` test collection with token chaining for Visual Studio and VS Code REST Client.

### Changed
- **Renamed Read Functions**: Converted all stored functions from legacy `sp_` to idiomatic PostgreSQL `fn_` prefix via EF Core migration `RenameReadFunctionsToFnPrefix`.
- **Timezone Underflow Safeguards**: Added `COALESCE(NULLIF(u."CreatedAt", '0001-01-01 00:00:00+00'::TIMESTAMPTZ), CURRENT_TIMESTAMP)` safeguards to user query functions to prevent `ArgumentOutOfRangeException` during Dapper deserialization on machines with positive UTC offsets.
- **Standardized Exception Handling**: Implemented ASP.NET Core `IExceptionHandler` (`GlobalExceptionHandler`) producing RFC 7807 Problem Details with status code mappings for validation, authentication, authorization, not found, conflicts, and internal server errors.
- **Security Hardening**: Externalized all secrets and connection strings; added `.env.example` and `appsettings.example.json` templates; strictly gitignored sensitive environment and configuration files.

### Removed
- Decommissioned legacy 3-tier layered projects (`TicketFlow.Core`, `TicketFlow.ControllerTests`, and `TicketFlow.ServiceTests`).
- Removed tightly coupled procedural services in favor of granular MediatR command and query handlers.

---

## [1.0.0] - 2026-08-15

### Added
- Initial ASP.NET Core Web API release using traditional 3-tier layered architecture.
- ASP.NET Core Identity authentication with JWT bearer tokens.
- Ticket lifecycle management (Open, InProgress, Resolved, Closed).
- Basic role-based access control (`Admin`, `SupportAgent`, `Customer`).
- Ticket comments, activity auditing, and time tracking entries.
- Swagger OpenAPI documentation.
