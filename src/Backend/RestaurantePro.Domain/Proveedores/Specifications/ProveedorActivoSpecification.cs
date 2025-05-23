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
        /// Este método evalúa si el proveedor tiene pagos vencidos basándose en su historial
        /// de pagos, fecha de último pago y última compra.
        /// </summary>
        /// <param name="proveedor">Proveedor a evaluar</param>
        /// <param name="diasGracia">Días de gracia permitidos para pagos pendientes</param>
        /// <returns>True si el proveedor tiene pagos pendientes fuera del periodo de gracia, False en caso contrario</returns>
        private static bool TienePagosPendientesFueraDeGracia(Proveedor proveedor, int diasGracia)
        {
            if (proveedor == null)
                throw new ArgumentNullException(nameof(proveedor));
            
            if (!proveedor.UltimaCompra.HasValue)
                return false; // Sin compras no hay pagos pendientes
            
            if (!proveedor.UltimoPago.HasValue)
            {
                // Si hay compra pero no hay pago registrado, verificar si ya pasó el tiempo de gracia
                var diasDesdeUltimaCompra = (DateTime.Now - proveedor.UltimaCompra.Value).TotalDays;
                return diasDesdeUltimaCompra > diasGracia;
            }
            
            // Si la fecha del último pago es posterior a la última compra, no hay pagos pendientes
            if (proveedor.UltimoPago > proveedor.UltimaCompra)
                return false;
            
            // Si el último pago es anterior a la última compra, verificar si está dentro del periodo de gracia
            var diasPendientes = (DateTime.Now - proveedor.UltimaCompra.Value).TotalDays;
            return diasPendientes > diasGracia;
        }
    }
} 
