namespace RestaurantePro.Domain.Proveedores.Builders;

/// <summary>
/// Builder para construir instancias de Proveedor paso a paso con validaciones fluidas.
/// Permite crear proveedores complejos de manera segura y legible.
/// </summary>
public class ProveedorBuilder
{
    private string? _nombre;
    private string? _nombreContacto;
    private string? _email;
    private string? _telefono;
    private string? _direccion;
    private string? _ciudad;
    private string? _codigoPostal;
    private string? _pais;
    private string? _rfc;
    private string? _informacionBancaria;
    private int? _diasCredito;
    private string? _observaciones;
    private readonly List<ContactoProveedorInfo> _contactosAdicionales = new();
    private readonly List<CategoriaProveedorInfo> _categorias = new();
    
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<ProveedorBuilder> _logger;

    /// <summary>
    /// Constructor del builder
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger para eventos del builder</param>
    public ProveedorBuilder(INotificationManager notificationManager, ILogger<ProveedorBuilder> logger)
    {
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Establece el nombre del proveedor
    /// </summary>
    /// <param name="nombre">Nombre del proveedor</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder ConNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            _notificationManager.AddError("El nombre del proveedor no puede estar vacío", "Nombre", nameof(nombre));
            return this;
        }

        if (nombre.Length > 100)
        {
            _notificationManager.AddError("El nombre del proveedor no puede exceder 100 caracteres", "Nombre", nameof(nombre));
            return this;
        }

        _nombre = nombre.Trim();
        _logger.LogDebug("Nombre del proveedor establecido: {Nombre}", _nombre);
        return this;
    }

    /// <summary>
    /// Establece el nombre del contacto principal
    /// </summary>
    /// <param name="nombreContacto">Nombre del contacto principal</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder ConContactoPrincipal(string nombreContacto)
    {
        if (string.IsNullOrWhiteSpace(nombreContacto))
        {
            _notificationManager.AddError("El nombre del contacto principal no puede estar vacío", "NombreContacto", nameof(nombreContacto));
            return this;
        }

        if (nombreContacto.Length > 100)
        {
            _notificationManager.AddError("El nombre del contacto no puede exceder 100 caracteres", "NombreContacto", nameof(nombreContacto));
            return this;
        }

        _nombreContacto = nombreContacto.Trim();
        _logger.LogDebug("Contacto principal establecido: {NombreContacto}", _nombreContacto);
        return this;
    }

    /// <summary>
    /// Establece el email del proveedor
    /// </summary>
    /// <param name="email">Email del proveedor</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder ConEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _notificationManager.AddError("El email del proveedor no puede estar vacío", "Email", nameof(email));
            return this;
        }

        try
        {
            // Validar formato de email usando el ValueObject
            var emailVO = Email.Create(email);
            _email = email.Trim();
            _logger.LogDebug("Email del proveedor establecido: {Email}", _email);
        }
        catch (Exception ex)
        {
            _notificationManager.AddError($"El formato del email no es válido: {ex.Message}", "Email", nameof(email));
        }

        return this;
    }

    /// <summary>
    /// Establece el teléfono del proveedor
    /// </summary>
    /// <param name="telefono">Teléfono del proveedor</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder ConTelefono(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
        {
            _notificationManager.AddError("El teléfono del proveedor no puede estar vacío", "Telefono", nameof(telefono));
            return this;
        }

        try
        {
            // Validar formato de teléfono usando el ValueObject
            var telefonoVO = PhoneNumber.Create(telefono);
            _telefono = telefono.Trim();
            _logger.LogDebug("Teléfono del proveedor establecido: {Telefono}", _telefono);
        }
        catch (Exception ex)
        {
            _notificationManager.AddError($"El formato del teléfono no es válido: {ex.Message}", "Telefono", nameof(telefono));
        }

        return this;
    }

    /// <summary>
    /// Establece la dirección completa del proveedor
    /// </summary>
    /// <param name="direccion">Dirección</param>
    /// <param name="ciudad">Ciudad</param>
    /// <param name="codigoPostal">Código postal</param>
    /// <param name="pais">País (default: México)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder ConDireccion(string direccion, string ciudad, string codigoPostal, string pais = "México")
    {
        if (string.IsNullOrWhiteSpace(direccion))
        {
            _notificationManager.AddError("La dirección del proveedor no puede estar vacía", "Direccion", nameof(direccion));
            return this;
        }

        if (string.IsNullOrWhiteSpace(ciudad))
        {
            _notificationManager.AddError("La ciudad del proveedor no puede estar vacía", "Ciudad", nameof(ciudad));
            return this;
        }

        if (string.IsNullOrWhiteSpace(codigoPostal))
        {
            _notificationManager.AddError("El código postal del proveedor no puede estar vacío", "CodigoPostal", nameof(codigoPostal));
            return this;
        }

        // Validar formato de código postal para México
        if (pais.Equals("México", StringComparison.OrdinalIgnoreCase) && 
            (!codigoPostal.All(char.IsDigit) || codigoPostal.Length != 5))
        {
            _notificationManager.AddError("El código postal debe tener 5 dígitos para México", "CodigoPostal", nameof(codigoPostal));
            return this;
        }

        _direccion = direccion.Trim();
        _ciudad = ciudad.Trim();
        _codigoPostal = codigoPostal.Trim();
        _pais = pais.Trim();

        _logger.LogDebug("Dirección establecida: {Direccion}, {Ciudad}, {CodigoPostal}, {Pais}", 
            _direccion, _ciudad, _codigoPostal, _pais);
        return this;
    }

    /// <summary>
    /// Establece el RFC del proveedor
    /// </summary>
    /// <param name="rfc">RFC del proveedor</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder ConRFC(string rfc)
    {
        if (string.IsNullOrWhiteSpace(rfc))
        {
            _notificationManager.AddError("El RFC del proveedor no puede estar vacío", "RFC", nameof(rfc));
            return this;
        }

        // Validaciones básicas de RFC
        var rfcLimpio = rfc.Trim().ToUpperInvariant();
        
        if (rfcLimpio.Length < 10 || rfcLimpio.Length > 13)
        {
            _notificationManager.AddError("El RFC debe tener entre 10 y 13 caracteres", "RFC", nameof(rfc));
            return this;
        }

        // Validación básica de formato RFC (letras seguidas de números)
        if (!Regex.IsMatch(rfcLimpio, @"^[A-Z&Ñ]{3,4}[0-9]{6}[A-Z0-9]{3}$"))
        {
            _notificationManager.AddError("El formato del RFC no es válido", "RFC", nameof(rfc));
            return this;
        }

        _rfc = rfcLimpio;
        _logger.LogDebug("RFC establecido: {RFC}", _rfc);
        return this;
    }

    /// <summary>
    /// Establece la información bancaria del proveedor
    /// </summary>
    /// <param name="informacionBancaria">Información bancaria</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder ConInformacionBancaria(string informacionBancaria)
    {
        if (string.IsNullOrWhiteSpace(informacionBancaria))
        {
            _notificationManager.AddError("La información bancaria no puede estar vacía", "InformacionBancaria", nameof(informacionBancaria));
            return this;
        }

        if (informacionBancaria.Length > 500)
        {
            _notificationManager.AddError("La información bancaria no puede exceder 500 caracteres", "InformacionBancaria", nameof(informacionBancaria));
            return this;
        }

        _informacionBancaria = informacionBancaria.Trim();
        _logger.LogDebug("Información bancaria establecida");
        return this;
    }

    /// <summary>
    /// Establece los días de crédito del proveedor
    /// </summary>
    /// <param name="diasCredito">Días de crédito (0-365)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder ConDiasCredito(int diasCredito)
    {
        if (diasCredito < 0)
        {
            _notificationManager.AddError("Los días de crédito no pueden ser negativos", "DiasCredito", nameof(diasCredito));
            return this;
        }

        if (diasCredito > 365)
        {
            _notificationManager.AddError("Los días de crédito no pueden exceder 365 días", "DiasCredito", nameof(diasCredito));
            return this;
        }

        _diasCredito = diasCredito;
        _logger.LogDebug("Días de crédito establecidos: {DiasCredito}", _diasCredito);
        return this;
    }

    /// <summary>
    /// Agrega observaciones sobre el proveedor
    /// </summary>
    /// <param name="observaciones">Observaciones</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder ConObservaciones(string observaciones)
    {
        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            if (observaciones.Length > 1000)
            {
                _notificationManager.AddError("Las observaciones no pueden exceder 1000 caracteres", "Observaciones", nameof(observaciones));
                return this;
            }

            _observaciones = observaciones.Trim();
            _logger.LogDebug("Observaciones agregadas");
        }

        return this;
    }

    /// <summary>
    /// Agrega un contacto adicional al proveedor
    /// </summary>
    /// <param name="nombre">Nombre del contacto</param>
    /// <param name="cargo">Cargo del contacto</param>
    /// <param name="telefono">Teléfono del contacto</param>
    /// <param name="email">Email del contacto</param>
    /// <param name="notas">Notas adicionales</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder AgregarContacto(string nombre, string cargo, string telefono, string email, string? notas = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            _notificationManager.AddError("El nombre del contacto no puede estar vacío", "ContactoNombre", nameof(nombre));
            return this;
        }

        if (string.IsNullOrWhiteSpace(cargo))
        {
            _notificationManager.AddError("El cargo del contacto no puede estar vacío", "ContactoCargo", nameof(cargo));
            return this;
        }

        // Validar que no esté duplicado
        var contactoExistente = _contactosAdicionales.FirstOrDefault(c => 
            c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        
        if (contactoExistente != null)
        {
            _notificationManager.AddError($"Ya existe un contacto con el email {email}", "ContactoDuplicado", nameof(email));
            return this;
        }

        _contactosAdicionales.Add(new ContactoProveedorInfo(nombre.Trim(), cargo.Trim(), telefono?.Trim(), email?.Trim(), notas?.Trim()));
        _logger.LogDebug("Contacto adicional agregado: {Nombre} - {Cargo}", nombre, cargo);
        return this;
    }

    /// <summary>
    /// Agrega una categoría al proveedor
    /// </summary>
    /// <param name="categoria">Categoría del proveedor</param>
    /// <param name="porcentajeDescuento">Porcentaje de descuento (0-100)</param>
    /// <param name="esProveedorPrincipal">Indica si es el proveedor principal para esta categoría</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ProveedorBuilder EnCategoria(CategoriaProveedor categoria, decimal porcentajeDescuento = 0, bool esProveedorPrincipal = false)
    {
        if (porcentajeDescuento < 0 || porcentajeDescuento > 100)
        {
            _notificationManager.AddError("El porcentaje de descuento debe estar entre 0 y 100", "PorcentajeDescuento", nameof(porcentajeDescuento));
            return this;
        }

        // Validar que no esté duplicado
        var categoriaExistente = _categorias.FirstOrDefault(c => c.Categoria == categoria);
        if (categoriaExistente != null)
        {
            _notificationManager.AddError($"La categoría {categoria} ya fue agregada al proveedor", "CategoriaDuplicada", nameof(categoria));
            return this;
        }

        _categorias.Add(new CategoriaProveedorInfo(categoria, porcentajeDescuento, esProveedorPrincipal));
        _logger.LogDebug("Categoría agregada: {Categoria} con descuento {Descuento}%", categoria, porcentajeDescuento);
        return this;
    }

    /// <summary>
    /// Construye la instancia final del Proveedor
    /// </summary>
    /// <returns>Resultado con el proveedor creado o errores de validación</returns>
    public Result<Proveedor> Construir()
    {
        _logger.LogDebug("Iniciando construcción del proveedor");

        // Limpiar notificaciones previas
        _notificationManager.ClearErrors();

        try
        {
            // Validaciones finales obligatorias
            var hayErrores = false;

            if (string.IsNullOrWhiteSpace(_nombre))
            {
                _notificationManager.AddError("El nombre del proveedor es obligatorio", "Nombre", nameof(_nombre));
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(_nombreContacto))
            {
                _notificationManager.AddError("El nombre del contacto principal es obligatorio", "NombreContacto", nameof(_nombreContacto));
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(_email))
            {
                _notificationManager.AddError("El email del proveedor es obligatorio", "Email", nameof(_email));
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(_telefono))
            {
                _notificationManager.AddError("El teléfono del proveedor es obligatorio", "Telefono", nameof(_telefono));
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(_direccion))
            {
                _notificationManager.AddError("La dirección del proveedor es obligatoria", "Direccion", nameof(_direccion));
                hayErrores = true;
            }

            if (!_diasCredito.HasValue)
            {
                // Valor por defecto si no se especifica
                _diasCredito = 30;
                _notificationManager.AddInformation("Se asignaron 30 días de crédito por defecto", "DiasCredito");
            }

            // Si hay errores críticos, no continuar
            if (hayErrores)
            {
                _logger.LogWarning("Construcción del proveedor falló debido a errores de validación");
                return _notificationManager.ToResult<Proveedor>(null!);
            }

            // Crear el proveedor usando el factory method de la entidad
            var proveedor = Proveedor.Crear(
                _nombre!,
                _nombreContacto!,
                _email!,
                _telefono!,
                _direccion ?? string.Empty,
                _ciudad ?? string.Empty,
                _codigoPostal ?? string.Empty,
                _pais ?? "México",
                _rfc ?? string.Empty,
                _informacionBancaria ?? string.Empty,
                _diasCredito!.Value);

            // Agregar observaciones si existen
            if (!string.IsNullOrWhiteSpace(_observaciones))
            {
                proveedor.AgregarObservaciones(_observaciones);
            }

            // Agregar contactos adicionales
            foreach (var contacto in _contactosAdicionales)
            {
                proveedor.AgregarContacto(
                    contacto.Nombre,
                    contacto.Cargo,
                    contacto.Telefono ?? string.Empty,
                    contacto.Email ?? string.Empty,
                    false, // No es principal
                    contacto.Notas);
            }

            // Agregar categorías
            foreach (var categoria in _categorias)
            {
                proveedor.AgregarCategoria(categoria.Categoria, categoria.PorcentajeDescuento, categoria.EsProveedorPrincipal);
            }

            _logger.LogInformation("Proveedor construido exitosamente: {ProveedorId} - {Nombre}", proveedor.Id, proveedor.Nombre);
            return Result.Success(proveedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante la construcción del proveedor");
            _notificationManager.AddError($"Error interno: {ex.Message}", "ConstruccionProveedor", nameof(ex));
            return _notificationManager.ToResult<Proveedor>(null!);
        }
    }

    /// <summary>
    /// Reinicia el builder para permitir reutilización
    /// </summary>
    /// <returns>Builder reiniciado</returns>
    public ProveedorBuilder Reset()
    {
        _nombre = null;
        _nombreContacto = null;
        _email = null;
        _telefono = null;
        _direccion = null;
        _ciudad = null;
        _codigoPostal = null;
        _pais = null;
        _rfc = null;
        _informacionBancaria = null;
        _diasCredito = null;
        _observaciones = null;
        _contactosAdicionales.Clear();
        _categorias.Clear();

        // Limpiar notificaciones
        _notificationManager.ClearErrors();

        _logger.LogDebug("ProveedorBuilder reiniciado");
        return this;
    }

    /// <summary>
    /// Método de conveniencia para crear un builder configurado
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger</param>
    /// <returns>Nuevo builder configurado</returns>
    public static ProveedorBuilder Nuevo(INotificationManager notificationManager, ILogger<ProveedorBuilder> logger)
    {
        return new ProveedorBuilder(notificationManager, logger);
    }

    /// <summary>
    /// Clase interna para almacenar información de contactos adicionales
    /// </summary>
    private class ContactoProveedorInfo
    {
        public string Nombre { get; }
        public string Cargo { get; }
        public string? Telefono { get; }
        public string? Email { get; }
        public string? Notas { get; }

        public ContactoProveedorInfo(string nombre, string cargo, string? telefono, string? email, string? notas)
        {
            Nombre = nombre;
            Cargo = cargo;
            Telefono = telefono;
            Email = email;
            Notas = notas;
        }
    }

    /// <summary>
    /// Clase interna para almacenar información de categorías
    /// </summary>
    private class CategoriaProveedorInfo
    {
        public CategoriaProveedor Categoria { get; }
        public decimal PorcentajeDescuento { get; }
        public bool EsProveedorPrincipal { get; }

        public CategoriaProveedorInfo(CategoriaProveedor categoria, decimal porcentajeDescuento, bool esProveedorPrincipal)
        {
            Categoria = categoria;
            PorcentajeDescuento = porcentajeDescuento;
            EsProveedorPrincipal = esProveedorPrincipal;
        }
    }
} 