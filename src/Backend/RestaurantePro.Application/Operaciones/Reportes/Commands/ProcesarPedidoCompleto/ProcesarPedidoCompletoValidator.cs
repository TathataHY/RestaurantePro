using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Reportes.DTOs;

namespace RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto
{
    /// <summary>
    /// Validador para el comando de procesamiento completo de pedido
    /// </summary>
    public class ProcesarPedidoCompletoValidator : AbstractValidator<ProcesarPedidoCompletoCommand>
    {
        private readonly IComandaRepository _comandaRepository;
        
        public ProcesarPedidoCompletoValidator(IComandaRepository comandaRepository)
        {
            _comandaRepository = comandaRepository;
            
            RuleFor(x => x.ComandaId)
                .NotEmpty().WithMessage("El ID de la comanda es requerido");
                
            RuleFor(x => x.RequierePago)
                .NotNull().WithMessage("Debe especificar si requiere pago");
                
            When(x => x.RequierePago, () => {
                RuleFor(x => x.TipoPago)
                    .NotEmpty().WithMessage("El tipo de pago es requerido cuando se requiere pago");
                    
                RuleFor(x => x.InfoPago)
                    .NotNull().WithMessage("La información de pago es requerida cuando se requiere pago");
                    
                When(x => x.TipoPago == "Tarjeta", () => {
                    RuleFor(x => x.InfoPago.NumeroTarjeta)
                        .NotEmpty().WithMessage("El número de tarjeta es requerido para pagos con tarjeta");
                });
                
                When(x => x.TipoPago == "Digital", () => {
                    RuleFor(x => x.InfoPago.CodigoTransaccion)
                        .NotEmpty().WithMessage("El código de transacción es requerido para pagos digitales");
                });
                
                When(x => x.TipoPago == "Transferencia", () => {
                    RuleFor(x => x.InfoPago.CodigoTransferencia)
                        .NotEmpty().WithMessage("El código de transferencia es requerido para pagos por transferencia");
                });
                
                When(x => x.TipoPago == "Efectivo", () => {
                    RuleFor(x => x.InfoPago.MontoRecibido)
                        .GreaterThanOrEqualTo(x => x.InfoPago.MontoTotal)
                        .WithMessage("El monto recibido debe ser mayor o igual al monto total para pagos en efectivo");
                });
            });
            
            RuleFor(x => x.ClienteId)
                .NotEmpty().WithMessage("El ID del cliente es requerido");
            
            // Otras validaciones generales
            RuleFor(x => x.Observaciones)
                .MaximumLength(500)
                .WithMessage("Las observaciones no pueden exceder los 500 caracteres");
        }
    }
} 