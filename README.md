# Project Management SaaS

Phase 1 creates the production foundation for a multi-tenant project management SaaS. It intentionally excludes business modules such as projects, issues, boards, workflows, billing, and reporting.

## Architecture

- `src/backend/ProjectManagementSaaS.Api` owns HTTP concerns, versioned controllers, middleware, Swagger, CORS, and health endpoints.
- `src/backend/ProjectManagementSaaS.Application` owns application contracts, validation, security constants, and use-case abstractions.
- `src/backend/ProjectManagementSaaS.Domain` owns dependency-free domain primitives only. No business entities are included in Phase 1.
- `src/backend/ProjectManagementSaaS.Infrastructure` owns EF Core, SQL Server, Identity, JWT creation, refresh-token persistence, seed data, and external integrations.
- `src/frontend/project-management-saas` owns the React/Vite application shell, routing, API client, TanStack Query, Zustand auth state, MUI theme, and protected routes.

## Local Configuration

Copy `.env.example` to `.env` for Docker-based local development, then replace every placeholder secret before starting the stack.

```powershell
docker compose up --build
```

The API requires the following settings from environment variables, user secrets, or an external secret store:

- `ConnectionStrings__DefaultConnection`
- `Authentication__Jwt__SigningKey`
- `Authentication__Jwt__Issuer`
- `Authentication__Jwt__Audience`
- `Cors__AllowedOrigins__0`

Bootstrap admin seed data is disabled unless explicitly enabled through configuration.
