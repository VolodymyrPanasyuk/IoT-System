using System.Text;
using DotNetEnv;
using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Mappers;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using IoT_System.Infrastructure.Extensions;
using IoT_System.Infrastructure.Repositories;
using IoT_System.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// ====== LOAD ENVIRONMENT VARIABLES ======
// Load local environment variables from .env.local first, then fallback to .env
// These variables will overwrite existing environment variables if present
Env.TraversePath().Load(".env.local");
Env.TraversePath().Load(".env");

// ====== CREATE BUILDER ======
var builder = WebApplication.CreateBuilder(args);

// Add environment variables to the configuration
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<AuthDbContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, Role>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// ====== Repositories ======
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<JwtService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// ====== CORS ======
var allowedOrigins = builder.Configuration["CORS__AllowedOrigins"]?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(MappingProfile).Assembly); });

// ====== CONTROLLERS & SWAGGER ======
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ===== Apply Migrations Automatically =====
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();
}

app.UseGlobalExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Use CORS
app.UseCors("DefaultCorsPolicy");

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Run the application
app.Run();