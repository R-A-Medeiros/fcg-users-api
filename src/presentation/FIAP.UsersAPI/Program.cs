using Api.Configurations.Extensions;
using FIAP.Domain.Security.Tokens;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using FIAP.Infrastructure;
using FIAP.Application;
using FIAP.Infrastructure.DataAccess;
using FIAP.Infrastructure.Security.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration ---
//var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
//builder.Services.Configure<JwtOptions>(jwtSection);
//var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? builder.Configuration["ConnectionStrings__Postgres"];

// --- Database ---
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Auth ---
//builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddScoped<ITokenProvider, HttpContextTokenValue>();

builder.Services.AddHttpContextAccessor();

//builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)));

var signingKey = builder.Configuration.GetValue<string>("Settings:Jwt:SigningKey");

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = new TimeSpan(0),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey!)),
        // Garante que [Authorize(Roles = ...)] enxergue a claim de papel emitida pelo token.
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.Name
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSwagger()
            .AddCorsPolicy()
            .AddOptions(builder.Configuration);

// --- Messaging (MassTransit + RabbitMQ) ---
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

// --- Web ---
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// Semeia um administrador inicial (idempotente; credenciais vêm de Settings:Seed).
await app.Services.SeedAdminAsync();

app.UseApiCorsPolicy();

app.MapOpenApi();                 // /openapi/v1.json (documento nativo)
app.UseApiSwagger();              // Swagger UI em /swagger sobre o doc nativo
app.MapScalarApiReference(o => o.WithOpenApiRoutePattern("/openapi/v1.json")); // /scalar/v1

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
//app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "UsersAPI" }));

app.Run();
