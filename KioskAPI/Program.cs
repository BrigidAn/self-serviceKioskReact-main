using KioskAPI.Data;
using KioskAPI.interfaces;
using KioskAPI.Models;
using KioskAPI.Repository;
using KioskAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/kioskapi-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<CloudinaryService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

// ===== Database Context =====
// Secrets come from user-secrets (local dev) or environment variables (Docker / hosting) — never appsettings.json.
const string Placeholder = "SET_IN_USER_SECRETS_OR_ENV";

string RequireSetting(string key)
{
  var value = builder.Configuration[key];
  if (string.IsNullOrWhiteSpace(value) || value == Placeholder)
  {
    throw new InvalidOperationException(
      $"Missing configuration '{key}'. Set it with `dotnet user-secrets set \"{key}\" \"<value>\"` " +
      $"or the environment variable '{key.Replace(":", "__")}'. See README.md.");
  }

  return value;
}

var connectionString = RequireSetting("ConnectionStrings:DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, Role>(options =>
{
  options.Password.RequireDigit = true;
  options.Password.RequiredLength = 8;
  options.Password.RequireNonAlphanumeric = false;
  options.Password.RequireUppercase = false;
  options.Password.RequireLowercase = true;
  options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<ITokenService, TokenService>();

var jwtKey = RequireSetting("Jwt:Key");
if (jwtKey.Length < 32)
{
  throw new InvalidOperationException("Jwt:Key must be at least 32 characters long.");
}

var key = Encoding.UTF8.GetBytes(jwtKey);

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
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateIssuerSigningKey = true,
    ValidateLifetime = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ClockSkew = TimeSpan.Zero
  };
});

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "KioskAPI API", Version = "v1" });

  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Description = "Enter JWT token like: **Bearer YOUR_TOKEN_HERE**",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT"
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

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowReactApp", policy =>
      policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// Optional: apply EF Core migrations and load demo data at startup (used by the Docker demo).
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
  using var scope = app.Services.CreateScope();
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  db.Database.Migrate();

  if (app.Configuration.GetValue<bool>("Database:SeedDemoData"))
  {
    // The script only inserts when the database has no users/products, so it is safe on every start.
    var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "DemoData.sql");
    db.Database.ExecuteSqlRaw(File.ReadAllText(seedPath));
    Log.Information("Demo data seed script executed ({SeedPath})", seedPath);
  }
}

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.Run();
