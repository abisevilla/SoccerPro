using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using SoccerPro.Api.Authentication;
using SoccerPro.Api.Middleware;
using SoccerPro.Application;
using SoccerPro.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration));

builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .Validate(options => !string.IsNullOrWhiteSpace(options.Key) && options.Key.Length >= 32,
        "Jwt:Key must contain at least 32 characters.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
    .Validate(options => options.ExpirationMinutes > 0, "Jwt:ExpirationMinutes must be greater than zero.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password),
        "Jwt:Username and Jwt:Password are required.")
    .ValidateOnStart();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("The Jwt configuration section is required.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var connection = new SqliteConnection("Data Source=SoccerPro;Mode=Memory;Cache=Shared");
connection.Open();

builder.Services.AddSingleton(connection);
builder.Services.AddDbContext<SoccerProDbContext>(options => options.UseSqlite(connection));
builder.Services.AddScoped(typeof(IWriteRepository<>), typeof(EfWriteRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IIdempotencyStore, IdempotencyStore>();
builder.Services.AddScoped<IApplicationEventLogger, ApplicationEventLogger>();
builder.Services.AddScoped<ISoccerQueryService, SoccerQueryService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<MatchService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT. Swagger enviara automaticamente el encabezado: Bearer {token}."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });
});
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SoccerProDbContext>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(db, unitOfWork);
}

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
