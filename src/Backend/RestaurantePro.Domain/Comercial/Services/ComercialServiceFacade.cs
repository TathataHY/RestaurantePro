namespace RestaurantePro.Domain.Comercial.Services
{
    /// <summary>
    /// Implementación de la fachada de servicios para el contexto de Comercial
    /// </summary>
    public class ComercialServiceFacade : IComercialServiceFacade
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IClientesFrecuentesPolicy _clientesFrecuentesPolicy;
        private readonly IServicioFidelizacion _servicioFidelizacion;
        
        public ComercialServiceFacade(
            IClienteRepository clienteRepository,
            IClientesFrecuentesPolicy clientesFrecuentesPolicy,
            IServicioFidelizacion servicioFidelizacion)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _clientesFrecuentesPolicy = clientesFrecuentesPolicy ?? throw new ArgumentNullException(nameof(clientesFrecuentesPolicy));
            _servicioFidelizacion = servicioFidelizacion ?? throw new ArgumentNullException(nameof(servicioFidelizacion));
        }

        /// <inheritdoc />
        public async Task<Cliente?> ObtenerClientePorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _clienteRepository.ObtenerPorIdAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Cliente> RegistrarNuevoClienteConTarjetaAsync(
            string nombre, 
            string apellidos, 
            string email, 
            string? telefono = null, 
            CancellationToken cancellationToken = default)
        {
            // Crear un nuevo cliente
            var clienteNombre = ClienteNombre.Crear(nombre, apellidos);
            var cliente = Cliente.Crear(clienteNombre, email, telefono);
            
            // Crear y asignar tarjeta de fidelización
            cliente.CrearTarjetaFidelizacion();
            
            // Persistir el cliente
            await _clienteRepository.AgregarAsync(cliente);
            await _clienteRepository.GuardarCambiosAsync(cancellationToken);
            
            return cliente;
        }

        /// <inheritdoc />
        public async Task<Cliente?> ActualizarDatosClienteAsync(
            Guid clienteId, 
            string? nombre = null, 
            string? apellidos = null, 
            string? email = null, 
            string? telefono = null, 
            CancellationToken cancellationToken = default)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
            if (cliente == null)
            {
                return null;
            }
            
            // Actualizar nombre si se proporcionó
            if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(apellidos))
            {
                var nuevoNombre = ClienteNombre.Crear(nombre, apellidos);
                cliente.ActualizarNombre(nuevoNombre);
            }
            
            // Actualizar email si se proporcionó
            if (!string.IsNullOrEmpty(email))
            {
                cliente.ActualizarEmail(email);
            }
            
            // Actualizar teléfono si se proporcionó
            if (!string.IsNullOrEmpty(telefono))
            {
                cliente.ActualizarTelefono(telefono);
            }
            
            // Persistir cambios
            await _clienteRepository.ActualizarAsync(cliente);
            await _clienteRepository.GuardarCambiosAsync(cancellationToken);
            
            return cliente;
        }

        /// <inheritdoc />
        public async Task<bool> AsignarPuntosClienteAsync(
            Guid clienteId, 
            int puntos, 
            Guid comandaId, 
            CancellationToken cancellationToken = default)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
            if (cliente == null || !cliente.TieneTarjetaFidelizacion())
            {
                return false;
            }
            
            cliente.AgregarPuntosFidelizacion(puntos, comandaId.ToString());
            
            await _clienteRepository.ActualizarAsync(cliente);
            await _clienteRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }

        /// <inheritdoc />
        public async Task<decimal> CalcularDescuentoPuntosFidelizacionAsync(
            Guid clienteId, 
            decimal montoTotal, 
            CancellationToken cancellationToken = default)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
            if (cliente == null || !cliente.TieneTarjetaFidelizacion())
            {
                return 0;
            }
            
            var puntos = cliente.ObtenerPuntosFidelizacionDisponibles();
            return _servicioFidelizacion.CalcularDescuentoPorPuntos(puntos, montoTotal);
        }

        /// <inheritdoc />
        public async Task<decimal> CanjearPuntosPorDescuentoAsync(
            Guid clienteId, 
            int puntosAUtilizar, 
            Guid comandaId, 
            CancellationToken cancellationToken = default)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
            if (cliente == null || !cliente.TieneTarjetaFidelizacion())
            {
                return 0;
            }
            
            var puntosDisponibles = cliente.ObtenerPuntosFidelizacionDisponibles();
            if (puntosAUtilizar <= 0 || puntosAUtilizar > puntosDisponibles)
            {
                return 0;
            }
            
            // Calcular descuento
            var descuento = _servicioFidelizacion.CalcularDescuentoPorPuntos(puntosAUtilizar, 0);
            
            // Registrar el uso de puntos
            cliente.UsarPuntosFidelizacion(puntosAUtilizar, $"Descuento en comanda {comandaId}");
            
            // Persistir cambios
            await _clienteRepository.ActualizarAsync(cliente);
            await _clienteRepository.GuardarCambiosAsync(cancellationToken);
            
            return descuento;
        }

        /// <inheritdoc />
        public async Task<Dictionary<Guid, SegmentoCliente>> EjecutarPoliticaClientesFrecuentesAsync(
            IEnumerable<Guid>? clienteIds = null, 
            CancellationToken cancellationToken = default)
        {
            // Obtener clientes a procesar
            var clientes = clienteIds != null
                ? await _clienteRepository.ObtenerClientesPorIdsAsync(clienteIds, cancellationToken)
                : await _clienteRepository.ObtenerTodosConHistorialVisitasAsync(90, cancellationToken);
            
            // Ejecutar política
            var resultado = await _clientesFrecuentesPolicy.EjecutarAsync(90, cancellationToken);
            
            // Construir diccionario de resultados
            var resultadoClientes = new Dictionary<Guid, SegmentoCliente>();
            
            // Agregar todos los clientes actualizados
            foreach (var clienteId in resultado.ClientesActualizados)
            {
                var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                if (cliente != null)
                {
                    resultadoClientes[clienteId] = cliente.Segmento;
                }
            }
            
            return resultadoClientes;
        }
    }
} 