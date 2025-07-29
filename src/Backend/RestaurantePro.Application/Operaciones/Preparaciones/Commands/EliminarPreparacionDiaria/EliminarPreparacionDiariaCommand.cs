using System;
using MediatR;
using FluentValidation;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.EliminarPreparacionDiaria
{
    /// <summary>
    /// Command para eliminar una preparación diaria
    /// </summary>
    public class EliminarPreparacionDiariaCommand : IRequest<Result>
    {
        /// <summary>
        /// ID de la preparación diaria a eliminar
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Validador para el comando de eliminar preparación diaria
    /// </summary>
    public class EliminarPreparacionDiariaCommandValidator : AbstractValidator<EliminarPreparacionDiariaCommand>
    {
        public EliminarPreparacionDiariaCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID de la preparación diaria es obligatorio");
        }
    }
} 