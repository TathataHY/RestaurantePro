using RestaurantePro.Domain.Core.SharedKernel.Specifications;
using RestaurantePro.Domain.Proveedores.Entities;

namespace RestaurantePro.Domain.Proveedores.Specifications
{
    /// <summary>
    /// Especificación que verifica si un proveedor está activo y elegible para recibir órdenes de compra.
    /// Un proveedor está elegible si:
    /// 1. Está activo
    /// 2. Tiene información de contacto válida (email y teléfono)
    /// 3. No está en periodo de gracia para pagos pendientes (opcional)
    /// </summary>
    public class ProveedorActivoSpecification : SpecificationBase<Proveedor>
    {
        private readonly bool _verificarPagosPendientes;
        private readonly int _diasGraciaPagos;
        
        /// <summary>
        /// Crea una nueva instancia de la especificación
        /// </summary>
        /// <param name="verificarPagosPendientes">Indica si debe verificarse que el proveedor no tenga pagos pendientes fuera del periodo de gracia</param>
        /// <param name="diasGraciaPagos">Días de gracia para pagos pendientes (por defecto 30 días)</param>
        public ProveedorActivoSpecification(bool verificarPagosPendientes = false, int diasGraciaPagos = 30)
        {
            _verificarPagosPendientes = verificarPagosPendientes;
            _diasGraciaPagos = diasGraciaPagos;
        }
        
        /// <summary>
        /// Verifica si un proveedor cumple con los criterios para estar activo y elegible
        /// </summary>
        /// <param name="proveedor">Proveedor a evaluar</param>
        /// <returns>True si el proveedor está activo y elegible, False en caso contrario</returns>
        public override bool IsSatisfiedBy(Proveedor proveedor)
        {
            // Verificación básica
            if (proveedor == null)
                return false;
                
            // Debe estar activo
            if (!proveedor.Activo)
                return false;
                
            // Debe tener información de contacto válida
            if (string.IsNullOrWhiteSpace(proveedor.Email) || string.IsNullOrWhiteSpace(proveedor.Telefono))
                return false;
                
            // Verificación de pagos pendientes (si se solicitó)
            if (_verificarPagosPendientes)
            {
                // En un sistema real, deberíamos consultar los pagos pendientes del proveedor
                // Para este ejemplo, simulamos esta verificación utilizando un método del dominio
                // que podría implementarse posteriormente
                if (TienePagosPendientesFueraDeGracia(proveedor, _diasGraciaPagos))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Verifica si un proveedor tiene pagos pendientes fuera del periodo de gracia
        /// Nota: Este método es una simulación. En un sistema real, requeriría consultar
        /// información financiera del proveedor desde un repositorio.
        /// </summary>
        private bool TienePagosPendientesFueraDeGracia(Proveedor proveedor, int diasGracia)
        {
            // Esta es una implementación simulada
            // En una implementación real, se consultaría una fuente de datos financieros
            
            // Por ahora, asumimos que ningún proveedor tiene pagos pendientes fuera de gracia
            return false;
        }
    }
} 