using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
builder.Services.AddSwaggerGen();

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
// =======================
app.UseSwagger();
app.UseSwaggerUI();

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

app.Run();
