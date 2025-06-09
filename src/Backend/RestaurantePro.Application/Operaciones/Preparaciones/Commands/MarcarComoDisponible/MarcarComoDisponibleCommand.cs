using System;
using FluentValidation;
using MediatR;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.MarcarComoDisponible
{
    /// <summary>
    /// Comando para marcar una preparación como disponible para consumo
    /// </summary>
    public class MarcarComoDisponibleCommand : IRequest<Result>
    {
        /// <summary>
        /// ID de la preparación a marcar como disponible
        /// </summary>
        public Guid PreparacionId { get; set; }
        
        /// <summary>
        /// Observaciones adicionales sobre el cambio de estado
        /// </summary>
        public string Observaciones { get; set; }
    }

    /// <summary>
    /// Validador para el comando de marcar como disponible
    /// </summary>
    public class MarcarComoDisponibleCommandValidator : AbstractValidator<MarcarComoDisponibleCommand>
    {
        /// <summary>
        /// Constructor del validador
        /// </summary>
        public MarcarComoDisponibleCommandValidator()
        {
            RuleFor(v => v.PreparacionId)
                .NotEmpty().WithMessage("Se requiere el ID de la preparación");
        }
    }
} 