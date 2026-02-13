# AI PROMPTS & Development Iterations

## 1. Primary Objectives & Prompts

### Phase 1: Foundation & Section A
- **Prompt**: "Initialize a .NET 10 solution and Next.js frontend. Implement multi-tenancy using a shared database and schema. Create a 'Patients' entity with a 'PrimaryBranchId'. Ensure every database query is automatically scoped by TenantId."
- **AI Output Acceptance**: Accepted the use of `ITenantEntity` interface and Global Query Filters in EF Core.
- **Validation**: Manual cURL requests with different `X-Tenant-Id` headers proved data isolation.

### Phase 2: Section B (RBAC & Many-to-Many)
- **Prompt**: "Implement Role-Based Access Control for Admin, User, and Viewer. A user can belong to multiple branches. Create a custom Authentication Handler to support testing via X-Test-User-Id."
- **AI Output Rejection**: Initial suggestion used a simple Middleware to set roles. **Rejected** in favor of a proper `AuthenticationHandler` and `AuthorizationPolicies` to ensure it integrates correctly with standard ASP.NET `[Authorize]` attributes.
- **Validation**: Attempted to manage users with a 'Viewer' identity and confirmed receiving `403 Forbidden`.

### Phase 3: Section C & D (Logic & Perf)
- **Prompt**: "Implement Appointments with a composite unique index on Tenant, Branch, Patient, and StartTime for duplicate prevention. Add Redis caching for the patient list, ensuring keys are tenant-scoped."
- **AI Output Iteration**: The first caching implementation used a generic key. I **iterated** to include the `TenantId` and `BranchId` in the key string to prevent cache poisoning across clinics.
- **Validation**: Verified with `time curl` that the second request was significantly faster than the first.

## 2. Decision Logic & Validation

### Why Global Query Filters?
Instead of adding `.Where(t => t.TenantId == ...)` to every controller, I chose filters in `OnModelCreating`.
- **Validation**: Created a unit test `TenantScopingTests.cs` that explicitly checks if a direct `context.Patients.ToListAsync()` call leaks data. It passed, proving the filter works globally.

### Why MassTransit In-Memory?
During the RabbitMQ integration, the local environment had credential issues (`ACCESS_REFUSED`).
- **Decision**: Switched to `UsingInMemory()` while keeping all Messaging contracts (Events, PublishEndpoint) identical.
- **AI Judgment**: It was better to provide a 100% runnable, bug-free messaging flow for the "Create Appointment" feature than to leave a crashing RabbitMQ connection in the final submission.

## 3. Correctness Checklist
- [x] **EF Core Migrations**: Run automatically on startup? Verified via `context.Database.Migrate()` in `Program.cs`.
- [x] **Docker Port Mappings**: No conflicts? Backend on 5001 (host) / 5000 (container) to avoid common 5000 macOS AirPlay conflicts.
- [x] **Constraint Handling**: Duplicate phone/appointment? Verified with 409 Conflict responses.
