using System;
using MediatR;
using FluentValidation;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacionDiaria
{
    /// <summary>
    /// Command para crear una nueva preparación diaria
    /// </summary>
    public class CrearPreparacionDiariaCommand : IRequest<Result<PreparacionDiariaDto>>
    {
        /// <summary>
        /// ID del producto que se preparará
        /// </summary>
        public Guid ProductoId { get; set; }
        
        /// <summary>
        /// Cantidad a preparar
        /// </summary>
        public int Cantidad { get; set; }
        
        /// <summary>
        /// ID del chef que realiza la preparación
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
    /// Validador para el comando de crear preparación diaria
    /// </summary>
    public class CrearPreparacionDiariaCommandValidator : AbstractValidator<CrearPreparacionDiariaCommand>
    {
        public CrearPreparacionDiariaCommandValidator()
        {
            RuleFor(x => x.ProductoId)
                .NotEmpty().WithMessage("El ID del producto es obligatorio");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero");

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