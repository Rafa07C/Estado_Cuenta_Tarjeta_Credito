using CreditCardStatement.Api.Hubs;
using CreditCardStatement.Api.Infrastructure;
using CreditCardStatement.Api.Middleware;
using CreditCardStatement.Core.Interfaces;
using MediatR;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CreditCard Statement API",
        Version = "v1",
        Description = "REST API for Credit Card Statement management"
    });
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(CreditCardStatement.Api.Mappings.MappingProfile));

// MediatR
builder.Services.AddMediatR(typeof(Program));

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Healthcheck
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

// CORS (para que el MVC pueda consumir la API) - SignalR requiere AllowCredentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvc", policy =>
        policy.WithOrigins(
                "https://localhost:7063",
                "http://localhost:5063",
                "https://creditcard-mvc-rafael-bvfrbpf0bsf0ayak.canadacentral-01.azurewebsites.net"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger habilitado también en Production (Azure)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CreditCard Statement API v1");
    c.RoutePrefix = "swagger";
});

// Redirección a Swagger en la raíz
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseCors("AllowMvc");
app.UseAuthorization();
app.MapControllers();

// Healthcheck endpoint
app.MapHealthChecks("/health");

// SignalR Hub
app.MapHub<TransactionHub>("/hubs/transactions");

app.Run();