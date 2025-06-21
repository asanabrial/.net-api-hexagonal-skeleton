using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Handler;
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

builder.Services.AddExceptionHandler<MinimalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOptions();

var connectionStr = builder.Configuration.GetConnectionString("HexagonalSkeleton");
var serverVersion = ServerVersion.AutoDetect(connectionStr);

builder.Services.AddDbContextPool<AppDbContext>(
    dbContextOptions =>
        dbContextOptions.UseMySql(connectionStr, serverVersion, options =>
            options.MigrationsAssembly("HexagonalSkeleton.MigrationDb"))
        // The following three options help with debugging, but should
        // be changed or removed for production.
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);


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