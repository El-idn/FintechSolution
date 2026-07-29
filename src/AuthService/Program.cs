using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Extensions;
using SharedKernel.Services;
using SharedKernel.Interfaces;
using AuthService.Data;
using AuthService.Repositories;
using AuthService.Services;
using AuthService.Interfaces;
using Serilog;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Constants;
using System.IdentityModel.Tokens.Jwt;
using AuthService.Infrastructure.Identity;
using MassTransit;

BootstrapExtensions.ConfigureSerilog();

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // 👈 prevents ASP.NET from renaming "sub" to "nameidentifier"

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        var username = builder.Configuration["RabbitMq:Username"] ?? "guest";
        var password = builder.Configuration["RabbitMq:Password"] ?? "guest";
        var virtualHost = builder.Configuration["RabbitMq:VirtualHost"] ?? "/";
        cfg.Host(host, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });
        cfg.ConfigureEndpoints(context);
    });
});

// EF Core DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Dependency Injection
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();
builder.Services.AddScoped<IJwtService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new JwtService(config);
});
// builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();
builder.Services.AddSingleton<IEmailSender, LoggingEmailSender>();
builder.Services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuditLogger, AuditLogger>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key is missing");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password policy
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings (optional)
    options.SignIn.RequireConfirmedEmail = true;  // Enforces email verification before login
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();  // Needed for email/token workflows

builder.Services.AddAuthorization(options =>
{
    // Role-based
    options.AddPolicy(PolicyConstants.OnlyAdmins, policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy(PolicyConstants.OnlyCustomers, policy =>
        policy.RequireRole("Customer"));

    options.AddPolicy(PolicyConstants.OnlyAuditors, policy =>
        policy.RequireRole("Auditor"));

    // Claim-based
    options.AddPolicy(PolicyConstants.CanCreateAccount, policy =>
        policy.RequireClaim("Permission", PermissionConstants.CreateAccount));

    options.AddPolicy(PolicyConstants.CanViewAuditLogs, policy =>
        policy.RequireClaim("Permission", PermissionConstants.ViewAuditLogs));

    options.AddPolicy(PolicyConstants.CanAccessPII, policy =>
        policy.RequireClaim("Permission", PermissionConstants.AccessPII));
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedRolesAndAdminAsync(services);
}

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception during startup.");
}
finally
{
    Log.CloseAndFlush();
}