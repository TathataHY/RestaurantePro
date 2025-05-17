using MediatR;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.OrdenesCompra.Commands.RecibirOrdenCompra
{
    public class RecibirOrdenCompraCommand : IRequest<bool>
    {
        /// <summary>
        /// ID de la orden de compra
        /// </summary>
        public int OrdenCompraId { get; set; }

        /// <summary>
        /// Fecha de recepción
        /// </summary>
        public DateTime FechaRecepcion { get; set; } = DateTime.Now;

        /// <summary>
        /// Observaciones de la recepción
        /// </summary>
        public string Observaciones { get; set; }

        /// <summary>
        /// Indica si la recepción es parcial o completa
        /// </summary>
        public bool EsRecepcionParcial { get; set; }

        /// <summary>
        /// Lista de detalles de recepción
        /// </summary>
        public List<DetalleRecepcionDto> Detalles { get; set; } = new List<DetalleRecepcionDto>();
    }

    public class DetalleRecepcionDto
    {
        /// <summary>
        /// ID del detalle de la orden de compra
        /// </summary>
        public int DetalleOrdenCompraId { get; set; }

        /// <summary>
        /// Cantidad recibida
        /// </summary>
        public decimal CantidadRecibida { get; set; }

        /// <summary>
        /// Indica si hubo alguna inconformidad en la recepción
        /// </summary>
        public bool TieneInconformidad { get; set; }

        /// <summary>
        /// Descripción de la inconformidad si la hay
        /// </summary>
        public string DescripcionInconformidad { get; set; }
    }
} 