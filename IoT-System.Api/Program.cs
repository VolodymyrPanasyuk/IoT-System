using System.Text;
using DotNetEnv;
using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Interfaces.Services;
using IoT_System.Application.Mappers;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using IoT_System.Infrastructure.Data;
using IoT_System.Infrastructure.Middlewares;
using IoT_System.Infrastructure.Repositories;
using IoT_System.Infrastructure.Services;
using IoT_System.Infrastructure.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// ====== LOAD ENVIRONMENT VARIABLES ======
Env.TraversePath().Load(".env.local");
Env.TraversePath().Load(".env");

// ====== CREATE BUILDER ======
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// ====== DATABASE ======
builder.Services.AddDbContext<AuthDbContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ====== IDENTITY ======
builder.Services.AddIdentityCore<User>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager();

// ====== REPOSITORIES ======
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupRoleRepository, GroupRoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserGroupRepository, UserGroupRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();


// ====== SERVICES ======
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<JwtService>();

// ====== JWT AUTHENTICATION ======
builder.Services.AddAuthentication(options =>
    {
        // Explicitly set JWT Bearer as default scheme for all auth operations
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
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
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? [];

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

// ====== AUTOMAPPER ======
builder.Services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(AuthMappingProfile).Assembly); });

// ====== CONTROLLERS & SWAGGER ======
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // API Information
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "IoT System API"
    });

    // JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    // Apply conditional authorization filter
    options.OperationFilter<SwaggerAuthorizeOperationFilter>();
});

var app = builder.Build();

// ====== APPLY MIGRATIONS & SEED DATA ======
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();

    // Seed initial data (roles and super admin user)
    await DatabaseSeeder.SeedAsync(app.Services);
}

// ====== MIDDLEWARE PIPELINE ======
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .AllowAnonymous();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "IoT System API v1");
    options.DocumentTitle = "IoT System API - Swagger UI";
    options.RoutePrefix = "swagger";

    // UI customization
    options.DefaultModelsExpandDepth(10);
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DisplayRequestDuration();
    options.EnableDeepLinking();
    options.EnableFilter();
    options.ShowExtensions();
    options.EnableTryItOutByDefault();

    if (builder.Environment.IsDevelopment())
    {
        options.EnablePersistAuthorization();
    }
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("DefaultCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();