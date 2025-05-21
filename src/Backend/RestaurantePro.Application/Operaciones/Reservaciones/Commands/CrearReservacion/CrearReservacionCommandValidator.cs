using System;
using FluentValidation;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion
{
    /// <summary>
    /// Validador para el comando CrearReservacionCommand
    /// </summary>
    public class CrearReservacionCommandValidator : AbstractValidator<CrearReservacionCommand>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IReservacionRepository _reservacionRepository;

        public CrearReservacionCommandValidator(
            IClienteRepository clienteRepository,
            IMesaRepository mesaRepository,
            IReservacionRepository reservacionRepository)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
            _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));

            RuleFor(v => v.ClienteId)
                .NotEmpty().WithMessage("El ID del cliente es requerido.")
                .MustAsync(async (clienteId, cancellation) => 
                {
                    var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellation);
                    return cliente != null;
                }).WithMessage("El cliente especificado no existe.");

            RuleFor(v => v.FechaReservacion)
                .NotEmpty().WithMessage("La fecha de reservación es requerida.")
                .GreaterThan(DateTime.Now.AddMinutes(30)).WithMessage("La reservación debe realizarse con al menos 30 minutos de anticipación.")
                .LessThan(DateTime.Now.AddMonths(3)).WithMessage("No se pueden hacer reservaciones con más de 3 meses de anticipación.");

            RuleFor(v => v.DuracionMinutos)
                .GreaterThanOrEqualTo(30).WithMessage("La duración mínima es de 30 minutos.")
                .LessThanOrEqualTo(240).WithMessage("La duración máxima es de 4 horas (240 minutos).");

            RuleFor(v => v.CantidadComensales)
                .GreaterThan(0).WithMessage("La cantidad de comensales debe ser mayor a cero.")
                .LessThanOrEqualTo(20).WithMessage("La cantidad máxima de comensales por reservación es 20.");

            When(v => v.MesaId.HasValue, () =>
            {
                RuleFor(v => v.MesaId)
                    .MustAsync(async (mesaId, cancellation) => 
                    {
                        try
                        {
                            var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId!.Value);
                            return mesa != null;
                        }
                        catch (KeyNotFoundException)
                        {
                            return false;
                        }
                    }).WithMessage("La mesa especificada no existe.")
                    .MustAsync(async (cmd, mesaId, cancellation) =>
                    {
                        if (!mesaId.HasValue) return true;
                        
                        try
                        {
                            var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId.Value);
                            return mesa.Capacidad >= cmd.CantidadComensales;
                        }
                        catch (KeyNotFoundException)
                        {
                            return false;
                        }
                    }).WithMessage("La mesa seleccionada no tiene capacidad suficiente para la cantidad de comensales.")
                    .MustAsync(async (cmd, mesaId, cancellation) =>
                    {
                        if (!mesaId.HasValue) return true;
                        
                        var duracion = TimeSpan.FromMinutes(cmd.DuracionMinutos);
                        return await _reservacionRepository.VerificarDisponibilidadMesaAsync(
                            mesaId.Value, 
                            cmd.FechaReservacion, 
                            cmd.FechaReservacion.TimeOfDay, 
                            cmd.DuracionMinutos, 
                            cancellation);
                    }).WithMessage("La mesa seleccionada no está disponible en el horario solicitado.");
            });

            RuleFor(v => v.Telefono)
                .NotEmpty().WithMessage("El teléfono de contacto es requerido.")
                .MinimumLength(8).WithMessage("El teléfono debe tener al menos 8 caracteres.")
                .MaximumLength(15).WithMessage("El teléfono no puede exceder los 15 caracteres.");

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("El email de contacto es requerido.")
                .EmailAddress().WithMessage("El formato del email no es válido.");

            RuleFor(v => v.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres.");

            RuleFor(v => v.OcasionEspecial)
                .MaximumLength(100).WithMessage("La ocasión especial no puede exceder los 100 caracteres.");

            RuleFor(v => v.SolicitudesEspeciales)
                .MaximumLength(500).WithMessage("Las solicitudes especiales no pueden exceder los 500 caracteres.");
        }
    }
} 