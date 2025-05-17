using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Inventario.Dtos
{
    public class SugerenciaCompraDto
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
        /// Categoría del ingrediente
        /// </summary>
        public string Categoria { get; set; }

        /// <summary>
        /// Stock actual
        /// </summary>
        public decimal StockActual { get; set; }

        /// <summary>
        /// Stock mínimo definido
        /// </summary>
        public decimal StockMinimo { get; set; }

        /// <summary>
        /// Stock óptimo definido
        /// </summary>
        public decimal StockOptimo { get; set; }

        /// <summary>
        /// Unidad de medida
        /// </summary>
        public string UnidadMedida { get; set; }

        /// <summary>
        /// Consumo promedio diario calculado
        /// </summary>
        public decimal ConsumoDiarioPromedio { get; set; }

        /// <summary>
        /// Consumo proyectado para el periodo
        /// </summary>
        public decimal ConsumoProyectado { get; set; }

        /// <summary>
        /// Cantidad sugerida para comprar
        /// </summary>
        public decimal CantidadSugerida { get; set; }

        /// <summary>
        /// Urgencia de la compra (Alta, Media, Baja)
        /// </summary>
        public string Urgencia { get; set; }

        /// <summary>
        /// Costo unitario estimado
        /// </summary>
        public decimal CostoUnitarioEstimado { get; set; }

        /// <summary>
        /// Costo total estimado de la compra
        /// </summary>
        public decimal CostoTotalEstimado { get; set; }

        /// <summary>
        /// ID del proveedor principal para este ingrediente
        /// </summary>
        public int? ProveedorPrincipalId { get; set; }

        /// <summary>
        /// Nombre del proveedor principal
        /// </summary>
        public string NombreProveedorPrincipal { get; set; }

        /// <summary>
        /// Lista de proveedores alternativos que ofrecen este ingrediente
        /// </summary>
        public List<ProveedorAlternativoDto> ProveedoresAlternativos { get; set; } = new List<ProveedorAlternativoDto>();
    }

    public class ProveedorAlternativoDto
    {
        /// <summary>
        /// ID del proveedor
        /// </summary>
        public int ProveedorId { get; set; }

        /// <summary>
        /// Nombre del proveedor
        /// </summary>
        public string NombreProveedor { get; set; }

        /// <summary>
        /// Precio ofrecido por el proveedor
        /// </summary>
        public decimal PrecioUnitario { get; set; }
    }
} 