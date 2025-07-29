using System;
using MediatR;
using FluentValidation;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.ActualizarPreparacionDiaria
{
    /// <summary>
    /// Command para actualizar una preparación diaria existente
    /// </summary>
    public class ActualizarPreparacionDiariaCommand : IRequest<Result<PreparacionDiariaDto>>
    {
        /// <summary>
        /// ID de la preparación diaria a actualizar
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// ID del producto que se preparó
        /// </summary>
        public Guid ProductoId { get; set; }
        
        /// <summary>
        /// Cantidad preparada
        /// </summary>
        public int CantidadPreparada { get; set; }
        
        /// <summary>
        /// Cantidad disponible actualmente
        /// </summary>
        public int CantidadDisponible { get; set; }
        
        /// <summary>
        /// ID del chef que realizó la preparación
        /// </summary>
        public Guid ChefId { get; set; }
        
        /// <summary>
        /// Fecha de vencimiento de la preparación
        /// </summary>
        public DateTime FechaVencimiento { get; set; }
        
        /// <summary>
        /// Observaciones adicionales sobre la preparación
        /// </summary>
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Validador para el comando de actualizar preparación diaria
    /// </summary>
    public class ActualizarPreparacionDiariaCommandValidator : AbstractValidator<ActualizarPreparacionDiariaCommand>
    {
        public ActualizarPreparacionDiariaCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID de la preparación diaria es obligatorio");

            RuleFor(x => x.ProductoId)
                .NotEmpty().WithMessage("El ID del producto es obligatorio");

            RuleFor(x => x.CantidadPreparada)
                .GreaterThan(0).WithMessage("La cantidad preparada debe ser mayor que cero");

            RuleFor(x => x.CantidadDisponible)
                .GreaterThanOrEqualTo(0).WithMessage("La cantidad disponible no puede ser negativa");

            RuleFor(x => x.CantidadDisponible)
                .LessThanOrEqualTo(x => x.CantidadPreparada)
                .WithMessage("La cantidad disponible no puede ser mayor que la cantidad preparada");

            RuleFor(x => x.ChefId)
                .NotEmpty().WithMessage("El ID del chef es obligatorio");

            RuleFor(x => x.FechaVencimiento)
                .NotEmpty().WithMessage("La fecha de vencimiento es obligatoria")
                .GreaterThan(DateTime.Now).WithMessage("La fecha de vencimiento debe ser futura");

            RuleFor(x => x.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres");
        }
    }
} 