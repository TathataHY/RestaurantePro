using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RestaurantePro.Core.Commands;
using RestaurantePro.Core.Queries;
using MediatR;
using RestaurantePro.Core.Behaviors;
using RestaurantePro.Core.Handlers.CommandHandlers;
using System.Reflection;
using AutoMapper;
using RestaurantePro.Core.Validators;
using RestaurantePro.Core.Services;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Interfaces.Repositories;
using RestaurantePro.Infrastructure.Data;
using RestaurantePro.Infrastructure.Repositories;
using RestaurantePro.Core.Interfaces.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using RestaurantePro.Infrastructure.Services;
using RestaurantePro.Core.Settings;
using Microsoft.AspNetCore.Identity;
using RestaurantePro.Core.Identity;
using Microsoft.AspNetCore.SignalR;
using Hangfire;
using Hangfire.Dashboard;
using RestaurantePro.Core.Models;
using RestaurantePro.Api.Filters;
using RestaurantePro.Core.Interfaces.Hubs;
using RestaurantePro.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurar la cadena de conexión a la base de datos
builder.Services.AddDbContext<RestauranteContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

// Agregar después de builder.Services.AddControllers();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireClaim("Role", "Administrador"));
    options.AddPolicy("RequireMeseroRole", policy => 
        policy.RequireClaim("Role", "Mesero"));
    options.AddPolicy("RequireCocineroRole", policy => 
        policy.RequireClaim("Role", "Cocinero"));
});

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<CreateComandaCommandHandler>();
builder.Services.AddScoped<UpdateComandaCommandHandler>();
builder.Services.AddScoped<DeleteComandaCommandHandler>();
builder.Services.AddScoped<GetComandasQueryHandler>();
builder.Services.AddScoped<GetPendientesComandasQueryHandler>();

// Registrar AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Registrar MediatR y sus comportamientos
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Registrar Validators
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreateComandaCommand).Assembly);

// Registrar validadores
builder.Services.AddValidatorsFromAssembly(typeof(ComandaValidator).Assembly);

// Registrar Unit of Work y Repositorios
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IComandaRepository, ComandaRepository>();
builder.Services.AddScoped<IMesaRepository, MesaRepository>();

// Registrar Servicios
builder.Services.AddScoped<IComandaService, ComandaService>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

// Registrar Handlers
builder.Services.AddScoped<CreateComandaCommandHandler>();
builder.Services.AddScoped<UpdateComandaCommandHandler>();
builder.Services.AddScoped<DeleteComandaCommandHandler>();

// Agregar después de la línea 30:
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<RestauranteContext>()
.AddDefaultTokenProviders();

// Configurar DbContext
builder.Services.AddDbContext<RestauranteContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar Repositorios
builder.Services.AddScoped<IComandaRepository, ComandaRepository>();
builder.Services.AddScoped<IMesaRepository, MesaRepository>();
builder.Services.AddScoped<IPlatoRepository, PlatoRepository>();

// Después de las líneas existentes
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();

// Registrar comportamiento de validación
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Registrar comportamientos adicionales
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

// Configurar caché distribuida
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "RestaurantePro_";
});

// Agregar después de las líneas existentes
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()
               .WithOrigins("http://localhost:5173"); // Ajusta según tu frontend
    });
});

// Después de las líneas existentes
builder.Services.AddSignalR();
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<IComandaHub, ComandaHub>();

// Configurar SignalR con autenticación
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
});

// Después de las líneas existentes
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Después de la línea 103, agregar:
builder.Services.AddScoped<ComandaStateManager>();

// Agregar HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Registrar UserContext
builder.Services.AddScoped<IUserContext, UserContext>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("SignalRPolicy");
app.MapHub<ComandaHub>("/comandaHub");

app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.Run();
