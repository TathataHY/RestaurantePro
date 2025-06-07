using FluentValidation;
using RestaurantePro.Application.Operaciones.Reportes.DTOs;

namespace RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto
{
    /// <summary>
    /// Validador para el comando de procesamiento completo de pedido
    /// </summary>
    public class ProcesarPedidoCompletoValidator : AbstractValidator<ProcesarPedidoCompletoCommand>
    {
        public ProcesarPedidoCompletoValidator()
        {
            RuleFor(x => x.ComandaId)
                .NotEmpty()
                .WithMessage("El ID de la comanda es requerido");

            RuleFor(x => x.UsuarioId)
                .NotEmpty()
                .WithMessage("El ID del usuario es requerido");
                
            // Validaciones condicionales según el tipo de pago
            When(x => x.RequierePago && x.TipoPago == "Tarjeta" && x.InfoPago != null, () => {
                RuleFor(x => x.InfoPago.NumeroTarjeta)
                    .NotEmpty()
                    .WithMessage("El número de tarjeta es requerido para pagos con tarjeta");
                    
                RuleFor(x => x.InfoPago.FechaVencimiento)
                    .NotEmpty()
                    .WithMessage("La fecha de vencimiento es requerida para pagos con tarjeta");
                    
                RuleFor(x => x.InfoPago.CodigoSeguridad)
                    .NotEmpty()
                    .WithMessage("El código de seguridad es requerido para pagos con tarjeta");
            });
            
            When(x => x.RequierePago && x.TipoPago == "Digital" && x.InfoPago != null, () => {
                RuleFor(x => x.InfoPago.CodigoTransaccion)
                    .NotEmpty()
                    .WithMessage("El código de transacción es requerido para pagos digitales");
            });
            
            When(x => x.RequierePago && x.TipoPago == "Transferencia" && x.InfoPago != null, () => {
                RuleFor(x => x.InfoPago.CodigoTransferencia)
                    .NotEmpty()
                    .WithMessage("El código de transferencia es requerido para pagos por transferencia");
            });
            
            When(x => x.RequierePago && x.TipoPago == "Efectivo" && x.InfoPago != null, () => {
                RuleFor(x => x.InfoPago.MontoRecibido)
                    .GreaterThanOrEqualTo(x => x.InfoPago.MontoTotal)
                    .WithMessage("El monto recibido debe ser mayor o igual al total a pagar");
            });
            
            // Otras validaciones generales
            RuleFor(x => x.Observaciones)
                .MaximumLength(500)
                .WithMessage("Las observaciones no pueden exceder los 500 caracteres");
        }
    }
} 