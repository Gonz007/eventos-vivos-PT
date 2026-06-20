# EventBooking — Sistema de Reservas EventosVivos

Backend completo para gestión de eventos y reservas construido con **.NET 9**, **ASP.NET Core**, **Entity Framework Core 9** y **SQLite**.

---

## Estructura del repositorio

```
EventBooking/
├── eventbooking.db              ← base de datos SQLite (raíz del repo)
├── README.md
├── EventBooking.sln
├── src/
│   ├── EventBooking.Domain/         Entidades, Enums, Excepciones de dominio
│   ├── EventBooking.Application/    Commands, Queries, Handlers, DTOs, Validators (MediatR)
│   ├── EventBooking.Infrastructure/ DbContext, Configuraciones EF, Migraciones, Seed
│   └── EventBooking.API/            Controllers, Middleware, DI, Program.cs
└── tests/
    ├── EventBooking.UnitTests/       Tests de handlers con SQLite in-memory
    └── EventBooking.IntegrationTests/ Tests HTTP con WebApplicationFactory
```

---

## Tecnologías

| Capa | Tecnología |
|---|---|
| Runtime | .NET 9 |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 9 |
| Base de datos | **SQLite** — archivo `eventbooking.db` en raíz del repo |
| CQRS / Mediator | MediatR 12 |
| Validaciones | FluentValidation 11 |
| Documentación | Swagger / OpenAPI (Swashbuckle 7) |
| Tests | xUnit + FluentAssertions + Moq |

---

## Arquitectura

Arquitectura en capas (inspirada en Clean Architecture):

- **Domain** — sin dependencias externas. Entidades, enums y excepciones de dominio puras.
- **Application** — orquestación con MediatR (CQRS). Pipeline de validación via `IPipelineBehavior<,>`.
- **Infrastructure** — persistencia con EF Core + SQLite. Migraciones y seed de datos.
- **API** — capa HTTP: controllers delgados que despachan a MediatR. Middleware global de errores con `ProblemDetails`.

---

## Requisitos previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- No requiere Docker, SQL Server, LocalDB ni ningún motor externo

---

## Ejecución local (3 comandos)

```bash
# 1. Restaurar paquetes NuGet
dotnet restore

# 2. Aplicar migraciones EF Core → crea eventbooking.db en la raíz del repo
dotnet ef database update \
  --project src/EventBooking.Infrastructure \
  --startup-project src/EventBooking.API

# 3. Levantar la API
dotnet run --project src/EventBooking.API
```

> Si `dotnet ef` no está instalado:
> ```bash
> dotnet tool install --global dotnet-ef --version 9.0.0
> ```

La API estará disponible en:
- **http://localhost:5201** — API REST
- **http://localhost:5201/swagger** — Swagger UI interactivo

---

## Base de datos

El archivo `eventbooking.db` se genera **automáticamente en la raíz del repositorio**:

```
EventBooking/
├── eventbooking.db   ← creado aquí por dotnet ef database update (o al primer dotnet run)
├── src/
├── tests/
└── README.md
```

Al iniciar, el servidor:
1. Aplica automáticamente las migraciones pendientes (`MigrateAsync`)
2. Carga el seed inicial de venues si la tabla está vacía

### Venues precargados

| ID | Nombre | Capacidad | Ciudad |
|---|---|---|---|
| 1 | Auditorio Central | 200 | Bogotá |
| 2 | Sala Norte | 50 | Bogotá |
| 3 | Arena Sur | 500 | Medellín |

---

## Endpoints

### Eventos

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/events` | Crear evento |
| `GET` | `/api/events` | Listar con filtros opcionales |
| `GET` | `/api/events/{id}` | Obtener evento por ID |
| `GET` | `/api/events/{id}/report` | Reporte de ocupación |

**Filtros opcionales en `GET /api/events`:**

| Parámetro | Tipo | Valores |
|---|---|---|
| `type` | int | 1=Conference, 2=Workshop, 3=Concert |
| `venueId` | int | ID del venue |
| `status` | int | 1=Active, 2=Cancelled, 3=Completed |
| `search` | string | Búsqueda parcial en título (case-insensitive) |
| `startDateFrom` | DateTime | Rango inicio desde |
| `startDateTo` | DateTime | Rango inicio hasta |

### Reservas

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/reservations` | Crear reserva |
| `GET` | `/api/reservations/{id}` | Consultar reserva |
| `POST` | `/api/reservations/{id}/confirm` | Confirmar pago |
| `POST` | `/api/reservations/{id}/cancel` | Cancelar reserva |

---

## Reglas de negocio

| ID | Regla |
|---|---|
| RN-01 | La capacidad del evento no puede superar la capacidad del venue |
| RN-02 | No pueden existir eventos activos con horarios superpuestos en el mismo venue |
| RN-03 | Eventos en fin de semana (sáb/dom) no pueden iniciar a las 22:00 o después |
| RN-04 | No se permiten reservas con menos de 1 hora antes del inicio del evento |
| RN-05 | Eventos con precio > $100 permiten máximo 10 entradas por reserva |
| RF-03 | Con menos de 24 horas para el inicio, máximo 5 entradas por reserva |
| RN-06 | El estado del evento cambia a `Completed` automáticamente cuando se supera su `EndDate` |
| RN-07 | Cancelación con < 48 horas del evento: `IsLostSale=true`, no se libera capacidad |

---

## Tests

```bash
# Todos los tests (unit + integration)
dotnet test

# Solo unitarios
dotnet test tests/EventBooking.UnitTests

# Solo integración
dotnet test tests/EventBooking.IntegrationTests
```

### Cobertura

| Suite | Tests | Cobertura |
|---|---|---|
| **Unit** | 6 | CreateEvent éxito, venue no encontrado, capacidad excedida, solapamiento de venues, restricción fin de semana ×2 |
| **Integration** | 4 | GET events 200, POST event 201, POST event capacidad excedida 422, GET event inexistente 404 |

Los tests de integración usan `WebApplicationFactory<Program>` con SQLite en memoria completamente aislada — sin afectar `eventbooking.db`.

---

## Manejo de errores

Todos los errores retornan `application/problem+json` (RFC 7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "title": "Business Rule Violation",
  "status": 422,
  "detail": "There is already an active event at this venue during the requested time slot."
}
```

| Excepción | HTTP Status |
|---|---|
| `NotFoundException` | 404 Not Found |
| `DomainException` | 422 Unprocessable Entity |
| `ValidationException` (FluentValidation) | 400 Bad Request |
| Otros | 500 Internal Server Error |
