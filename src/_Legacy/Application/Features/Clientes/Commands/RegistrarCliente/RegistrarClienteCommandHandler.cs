using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Clientes.Commands.RegistrarCliente
{
    public class RegistrarClienteCommandHandler : IRequestHandler<RegistrarClienteCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _currentUserService;

        public RegistrarClienteCommandHandler(
            IApplicationDbContext context,
            IDateTime dateTime,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _dateTime = dateTime;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(RegistrarClienteCommand request, CancellationToken cancellationToken)
        {
            // Verificar si el cliente ya existe (por email o teléfono)
            var clienteExistente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == request.Email || c.Telefono == request.Telefono, cancellationToken);

            if (clienteExistente != null)
            {
                throw new ApplicationException("Ya existe un cliente con el mismo email o teléfono.");
            }

            // Obtener el nivel inicial (el de menor orden)
            var nivelInicial = await _context.NivelesFidelizacion
                .Where(n => n.Activo)
                .OrderBy(n => n.Orden)
                .FirstOrDefaultAsync(cancellationToken);

            if (nivelInicial == null)
            {
                throw new ApplicationException("No hay niveles de fidelización activos en el sistema.");
            }

            // Crear el nuevo cliente
            var fechaActual = _dateTime.Now;
            var cliente = new Cliente
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Telefono = request.Telefono,
                FechaNacimiento = request.FechaNacimiento,
                FechaRegistro = fechaActual,
                Observaciones = request.Observaciones,
                Activo = true,
                TotalVisitas = 0,
                TotalGastado = 0,
                PuntosAcumulados = 0,
                PuntosRedimidos = 0,
                CreadoPor = _currentUserService.UserId,
                Creado = fechaActual
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync(cancellationToken);

            // Si se solicitó crear una tarjeta de fidelización
            if (request.CrearTarjeta)
            {
                var tarjeta = new TarjetaFidelizacion
                {
                    ClienteId = cliente.Id,
                    Codigo = GenerarCodigoTarjeta(),
                    FechaEmision = fechaActual,
                    FechaExpiracion = fechaActual.AddYears(2), // Tarjeta válida por 2 años
                    Estado = EstadoTarjeta.Emitida,
                    NivelId = nivelInicial.Id,
                    PuntosAcumulados = 0,
                    PuntosDisponibles = 0,
                    PuntosCanjeados = 0,
                    CreadoPor = _currentUserService.UserId,
                    Creado = fechaActual
                };

                _context.TarjetasFidelizacion.Add(tarjeta);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return cliente.Id;
        }

        private string GenerarCodigoTarjeta()
        {
            // Generar un código único para la tarjeta (FID seguido de 8 dígitos aleatorios)
            var random = new Random();
            var codigo = $"FID-{random.Next(10000000, 99999999)}";
            return codigo;
        }
    }
} 