using EmpanadasDLujo.API.Data;
using EmpanadasDLujo.API.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Basic Authentication ────────────────────────────────────
builder.Services.AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);

builder.Services.AddAuthorization();

// ─── Controllers ─────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Swagger with Basic Auth support ─────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Empanadas D'Lujo API",
        Version = "v1",
        Description = "API de gestión de productos, precios y ventas"
    });

    c.AddSecurityDefinition("BasicAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Ingrese usuario y contraseña"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "BasicAuth" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ─── Middleware pipeline ──────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
