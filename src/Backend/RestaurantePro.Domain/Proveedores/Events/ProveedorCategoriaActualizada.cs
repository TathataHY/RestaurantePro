using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Proveedores.Enums;

namespace RestaurantePro.Domain.Proveedores.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza una categoría de un proveedor
    /// </summary>
    public class ProveedorCategoriaActualizada : DomainEvent
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
        /// Categoría actualizada
        /// </summary>
        public CategoriaProveedor Categoria { get; }
        
        /// <summary>
        /// Porcentaje de descuento anterior
        /// </summary>
        public decimal PorcentajeDescuentoAnterior { get; }
        
        /// <summary>
        /// Nuevo porcentaje de descuento
        /// </summary>
        public decimal PorcentajeDescuentoNuevo { get; }
        
        /// <summary>
        /// Indica si el proveedor era principal antes de la actualización
        /// </summary>
        public bool EraProveedorPrincipal { get; }
        
        /// <summary>
        /// Indica si el proveedor es principal después de la actualización
        /// </summary>
        public bool EsProveedorPrincipal { get; }
        
        /// <summary>
        /// Constructor para el evento ProveedorCategoriaActualizada
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="nombreProveedor">Nombre del proveedor</param>
        /// <param name="categoria">Categoría actualizada</param>
        /// <param name="porcentajeDescuentoAnterior">Porcentaje de descuento anterior</param>
        /// <param name="porcentajeDescuentoNuevo">Nuevo porcentaje de descuento</param>
        /// <param name="eraProveedorPrincipal">Si era proveedor principal</param>
        /// <param name="esProveedorPrincipal">Si es proveedor principal ahora</param>
        public ProveedorCategoriaActualizada(
            Guid proveedorId,
            string nombreProveedor,
            CategoriaProveedor categoria,
            decimal porcentajeDescuentoAnterior,
            decimal porcentajeDescuentoNuevo,
            bool eraProveedorPrincipal,
            bool esProveedorPrincipal)
        {
            ProveedorId = proveedorId;
            NombreProveedor = nombreProveedor;
            Categoria = categoria;
            PorcentajeDescuentoAnterior = porcentajeDescuentoAnterior;
            PorcentajeDescuentoNuevo = porcentajeDescuentoNuevo;
            EraProveedorPrincipal = eraProveedorPrincipal;
            EsProveedorPrincipal = esProveedorPrincipal;
        }
    }
} 