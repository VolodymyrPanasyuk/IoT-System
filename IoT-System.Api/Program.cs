using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// ====== JWT ======
var key = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
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

// ====== CONTROLLERS & SWAGGER ======
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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