namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Clase que representa el resultado de la verificación de stock
    /// </summary>
    public class ResultadoVerificacionStock
    {
        /// <summary>
        /// Órdenes de compra generadas durante la verificación
        /// </summary>
        public List<OrdenCompra> OrdenesGeneradas { get; } = new List<OrdenCompra>();
        
        /// <summary>
        /// Órdenes de compra existentes que fueron actualizadas
        /// </summary>
        public List<OrdenCompra> OrdenesActualizadas { get; } = new List<OrdenCompra>();
        
        /// <summary>
        /// Errores ocurridos durante la verificación
        /// </summary>
        public List<string> Errores { get; } = new List<string>();
    }
    
    /// <summary>
    /// Servicio de dominio que verifica el stock de ingredientes y genera órdenes de compra automáticas
    /// </summary>
    public class VerificadorStock : IVerificadorStock
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public VerificadorStock(
            IIngredienteRepository ingredienteRepository,
            IOrdenCompraRepository ordenCompraRepository,
            IProveedorRepository proveedorRepository,
            IDateTimeService dateTimeService)
        {
            _ingredienteRepository = ingredienteRepository;
            _ordenCompraRepository = ordenCompraRepository;
            _proveedorRepository = proveedorRepository;
            _dateTimeService = dateTimeService;
        }
        
        /// <summary>
        /// Verifica los ingredientes con stock bajo y genera órdenes de compra automáticas
        /// </summary>
        /// <returns>Resultado de la verificación</returns>
        public async Task<ResultadoVerificacionStock> VerificarYGenerarOrdenesCompraAsync(CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoVerificacionStock();
            
            // Obtener ingredientes con stock bajo
            var ingredientesBajoStock = await _ingredienteRepository.ObtenerConStockBajoAsync();
            
            if (!ingredientesBajoStock.Any())
            {
                return resultado; // No hay ingredientes con stock bajo
            }
            
            // Agrupar ingredientes por proveedor
            var ingredientesPorProveedor = ingredientesBajoStock
                .Where(i => i.ProveedorPrincipalId.HasValue)
                .GroupBy(i => i.ProveedorPrincipalId.Value)
                .ToDictionary(g => g.Key, g => g.ToList());
                
            // Procesar cada grupo de ingredientes por proveedor
            foreach (var kvp in ingredientesPorProveedor)
            {
                var proveedorId = kvp.Key;
                var ingredientes = kvp.Value;
                
                // Obtener el proveedor
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
                
                if (proveedor == null)
                {
                    resultado.Errores.Add($"No se encontró el proveedor con ID {proveedorId}");
                    continue;
                }
                
                // Verificar si el proveedor está activo
                if (!proveedor.EstaActivo)
                {
                    resultado.Errores.Add($"El proveedor {proveedor.Nombre} (ID: {proveedorId}) no está activo");
                    continue;
                }
                
                // Verificar si ya existe una orden pendiente para este proveedor
                var ordenesPendientes = await _ordenCompraRepository.ObtenerPendientesPorProveedorAsync(proveedorId, cancellationToken);
                
                if (ordenesPendientes.Any())
                {
                    // Actualizar orden existente en lugar de crear una nueva
                    var ordenExistente = ordenesPendientes.First();
                    ActualizarOrdenExistente(ordenExistente, ingredientes, resultado);
                }
                else
                {
                    // Crear nueva orden de compra
                    CrearNuevaOrden(proveedor, ingredientes, resultado);
                }
            }
            
            // Guardar las órdenes generadas y actualizadas
            foreach (var orden in resultado.OrdenesGeneradas)
            {
                await _ordenCompraRepository.AddAsync(orden);
            }
            
            foreach (var orden in resultado.OrdenesActualizadas)
            {
                await _ordenCompraRepository.UpdateAsync(orden);
            }
            
            return resultado;
        }
        
        /// <summary>
        /// Actualiza una orden de compra existente con nuevos ingredientes
        /// </summary>
        private void ActualizarOrdenExistente(OrdenCompra orden, List<Ingrediente> ingredientes, ResultadoVerificacionStock resultado)
        {
            var ingredientesAgregados = false;
            
            foreach (var ingrediente in ingredientes)
            {
                // Verificar si el ingrediente ya está en la orden
                if (orden.Items.Any(i => i.IngredienteId == ingrediente.Id))
                {
                    continue; // Ya existe en la orden
                }
                
                // Calcular la cantidad a pedir (diferencia entre stock mínimo y stock actual)
                var cantidadAPedir = CalcularCantidadAPedir(ingrediente);
                
                // Agregar el ingrediente a la orden
                orden.AgregarItem(ingrediente.Id, ingrediente.Nombre, cantidadAPedir, ingrediente.UnidadMedida);
                ingredientesAgregados = true;
            }
            
            if (ingredientesAgregados)
            {
                resultado.OrdenesActualizadas.Add(orden);
            }
        }
        
        /// <summary>
        /// Crea una nueva orden de compra para un proveedor con ingredientes
        /// </summary>
        private void CrearNuevaOrden(Proveedor proveedor, List<Ingrediente> ingredientes, ResultadoVerificacionStock resultado)
        {
            // Crear nueva orden
            var nuevaOrden = OrdenCompra.Crear(
                proveedor.Id,
                $"Orden automática - {_dateTimeService.Now:dd/MM/yyyy}",
                _dateTimeService.Now);
                
            // Agregar los ingredientes a la orden
            foreach (var ingrediente in ingredientes)
            {
                // Calcular la cantidad a pedir
                var cantidadAPedir = CalcularCantidadAPedir(ingrediente);
                
                // Agregar el ingrediente a la orden
                nuevaOrden.AgregarItem(ingrediente.Id, ingrediente.Nombre, cantidadAPedir, ingrediente.UnidadMedida);
            }
            
            resultado.OrdenesGeneradas.Add(nuevaOrden);
        }
        
        /// <summary>
        /// Calcula la cantidad a pedir de un ingrediente (diferencia entre stock mínimo y stock actual)
        /// </summary>
        private decimal CalcularCantidadAPedir(Ingrediente ingrediente)
        {
            // Calcular la cantidad necesaria para llegar al stock mínimo más un 20%
            var cantidadFaltante = ingrediente.StockMinimo - ingrediente.Stock;
            var cantidadSugerida = cantidadFaltante * 1.2m; // Agregar un 20% extra
            
            // Redondear hacia arriba a un número entero
            return Math.Ceiling(cantidadSugerida);
        }
    }
} 