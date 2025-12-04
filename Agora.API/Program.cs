using Agora.Application.Service;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Agora.Infrastructure;
using Agora.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

// Cấu hình Serilog để ghi vào file
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Thiết lập mức ghi
    .WriteTo.Console() // Ghi log ra console
    .WriteTo.File("Logs/connection_logs.txt", rollingInterval: RollingInterval.Day) // Ghi vào file với mỗi ngày một file mới
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(); // Sử dụng Serilog làm logging provider

    builder.Services.AddDbContext<AgoraDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Agora API", Version = "v1" });

    // Configure Swagger to use JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<Agora.Application.EventHandlers.UserRegisteredEventHandler>();

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing")))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<AgoraDbContext>();
            var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var user = await dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    context.Fail("User no longer exists.");
                    return;
                }

                // Kiểm tra nếu user bị ban
                if (user.Role == -1)
                {
                    context.Fail("User is banned.");
                    return;
                }

                if(context.Principal == null)
                {
                    context.Fail("Invalid token.");
                    return;
                }

                // Kiểm tra role hiện tại trong DB có khớp với claim không
                var roleClaim = context.Principal.FindFirst(ClaimTypes.Role);
                if (roleClaim != null && roleClaim.Value != user.Role.ToString())
                {
                    context.Fail("User role has changed. Please login again.");
                    return;
                }
            }
            else
            {
                context.Fail("Invalid token.");
            }
        }
    };
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


var app = builder.Build();

// Subscribe to events
using (var scope = app.Services.CreateScope())
{
    var eventBus = scope.ServiceProvider.GetRequiredService<Agora.Domain.Interfaces.IEventBus>();
    await eventBus.Subscribe<Agora.Domain.Events.UserRegisteredEvent, Agora.Application.EventHandlers.UserRegisteredEventHandler>();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agora API v1"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}