using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Data;
using HexagonalSkeleton.API.Handler;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Custom configuration
var configSection = builder.Configuration.GetSection(key: "AppSettings");
var appSettings = configSection.Get<AppSettings>()!;

builder.Services.AddScopes();
builder.Services.AddSingletons();
builder.Services.AddTransients();
builder.Services.AddSwagger();
builder.Services.AddAuthentication(appSettings);

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Host.UseSerilog(configureLogger: (context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOptions();

var connectionStr = builder.Configuration.GetConnectionString("HexagonalSkeleton");
var serverVersion = ServerVersion.AutoDetect(connectionStr);

builder.Services.AddDbContextPool<AppDbContext>(
    dbContextOptions =>
        dbContextOptions.UseMySql(connectionStr, serverVersion)
        // The following three options help with debugging, but should
        // be changed or removed for production.
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);


builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));
builder.Services.AddRouting(opt =>
{
    opt.LowercaseUrls = true;
});

builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();



builder.Services.Configure<AppSettings>(configSection);

var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();
app.UseExceptionHandler();
app.MapSwagger();
app.UseSerilogRequestLogging();

app.Run();

public partial class Program
{
    // Expose the class for use in integration test
}