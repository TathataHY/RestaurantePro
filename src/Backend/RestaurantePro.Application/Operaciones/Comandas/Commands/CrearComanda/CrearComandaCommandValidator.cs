using System;
using FluentValidation;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda
{
    /// <summary>
    /// Validador para el comando CrearComandaCommand
    /// </summary>
    public class CrearComandaCommandValidator : AbstractValidator<CrearComandaCommand>
    {
        private readonly IMesaRepository _mesaRepository;

        public CrearComandaCommandValidator(IMesaRepository mesaRepository)
        {
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));

            RuleFor(v => v.MesaId)
                .NotEmpty().WithMessage("El ID de la mesa es requerido.")
                .MustAsync(async (mesaId, cancellation) => 
                {
                    try 
                    {
                        var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                        return mesa != null;
                    }
                    catch (KeyNotFoundException)
                    {
                        return false;
                    }
                }).WithMessage("La mesa especificada no existe.");

            RuleFor(v => v.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres.");
        }
    }
} 