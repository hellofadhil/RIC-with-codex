using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Core.Models;
using Core.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnePro.API.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// DB
builder.Services.AddDbContext<OneProDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DbContextConnection"))
);

// Controller
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Register repositories
builder.Services.AddConfigureServices();

// Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// JWT
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Key:Jwt"]!)
            ),
            RoleClaimType = "role",
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "Enter your JWT token",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    },
                },
                new List<string>()
            },
        }
    );
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OnePro API");
    c.RoutePrefix = string.Empty;
});

// middleware
app.UseCors("AllowOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
