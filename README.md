# Clinic POS — Multi-Tenant System

A high-performance Clinic Point of Sale system built with **.NET 10**, **Next.js**, **PostgreSQL**, **Redis**, and **MassTransit**.

## 🏗 Architecture Overview & Tenant Safety

The system uses a **Logic-based Multi-Tenancy** strategy with a shared database and shared schema.

### How Tenant Safety Works:
1.  **Identity Propagation**: Every request must provide a `X-Tenant-Id` header (or have it derived from user claims).
2.  **Global Query Filters**: In the `ApplicationDbContext`, we apply a filter to all entities implementing `ITenantEntity`:
    `modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantId)`. This ensures that every `SELECT` query automatically includes a `WHERE TenantId = ...` clause, preventing accidental data leaks.
3.  **Secure Writes**: The `SaveChangesAsync` method is overridden to automatically populate the `TenantId` field from the current execution context, ensuring data is always saved to the correct silo.
4.  **Isolation Verification**: Our automated test suite specifically tests that "Tenant A" cannot read data from "Tenant B".

## ⚖️ Assumptions and Tradeoffs

-   **Shared Database**: Chosen for simplicity and low infrastructure cost in v1. For massive scale, this could transition to a "Database-per-Tenant" model.
-   **Primary Branch**: We assumed a patient belongs primarily to one branch for registration (Section A requirements), but the architecture supports appointments across any branch.
-   **Stub Auth**: Used a `X-Test-User-Id` header for development to allow server-side policy enforcement without the complexity of a full OIDC/Keycloak setup in this slice.

## 🚀 How to Run (One Command)

```bash
docker compose up --build
```

-   **Frontend**: http://localhost:3000
-   **Backend API**: http://localhost:5001
-   **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## 🔑 Seeded Users & Login

The system seeds a default tenant and three users with different roles. Use the `X-Test-User-Id` header in your requests:

| Role   | User ID (X-Test-User-Id)                | Permissions                                      |
| :----- | :-------------------------------------- | :----------------------------------------------- |
| Admin  | `33333333-3333-3333-1111-111111111111` | Full Access (Create Patient, Create Appointment) |
| User   | `33333333-3333-3333-2222-222222222222` | Create Patient/Appointment                       |
| Viewer | `33333333-3333-3333-3333-333333333333` | Read-only (Cannot Create)                        |

**Tenant ID**: `11111111-1111-1111-1111-111111111111`

## 📡 API Examples (cURL)

### List Patients (Cached)
```bash
curl -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
     -H "X-Test-User-Id: 33333333-3333-3333-1111-111111111111" \
     http://localhost:5001/api/Patients
```

### Create Appointment (Event Published)
```bash
curl -X POST http://localhost:5001/api/Appointments \
     -H "Content-Type: application/json" \
     -H "X-Test-User-Id: 33333333-3333-3333-1111-111111111111" \
     -d '{
       "patientId": "df9fbcd4-ef1b-460a-a438-e9195235627d",
       "branchId": "22222222-2222-2222-2222-222222222222",
       "startAt": "2026-04-01T09:00:00Z"
     }'
```

## 🧪 How to Run Tests

### Backend
```bash
cd src/backend/ClinicPOS.Tests
dotnet test
```

### Frontend
```bash
cd src/frontend
npm test
```
