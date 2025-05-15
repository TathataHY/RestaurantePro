using RestaurantePro.Domain.Enums;
using System.Collections.Generic;

namespace RestaurantePro.Application.DTOs
{
    /// <summary>
    /// DTO para transferencia de datos de detalle de comanda
    /// </summary>
    public class ComandaDetalleDto
    {
        /// <summary>
        /// ID del detalle
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la comanda
        /// </summary>
        public int ComandaId { get; set; }

        /// <summary>
        /// ID del producto
        /// </summary>
        public int ProductoId { get; set; }

        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string NombreProducto { get; set; }

        /// <summary>
        /// Descripción del producto
        /// </summary>
        public string DescripcionProducto { get; set; }

        /// <summary>
        /// Cantidad solicitada
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio unitario
        /// </summary>
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Subtotal (Cantidad * PrecioUnitario)
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Estado del detalle
        /// </summary>
        public EstadoComandaDetalle Estado { get; set; }

        /// <summary>
        /// Nombre del estado para mostrar
        /// </summary>
        public string EstadoNombre => Estado.ToString();

        /// <summary>
        /// Notas especiales
        /// </summary>
        public string NotasEspeciales { get; set; }

        /// <summary>
        /// Tiempo estimado de preparación en minutos
        /// </summary>
        public int TiempoEstimadoPreparacion { get; set; }

        /// <summary>
        /// Lista de personalizaciones del detalle
        /// </summary>
        public List<ComandaDetallePersonalizacionDto> Personalizaciones { get; set; } = new List<ComandaDetallePersonalizacionDto>();
    }

    /// <summary>
    /// DTO para transferencia de datos de personalización de detalle de comanda
    /// </summary>
    public class ComandaDetallePersonalizacionDto
    {
        /// <summary>
        /// ID de la personalización
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del detalle de comanda
        /// </summary>
        public int ComandaDetalleId { get; set; }

        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public int IngredienteId { get; set; }

        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string NombreIngrediente { get; set; }

        /// <summary>
        /// Acción de personalización (Agregar, Quitar, Extra)
        /// </summary>
        public AccionPersonalizacion Accion { get; set; }

        /// <summary>
        /// Nombre de la acción para mostrar
        /// </summary>
        public string AccionNombre => Accion.ToString();

        /// <summary>
        /// Cantidad del ingrediente
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Precio adicional (si aplica)
        /// </summary>
        public decimal PrecioAdicional { get; set; }
    }
} 