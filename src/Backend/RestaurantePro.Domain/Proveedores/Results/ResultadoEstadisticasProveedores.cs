namespace RestaurantePro.Domain.Proveedores.Results
{
    /// <summary>
    /// Resultado de la consulta de estadísticas de proveedores dentro del dominio
    /// </summary>
    public class ResultadoEstadisticasProveedores
    {
        /// <summary>
        /// Cantidad total de proveedores registrados
        /// </summary>
        public int TotalProveedores { get; }

        /// <summary>
        /// Cantidad de proveedores activos
        /// </summary>
        public int ProveedoresActivos { get; }

        /// <summary>
        /// Cantidad de órdenes realizadas en el periodo
        /// </summary>
        public int OrdenesEnPeriodo { get; }

        /// <summary>
        /// Valor total de las órdenes realizadas en el periodo
        /// </summary>
        public decimal ValorTotalOrdenesEnPeriodo { get; }

        /// <summary>
        /// Tiempo promedio de entrega en días
        /// </summary>
        public decimal TiempoPromedioEntrega { get; }

        /// <summary>
        /// Top proveedores por volumen de compra
        /// </summary>
        public IReadOnlyList<EstadisticaProveedor> TopProveedoresPorVolumen { get; }

        /// <summary>
        /// Proveedores con órdenes pendientes
        /// </summary>
        public int ProveedoresConOrdenesPendientes { get; }

        /// <summary>
        /// Fecha de último cálculo de las estadísticas
        /// </summary>
        public DateTime FechaCalculo { get; }

        /// <summary>
        /// Constructor para resultado de estadísticas
        /// </summary>
        public ResultadoEstadisticasProveedores(
            int totalProveedores,
            int proveedoresActivos,
            int ordenesEnPeriodo,
            decimal valorTotalOrdenesEnPeriodo,
            decimal tiempoPromedioEntrega,
            List<EstadisticaProveedor> topProveedoresPorVolumen,
            int proveedoresConOrdenesPendientes,
            DateTime fechaCalculo)
        {
            TotalProveedores = totalProveedores;
            ProveedoresActivos = proveedoresActivos;
            OrdenesEnPeriodo = ordenesEnPeriodo;
            ValorTotalOrdenesEnPeriodo = valorTotalOrdenesEnPeriodo;
            TiempoPromedioEntrega = tiempoPromedioEntrega;
            TopProveedoresPorVolumen = topProveedoresPorVolumen.AsReadOnly();
            ProveedoresConOrdenesPendientes = proveedoresConOrdenesPendientes;
            FechaCalculo = fechaCalculo;
        }
    }

    /// <summary>
    /// Estadísticas individuales de un proveedor
    /// </summary>
    public class EstadisticaProveedor
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
        /// Total de órdenes realizadas al proveedor
        /// </summary>
        public int TotalOrdenes { get; }

        /// <summary>
        /// Valor total de las órdenes realizadas al proveedor
        /// </summary>
        public decimal ValorTotalOrdenes { get; }

        /// <summary>
        /// Tiempo promedio de entrega del proveedor en días
        /// </summary>
        public decimal TiempoPromedioEntrega { get; }

        /// <summary>
        /// Constructor para estadísticas de proveedor individual
        /// </summary>
        public EstadisticaProveedor(
            Guid proveedorId,
            string nombreProveedor,
            int totalOrdenes,
            decimal valorTotalOrdenes,
            decimal tiempoPromedioEntrega)
        {
            ProveedorId = proveedorId;
            NombreProveedor = nombreProveedor;
            TotalOrdenes = totalOrdenes;
            ValorTotalOrdenes = valorTotalOrdenes;
            TiempoPromedioEntrega = tiempoPromedioEntrega;
        }
    }
} 