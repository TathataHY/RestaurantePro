using System;
using MediatR;
using FluentValidation;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.ConsumirPreparacionDiaria
{
    /// <summary>
    /// Command para consumir una cantidad específica de una preparación diaria
    /// </summary>
    public class ConsumirPreparacionDiariaCommand : IRequest<Result<PreparacionDiariaDto>>
    {
        /// <summary>
        /// ID de la preparación diaria a consumir
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Cantidad a consumir
        /// </summary>
        public int Cantidad { get; set; }
        
        /// <summary>
        /// Observaciones sobre el consumo
        /// </summary>
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Validador para el comando de consumir preparación diaria
    /// </summary>
    public class ConsumirPreparacionDiariaCommandValidator : AbstractValidator<ConsumirPreparacionDiariaCommand>
    {
        public ConsumirPreparacionDiariaCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID de la preparación diaria es obligatorio");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad a consumir debe ser mayor que cero");

            RuleFor(x => x.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres");
        }
    }
} 