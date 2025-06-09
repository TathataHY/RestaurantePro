using System;
using MediatR;
using FluentValidation;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacion
{
    /// <summary>
    /// Comando para crear una nueva preparación diaria
    /// </summary>
    public class CrearPreparacionCommand : IRequest<Guid>
    {
        /// <summary>
        /// ID del producto que se preparó
        /// </summary>
        public Guid ProductoId { get; set; }
        
        /// <summary>
        /// Cantidad preparada
        /// </summary>
        public int Cantidad { get; set; }
        
        /// <summary>
        /// ID del chef responsable
        /// </summary>
        public Guid ChefId { get; set; }
        
        /// <summary>
        /// Fecha y hora de vencimiento
        /// </summary>
        public DateTime FechaVencimiento { get; set; }
        
        /// <summary>
        /// Observaciones adicionales
        /// </summary>
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Validador para el comando de crear preparación
    /// </summary>
    public class CrearPreparacionCommandValidator : AbstractValidator<CrearPreparacionCommand>
    {
        public CrearPreparacionCommandValidator()
        {
            RuleFor(x => x.ProductoId)
                .NotEmpty().WithMessage("El ID del producto es obligatorio");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero");

            RuleFor(x => x.ChefId)
                .NotEmpty().WithMessage("El ID del chef es obligatorio");

            RuleFor(x => x.FechaVencimiento)
                .Must(fecha => fecha > DateTime.Now)
                .WithMessage("La fecha de vencimiento debe ser futura");

            RuleFor(x => x.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden superar los 500 caracteres");
        }
    }
} 