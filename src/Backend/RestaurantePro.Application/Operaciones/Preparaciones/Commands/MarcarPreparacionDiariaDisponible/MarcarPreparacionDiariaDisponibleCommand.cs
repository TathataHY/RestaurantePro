using System;
using MediatR;
using FluentValidation;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.MarcarPreparacionDiariaDisponible
{
    /// <summary>
    /// Command para marcar una preparación diaria como disponible
    /// </summary>
    public class MarcarPreparacionDiariaDisponibleCommand : IRequest<Result<PreparacionDiariaDto>>
    {
        /// <summary>
        /// ID de la preparación diaria a marcar como disponible
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Validador para el comando de marcar preparación diaria como disponible
    /// </summary>
    public class MarcarPreparacionDiariaDisponibleCommandValidator : AbstractValidator<MarcarPreparacionDiariaDisponibleCommand>
    {
        public MarcarPreparacionDiariaDisponibleCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID de la preparación diaria es obligatorio");
        }
    }
} 