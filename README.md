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
├───HexagonalSkeleton.API // API project
│   ├───Config // Configs for the Program.cs
│   ├───Data // Data classes like DBContext
│   ├───Features
│   │   └───User // Feature example
│   │       ├───Application // The application of our feature
│   │       │   ├───Command
│   │       │   ├───Event
│   │       │   └───Query
│   │       ├───Domain // The domain of our feature
│   │       └───Infrastructure // The infrastructure of our feature
│   ├───Handler // Handlers for the API
│   ├───Identity // Identity management of our JWT
│   ├───Migrations // Migrations generated with EF Core
│   └───Properties
├───HexagonalSkeleton.CommonCore // Generic layer, and reusable for other projects
│   ├───Auth
│   ├───Constants
│   ├───Data
│   │   ├───Entity
│   │   ├───Repository
│   │   └───UnitOfWork
│   ├───Event
│   ├───Extension
└───HexagonalSkeleton.Test // Project intended to execute unit and integration tests
    ├───Integration
    │   └───User
    │       └───Infrastructure
    └───Unit
        └───User
            ├───Application
            │   ├───Command
            │   └───Query
            ├───Domain
            └───Infrastructure
```
