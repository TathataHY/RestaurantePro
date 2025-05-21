using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Operaciones.Comandas.DTOs
{
    /// <summary>
    /// DTO para representar un Item de Comanda en la capa de aplicación
    /// </summary>
    public class ItemComandaDto
    {
        /// <summary>
        /// Identificador único del item
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// ID de la comanda a la que pertenece
        /// </summary>
        public Guid ComandaId { get; set; }
        
        /// <summary>
        /// ID del producto
        /// </summary>
        public Guid ProductoId { get; set; }
        
        /// <summary>
        /// Nombre del producto (para mostrar en interfaces)
        /// </summary>
        public string NombreProducto { get; set; } = string.Empty;
        
        /// <summary>
        /// Cantidad solicitada
        /// </summary>
        public int Cantidad { get; set; }
        
        /// <summary>
        /// Precio unitario del producto al momento de la comanda
        /// </summary>
        public decimal PrecioUnitario { get; set; }
        
        /// <summary>
        /// Subtotal (cantidad * precio unitario)
        /// </summary>
        public decimal Subtotal => Cantidad * PrecioUnitario;
        
        /// <summary>
        /// Observaciones específicas para este item
        /// </summary>
        public string? Observaciones { get; set; }
        
        /// <summary>
        /// Indica si el item está cancelado
        /// </summary>
        public bool Cancelado { get; set; }
        
        /// <summary>
        /// Personalizaciones del item (extras, exclusiones, etc.)
        /// </summary>
        public List<PersonalizacionItemDto> Personalizaciones { get; set; } = new List<PersonalizacionItemDto>();
        
        /// <summary>
        /// Ingredientes del producto
        /// </summary>
        public List<IngredienteItemDto> Ingredientes { get; set; } = new List<IngredienteItemDto>();
    }

    /// <summary>
    /// DTO para representar una personalización de un item
    /// </summary>
    public class PersonalizacionItemDto
    {
        /// <summary>
        /// Identificador único de la personalización
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Tipo de personalización (Agregar, Quitar, Sustituir)
        /// </summary>
        public string Tipo { get; set; } = string.Empty;
        
        /// <summary>
        /// Descripción de la personalización
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;
        
        /// <summary>
        /// Costo adicional por la personalización
        /// </summary>
        public decimal? CostoAdicional { get; set; }
    }

    /// <summary>
    /// DTO para representar un ingrediente de un item
    /// </summary>
    public class IngredienteItemDto
    {
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; set; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; set; } = string.Empty;
        
        /// <summary>
        /// Cantidad del ingrediente
        /// </summary>
        public decimal Cantidad { get; set; }
        
        /// <summary>
        /// Unidad de medida
        /// </summary>
        public string Unidad { get; set; } = string.Empty;
    }
} 