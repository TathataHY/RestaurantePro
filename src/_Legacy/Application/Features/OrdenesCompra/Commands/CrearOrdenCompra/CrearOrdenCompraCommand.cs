using MediatR;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.OrdenesCompra.Commands.CrearOrdenCompra
{
    public class CrearOrdenCompraCommand : IRequest<int>
    {
        /// <summary>
        /// ID del proveedor al que se le realiza la orden
        /// </summary>
        public int ProveedorId { get; set; }

        /// <summary>
        /// Fecha estimada de entrega (opcional)
        /// </summary>
        public DateTime? FechaEntregaEstimada { get; set; }

        /// <summary>
        /// Observaciones generales de la orden
        /// </summary>
        public string Observaciones { get; set; }

        /// <summary>
        /// Descuento global aplicado a la orden (porcentaje)
        /// </summary>
        public decimal Descuento { get; set; } = 0;

        /// <summary>
        /// Lista de detalles (items) de la orden de compra
        /// </summary>
        public List<DetalleOrdenCompraDto> Detalles { get; set; } = new List<DetalleOrdenCompraDto>();
    }

    public class DetalleOrdenCompraDto
    {
        /// <summary>
        /// ID del ingrediente a ordenar
        /// </summary>
        public int IngredienteId { get; set; }

        /// <summary>
        /// Cantidad a ordenar
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Unidad de medida
        /// </summary>
        public string UnidadMedida { get; set; }

        /// <summary>
        /// Precio unitario
        /// </summary>
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Porcentaje de impuestos aplicado al detalle
        /// </summary>
        public decimal PorcentajeImpuesto { get; set; } = 0;

        /// <summary>
        /// Porcentaje de descuento aplicado al detalle
        /// </summary>
        public decimal PorcentajeDescuento { get; set; } = 0;

        /// <summary>
        /// Observaciones específicas del detalle
        /// </summary>
        public string Observaciones { get; set; }
    }
} 