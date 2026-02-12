using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PoultryDistributionSystem.API.Extensions;
using PoultryDistributionSystem.API.Middleware;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Application.Mappings;
using PoultryDistributionSystem.Application.Services;
using PoultryDistributionSystem.Application.Validators.Auth;
using PoultryDistributionSystem.Application.Validators.Chicken;
using PoultryDistributionSystem.Application.Validators.Farm;
using PoultryDistributionSystem.Application.Validators.Supplier;
using PoultryDistributionSystem.Domain.Interfaces;
using PoultryDistributionSystem.Infrastructure.Data;
using PoultryDistributionSystem.Infrastructure.Repositories;
using PoultryDistributionSystem.Infrastructure.Services;
using PoultryDistributionSystem.Infrastructure.Services.Interfaces;
using Infrastructure = PoultryDistributionSystem.Infrastructure.Services.Interfaces;
using Application = PoultryDistributionSystem.Application.Interfaces;
using InfrastructureServices = PoultryDistributionSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Register UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<IInventoryService, InventoryService>(); // Register before services that depend on it
builder.Services.AddScoped<IChickenService, ChickenService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<ICleanerService, CleanerService>();
builder.Services.AddScoped<IDistributionService, DistributionService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInventoryService, InventoryService>(); // Register before services that depend on it
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IRouteOptimizationService, RouteOptimizationService>();
builder.Services.AddScoped<IForecastingService, ForecastingService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<Infrastructure.IPdfService, InfrastructureServices.PdfService>();

// Register Infrastructure Services
builder.Services.AddScoped<Infrastructure.IJwtTokenService, InfrastructureServices.JwtTokenService>();
builder.Services.AddScoped<Infrastructure.ILoggingService, InfrastructureServices.LoggingService>();
builder.Services.AddScoped<Infrastructure.IEmailService, InfrastructureServices.EmailService>();
builder.Services.AddScoped<Application.IEmailService>(sp =>
{
    var emailService = sp.GetRequiredService<Infrastructure.IEmailService>();
    return new InfrastructureServices.EmailServiceAdapter(emailService);
});
builder.Services.AddScoped<Infrastructure.IFileStorageService, InfrastructureServices.FileStorageService>();
builder.Services.AddScoped<Infrastructure.IPaymentGatewayService>(sp => 
    new InfrastructureServices.PaymentGatewayService(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddScoped<Infrastructure.IOAuthService, InfrastructureServices.OAuthService>();

// Register Application JWT Token Service (adapter)
builder.Services.AddScoped<Application.IJwtTokenService, JwtTokenServiceAdapter>();

// Register Application Payment Gateway Service (adapter)
builder.Services.AddScoped<Application.IPaymentGatewayService, InfrastructureServices.PaymentGatewayServiceAdapter>();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configure FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSupplierValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateFarmValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateChickenValidator>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "PoultryDistributionSystem",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "PoultryDistributionSystem",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Register background services
builder.Services.AddHostedService<InfrastructureServices.NotificationBackgroundService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Poultry Distribution System API",
        Version = "v1",
        Description = "API for managing poultry distribution business operations"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register background services
builder.Services.AddHostedService<InfrastructureServices.NotificationBackgroundService>();

var app = builder.Build();

// Seed database on startup
await app.SeedDatabaseAsync();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Poultry Distribution System API v1");
        c.RoutePrefix = "swagger"; // Swagger UI at https://localhost:7113/swagger
    });
}

// Register background services
builder.Services.AddHostedService<InfrastructureServices.NotificationBackgroundService>();

// Add middleware (CORS must be early in pipeline)
app.UseCors("AllowAll");

app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<AuditMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
