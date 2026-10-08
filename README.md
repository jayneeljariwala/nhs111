# NHS 111 Telehealth Integration Platform

> ASP.NET Core 8 Web API platform integrating NHS 111 dispositions with Ummanu (telehealth) and Adastra (urgent care) via RabbitMQ, Repository Pattern, and Clean Architecture across 3 decoupled solutions.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Solutions](#solutions)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Environment Variables](#environment-variables)
- [API Reference](#api-reference)
- [Routing Logic](#routing-logic)
- [End-to-End Flow](#end-to-end-flow)
- [Running Tests](#running-tests)
- [Project Structure](#project-structure)
- [Contributing](#contributing)

---

## Overview

When a patient calls **NHS 111**, a clinical disposition is generated — a recommendation for where and how quickly they need care. This platform intercepts that disposition, applies intelligent routing rules based on clinical codes (DOS/Dx parameters), and dispatches it asynchronously to one or both downstream systems:

- **Ummanu** — a telehealth platform for remote video consultations
- **Adastra** — an urgent care system for in-person or phone appointments

Once Ummanu handles a case, it sends a status callback that automatically resolves the corresponding Adastra record, preventing duplicate treatment.

This project was presented at **UKI Summit 2022** as a response to the Advanced Software cyberattack that took down NHS 111 services in August 2022 — demonstrating how a cloud-native, decoupled architecture can keep critical healthcare services running even when primary systems fail.

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    NHS 111 (HSCN / ITK)                 │
│           Incoming patient dispositions via ITK          │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│                    API Gateway                           │
│          JWT Auth · Rate Limiting · SSL                  │
└───────────────────────┬─────────────────────────────────┘
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
   Disposition      Routing      Notification
    Service         Service        Service
   (Parse ITK)   (DOS/Dx rules)  (Welfare calls)
          │
          ▼
┌─────────────────────────────────────────────────────────┐
│           RabbitMQ Message Queue                         │
│     disposition.ummanu  |  disposition.adastra           │
└──────────────┬──────────────────────┬───────────────────┘
               │                      │
               ▼                      ▼
    ┌──────────────────┐   ┌──────────────────────┐
    │  Ummanu Consumer │   │   Adastra Consumer   │
    │  (Worker Service)│   │   (Worker Service)   │
    └────────┬─────────┘   └──────────┬───────────┘
             │                        │
             ▼                        ▼
    ┌──────────────────┐   ┌──────────────────────┐
    │ Ummanu Dummy API │   │  Adastra Dummy API   │
    │   (port 7001)    │   │    (port 7002)       │
    └──────────────────┘   └──────────────────────┘
             │
             │  Status callback (ResolvedByUmmanu)
             └──────────────────────►─────────────┘

┌─────────────────────────────────────────────────────────┐
│                  Repository Pattern                      │
│   IDispositionRepo · IPatientRepo · IAuditRepo          │
└───────────────────────┬─────────────────────────────────┘
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
     PostgreSQL       Redis        Blob
     (main data)    (caching)    (recordings)
```

---

## Solutions

This project is split into **3 independent Visual Studio solutions**, each with its own database and Docker Compose setup:

| Solution             | Description                                   | Port   |
| -------------------- | --------------------------------------------- | ------ |
| `NHS111.Integration` | Main integration platform                     | `5000` |
| `Ummanu.DummyAPI`    | Simulates the real Ummanu telehealth platform | `7001` |
| `Adastra.DummyAPI`   | Simulates the real Adastra urgent care system | `7002` |

All 3 solutions communicate over a shared Docker network: `nhs111-network`.

---

## Tech Stack

| Layer          | Technology                                               |
| -------------- | -------------------------------------------------------- |
| Framework      | ASP.NET Core 8 Web API                                   |
| Language       | C# 12                                                    |
| ORM            | Entity Framework Core 8 (Npgsql)                         |
| Database       | PostgreSQL 16                                            |
| Cache          | Redis 7                                                  |
| Message Broker | RabbitMQ 3 (via MassTransit)                             |
| Architecture   | Clean Architecture + Repository Pattern + CQRS (MediatR) |
| Validation     | FluentValidation                                         |
| Auth           | JWT Bearer                                               |
| Logging        | Serilog + Seq                                            |
| Containers     | Docker + Docker Compose                                  |
| Testing        | xUnit + Moq + FluentAssertions                           |
| API Docs       | Swagger / OpenAPI                                        |
| Resilience     | Polly (retry policies)                                   |

---

## Prerequisites

Before running this project, make sure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [dotnet-ef CLI tool](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)

```bash
# Install EF Core CLI tool globally
dotnet tool install --global dotnet-ef
```

---

## Quick Start

### Step 1 — Create the shared Docker network

This only needs to be done once. All 3 solutions share this network.

```bash
docker network create nhs111-network
```

### Step 2 — Start Ummanu Dummy API

```bash
cd Ummanu.DummyAPI
docker-compose up -d
```

Verify it is running:

```
http://localhost:7001/swagger
http://localhost:7001/health
```

### Step 3 — Start Adastra Dummy API

```bash
cd Adastra.DummyAPI
docker-compose up -d
```

Verify it is running:

```
http://localhost:7002/swagger
http://localhost:7002/health
```

### Step 4 — Start the Main Integration Platform

```bash
cd NHS111.Integration
docker-compose up -d
```

### Step 5 — Verify all services are running

| Service                | URL                                    |
| ---------------------- | -------------------------------------- |
| Main API + Swagger     | http://localhost:5000/swagger          |
| RabbitMQ Management UI | http://localhost:15672 (guest / guest) |
| Seq Log Viewer         | http://localhost:5341                  |
| Ummanu Dummy API       | http://localhost:7001/swagger          |
| Adastra Dummy API      | http://localhost:7002/swagger          |

---

## Environment Variables

### NHS111.Integration

| Variable                               | Default                                       | Description                            |
| -------------------------------------- | --------------------------------------------- | -------------------------------------- |
| `ConnectionStrings__DefaultConnection` | `Host=postgres;Port=5432;Database=nhs111;...` | PostgreSQL connection string           |
| `ConnectionStrings__Redis`             | `redis:6379`                                  | Redis connection string                |
| `RabbitMQ__Host`                       | `rabbitmq`                                    | RabbitMQ hostname                      |
| `RabbitMQ__Username`                   | `guest`                                       | RabbitMQ username                      |
| `RabbitMQ__Password`                   | `guest`                                       | RabbitMQ password                      |
| `ExternalApis__UmmanuBaseUrl`          | `http://ummanu-dummy-api:7001`                | Ummanu API base URL                    |
| `ExternalApis__AdastraBaseUrl`         | `http://adastra-dummy-api:7002`               | Adastra API base URL                   |
| `Jwt__Key`                             | `NHS111-Super-Secret-Key-2024`                | JWT signing key (change in production) |
| `Jwt__Issuer`                          | `NHS111.API`                                  | JWT issuer                             |
| `Jwt__ExpiryMinutes`                   | `60`                                          | JWT token expiry                       |

### Ummanu.DummyAPI

| Variable                               | Default                                          | Description                  |
| -------------------------------------- | ------------------------------------------------ | ---------------------------- |
| `ConnectionStrings__DefaultConnection` | `Host=postgres;Port=5432;Database=ummanu_db;...` | PostgreSQL connection string |

### Adastra.DummyAPI

| Variable                               | Default                                           | Description                  |
| -------------------------------------- | ------------------------------------------------- | ---------------------------- |
| `ConnectionStrings__DefaultConnection` | `Host=postgres;Port=5432;Database=adastra_db;...` | PostgreSQL connection string |

---

## API Reference

### NHS111.Integration — Main API (port 5000)

#### Dispositions

| Method | Endpoint                            | Description                                             |
| ------ | ----------------------------------- | ------------------------------------------------------- |
| `POST` | `/api/dispositions`                 | Receive NHS 111 disposition, route and publish to queue |
| `GET`  | `/api/dispositions/{id}`            | Get disposition by ID (Redis cached)                    |
| `GET`  | `/api/dispositions/pending`         | List all pending dispositions                           |
| `GET`  | `/api/dispositions/status/{status}` | Filter by status                                        |
| `PUT`  | `/api/dispositions/{id}/status`     | Update disposition status (consumer callback)           |

**Example — Create Disposition:**

```json
POST /api/dispositions
{
  "nhsNumber": "1234567890",
  "patientName": "John Smith",
  "dateOfBirth": "1985-06-15T00:00:00Z",
  "dxCode": "Dx012",
  "dosCode": "SRV001",
  "urgency": "Immediate"
}
```

**Example Response:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nhsNumber": "1234567890",
  "patientName": "John Smith",
  "dxCode": "Dx012",
  "dosCode": "SRV001",
  "urgency": "Immediate",
  "routedTo": "Ummanu",
  "status": "InQueue",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

#### Patients

| Method | Endpoint                    | Description               |
| ------ | --------------------------- | ------------------------- |
| `GET`  | `/api/patients/{nhsNumber}` | Get patient by NHS number |
| `POST` | `/api/patients`             | Register new patient      |

#### Audit

| Method | Endpoint                     | Description                            |
| ------ | ---------------------------- | -------------------------------------- |
| `GET`  | `/api/audit/{dispositionId}` | Get full audit trail for a disposition |

#### Health

| Method | Endpoint  | Description                                        |
| ------ | --------- | -------------------------------------------------- |
| `GET`  | `/health` | Check PostgreSQL, Redis, and RabbitMQ connectivity |

---

### Ummanu Dummy API (port 7001)

| Method   | Endpoint                                        | Description                         |
| -------- | ----------------------------------------------- | ----------------------------------- |
| `POST`   | `/api/appointments`                             | Create appointment from disposition |
| `GET`    | `/api/appointments/{id}`                        | Get appointment by ID               |
| `GET`    | `/api/appointments/disposition/{dispositionId}` | Get by disposition ID               |
| `GET`    | `/api/appointments/queue`                       | Get full waiting room queue         |
| `PUT`    | `/api/appointments/{id}/status`                 | Update appointment status           |
| `PUT`    | `/api/appointments/{id}/assign-clinician`       | Assign clinician                    |
| `DELETE` | `/api/appointments/{id}`                        | Cancel appointment                  |

---

### Adastra Dummy API (port 7002)

| Method   | Endpoint                                     | Description                                     |
| -------- | -------------------------------------------- | ----------------------------------------------- |
| `POST`   | `/api/referrals`                             | Create referral from disposition                |
| `GET`    | `/api/referrals/{id}`                        | Get referral by ID                              |
| `GET`    | `/api/referrals/disposition/{dispositionId}` | Get by disposition ID                           |
| `GET`    | `/api/referrals`                             | List all referrals (optional `?status=` filter) |
| `PUT`    | `/api/referrals/{id}/status`                 | Update referral status                          |
| `PUT`    | `/api/referrals/{id}/resolve`                | Resolve referral (by Ummanu or Adastra)         |
| `PUT`    | `/api/referrals/{id}/assign-staff`           | Assign staff member                             |
| `DELETE` | `/api/referrals/{id}`                        | Cancel referral                                 |

**Example — Resolve by Ummanu:**

```json
PUT /api/referrals/{id}/resolve
{
  "resolvedBy": "Ummanu",
  "notes": "Patient consulted via Ummanu video call. No further action needed."
}
```

---

## Routing Logic

The `DispositionRoutingService` applies the following rules in order to determine where a disposition is sent:

| Rule   | Condition                                                                  | Destination                           |
| ------ | -------------------------------------------------------------------------- | ------------------------------------- |
| Rule 1 | DxCode starts with `Dx0` **AND** urgency is `Immediate` or `UrgentSameDay` | **Ummanu** only                       |
| Rule 2 | DxCode starts with `Dx1` **OR** `Dx2`                                      | **Adastra** only                      |
| Rule 3 | All other cases                                                            | **Both** (Adastra as safety fallback) |

**Examples:**

| DxCode  | Urgency       | Routed To |
| ------- | ------------- | --------- |
| `Dx012` | Immediate     | Ummanu    |
| `Dx001` | UrgentSameDay | Ummanu    |
| `Dx012` | Routine       | Both      |
| `Dx112` | Any           | Adastra   |
| `Dx212` | Any           | Adastra   |
| `Dx312` | Any           | Both      |
| `Dx999` | Any           | Both      |

---

## End-to-End Flow

Here is a complete walkthrough of what happens when a disposition is received:

### Happy Path — Routed to Ummanu

```
1. POST /api/dispositions
   DxCode: "Dx012", Urgency: "Immediate"

2. DispositionRoutingService
   → Rule 1 matches → RoutedTo = Ummanu

3. DispositionCreatedEvent published
   → RabbitMQ queue: "disposition.ummanu"

4. NHS111.UmmanuConsumer picks up message
   → POST http://ummanu-dummy-api:7001/api/appointments
   → Appointment created, WaitingRoomPosition = 1 (Immediate = front of queue)

5. Ummanu Consumer publishes DispositionStatusUpdatedEvent
   → NewStatus = "InProgress", UpdatedBy = "Ummanu"

6. NHS111.AdastraConsumer picks up status event
   → PUT http://adastra-dummy-api:7002/api/referrals/{id}/resolve
   → Body: { resolvedBy: "Ummanu" }
   → Adastra referral marked as ResolvedByUmmanu ✓

7. Ummanu background simulation (AppointmentSimulationService):
   → After 5s: clinician assigned, Status = InConsultation
   → After 10s: consultation complete, Status = Completed ✓
```

### Safety Fallback — Routed to Both

```
1. POST /api/dispositions
   DxCode: "Dx999", Urgency: "Routine"

2. DispositionRoutingService
   → Rule 3 (fallback) → RoutedTo = Both

3. DispositionCreatedEvent published to BOTH queues:
   → "disposition.ummanu"
   → "disposition.adastra"

4. Both consumers process independently
   → Ummanu creates appointment
   → Adastra creates referral

5. Ummanu completes first
   → Publishes DispositionStatusUpdatedEvent (UpdatedBy = Ummanu)
   → Adastra Consumer receives event
   → Calls PUT /api/referrals/{id}/resolve with resolvedBy = "Ummanu"
   → Adastra referral = ResolvedByUmmanu (no duplicate treatment) ✓
```

---

## Running Tests

### Unit Tests

```bash
cd NHS111.Integration
dotnet test tests/NHS111.UnitTests
```

### All Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage Areas

| Test Class                               | What It Tests                                      |
| ---------------------------------------- | -------------------------------------------------- |
| `DispositionRoutingServiceTests`         | All 3 routing rule branches (7 test cases)         |
| `CreateDispositionCommandHandlerTests`   | Command handler, queue publishing, audit logging   |
| `CreateDispositionRequestValidatorTests` | FluentValidation rules (valid + invalid cases)     |
| `GlobalExceptionMiddlewareTests`         | ProblemDetails shape, correlation ID header        |
| `DispositionRepositoryTests`             | EF Core queries, Redis cache hit/miss/invalidation |
| `UmmanuDispositionConsumerTests`         | HTTP success, retry, skip logic                    |
| `AdastraStatusConsumerTests`             | Ummanu callback handling, skip logic               |

---

## Project Structure

```
NHS111.Integration/
├── src/
│   ├── NHS111.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   ├── NHS111.Application/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── DTOs/
│   │   │   ├── Requests/
│   │   │   └── Responses/
│   │   ├── Validators/
│   │   ├── Services/
│   │   ├── Exceptions/
│   │   └── Extensions/
│   ├── NHS111.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Events/
│   │   └── Interfaces/
│   ├── NHS111.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── NHS111DbContext.cs
│   │   │   ├── Configurations/
│   │   │   ├── Repositories/
│   │   │   └── Migrations/
│   │   ├── Messaging/
│   │   ├── Http/
│   │   └── Extensions/
│   └── NHS111.Contracts/
│       └── Events/
├── consumers/
│   ├── NHS111.UmmanuConsumer/
│   │   ├── Consumers/
│   │   ├── Program.cs
│   │   └── Dockerfile
│   └── NHS111.AdastraConsumer/
│       ├── Consumers/
│       ├── Program.cs
│       └── Dockerfile
├── tests/
│   ├── NHS111.UnitTests/
│   └── NHS111.IntegrationTests/
├── docker-compose.yml
├── docker-compose.override.yml
└── NHS111.Integration.sln

Ummanu.DummyAPI/
├── src/
│   └── Ummanu.DummyAPI/
│       ├── Controllers/
│       ├── Domain/
│       ├── Persistence/
│       ├── Repositories/
│       ├── Services/           ← AppointmentSimulationService
│       ├── DTOs/
│       ├── Program.cs
│       └── Dockerfile
├── tests/
│   └── Ummanu.DummyAPI.Tests/
├── docker-compose.yml
└── Ummanu.DummyAPI.sln

Adastra.DummyAPI/
├── src/
│   └── Adastra.DummyAPI/
│       ├── Controllers/
│       ├── Domain/
│       ├── Persistence/
│       ├── Repositories/
│       ├── Services/           ← ReferralSimulationService
│       ├── DTOs/
│       ├── Program.cs
│       └── Dockerfile
├── tests/
│   └── Adastra.DummyAPI.Tests/
├── docker-compose.yml
└── Adastra.DummyAPI.sln
```

---

## Database Ports

Each solution uses its own isolated PostgreSQL instance:

| Solution           | Database     | Host Port |
| ------------------ | ------------ | --------- |
| NHS111.Integration | `nhs111`     | `5432`    |
| Ummanu.DummyAPI    | `ummanu_db`  | `5433`    |
| Adastra.DummyAPI   | `adastra_db` | `5434`    |

---

## Stopping the Platform

```bash
# Stop all solutions
cd NHS111.Integration && docker-compose down
cd ../Ummanu.DummyAPI && docker-compose down
cd ../Adastra.DummyAPI && docker-compose down

# Remove the shared network (only when fully done)
docker network rm nhs111-network
```

To also remove persisted data volumes:

```bash
docker-compose down -v
```

---

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature-name`
5. Open a Pull Request

Please make sure `dotnet build` and `dotnet test` pass before submitting a PR.

---

## Acknowledgements

This project was inspired by the real-world cyberattack on Advanced Software in August 2022, which disrupted NHS 111 services across the UK. The original solution was presented at UKI Summit 2022 by Shahriar Haque (COO, Integrella) and Itzik Levy (CEO, Ummanu Health), demonstrating how modern integration architecture can serve as a resilient fallback when primary healthcare IT systems fail.

---

## License

This project is licensed under the MIT License.
