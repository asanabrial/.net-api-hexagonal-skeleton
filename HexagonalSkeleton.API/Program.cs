using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Handler;
using HexagonalSkeleton.API.Handler.ExceptionMapping;
using HexagonalSkeleton.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddScopes();
builder.Services.AddSingletons();
builder.Services.AddTransients();
builder.Services.AddSwagger();
builder.Services.AddAuthentication(builder.Configuration);
// AutoMapper with API profiles
builder.Services.AddAutoMapperProfiles();

builder.Services.AddControllers();

builder.Host.UseSerilog(configureLogger: (context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddAuthorization();

// Exception handling services - Using modular architecture
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

// Exception mapping services - Using Singleton lifetime
builder.Services.AddSingleton<ExceptionMappingService>();
builder.Services.AddSingleton<IExceptionMapper, DomainExceptionMapper>();
builder.Services.AddSingleton<IExceptionMapper, ApplicationExceptionMapper>();
builder.Services.AddSingleton<IExceptionMapper, ValidationExceptionMapper>();

builder.Services.AddOptions();

// Configure production database with MySQL
builder.Services.AddProductionDatabase(builder.Configuration);

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssemblyContaining<HexagonalSkeleton.Application.Command.RegisterUserCommand>();
});

builder.Services.AddValidatorsFromAssemblyContaining<HexagonalSkeleton.Application.Command.RegisterUserCommand>();
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