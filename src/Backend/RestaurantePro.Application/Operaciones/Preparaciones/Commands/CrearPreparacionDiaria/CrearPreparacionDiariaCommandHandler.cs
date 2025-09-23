using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacionDiaria
{
    /// <summary>
    /// Manejador para el comando de crear preparación diaria
    /// </summary>
    public class CrearPreparacionDiariaCommandHandler : IRequestHandler<CrearPreparacionDiariaCommand, Result<PreparacionDiariaDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<CrearPreparacionDiariaCommandHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public CrearPreparacionDiariaCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IDateTimeService dateTimeService,
            ILogger<CrearPreparacionDiariaCommandHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeService = dateTimeService;
            _logger = logger;
        }

        /// <summary>
        /// Maneja el comando para crear una nueva preparación diaria
        /// </summary>
        public async Task<Result<PreparacionDiariaDto>> Handle(
            CrearPreparacionDiariaCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("🍳 Creando preparación diaria - Producto: {ProductoId}, Cantidad: {Cantidad}, Chef: {ChefId}", 
                request.ProductoId, request.Cantidad, request.ChefId);

            try
            {
                // Verificar que el producto existe
                var producto = await _context.Productos.FindAsync(new object[] { request.ProductoId }, cancellationToken);
                if (producto == null)
                {
                    _logger.LogWarning("⚠️ Producto no encontrado: {ProductoId}", request.ProductoId);
                    return Result.Failure<PreparacionDiariaDto>($"Producto no encontrado: {request.ProductoId}");
                }

                // 🔧 CORRECCIÓN: Ajustar fecha de vencimiento si es 00:00:00 a 23:59:59
                var fechaVencimiento = request.FechaVencimiento;
                if (fechaVencimiento.TimeOfDay == TimeSpan.Zero)
                {
                    fechaVencimiento = fechaVencimiento.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    _logger.LogInformation("🕐 Ajustando fecha de vencimiento de 00:00:00 a 23:59:59 - Fecha original: {FechaOriginal}, Fecha ajustada: {FechaAjustada}", 
                        request.FechaVencimiento, fechaVencimiento);
                }

                // Crear la preparación diaria usando el factory method del dominio
                var preparacion = PreparacionDiaria.Crear(
                    request.ProductoId,
                    request.Cantidad,
                    request.ChefId,
                    fechaVencimiento, // Usar la fecha ajustada
                    request.Observaciones,
                    _dateTimeService.Now);

                // Guardar en la base de datos
                await _context.PreparacionesDiarias.AddAsync(preparacion, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Mapear a DTO
                var preparacionDto = _mapper.Map<PreparacionDiariaDto>(preparacion);

                _logger.LogInformation("✅ Preparación diaria creada exitosamente: {PreparacionId}", preparacion.Id);

                return Result.Success(preparacionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear preparación diaria");
                return Result.Failure<PreparacionDiariaDto>($"Error al crear preparación diaria: {ex.Message}");
            }
        }
    }
} 