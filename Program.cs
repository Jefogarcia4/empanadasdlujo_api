using System.Text;
using EmpanadasDLujo.API.Controllers;
using EmpanadasDLujo.API.Data;
using EmpanadasDLujo.API.Middleware;
using EmpanadasDLujo.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Authentication ──────────────────────────────────────────
// BasicAuth (esquema por defecto) protege el admin; ClientJwt protege el portal de clientes.
builder.Services.AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null)
    .AddJwtBearer(PortalController.ClientScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();

// ─── HttpClient ──────────────────────────────────────────────
builder.Services.AddHttpClient();

// ─── Servicios propios ───────────────────────────────────────
builder.Services.AddScoped<IWhatsAppSender, WhatsAppSender>();

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
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
