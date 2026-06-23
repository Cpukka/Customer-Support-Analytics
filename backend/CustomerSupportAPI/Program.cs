using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using CustomerSupportAPI.Data;
using CustomerSupportAPI.Repositories;
using CustomerSupportAPI.Services;
using CustomerSupportAPI.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 🔧 FIX: Use Railway's PORT or fallback to 8080
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 Starting application on port: {port}");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 🔧 FIX: Use the dynamic port
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Customer Support Analytics API",
        Version = "v1",
        Description = "AI-Powered Customer Support Analytics Platform",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Support Team",
            Email = "support@company.com"
        }
    });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Redis Cache (Optional - will work without Redis)
try
{
    var redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "CustomerSupportAPI";
        });
        Console.WriteLine("✅ Redis cache configured");
    }
    else
    {
        Console.WriteLine("⚠️ Redis not configured - using in-memory cache");
        builder.Services.AddMemoryCache();
    }
}
catch
{
    Console.WriteLine("⚠️ Redis configuration failed - using in-memory cache");
    builder.Services.AddMemoryCache();
}

// Add Authentication with JWT
try
{
    var secret = builder.Configuration["JwtSettings:Secret"];
    if (!string.IsNullOrEmpty(secret) && secret.Length >= 32)
    {
        var key = Encoding.ASCII.GetBytes(secret);
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                ClockSkew = TimeSpan.Zero
            };
        });
        Console.WriteLine("✅ JWT Authentication configured");
    }
    else
    {
        Console.WriteLine("⚠️ JWT not configured - authentication disabled");
    }
}
catch
{
    Console.WriteLine("⚠️ JWT configuration failed - authentication disabled");
}

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS - Allow all for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000", "http://localhost:3001", "http://localhost:5000", "http://localhost:8000" };
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add Repositories and Services
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddHttpClient<IAIService, AIService>(client =>
{
    var aiUrl = builder.Configuration["AIService:Url"] ?? "http://localhost:8000";
    client.BaseAddress = new Uri(aiUrl);
    client.Timeout = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>("AIService:TimeoutSeconds", 30)
    );
    Console.WriteLine($"✅ AI Service configured at: {aiUrl}");
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Response Compression
builder.Services.AddResponseCompression();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS - AllowAll for development
app.UseCors("AllowAll");

// Use Authentication & Authorization
try
{
    app.UseAuthentication();
    app.UseAuthorization();
    Console.WriteLine("✅ Authentication/Authorization middleware configured");
}
catch
{
    Console.WriteLine("⚠️ Authentication/Authorization middleware not configured");
}

// Use Response Compression
app.UseResponseCompression();

// Custom Middleware
try
{
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    Console.WriteLine("✅ Custom middleware configured");
}
catch
{
    Console.WriteLine("⚠️ Custom middleware not found - skipping");
}

// Map Controllers
app.MapControllers();

// Health Check Endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Ensure Database is created
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.CanConnect();
        Console.WriteLine("✅ Database connection successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database connection failed: {ex.Message}");
        Console.WriteLine("⚠️ Please check your connection string and make sure PostgreSQL is running.");
    }
}

app.Run();

// Ensure proper shutdown
AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();