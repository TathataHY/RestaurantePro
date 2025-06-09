using System;
using FluentValidation;
using MediatR;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.ConsumirPreparacion
{
    /// <summary>
    /// Comando para consumir una cantidad de un producto preparado
    /// </summary>
    public class ConsumirPreparacionCommand : IRequest<Result>
    {
        /// <summary>
        /// ID del producto a consumir
        /// </summary>
        public Guid ProductoId { get; set; }
        
        /// <summary>
        /// Cantidad a consumir
        /// </summary>
        public int Cantidad { get; set; }
        
        /// <summary>
        /// ID de la comanda que consume la preparación (opcional)
        /// </summary>
        public Guid? ComandaId { get; set; }
        
        /// <summary>
        /// Observaciones adicionales sobre el consumo
        /// </summary>
        public string? Observaciones { get; set; }
    }
    
    /// <summary>
    /// Validador para el comando ConsumirPreparacion
    /// </summary>
    public class ConsumirPreparacionCommandValidator : AbstractValidator<ConsumirPreparacionCommand>
    {
        public ConsumirPreparacionCommandValidator()
        {
            RuleFor(x => x.ProductoId)
                .NotEmpty().WithMessage("El ID del producto es obligatorio");
                
            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero");
                
            RuleFor(x => x.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden superar los 500 caracteres");
        }
    }
} 