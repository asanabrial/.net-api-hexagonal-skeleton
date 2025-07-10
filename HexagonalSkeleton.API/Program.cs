using System.Reflection;
using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Handler;
using HexagonalSkeleton.API.Handler.ExceptionMapping;
using HexagonalSkeleton.Application.Features.UserRegistration.Commands;
using MediatR;
using Serilog;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddScopes();
builder.Services.AddSingletons();
builder.Services.AddTransients();
builder.Services.AddSwagger();
builder.Services.AddAuthentication(builder.Configuration);

// Register MediatR and FluentValidation
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

// Add CQRS sync handlers only if query database is enabled
var queryConnectionString = builder.Configuration.GetConnectionString("DefaultQueryConnection");
if (!string.IsNullOrEmpty(queryConnectionString))
{
    builder.Services.AddCqrsSyncHandlers();
}

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Add consumers from Infrastructure assembly
    x.AddConsumersFromNamespaceContaining<HexagonalSkeleton.Infrastructure.Consumers.UserCreatedConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "hexagonal_vhost", h =>
        {
            h.Username("hexagonal_user");
            h.Password("hexagonal_password");
        });
        
        cfg.ConfigureEndpoints(context);
    });
});

// Register Integration Event Service
builder.Services.AddScoped<HexagonalSkeleton.Application.Services.IIntegrationEventService, 
    HexagonalSkeleton.Application.Services.MassTransitIntegrationEventService>();

builder.Services.AddRouting(opt =>
{
    opt.LowercaseUrls = true;
});

builder.Services.AddOptions();
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