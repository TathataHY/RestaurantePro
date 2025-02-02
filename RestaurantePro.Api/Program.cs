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
builder.Services.AddMediatR(typeof(CreateComandaCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Registrar Validators
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreateComandaCommand).Assembly);

// Registrar Unit of Work y Repositorios
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IComandaRepository, ComandaRepository>();
builder.Services.AddScoped<IMesaRepository, MesaRepository>();

// Registrar Servicios
builder.Services.AddScoped<IComandaService, ComandaService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

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

app.MapControllers();

app.Run();
