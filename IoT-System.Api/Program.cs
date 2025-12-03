using System.Text;
using DotNetEnv;
using IoT_System.Application.Common;
using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Application.Mappers;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using IoT_System.Infrastructure.Data;
using IoT_System.Infrastructure.Hubs;
using IoT_System.Infrastructure.Middlewares;
using IoT_System.Infrastructure.Repositories.Auth;
using IoT_System.Infrastructure.Repositories.IoT;
using IoT_System.Infrastructure.Services;
using IoT_System.Infrastructure.Services.Auth;
using IoT_System.Infrastructure.Services.IoT;
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
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(defaultConnectionString));
builder.Services.AddDbContext<IoTDbContext>(options => options.UseNpgsql(defaultConnectionString));

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

// ====== AUTH REPOSITORIES ======
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupRoleRepository, GroupRoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserGroupRepository, UserGroupRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();

// ====== IOT REPOSITORIES ======
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceFieldRepository, DeviceFieldRepository>();
builder.Services.AddScoped<IFieldMappingRepository, FieldMappingRepository>();
builder.Services.AddScoped<IMeasurementDateMappingRepository, MeasurementDateMappingRepository>();
builder.Services.AddScoped<IDeviceMeasurementRepository, DeviceMeasurementRepository>();
builder.Services.AddScoped<IFieldMeasurementValueRepository, FieldMeasurementValueRepository>();
builder.Services.AddScoped<IDeviceAccessPermissionRepository, DeviceAccessPermissionRepository>();

// ====== AUTH SERVICES ======
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IAccessValidationService, AccessValidationService>();

// ====== IOT SERVICES ======
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IDeviceFieldService, DeviceFieldService>();
builder.Services.AddScoped<IFieldMappingService, FieldMappingService>();
builder.Services.AddScoped<IMeasurementDateMappingService, MeasurementDateMappingService>();
builder.Services.AddScoped<IDeviceMeasurementService, DeviceMeasurementService>();
builder.Services.AddScoped<IDeviceAccessPermissionService, DeviceAccessPermissionService>();
builder.Services.AddScoped<IDataParsingService, DataParsingService>();
builder.Services.AddScoped<IThresholdService, ThresholdService>();
builder.Services.AddScoped<IIoTHubService, IoTHubService>();

// ====== SINGLETONS ======
builder.Services.AddSingleton<ThresholdTrackingService>();

// ====== HTTP CLIENT ======
builder.Services.AddHttpClient("HealthCheck", client => { client.Timeout = TimeSpan.FromSeconds(30); });

// ====== BACKGROUND SERVICES ======
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<HealthCheckBackgroundService>();
}

// ====== JWT AUTHENTICATION ======
builder.Services.AddAuthentication(options =>
    {
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

        // SignalR token from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
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

// ====== SIGNALR ======
builder.Services.AddSignalR();

// ====== AUTOMAPPER ======
builder.Services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(AuthMappingProfile).Assembly); });

// ====== CONTROLLERS & SWAGGER ======
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // ====== IoT Identity API ======
    options.SwaggerDoc(Constants.SwaggerGroups.Identity, new OpenApiInfo
    {
        Version = "v1",
        Title = "IoT Identity API"
    });

    // ====== IoT System API ======
    options.SwaggerDoc(Constants.SwaggerGroups.System, new OpenApiInfo
    {
        Version = "v1",
        Title = "IoT System API"
    });

    // ====== IoT External API ======
    options.SwaggerDoc(Constants.SwaggerGroups.External, new OpenApiInfo
    {
        Version = "v1",
        Title = "IoT External API"
    });

    // ====== JWT Bearer Authentication (for Identity and System APIs) ======
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Used for Identity and System APIs."
    });

    // ====== API Key Authentication (for External API) ======
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = Constants.ApiHeaders.ApiKey,
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "API Key for IoT device authentication. Used for External API."
    });

    // Apply conditional authorization filter
    options.OperationFilter<SwaggerAuthorizeOperationFilter>();
});

var app = builder.Build();

// ====== APPLY MIGRATIONS & SEED DATA ======
await DatabaseMigrationHelper.MigrateAllDatabasesAsync(app.Services);
await DatabaseSeeder.SeedAsync(app.Services);

// ====== MIDDLEWARE PIPELINE ======
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow })).AllowAnonymous();

// ====== SWAGGER UI ======
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // IoT Identity API
    options.SwaggerEndpoint($"/swagger/{Constants.SwaggerGroups.Identity}/swagger.json", "IoT Identity API v1");

    // IoT System API
    options.SwaggerEndpoint($"/swagger/{Constants.SwaggerGroups.System}/swagger.json", "IoT System API v1");

    // IoT External API
    options.SwaggerEndpoint($"/swagger/{Constants.SwaggerGroups.External}/swagger.json", "IoT External API v1");

    options.DocumentTitle = "IoT System APIs - Swagger UI";
    options.RoutePrefix = "swagger";

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

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("DefaultCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

// ====== SIGNALR HUB ======
app.MapHub<IoTHub>("/hubs/iot");

app.MapControllers();

app.Run();