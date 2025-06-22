<p align="center"><img src="https://github.com/user-attachments/assets/5d55f501-ed98-4245-a0ef-b620991c35df" alt="dotnet-icon" width="150" /></p>
<h1>Hexagonal Architecture, DDD & CQRS in .NET 9</h1>

<p align="center">
 Example of a <strong> skeleton .NET API application</strong> that uses the principles of Domain-Driven Design (DDD) and Command Query Responsibility Segregation (CQRS).
</p>

## Environment Setup

### Needed tools 🛠️

1. [Install Docker](https://www.docker.com/get-started) 🐋
2. [Install .NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) 🔧
3. Clone this project: `git clone https://github.com/asanabrialopez/.net-api-hexagonal-skeleton.git`
4. Move to the project folder: `cd net-api-hexagonal-skeleton`

### Quick Start 🚀

1. **Start database:**

    ```bash
    docker-compose up -d
    ```

2. **Run migrations:**

    ```bash
    dotnet ef database update --project HexagonalSkeleton.API
    ```

3. **Debug the API:**

    **Visual Studio:**

    - Open the project in Visual Studio
    - Select "Docker Development" profile
    - Press **F5** to start debugging

    **VS Code:**

    - Open the project in VS Code
    - Press **F5** to start debugging
    - The API will automatically start MariaDB and wait for it to be ready

    - API will open at http://localhost:5000/swagger
    - Database runs in Docker on localhost:3306

### 🐳 Docker Development Setup

This project is configured for **hybrid development**:

-   **MariaDB runs in Docker** (easy to manage, no local install needed)
-   **API runs locally** (full debugging capabilities with F5)

**Commands:**

```bash
# Start only database dependencies
docker-compose up -d

# Stop database
docker-compose down

# View database logs
docker-compose logs mariadb

# Check status
docker ps
```

### 🔧 VS Code Setup

**Available Tasks** (Ctrl+Shift+P → "Tasks: Run Task"):

-   `start-database` - Start MariaDB container and wait until health check passes
-   `build` - Build the solution

**Debugging:**

-   Press **F5** to start debugging
-   It will automatically build, start MariaDB, wait for health check, then launch the API
-   Swagger opens automatically

### API configuration ⚙️

All necessary settings are in the appsettings.json of the API. The connection string is configured to connect to MariaDB running in Docker on localhost:3306.

### Hexagonal Architecture

```scala
├───HexagonalSkeleton.API // API Layer - Controllers, Models, Configuration
│   ├───Config // DI Configuration and service extensions
│   ├───Controllers // REST API Controllers
│   ├───Extensions // API-specific extension methods
│   ├───Handler // Exception handlers and middleware
│   ├───Identity // JWT authentication and authorization
│   ├───Mapping // AutoMapper profiles for API models
│   ├───Models // API Request/Response models organized by feature
│   │   ├───Auth // Authentication models (LoginRequest, LoginResponse)
│   │   ├───Users // User management models (CreateUserRequest, UserResponse)
│   │   └───Common // Shared models (BaseApiResponse, PagedResponse)
│   └───Properties // Launch settings
│
├───HexagonalSkeleton.Application // Application Layer - Use Cases, Commands, Queries
│   ├───Command // Commands and handlers (CQRS Write operations)
│   ├───Query // Queries and handlers (CQRS Read operations)
│   ├───Dto // Application DTOs (internal to application layer)
│   ├───Event // Application events
│   ├───EventHandlers // Event handlers
│   ├───Exceptions // Custom application exceptions
│   └───Ports // Interfaces for external dependencies
│
├───HexagonalSkeleton.Domain // Domain Layer - Business Logic, Entities, Value Objects
│   ├───Common // Pure domain concepts (AggregateRoot, DomainEvent)
│   ├───Shared // Technical utilities accessible from all layers
│   │   └───Extensions // Generic extension methods (ListExtension)
│   ├───Events // Domain events
│   ├───Ports // Domain interfaces
│   ├───Services // Domain services
│   ├───ValueObjects // Value objects (Email, FullName, Location, etc.)
│   └───User.cs // User aggregate root
│
├───HexagonalSkeleton.Infrastructure // Infrastructure Layer - External Concerns
│   ├───Adapters // Implementations of domain/application ports
│   ├───Auth // Authentication and JWT services
│   ├───Extensions // Infrastructure-specific extensions
│   ├───Mapping // Entity mapping profiles
│   ├───Persistence // Data access repositories
│   ├───AppDbContext.cs // EF Core DbContext
│   └───UserEntity.cs // EF Core entity mapping
│
├───HexagonalSkeleton.MigrationDb // Database Migrations
│   └───Migrations // EF Core database migrations
│
└───HexagonalSkeleton.Test // Test Layer - Unit and Integration tests
    ├───Integration // End-to-end API tests
    │   └───API
    │       └───Controllers
    └───Unit // Isolated unit tests
        ├───API // API layer tests
        ├───Application // Application layer tests
        ├───Domain // Domain layer tests
        └───CommonCore // Shared utilities tests
```

### Architecture Patterns & Best Practices

This skeleton implements modern .NET API best practices:

#### **Clean Architecture & Hexagonal Pattern**

-   **API Layer**: Only HTTP concerns, delegates to Application layer via MediatR
-   **Application Layer**: Use cases, commands, queries (CQRS pattern)
-   **Domain Layer**: Pure business logic, entities, value objects
-   **Infrastructure Layer**: External dependencies, databases, external services

#### **Exception-Based Error Handling**

-   Custom exceptions map to appropriate HTTP status codes
-   `ExceptionHandler` handles application exceptions globally using a modular mapper system
-   No more `IsValid`/`Errors` patterns - exceptions bubble up naturally

#### **API Model Organization**

-   **Models/Auth**: Authentication-related requests/responses
-   **Models/Users**: User management requests/responses
-   **Models/Common**: Shared base classes and utilities
-   Consistent naming: `*Request` for inputs, `*Response` for outputs

#### **Domain Organization**

-   **Domain.Common**: Pure DDD concepts (AggregateRoot, DomainEvent)
-   **Domain.Shared**: Technical utilities accessible from all layers
-   Clear separation between domain logic and generic utilities

#### **CQRS with MediatR**

-   Commands for write operations
-   Queries for read operations
-   Handlers isolated and testable
-   Clean separation of concerns
