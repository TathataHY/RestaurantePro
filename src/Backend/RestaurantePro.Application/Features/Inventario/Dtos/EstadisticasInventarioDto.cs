using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Inventario.Dtos
{
    public class EstadisticasInventarioDto
    {
        /// <summary>
        /// Valor total del inventario actual
        /// </summary>
        public decimal ValorTotalInventario { get; set; }

        /// <summary>
        /// Cantidad de productos por debajo del stock mínimo
        /// </summary>
        public int ProductosBajoStock { get; set; }

        /// <summary>
        /// Productos con mayor rotación en el periodo
        /// </summary>
        public List<ProductoRotacionDto> ProductosMayorRotacion { get; set; } = new List<ProductoRotacionDto>();

        /// <summary>
        /// Productos sin movimiento en el periodo
        /// </summary>
        public List<InventarioDto> ProductosSinMovimiento { get; set; } = new List<InventarioDto>();

        /// <summary>
        /// Órdenes de compra pendientes
        /// </summary>
        public int OrdenesPendientes { get; set; }

        /// <summary>
        /// Valor total de las órdenes pendientes
        /// </summary>
        public decimal ValorOrdenesPendientes { get; set; }

        /// <summary>
        /// Movimientos por tipo en el periodo
        /// </summary>
        public Dictionary<string, MovimientoEstadisticaDto> MovimientosPorTipo { get; set; } = new Dictionary<string, MovimientoEstadisticaDto>();

        /// <summary>
        /// Fecha de la última actualización del inventario
        /// </summary>
        public DateTime UltimaActualizacion { get; set; }
    }

    public class ProductoRotacionDto
    {
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public int IngredienteId { get; set; }

        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string NombreIngrediente { get; set; }

        /// <summary>
        /// Cantidad total consumida en el periodo
        /// </summary>
        public decimal CantidadConsumida { get; set; }

        /// <summary>
        /// Unidad de medida
        /// </summary>
        public string UnidadMedida { get; set; }

        /// <summary>
        /// Valor total consumido
        /// </summary>
        public decimal ValorConsumido { get; set; }

        /// <summary>
        /// Cantidad de movimientos realizados
        /// </summary>
        public int NumeroMovimientos { get; set; }
    }

    public class MovimientoEstadisticaDto
    {
        /// <summary>
        /// Tipo de movimiento (entrada, salida, merma, etc.)
        /// </summary>
        public string TipoMovimiento { get; set; }

        /// <summary>
        /// Cantidad total de movimientos
        /// </summary>
        public int CantidadMovimientos { get; set; }

        /// <summary>
        /// Valor total de movimientos
        /// </summary>
        public decimal ValorTotal { get; set; }
    }
} 