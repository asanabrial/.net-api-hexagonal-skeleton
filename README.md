<img src="https://github.com/user-attachments/assets/5d55f501-ed98-4245-a0ef-b620991c35df" alt="dotnet-icon" width="100" style="display: inline;vertical-align: middle;padding-right:20px;"/>
<h1 style="display: inline;vertical-align: middle;">Hexagonal Architecture, DDD & CQRS in .NET 8</h1>

<p align="center" style="padding-left:120px">
 Example of a <strong> skeleton .NET API application</strong> that uses the principles of Domain-Driven Design (DDD) and Command Query Responsibility Segregation (CQRS).
</p>

## Environment Setup

### Needed tools 🛠️

1. [Install Docker](https://www.docker.com/get-started) 🐋
2. Clone this project: `git clone https://github.com/asanabrialopez/.net-api-hexagonal-skeleton.git`
3. Move to the project folder: `cd net-api-hexagonal-skeleton`
4. [Install image of mariadb](https://hub.docker.com/_/mariadb), create the DB and configure the connection string of API.
5. Run the migration: `dotnet ef database update --project HexagonalSkeleton.API`

### API configuration ⚙️

All necessary settings are in the appsettings.json of the API. This includes configuration for token signing and connection string.

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
