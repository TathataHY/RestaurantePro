

namespace RestaurantePro.Domain.Proveedores.Services
{
    /// <summary>
    /// Implementación de la fachada de servicios para el contexto de Proveedores
    /// </summary>
    public class ProveedoresServiceFacade : IProveedoresServiceFacade
    {
        private readonly IProveedorRepository _proveedorRepository;
        
        public ProveedoresServiceFacade(IProveedorRepository proveedorRepository)
        {
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
        }
        
        /// <inheritdoc />
        public async Task<Proveedor> RegistrarProveedorAsync(
            string nombre, 
            string rut, 
            string direccion, 
            string telefono, 
            string email, 
            string? sitioWeb = null, 
            string? notas = null, 
            CancellationToken cancellationToken = default)
        {
            // Verificar que no exista un proveedor con el mismo RUT
            var existente = await _proveedorRepository.ObtenerPorRutAsync(rut, cancellationToken);
            if (existente != null)
            {
                throw new InvalidOperationException($"Ya existe un proveedor con el RUT {rut}");
            }
            
            // Valores predeterminados para los parámetros faltantes
            string nombreContacto = nombre; // Usamos el nombre del proveedor como contacto principal
            string ciudad = "Santiago"; // Valor predeterminado
            string codigoPostal = "0000000"; // Valor predeterminado
            string pais = "Chile"; // Valor predeterminado requerido
            string informacionBancaria = string.Empty; // Valor predeterminado
            int diasCredito = 30; // Valor predeterminado: 30 días
            
            // Crear el proveedor
            var proveedor = Proveedor.Crear(
                nombre,
                nombreContacto,
                email,
                telefono,
                direccion,
                ciudad,
                codigoPostal,
                pais,
                rut, // RFC en México, RUT en Chile
                informacionBancaria,
                diasCredito);
            
            // Agregar notas si se proporcionaron
            if (!string.IsNullOrEmpty(notas))
            {
                proveedor.AgregarObservaciones(notas);
            }
            
            // Persistir el proveedor
            await _proveedorRepository.AgregarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            
            return proveedor;
        }
        
        /// <inheritdoc />
        public async Task<Proveedor?> ActualizarDatosProveedorAsync(
            Guid proveedorId, 
            string? direccion = null, 
            string? telefono = null, 
            string? email = null, 
            string? sitioWeb = null, 
            string? notas = null, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
            if (proveedor == null)
            {
                return null;
            }
            
            // Actualizar los datos que se proporcionaron
            if (direccion != null)
            {
                proveedor.ActualizarDireccion(direccion);
            }
            
            if (telefono != null)
            {
                proveedor.ActualizarTelefono(telefono);
            }
            
            if (email != null)
            {
                proveedor.ActualizarEmail(email);
            }
            
            if (sitioWeb != null)
            {
                proveedor.ActualizarSitioWeb(sitioWeb);
            }
            
            if (notas != null)
            {
                proveedor.ActualizarNotas(notas);
            }
            
            // Persistir cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            
            return proveedor;
        }
        
        /// <inheritdoc />
        public async Task<Proveedor?> AgregarContactoProveedorAsync(
            Guid proveedorId, 
            string nombre, 
            string cargo, 
            string telefono, 
            string email, 
            bool esPrincipal = false, 
            string? notas = null, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
            if (proveedor == null)
            {
                return null;
            }
            
            // Agregar el contacto
            proveedor.AgregarContacto(nombre, cargo, telefono, email, esPrincipal, notas);
            
            // Persistir cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            
            return proveedor;
        }
        
        /// <inheritdoc />
        public async Task<bool> DesactivarProveedorAsync(
            Guid proveedorId, 
            string motivo, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
            if (proveedor == null)
            {
                return false;
            }
            
            // Desactivar el proveedor
            proveedor.Desactivar(motivo);
            
            // Persistir cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<bool> ActivarProveedorAsync(
            Guid proveedorId, 
            CancellationToken cancellationToken = default)
        {
            // Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
            if (proveedor == null)
            {
                return false;
            }
            
            // Activar el proveedor
            proveedor.Activar();
            
            // Persistir cambios
            await _proveedorRepository.ActualizarAsync(proveedor);
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Proveedor>> ObtenerProveedoresPorTipoProductoAsync(
            string tipoProducto, 
            bool soloActivos = true, 
            CancellationToken cancellationToken = default)
        {
            // Obtener proveedores que ofrecen el tipo de producto
            var proveedores = await _proveedorRepository.ObtenerPorTipoProductoAsync(tipoProducto, cancellationToken);
            
            // Filtrar por activos si se requiere
            if (soloActivos)
            {
                proveedores = proveedores.Where(p => p.Activo).ToList();
            }
            
            return proveedores;
        }
    }
} 