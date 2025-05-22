namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Generador que analiza el inventario y crea órdenes de compra automáticas
    /// cuando los ingredientes están por debajo del stock mínimo
    /// </summary>
    public class GeneradorOrdenesCompra : IGeneradorOrdenesCompra
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor del generador de órdenes de compra
        /// </summary>
        public GeneradorOrdenesCompra(
            IIngredienteRepository ingredienteRepository,
            IOrdenCompraRepository ordenCompraRepository,
            IProveedorRepository proveedorRepository,
            IDateTimeService dateTimeService)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Guid>> GenerarOrdenesCompraAutomaticas()
        {
            // Obtener todos los ingredientes con stock bajo
            var ingredientesConStockBajo = await _ingredienteRepository.ObtenerConStockBajoAsync();
            
            if (!ingredientesConStockBajo.Any())
            {
                return Enumerable.Empty<Guid>();
            }
            
            // Agrupar ingredientes por proveedor para generar una orden por proveedor
            var ingredientesPorProveedor = ingredientesConStockBajo
                .Where(i => i.ProveedorPrincipalId.HasValue)
                .GroupBy(i => i.ProveedorPrincipalId.Value);
                
            var ordenesGeneradas = new List<Guid>();
            
            foreach (var grupo in ingredientesPorProveedor)
            {
                var proveedorId = grupo.Key;
                var ingredientes = grupo.ToList();
                
                // Verificar si hay ingredientes para este proveedor
                if (!ingredientes.Any())
                    continue;
                    
                // Obtener información del proveedor
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId);
                if (proveedor == null)
                    continue;
                
                // Crear la orden de compra
                var ordenCompra = OrdenCompra.Crear(
                    proveedorId,
                    $"Orden automática por stock bajo - {_dateTimeService.Now:dd/MM/yyyy}",
                    _dateTimeService.Now
                );
                
                // Agregar los items a la orden
                foreach (var ingrediente in ingredientes)
                {
                    // Calcular la cantidad a pedir (diferencia entre stock mínimo y actual, más un 20%)
                    var cantidadFaltante = ingrediente.StockMinimo - ingrediente.Stock;
                    var cantidadAPedir = Math.Ceiling(cantidadFaltante * 1.2m);
                    
                    ordenCompra.AgregarItem(
                        ingrediente.Id,
                        ingrediente.Nombre,
                        cantidadAPedir,
                        ingrediente.UnidadMedida
                    );
                }
                
                // Guardar la orden de compra
                await _ordenCompraRepository.AddAsync(ordenCompra);
                ordenesGeneradas.Add(ordenCompra.Id);
            }
            
            return ordenesGeneradas;
        }
        
        /// <inheritdoc />
        public async Task<Guid?> GenerarOrdenCompraParaIngrediente(Guid ingredienteId)
        {
            // Obtener el ingrediente
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId);
            if (ingrediente == null)
            {
                throw new ArgumentException($"No existe un ingrediente con el ID {ingredienteId}", nameof(ingredienteId));
            }
            
            // Verificar si el stock está por debajo del mínimo
            if (ingrediente.Stock >= ingrediente.StockMinimo)
            {
                return null; // No es necesario generar una orden
            }
            
            // Verificar que tenga proveedor principal asignado
            if (!ingrediente.ProveedorPrincipalId.HasValue)
            {
                throw new InvalidOperationException($"El ingrediente {ingrediente.Nombre} no tiene un proveedor principal asignado");
            }
            
            // Obtener el proveedor
            var proveedorId = ingrediente.ProveedorPrincipalId.Value;
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId);
            if (proveedor == null)
            {
                throw new InvalidOperationException($"No se encontró el proveedor con ID {proveedorId} asignado al ingrediente");
            }
            
            // Calcular la cantidad a pedir (diferencia entre stock mínimo y actual, más un 20%)
            var cantidadFaltante = ingrediente.StockMinimo - ingrediente.Stock;
            var cantidadAPedir = Math.Ceiling(cantidadFaltante * 1.2m);
            
            // Crear la orden de compra
            var ordenCompra = OrdenCompra.Crear(
                proveedorId,
                $"Orden automática para {ingrediente.Nombre} - {_dateTimeService.Now:dd/MM/yyyy}",
                _dateTimeService.Now
            );
            
            // Agregar el ingrediente a la orden
            ordenCompra.AgregarItem(
                ingrediente.Id,
                ingrediente.Nombre,
                cantidadAPedir,
                ingrediente.UnidadMedida
            );
            
            // Guardar la orden de compra
            await _ordenCompraRepository.AddAsync(ordenCompra);
            return ordenCompra.Id;
        }
    }
} 