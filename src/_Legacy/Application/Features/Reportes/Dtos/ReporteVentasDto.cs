using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Reportes.Dtos
{
    public class ReporteVentasDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal VentasTotales { get; set; }
        public int CantidadComandas { get; set; }
        public decimal TicketPromedio { get; set; }
        public List<VentaPorDiaDto> VentasPorDia { get; set; } = new List<VentaPorDiaDto>();
        public List<VentaPorMetodoPagoDto> VentasPorMetodoPago { get; set; } = new List<VentaPorMetodoPagoDto>();
        public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; } = new List<ProductoMasVendidoDto>();
    }

    public class VentaPorDiaDto
    {
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int CantidadComandas { get; set; }
    }

    public class VentaPorMetodoPagoDto
    {
        public string MetodoPago { get; set; }
        public decimal Total { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class ProductoMasVendidoDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }
} 