using CRMS_API.Domain.Data;
using Microsoft.EntityFrameworkCore;
using CRMS_API.Services.Interfaces;
using CRMS_API.Services.Helpers;
using CRMS_API.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using CRMS_API.Services.Implementations;
using CRMS_API.Api.Hubs;
using CRMS_API.Services.Background;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
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
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IMpesaSimulation, MpesaSimulation>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<IJwtGenerator, JwtGenerator>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

builder.Services.AddHostedService<GpsSimulatorService>();


var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT:Key is missing. Please configure it securely in appsettings.Development.Json");
}
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false; x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        x.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/telemetryHub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OwnerOnly", policy => policy.RequireRole(nameof(userRole.Owner)));
    options.AddPolicy("RenterOnly", policy => policy.RequireRole(nameof(userRole.Renter)));
});

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithOrigins("https://localhost:7269", "http://localhost:5177") 
                  .AllowCredentials();
        });
});


var app = builder.Build();

//seeding super admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();
        var logger = services.GetRequiredService<ILogger<Program>>(); 

        context.Database.Migrate(); 

        // Seed Super Admin
        await SeedSuperAdmin(context, passwordHasher, logger, builder.Configuration); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
}

async Task SeedSuperAdmin(AppDbContext context, IPasswordHasher<User> passwordHasher, ILogger<Program> logger, IConfiguration config)
{
    
    var superAdminEmail = config["SuperAdmin:Email"];
    var superAdminPassword = config["SuperAdmin:Password"]; // CHANGE THIS! Use secrets.json or env vars.

    if (!await context.Users.AnyAsync(u => u.Role == userRole.SuperAdmin))
    {
        var superAdmin = new User
        {
            Name = "Super Admin",
            Email = superAdminEmail,
            Role = userRole.SuperAdmin,
            IsEmailConfirmed = true, 
            IsActive = true,
        };
        superAdmin.PasswordHash = passwordHasher.HashPassword(superAdmin, superAdminPassword);

        context.Users.Add(superAdmin);
        await context.SaveChangesAsync();

        logger.LogInformation("Super Admin user created.");
        
        if (app.Environment.IsDevelopment())
        {
            logger.LogWarning($"Super Admin Email: {superAdminEmail}");
            logger.LogWarning($"Super Admin Initial Password: {superAdminPassword}");
        }
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<TelemetryHub>("/telemetryHub");

app.Run();
