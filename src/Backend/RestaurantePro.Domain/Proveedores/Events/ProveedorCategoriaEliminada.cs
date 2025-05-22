using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Proveedores.Enums;

namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se elimina una categoría de un proveedor
    /// </summary>
    public class ProveedorCategoriaEliminada : DomainEvent
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
        /// Categoría eliminada del proveedor
        /// </summary>
        public CategoriaProveedor Categoria { get; }
        
        /// <summary>
        /// Indica si este proveedor era el principal para esta categoría
        /// </summary>
        public bool EraProveedorPrincipal { get; }
        
        /// <summary>
        /// Constructor para el evento ProveedorCategoriaEliminada
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombreProveedor">Nombre del proveedor</param>
        /// <param name="categoria">Categoría eliminada</param>
        /// <param name="eraProveedorPrincipal">Si era proveedor principal para esta categoría</param>
        public ProveedorCategoriaEliminada(
            Guid proveedorId,
            string nombreProveedor,
            CategoriaProveedor categoria,
            bool eraProveedorPrincipal)
        {
            ProveedorId = proveedorId;
            NombreProveedor = nombreProveedor;
            Categoria = categoria;
            EraProveedorPrincipal = eraProveedorPrincipal;
        }
    }
} 