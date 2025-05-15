using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Clientes.Commands.ActivarTarjeta
{
    public class ActivarTarjetaCommandHandler : IRequestHandler<ActivarTarjetaCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _currentUserService;

        public ActivarTarjetaCommandHandler(
            IApplicationDbContext context,
            IDateTime dateTime,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _dateTime = dateTime;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(ActivarTarjetaCommand request, CancellationToken cancellationToken)
        {
            // Buscar el cliente
            var cliente = await _context.Clientes
                .Include(c => c.TarjetasFidelizacion)
                .FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken);

            if (cliente == null)
            {
                throw new NotFoundException(nameof(Cliente), request.ClienteId);
            }

            // Buscar la tarjeta emitida
            var tarjeta = cliente.TarjetasFidelizacion
                .FirstOrDefault(t => t.Estado == EstadoTarjeta.Emitida);

            if (tarjeta == null)
            {
                // Si no hay tarjeta emitida, verificar si ya hay una activa
                var tarjetaActiva = cliente.TarjetasFidelizacion
                    .FirstOrDefault(t => t.Estado == EstadoTarjeta.Activa);

                if (tarjetaActiva != null)
                {
                    // Ya hay una tarjeta activa, no es necesario hacer nada
                    return true;
                }
                
                // No hay tarjeta emitida ni activa, crear una nueva
                var nivelInicial = await _context.NivelesFidelizacion
                    .Where(n => n.Activo)
                    .OrderBy(n => n.Orden)
                    .FirstOrDefaultAsync(cancellationToken);

                if (nivelInicial == null)
                {
                    throw new ApplicationException("No hay niveles de fidelización activos en el sistema.");
                }

                var fechaActual = _dateTime.Now;
                tarjeta = new TarjetaFidelizacion
                {
                    ClienteId = cliente.Id,
                    Codigo = GenerarCodigoTarjeta(),
                    FechaEmision = fechaActual,
                    FechaActivacion = fechaActual,
                    FechaExpiracion = fechaActual.AddYears(2), // Tarjeta válida por 2 años
                    Estado = EstadoTarjeta.Activa,
                    NivelId = nivelInicial.Id,
                    PuntosAcumulados = 0,
                    PuntosDisponibles = 0,
                    PuntosCanjeados = 0,
                    CreadoPor = _currentUserService.UserId,
                    Creado = fechaActual
                };

                _context.TarjetasFidelizacion.Add(tarjeta);
                
                // Añadir puntos de bienvenida si corresponde
                var puntosIniciales = 100; // Puntos de bienvenida
                
                if (puntosIniciales > 0)
                {
                    tarjeta.PuntosAcumulados += puntosIniciales;
                    tarjeta.PuntosDisponibles += puntosIniciales;
                    
                    // Registrar el movimiento de puntos
                    var historialPuntos = new HistorialPuntos
                    {
                        TarjetaFidelizacionId = tarjeta.Id,
                        ClienteId = cliente.Id,
                        TipoMovimiento = TipoMovimientoPuntos.BonoInicial,
                        Puntos = puntosIniciales,
                        Descripcion = "Bono de bienvenida por activación de tarjeta",
                        Fecha = fechaActual,
                        UsuarioId = _currentUserService.UserId,
                        Creado = fechaActual,
                        CreadoPor = _currentUserService.UserId
                    };
                    
                    _context.HistorialPuntos.Add(historialPuntos);
                }
            }
            else
            {
                // Activar la tarjeta existente
                var fechaActual = _dateTime.Now;
                tarjeta.Estado = EstadoTarjeta.Activa;
                tarjeta.FechaActivacion = fechaActual;
                tarjeta.UltimaModificacion = fechaActual;
                tarjeta.UltimoUsuarioModificacionId = _currentUserService.UserId;
                
                // Si no tiene fecha de expiración, añadir 2 años desde la activación
                if (!tarjeta.FechaExpiracion.HasValue)
                {
                    tarjeta.FechaExpiracion = fechaActual.AddYears(2);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private string GenerarCodigoTarjeta()
        {
            // Generar un código único para la tarjeta
            var random = new Random();
            var codigo = $"FID-{random.Next(10000000, 99999999)}";
            return codigo;
        }
    }
} 