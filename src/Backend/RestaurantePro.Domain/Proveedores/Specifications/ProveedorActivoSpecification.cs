using System;
using System.Linq.Expressions;
using RestaurantePro.Domain.Core.SharedKernel.Specifications;

namespace RestaurantePro.Domain.Proveedores.Specifications
{
    /// <summary>
    /// Especificación que verifica si un proveedor está activo y elegible para recibir órdenes de compra.
    /// Un proveedor está elegible si:
    /// 1. Está activo
    /// 2. Tiene información de contacto válida (email y teléfono)
    /// 3. No está en periodo de gracia para pagos pendientes (opcional)
    /// </summary>
    public class ProveedorActivoSpecification : Specification<Proveedor>
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
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        /// <returns>Expresión que representa esta especificación</returns>
        public override Expression<Func<Proveedor, bool>> ToExpression()
        {
            if (!_verificarPagosPendientes)
            {
                // Versión simple sin verificar pagos pendientes
                return proveedor => 
                    proveedor != null &&
                    proveedor.EstaActivo &&
                    !string.IsNullOrWhiteSpace(proveedor.Email) && 
                    !string.IsNullOrWhiteSpace(proveedor.Telefono);
            }
            else
            {
                // Versión que verifica pagos pendientes
                // En este caso, como dependemos de un método privado que no puede ser parte
                // de la expresión LINQ, usamos un método de extensión IsSatisfiedBy
                // que delegará la lógica compleja al método local
                
                // Para una implementación real, esta verificación de pagos pendientes
                // debería idealmente ser otra especificación independiente que podamos componer
                return proveedor => 
                    proveedor != null &&
                    proveedor.EstaActivo &&
                    !string.IsNullOrWhiteSpace(proveedor.Email) && 
                    !string.IsNullOrWhiteSpace(proveedor.Telefono) &&
                    !TienePagosPendientesFueraDeGracia(proveedor, _diasGraciaPagos);
            }
        }
        
        /// <summary>
        /// Verifica si un proveedor tiene pagos pendientes fuera del periodo de gracia
        /// Nota: Este método es una simulación. En un sistema real, requeriría consultar
        /// información financiera del proveedor desde un repositorio.
        /// </summary>
        private static bool TienePagosPendientesFueraDeGracia(Proveedor proveedor, int diasGracia)
        {
            // Esta es una implementación simulada
            // En una implementación real, se consultaría una fuente de datos financieros
            
            // Por ahora, asumimos que ningún proveedor tiene pagos pendientes fuera de gracia
            return false;
        }
    }
} 