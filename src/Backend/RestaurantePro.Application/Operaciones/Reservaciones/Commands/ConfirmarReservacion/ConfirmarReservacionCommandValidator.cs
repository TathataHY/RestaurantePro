using System;
using FluentValidation;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion
{
    /// <summary>
    /// Validador para el comando ConfirmarReservacionCommand
    /// </summary>
    public class ConfirmarReservacionCommandValidator : AbstractValidator<ConfirmarReservacionCommand>
    {
        private readonly IReservacionRepository _reservacionRepository;

        public ConfirmarReservacionCommandValidator(IReservacionRepository reservacionRepository)
        {
            _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));

            RuleFor(v => v.ReservacionId)
                .NotEmpty().WithMessage("El ID de la reservación es requerido.")
                .MustAsync(async (reservacionId, cancellation) => 
                {
                    var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellation);
                    return reservacion != null;
                }).WithMessage("La reservación especificada no existe.");

            RuleFor(v => v.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres.");

            RuleFor(v => v.CodigoConfirmacion)
                .MaximumLength(20).WithMessage("El código de confirmación no puede exceder los 20 caracteres.");
        }
    }
} 