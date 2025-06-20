using System;
using MediatR;
using FluentValidation;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacion
{
    /// <summary>
    /// Command para crear una nueva preparación
    /// </summary>
    public class CrearPreparacionCommand : IRequest<Result<PreparacionDto>>
    {
        /// <summary>
        /// ID de la comanda
        /// </summary>
        public Guid ComandaId { get; set; }
        
        /// <summary>
        /// ID del producto que se preparó
        /// </summary>
        public Guid ProductoId { get; set; }
        
        /// <summary>
        /// Cantidad preparada
        /// </summary>
        public int Cantidad { get; set; }
        
        /// <summary>
        /// Prioridad de la preparación
        /// </summary>
        public int Prioridad { get; set; } = 1;
        
        /// <summary>
        /// Tiempo estimado para la preparación
        /// </summary>
        public TimeSpan? TiempoEstimado { get; set; }
        
        /// <summary>
        /// Observaciones adicionales
        /// </summary>
        public string? Observaciones { get; set; }
        
        /// <summary>
        /// ID del chef responsable
        /// </summary>
        public Guid? ChefId { get; set; }
        
        /// <summary>
        /// Fecha de vencimiento de la preparación
        /// </summary>
        public DateTime? FechaVencimiento { get; set; }
    }

    /// <summary>
    /// Validador para el comando de crear preparación
    /// </summary>
    public class CrearPreparacionCommandValidator : AbstractValidator<CrearPreparacionCommand>
    {
        public CrearPreparacionCommandValidator()
        {
            RuleFor(x => x.ComandaId)
                .NotEmpty().WithMessage("El ID de la comanda es obligatorio");

            RuleFor(x => x.ProductoId)
                .NotEmpty().WithMessage("El ID del producto es obligatorio");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero");

            RuleFor(x => x.Prioridad)
                .GreaterThan(0).WithMessage("La prioridad debe ser mayor que cero");

            RuleFor(x => x.TiempoEstimado)
                .Must(tiempo => tiempo >= TimeSpan.Zero)
                .WithMessage("El tiempo estimado debe ser mayor o igual a cero");

            RuleFor(x => x.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden superar los 500 caracteres");

            RuleFor(x => x.ChefId)
                .NotEmpty().WithMessage("El ID del chef es obligatorio");
        }
    }
} 