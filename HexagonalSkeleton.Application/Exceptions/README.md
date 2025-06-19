# Exception Hierarchy Documentation

## Simplified Exception Architecture

This document describes the improved exception hierarchy that eliminates redundancy and follows .NET best practices.

## Base Exceptions

### DomainException (Abstract)

-   **Purpose**: Base class for all domain-specific business exceptions
-   **HTTP Mapping**: Typically maps to HTTP 4xx client error codes
-   **Usage**: Use as base for domain-specific exceptions that need custom HTTP status codes

### BusinessException (Abstract)

-   **Purpose**: Base class for business rule violations
-   **HTTP Mapping**: Maps to HTTP 422 Unprocessable Entity
-   **Usage**: Use for violations of business logic rules

## Concrete Exceptions

### ValidationException

-   **Inherits from**: `ArgumentException` (leverages ASP.NET Core auto-mapping)
-   **HTTP Mapping**: Auto-mapped to HTTP 400 Bad Request
-   **Usage**: Input validation errors
-   **Properties**: `Errors` dictionary for detailed validation messages

### NotFoundException

-   **Inherits from**: `KeyNotFoundException` (leverages ASP.NET Core auto-mapping)
-   **HTTP Mapping**: Auto-mapped to HTTP 404 Not Found
-   **Usage**: When requested resources don't exist

### AuthenticationException

-   **Inherits from**: `System.Security.Authentication.AuthenticationException` (leverages ASP.NET Core auto-mapping)
-   **HTTP Mapping**: Auto-mapped to HTTP 401 Unauthorized
-   **Usage**: Authentication failures

### AuthorizationException

-   **Inherits from**: `UnauthorizedAccessException` (leverages ASP.NET Core auto-mapping)
-   **HTTP Mapping**: Auto-mapped to HTTP 403 Forbidden
-   **Usage**: Authorization failures

### BusinessRuleViolationException

-   **Inherits from**: `BusinessException`
-   **HTTP Mapping**: Custom mapped to HTTP 422 Unprocessable Entity
-   **Usage**: Specific business rule violations

### ConflictException

-   **Inherits from**: `DomainException`
-   **HTTP Mapping**: Custom mapped to HTTP 409 Conflict
-   **Usage**: Resource conflicts (e.g., duplicate resources)

### TooManyRequestsException

-   **Inherits from**: `DomainException`
-   **HTTP Mapping**: Custom mapped to HTTP 429 Too Many Requests
-   **Usage**: Rate limiting scenarios

## Exception Handler

### MinimalExceptionHandler

-   **Purpose**: Handles only exceptions that need custom HTTP status codes
-   **Strategy**: Leverages ASP.NET Core's built-in exception-to-HTTP mapping for standard exceptions
-   **Custom Handling**: Only handles `ValidationException`, `BusinessException`, `ConflictException`, and `TooManyRequestsException`

## Key Improvements

1. **Eliminated Redundancy**: Removed duplicate base classes like `ResourceConflictException`
2. **Leveraged Framework**: Used built-in exception types that ASP.NET Core maps automatically
3. **Minimal Handler**: Only handles exceptions that truly need custom mapping
4. **Clear Hierarchy**: Simple and maintainable exception structure
5. **Individual Files**: Each exception in its own file for better organization

## Usage Examples

```csharp
// Validation error
throw new ValidationException("email", "Email is required");

// Resource not found
throw new NotFoundException("User", userId);

// Authentication failure
throw new AuthenticationException("Invalid credentials");

// Authorization failure
throw new AuthorizationException("Insufficient permissions");

// Business rule violation
throw new BusinessRuleViolationException("Cannot delete user with active orders");

// Resource conflict
throw new ConflictException("User with this email already exists");

// Rate limiting
throw new TooManyRequestsException("Rate limit exceeded");
```

## File Organization

All exceptions are located in individual files under:
`HexagonalSkeleton.Application/Exceptions/`

-   `DomainException.cs` - Base classes
-   `ValidationException.cs`
-   `NotFoundException.cs`
-   `AuthenticationException.cs`
-   `AuthorizationException.cs`
-   `BusinessRuleViolationException.cs`
-   `ConflictException.cs`
-   `TooManyRequestsException.cs`
