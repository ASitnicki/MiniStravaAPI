using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiniStrava;
using MiniStrava.Repositories;
using MiniStrava.Services;
using MiniStrava.Utils;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =======================
// DB
// =======================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =======================
// Controllers + Swagger
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MiniStrava API",
        Version = "v1"
    });

    // JWT w Swagger UI (Authorize button)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Wpisz: Bearer {twój_token}"
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


// =======================
// DI - repozytoria/serwisy auth (Twoje istniej¹ce)
// =======================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// =======================
// DI - dodatki dla nowych endpointów (Activities/Sync)
// =======================
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<ISyncSessionRepository, SyncSessionRepository>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<ISyncService, SyncService>();

// =======================
// Host / port
// =======================
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// =======================
// JWT Auth
// =======================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// =======================
// Globalne ³apanie b³êdów (400/404 zamiast 500)
// =======================
app.UseApiExceptions();

// =======================
// Swagger
// UI: /api/documentation
// JSON: /api/documentation/v1/swagger.json
// =======================
app.UseSwagger(c =>
{
    c.RouteTemplate = "api/documentation/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/documentation/v1/swagger.json", "MiniStrava API v1");
    c.RoutePrefix = "api/documentation";
});


// =======================
// Auth
// =======================
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// =======================
// Init DB (Docker-friendly) – czeka na SQL Server, tworzy DB,
// odpala db-init/schema.sql tylko jeœli brak tabeli Users
// =======================
await DbInitializer.InitializeAsync(app.Configuration, app.Logger);
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db, app.Configuration, app.Logger);
}

app.Run();
