using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HexagonalSkeleton.API;
using System.Linq;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using FluentValidation;
using MediatR;
using AutoMapper;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Test
{
    /// <summary>
    /// Test implementation of IIntegrationEventService that doesn't require MassTransit
    /// </summary>
    public class TestIntegrationEventService : HexagonalSkeleton.Application.Services.IIntegrationEventService
    {
        private readonly ILogger<TestIntegrationEventService> _logger;

        public TestIntegrationEventService(ILogger<TestIntegrationEventService> logger)
        {
            _logger = logger;
        }

        public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class, HexagonalSkeleton.Application.IntegrationEvents.IIntegrationEvent
        {
            // In tests, just log that we would publish the event but don't perform any actual messaging operations
            _logger.LogInformation("Test: Would publish integration event {EventType}", typeof(T).Name);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Test implementation that replaces UserSyncService for tests
    /// Implements the same interface but avoids MongoDB operations
    /// </summary>
    public class TestUserSyncService : HexagonalSkeleton.Domain.Services.IUserSyncService
    {
        private readonly ILogger<TestUserSyncService> _logger;
        private readonly TestUserReadRepository _testReadRepository;

        public TestUserSyncService(ILogger<TestUserSyncService> logger, TestUserReadRepository testReadRepository)
        {
            _logger = logger;
            _testReadRepository = testReadRepository;
        }

        public async Task SyncUserAsync(HexagonalSkeleton.Domain.User user, CancellationToken cancellationToken = default)
        {
            // In tests, sync the user to our test read repository
            _testReadRepository.SimulateUserCreated(user);
            _logger.LogInformation("Test: Synced user {UserId} to test query store", user.Id);
            await Task.CompletedTask;
        }

        public async Task SyncUsersAsync(IEnumerable<HexagonalSkeleton.Domain.User> users, CancellationToken cancellationToken = default)
        {
            foreach (var user in users)
            {
                _testReadRepository.SimulateUserCreated(user);
            }
            _logger.LogInformation("Test: Synced {UserCount} users to test query store", users.Count());
            await Task.CompletedTask;
        }

        public async Task RemoveUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Test: Would remove user {UserId} from query store", userId);
            await Task.CompletedTask;
        }

        public async Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Test: Would deactivate user {UserId} in query store", userId);
            await Task.CompletedTask;
        }

        public async Task UpdateUserLastLoginAsync(Guid userId, DateTime loginTime, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Test: Would update last login for user {UserId} to {LoginTime}", userId, loginTime);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Test implementation of IUserWriteRepository that wraps the production repository
    /// but also syncs created users to the TestUserReadRepository
    /// </summary>
    public class TestUserWriteRepository : HexagonalSkeleton.Domain.Ports.IUserWriteRepository
    {
        private readonly HexagonalSkeleton.Domain.Ports.IUserWriteRepository _wrappedRepository;
        private readonly TestUserReadRepository _testReadRepository;
        private readonly ILogger<TestUserWriteRepository> _logger;

        public TestUserWriteRepository(
            HexagonalSkeleton.Infrastructure.Adapters.Command.UserCommandRepository wrappedRepository,
            TestUserReadRepository testReadRepository,
            ILogger<TestUserWriteRepository> logger)
        {
            _wrappedRepository = wrappedRepository;
            _testReadRepository = testReadRepository;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(HexagonalSkeleton.Domain.User user, CancellationToken cancellationToken = default)
        {
            var userId = await _wrappedRepository.CreateAsync(user, cancellationToken);
            
            // After creating in the command store, sync to our test read repository
            // Use current timestamp for CreatedAt since that's when the user was actually created
            var userWithId = HexagonalSkeleton.Domain.User.Reconstitute(
                userId,
                user.Email.Value,
                user.FullName.FirstName,
                user.FullName.LastName,
                user.Birthdate ?? DateTime.UtcNow.AddYears(-25),
                user.PhoneNumber?.Value ?? "",
                user.Location?.Latitude ?? 0,
                user.Location?.Longitude ?? 0,
                user.AboutMe ?? "",
                user.PasswordSalt,
                user.PasswordHash,
                user.LastLogin,
                DateTime.UtcNow, // Use current time for CreatedAt
                null, // UpdatedAt should be null for new users
                user.DeletedAt,
                user.IsDeleted,
                user.ProfileImageName);

            _testReadRepository.SimulateUserCreated(userWithId);
            _logger.LogInformation("Test: User {UserId} synced to test read repository", userId);
            
            return userId;
        }

        public Task UpdateAsync(HexagonalSkeleton.Domain.User user, CancellationToken cancellationToken = default)
        {
            return _wrappedRepository.UpdateAsync(user, cancellationToken);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _wrappedRepository.DeleteAsync(id, cancellationToken);
        }

        public Task<HexagonalSkeleton.Domain.User?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _wrappedRepository.GetTrackedByIdAsync(id, cancellationToken);
        }

        public Task<HexagonalSkeleton.Domain.User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _wrappedRepository.GetUserByEmailAsync(email, cancellationToken);
        }

        public Task SetLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return _wrappedRepository.SetLastLoginAsync(userId, cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _wrappedRepository.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Test implementation of IUserReadRepository that doesn't require MongoDB
    /// Simulates a simple in-memory store to track users created during tests
    /// </summary>
    public class TestUserReadRepository : HexagonalSkeleton.Domain.Ports.IUserReadRepository
    {
        private readonly Dictionary<Guid, HexagonalSkeleton.Domain.User> _users = new();
        private readonly Dictionary<string, Guid> _emailToId = new();
        private readonly Dictionary<string, Guid> _phoneToId = new();

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_emailToId.ContainsKey(email?.ToLower() ?? ""));
        }

        public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_phoneToId.ContainsKey(phoneNumber ?? ""));
        }

        public Task<List<HexagonalSkeleton.Domain.User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.Values.ToList());
        }

        public Task<HexagonalSkeleton.Domain.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<HexagonalSkeleton.Domain.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (_emailToId.TryGetValue(email?.ToLower() ?? "", out var userId))
            {
                _users.TryGetValue(userId, out var user);
                return Task.FromResult(user);
            }
            return Task.FromResult<HexagonalSkeleton.Domain.User?>(null);
        }

        public Task<HexagonalSkeleton.Domain.ValueObjects.PagedResult<HexagonalSkeleton.Domain.User>> GetUsersAsync(HexagonalSkeleton.Domain.ValueObjects.PaginationParams pagination, CancellationToken cancellationToken = default)
        {
            var allUsers = _users.Values.ToList();
            var result = new HexagonalSkeleton.Domain.ValueObjects.PagedResult<HexagonalSkeleton.Domain.User>(
                allUsers, 
                allUsers.Count, 
                pagination);
            return Task.FromResult(result);
        }

        public Task<HexagonalSkeleton.Domain.ValueObjects.PagedResult<HexagonalSkeleton.Domain.User>> SearchUsersAsync(HexagonalSkeleton.Domain.ValueObjects.PaginationParams pagination, string searchTerm, CancellationToken cancellationToken = default)
        {
            // Simple search implementation for tests - just return all users
            var allUsers = _users.Values.ToList();
            var result = new HexagonalSkeleton.Domain.ValueObjects.PagedResult<HexagonalSkeleton.Domain.User>(
                allUsers, 
                allUsers.Count, 
                pagination);
            return Task.FromResult(result);
        }

        public Task<HexagonalSkeleton.Domain.ValueObjects.PagedResult<HexagonalSkeleton.Domain.User>> GetUsersAsync(HexagonalSkeleton.Domain.Specifications.ISpecification<HexagonalSkeleton.Domain.User> specification, HexagonalSkeleton.Domain.ValueObjects.PaginationParams pagination, CancellationToken cancellationToken = default)
        {
            // For tests, just return all stored users regardless of specification
            // In a real implementation, you would apply the specification logic
            var allUsers = _users.Values.ToList();
            var result = new HexagonalSkeleton.Domain.ValueObjects.PagedResult<HexagonalSkeleton.Domain.User>(
                allUsers, 
                allUsers.Count, 
                pagination);
            return Task.FromResult(result);
        }

        public Task<List<HexagonalSkeleton.Domain.User>> GetUsersAsync(HexagonalSkeleton.Domain.Specifications.ISpecification<HexagonalSkeleton.Domain.User> specification, CancellationToken cancellationToken = default)
        {
            // For tests, just return all stored users regardless of specification
            return Task.FromResult(_users.Values.ToList());
        }

        public Task<int> CountUsersAsync(HexagonalSkeleton.Domain.Specifications.ISpecification<HexagonalSkeleton.Domain.User> specification, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.Count);
        }

        public Task<bool> AnyUsersAsync(HexagonalSkeleton.Domain.Specifications.ISpecification<HexagonalSkeleton.Domain.User> specification, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.Any());
        }

        /// <summary>
        /// Test helper method to simulate a user being created and stored
        /// This would be called when CommandDbContext saves a new user
        /// </summary>
        public void SimulateUserCreated(HexagonalSkeleton.Domain.User user)
        {
            _users[user.Id] = user;
            _emailToId[user.Email.Value.ToLower()] = user.Id;
            if (user.PhoneNumber?.Value != null)
            {
                _phoneToId[user.PhoneNumber.Value] = user.Id;
            }
        }
    }

    /// <summary>
    /// Custom WebApplicationFactory for integration testing
    /// Completely overrides production database configuration with in-memory database
    /// Clean separation - tests handle their own infrastructure needs
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // IMPORTANTE: Remover TODOS los servicios de base de datos después de que se hayan configurado
                RemoveAllDatabaseServices(services);
                
                // Remove MassTransit consumers that cause DI issues in tests
                RemoveMassTransitConsumers(services);
                
                // Configure test-specific in-memory database for Command operations
                services.AddDbContext<CommandDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestCommandDatabase");
                });
                
                // Register test-specific QueryDbContext - will skip index creation due to "Test" in database name
                services.AddSingleton<MongoDB.Driver.IMongoClient>(sp => 
                    new MongoDB.Driver.MongoClient("mongodb://localhost:27017"));
                
                // Register QueryDbContext for both direct usage and consumer dependencies
                services.AddScoped<QueryDbContext>(sp => 
                    new QueryDbContext(sp.GetRequiredService<MongoDB.Driver.IMongoClient>(), "TestDatabase"));
                
                // Also register as the interface type if there is one
                services.AddScoped<HexagonalSkeleton.Infrastructure.Persistence.Query.QueryDbContext>(sp => 
                    sp.GetRequiredService<QueryDbContext>());

                // Register TestUserReadRepository as singleton so it can be shared
                services.AddSingleton<TestUserReadRepository>();

                // Re-register CQRS services for testing with proper mocks
                // First register the actual UserCommandRepository
                services.AddScoped<HexagonalSkeleton.Infrastructure.Adapters.Command.UserCommandRepository>();
                
                // Then register our wrapped version
                services.AddScoped<HexagonalSkeleton.Domain.Ports.IUserWriteRepository, TestUserWriteRepository>();
                
                // Use the singleton TestUserReadRepository for read operations
                services.AddScoped<HexagonalSkeleton.Domain.Ports.IUserReadRepository>(sp => 
                    sp.GetRequiredService<TestUserReadRepository>());
                
                // Use test implementation of UserSyncService that syncs to TestUserReadRepository
                services.AddScoped<HexagonalSkeleton.Domain.Services.IUserSyncService>(sp => 
                    new TestUserSyncService(
                        sp.GetRequiredService<ILogger<TestUserSyncService>>(),
                        sp.GetRequiredService<TestUserReadRepository>()));
                
                // Use test implementation of IIntegrationEventService that avoids MassTransit operations
                services.AddScoped<HexagonalSkeleton.Application.Services.IIntegrationEventService, TestIntegrationEventService>();

                // Add FluentValidation for tests (since it's missing from production config)
                services.AddValidatorsFromAssembly(
                    typeof(HexagonalSkeleton.Application.Features.UserProfile.Commands.UpdateProfileUserCommand).Assembly);

                // Override MediatR configuration to include Application assembly
                services.RemoveAll(typeof(MediatR.IMediator));
                services.RemoveAll(typeof(MediatR.ISender));
                services.RemoveAll(typeof(MediatR.IPublisher));
                
                services.AddMediatR(cfg => {
                    cfg.RegisterServicesFromAssemblies(
                        typeof(Program).Assembly,
                        typeof(HexagonalSkeleton.Application.Features.UserProfile.Commands.UpdateProfileUserCommand).Assembly
                    );
                });

                // Add test-specific AutoMapper configuration
                services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile<HexagonalSkeleton.API.Mapping.ApiMappingProfile>();
                    cfg.AddProfile<TestMappingProfile>();
                });

                // Ensure database is created for tests
                EnsureTestDatabaseCreated(services);
            });
        }

        /// <summary>
        /// Removes ALL database-related services to completely clear the DI container
        /// This is more aggressive and ensures no conflicts between providers
        /// </summary>
        private static void RemoveAllDatabaseServices(IServiceCollection services)
        {
            var descriptorsToRemove = services
                .Where(d => 
                    // DbContext types - but we'll re-register QueryDbContext for tests
                    d.ServiceType == typeof(CommandDbContext) ||
                    d.ServiceType == typeof(DbContextOptions<CommandDbContext>) ||
                    
                    // Generic DbContext services for EF Core 
                    (d.ServiceType.IsGenericType && 
                     (d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) ||
                      d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextFactory<>))) ||
                    
                    // Any EF Core/PostgreSQL services
                    d.ServiceType.FullName?.Contains("EntityFramework") == true ||
                    d.ServiceType.FullName?.Contains("Npgsql") == true ||
                    
                    // Production MongoDB services - but not consumers
                    (d.ServiceType == typeof(MongoDB.Driver.IMongoClient) && 
                     !d.ImplementationType?.FullName?.Contains("Consumer") == true) ||
                    (d.ServiceType == typeof(QueryDbContext) && 
                     !d.ImplementationType?.FullName?.Contains("Consumer") == true) ||
                    
                    // CQRS repository services
                    d.ServiceType.FullName?.Contains("IUserReadRepository") == true ||
                    d.ServiceType.FullName?.Contains("IUserWriteRepository") == true ||
                    d.ImplementationType?.FullName?.Contains("UserReadRepositoryMongoAdapter") == true ||
                    d.ImplementationType?.FullName?.Contains("UserCommandRepository") == true ||
                    
                    // UserSyncService services (but not consumers)
                    (d.ServiceType.FullName?.Contains("IUserSyncService") == true &&
                     !d.ImplementationType?.FullName?.Contains("Consumer") == true) ||
                    (d.ImplementationType?.FullName?.Contains("UserSyncService") == true &&
                     !d.ImplementationType?.FullName?.Contains("Consumer") == true))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }
        }

        /// <summary>
        /// Removes production database-related services to avoid conflicts
        /// Keeps MongoDB services since we configure them for testing
        /// </summary>
        private static void RemoveDbContextServices(IServiceCollection services)
        {
            var descriptorsToRemove = services
                .Where(d => 
                    // Remove PostgreSQL-related DbContext services only
                    d.ServiceType == typeof(DbContextOptions<CommandDbContext>) ||
                    d.ServiceType == typeof(CommandDbContext) ||
                    // Generic DbContext services for EF Core
                    (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                    // PostgreSQL provider services only
                    d.ServiceType.FullName?.Contains("Npgsql") == true ||
                    // MongoDB services that might conflict
                    d.ServiceType == typeof(QueryDbContext) ||
                    d.ServiceType == typeof(MongoDB.Driver.IMongoClient))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }
        }

        /// <summary>
        /// Removes CQRS services that depend on production databases
        /// These will be re-registered with test configurations
        /// </summary>
        private static void RemoveCqrsServices(IServiceCollection services)
        {
            var cqrsDescriptorsToRemove = services
                .Where(d => 
                    // Remove services that depend on QueryDbContext or CommandDbContext
                    d.ImplementationType?.FullName?.Contains("UserReadRepositoryMongoAdapter") == true ||
                    d.ImplementationType?.FullName?.Contains("UserCommandRepository") == true ||
                    d.ImplementationType?.FullName?.Contains("UserSyncService") == true ||
                    d.ServiceType.FullName?.Contains("IUserReadRepository") == true ||
                    d.ServiceType.FullName?.Contains("IUserWriteRepository") == true)
                .ToList();

            foreach (var descriptor in cqrsDescriptorsToRemove)
            {
                services.Remove(descriptor);
            }
        }

        /// <summary>
        /// Removes MassTransit consumers that cause DI issues in tests
        /// Integration tests don't need the message bus consumers
        /// </summary>
        private static void RemoveMassTransitConsumers(IServiceCollection services)
        {
            var consumersToRemove = services
                .Where(d => 
                    d.ImplementationType?.FullName?.Contains("Consumer") == true ||
                    d.ServiceType.FullName?.Contains("Consumer") == true ||
                    d.ServiceType.FullName?.Contains("MassTransit") == true ||
                    d.ServiceType.FullName?.Contains("IIntegrationEventService") == true ||
                    d.ImplementationType?.FullName?.Contains("MassTransitIntegrationEventService") == true ||
                    d.ImplementationType?.FullName?.Contains("MassTransit") == true ||
                    // Remove hosted services from MassTransit
                    (d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) && 
                     d.ImplementationType?.FullName?.Contains("MassTransit") == true))
                .ToList();

            foreach (var descriptor in consumersToRemove)
            {
                services.Remove(descriptor);
            }
        }

        /// <summary>
        /// Ensures the test database is created and ready for use
        /// </summary>
        private static void EnsureTestDatabaseCreated(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            context.Database.EnsureCreated();
        }
    }

    /// <summary>
    /// AutoMapper profile specifically for tests
    /// Includes mappings missing from production that tests need
    /// </summary>
    public class TestMappingProfile : Profile
    {
        public TestMappingProfile()
        {
            // Mapping para el registro de usuarios que falta en producción
            CreateMap<RegisterUserDto, LoginResponse>()
                .ForMember(dest => dest.AccessToken, opt => opt.MapFrom(src => src.AccessToken))
                .ForMember(dest => dest.TokenType, opt => opt.MapFrom(src => src.TokenType))
                .ForMember(dest => dest.ExpiresIn, opt => opt.MapFrom(src => src.ExpiresIn))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));
        }
    }
}
