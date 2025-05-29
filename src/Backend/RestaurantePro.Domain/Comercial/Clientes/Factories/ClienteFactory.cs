namespace RestaurantePro.Domain.Comercial.Clientes.Factories;

/// <summary>
/// Factory para crear instancias de Cliente siguiendo el patrón Factory con Result/Notification
/// </summary>
public class ClienteFactory : EntityFactoryBase<Cliente, Guid>
{
    /// <summary>
    /// Constructor del factory
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger para eventos del factory</param>
    public ClienteFactory(INotificationManager notificationManager, ILogger<ClienteFactory> logger)
        : base(notificationManager, logger)
    {
    }

    /// <summary>
    /// Crea una nueva entidad Cliente validando primero los parámetros
    /// </summary>
    /// <param name="parameters">Parámetros de creación como ClienteCreationParameters</param>
    /// <returns>Resultado con la entidad creada o errores</returns>
    protected override Cliente? CrearEntidadInterno(object parameters)
    {
        if (parameters is not ClienteCreationParameters parametros)
        {
            Logger.LogError("Los parámetros deben ser de tipo ClienteCreationParameters");
            return null;
        }

        try
        {
            // Crear ValueObjects a partir de los strings
            var nombre = ClienteNombre.Crear(parametros.Nombre, parametros.Apellidos);
            var email = Email.Create(parametros.Email);
            var telefono = PhoneNumber.Create(parametros.Telefono);

            // Crear la entidad Cliente
            var id = parametros.Id ?? Guid.NewGuid();
            var cliente = Cliente.Crear(id, nombre, email, telefono, parametros.FechaNacimiento, parametros.EstaActivo);

            Logger.LogInformation("Cliente {ClienteId} creado exitosamente con email {Email}", 
                cliente.Id, email.Value);

            return cliente;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al crear Cliente: {Message}", ex.Message);
            throw; // Re-lanzar para que EntityFactoryBase lo maneje
        }
    }

    /// <summary>
    /// Reconstruye una entidad Cliente desde datos persistidos
    /// </summary>
    /// <param name="id">ID de la entidad</param>
    /// <param name="data">Datos para reconstruir como ClienteReconstructionData</param>
    /// <returns>Entidad reconstruida</returns>
    protected override Cliente? ReconstruirEntidadInterno(Guid id, object data)
    {
        if (data is not ClienteReconstructionData datos)
        {
            Logger.LogError("Los datos deben ser de tipo ClienteReconstructionData");
            return null;
        }

        try
        {
            // Crear ValueObjects
            var nombre = ClienteNombre.Crear(datos.Nombre, datos.Apellidos);
            var email = Email.Create(datos.Email);
            var telefono = PhoneNumber.Create(datos.Telefono);

            // Reconstruir entidad usando reflection para establecer propiedades privadas
            var cliente = Cliente.Crear(id, nombre, email, telefono, datos.FechaNacimiento, datos.EstaActivo);

            // Usar reflection para establecer valores que no se pueden establecer a través de los métodos públicos
            SetPrivateProperty(cliente, nameof(Cliente.PuntosAcumulados), datos.PuntosAcumulados);
            SetPrivateProperty(cliente, nameof(Cliente.CantidadVisitas), datos.CantidadVisitas);
            SetPrivateProperty(cliente, nameof(Cliente.Segmento), datos.Segmento);
            SetPrivateProperty(cliente, nameof(Cliente.TarjetaFidelizacionPrincipalId), datos.TarjetaFidelizacionPrincipalId);

            Logger.LogDebug("Cliente {ClienteId} reconstruido exitosamente", id);
            return cliente;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al reconstruir Cliente {ClienteId}: {Message}", id, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Validaciones específicas para ClienteCreationParameters
    /// </summary>
    /// <param name="parameters">Parámetros a validar</param>
    protected override void ValidarParametrosEspecificos(object parameters)
    {
        if (parameters is not ClienteCreationParameters parametros)
        {
            AgregarError("TipoParametros", "Los parámetros deben ser de tipo ClienteCreationParameters");
            return;
        }

        // Validar campos requeridos
        ValidarStringRequerido(parametros.Nombre, nameof(parametros.Nombre), 100);
        ValidarStringRequerido(parametros.Apellidos, nameof(parametros.Apellidos), 100);
        ValidarStringRequerido(parametros.Email, nameof(parametros.Email), 254);
        ValidarStringRequerido(parametros.Telefono, nameof(parametros.Telefono), 20);

        // Validar fecha de nacimiento (mayor de edad)
        if (parametros.FechaNacimiento > DateTime.Now.AddYears(-18))
        {
            AgregarError(nameof(parametros.FechaNacimiento), "El cliente debe ser mayor de 18 años");
        }

        // Validar formato de email
        if (!string.IsNullOrWhiteSpace(parametros.Email))
        {
            try
            {
                Email.Create(parametros.Email);
            }
            catch (ArgumentException ex)
            {
                AgregarError(nameof(parametros.Email), $"Formato de email inválido: {ex.Message}");
            }
        }

        // Validar formato de teléfono
        if (!string.IsNullOrWhiteSpace(parametros.Telefono))
        {
            try
            {
                PhoneNumber.Create(parametros.Telefono);
            }
            catch (ArgumentException ex)
            {
                AgregarError(nameof(parametros.Telefono), $"Formato de teléfono inválido: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Establece una propiedad privada usando reflection
    /// </summary>
    private void SetPrivateProperty<T>(Cliente cliente, string propertyName, T value)
    {
        var property = typeof(Cliente).GetProperty(propertyName, 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if (property != null && property.CanWrite)
        {
            property.SetValue(cliente, value);
        }
        else
        {
            // Si no se puede establecer la propiedad directamente, buscar un setter privado
            var backingField = typeof(Cliente).GetField($"<{propertyName}>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (backingField != null)
            {
                backingField.SetValue(cliente, value);
            }
            else
            {
                Logger.LogWarning("No se pudo establecer la propiedad {PropertyName} en Cliente", propertyName);
            }
        }
    }

    /// <summary>
    /// Método de conveniencia para crear un Cliente con parámetros directos
    /// </summary>
    /// <param name="nombres">Nombres del cliente</param>
    /// <param name="apellidos">Apellidos del cliente</param>
    /// <param name="email">Email del cliente</param>
    /// <param name="telefono">Teléfono del cliente</param>
    /// <param name="fechaNacimiento">Fecha de nacimiento</param>
    /// <param name="activo">Si el cliente está activo</param>
    /// <returns>Resultado con el Cliente creado</returns>
    public Result<Cliente> CrearCliente(
        string nombres,
        string apellidos,
        string email,
        string telefono,
        DateTime fechaNacimiento,
        bool activo = true)
    {
        var parameters = new ClienteCreationParameters
        {
            Nombre = nombres,
            Apellidos = apellidos,
            Email = email,
            Telefono = telefono,
            FechaNacimiento = fechaNacimiento,
            EstaActivo = activo
        };

        return Crear(parameters);
    }
}

/// <summary>
/// Parámetros para crear un nuevo Cliente
/// </summary>
public class ClienteCreationParameters
{
    public Guid? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public bool EstaActivo { get; set; } = true;
}

/// <summary>
/// Datos para reconstruir un Cliente existente
/// </summary>
public class ClienteReconstructionData
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public bool EstaActivo { get; set; } = true;
    public int PuntosAcumulados { get; set; } = 0;
    public int CantidadVisitas { get; set; } = 0;
    public SegmentoCliente Segmento { get; set; } = SegmentoCliente.SinClasificar;
    public Guid? TarjetaFidelizacionPrincipalId { get; set; }
} 