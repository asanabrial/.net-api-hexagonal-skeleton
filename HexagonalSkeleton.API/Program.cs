using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Handler;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// === Hexagonal Architecture Configuration ===
// Following clean architecture principles with explicit service registration

// Core Domain Services (Inner Layer)
builder.Services.AddDomainServices();

// Infrastructure Services (Outer Layer) 
builder.Services.AddSingletons();

// API Layer Configuration
builder.Services.AddSwagger();
builder.Services.AddAuthentication(builder.Configuration);

// CQRS Architecture Configuration
builder.Services.AddCqrsLayer();

// AutoMapper with API profiles
builder.Services.AddAutoMapperProfiles();

builder.Services.AddControllers();

builder.Host.UseSerilog(configureLogger: (context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddAuthorization();

// Exception handling services - Using modular architecture
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOptions();

// Configure CQRS databases - Using updated extension method
builder.Services.AddCqrsDatabases(builder.Configuration);

// CQRS services configuration
builder.Services.AddCqrsServices();

// MassTransit configuration
builder.Services.AddMassTransitWithRabbitMQ(builder.Configuration);

builder.Services.AddRouting(opt =>
{
    opt.LowercaseUrls = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
    // Expose the class for use in integration test
}
