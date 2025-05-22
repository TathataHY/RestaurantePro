using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Proveedores.Enums;

namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se agrega una categoría a un proveedor
    /// </summary>
    public class ProveedorCategoriaAgregada : DomainEvent
    {
        /// <summary>
        /// ID del proveedor
        /// </summary>
        public Guid ProveedorId { get; }
        
        /// <summary>
        /// Nombre del proveedor
        /// </summary>
        public string NombreProveedor { get; }
        
        /// <summary>
        /// Categoría agregada al proveedor
        /// </summary>
        public CategoriaProveedor Categoria { get; }
        
        /// <summary>
        /// Porcentaje de descuento para esta categoría
        /// </summary>
        public decimal PorcentajeDescuento { get; }
        
        /// <summary>
        /// Indica si este proveedor es el principal para esta categoría
        /// </summary>
        public bool EsProveedorPrincipal { get; }
        
        /// <summary>
        /// Constructor para el evento ProveedorCategoriaAgregada
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombreProveedor">Nombre del proveedor</param>
        /// <param name="categoria">Categoría agregada</param>
        /// <param name="porcentajeDescuento">Porcentaje de descuento acordado</param>
        /// <param name="esProveedorPrincipal">Si es proveedor principal para esta categoría</param>
        public ProveedorCategoriaAgregada(
            Guid proveedorId, 
            string nombreProveedor, 
            CategoriaProveedor categoria, 
            decimal porcentajeDescuento, 
            bool esProveedorPrincipal)
        {
            ProveedorId = proveedorId;
            NombreProveedor = nombreProveedor;
            Categoria = categoria;
            PorcentajeDescuento = porcentajeDescuento;
            EsProveedorPrincipal = esProveedorPrincipal;
        }
    }
} 