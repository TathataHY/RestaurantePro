namespace RestaurantePro.Domain.Inventario.Ingredientes.Factories;

/// <summary>
/// Factory para crear instancias de Ingrediente siguiendo el patrón Factory con Result/Notification
/// </summary>
public class IngredienteFactory : EntityFactoryBase<Ingrediente, Guid>
{
    /// <summary>
    /// Constructor del factory
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger para eventos del factory</param>
    public IngredienteFactory(INotificationManager notificationManager, ILogger<IngredienteFactory> logger)
        : base(notificationManager, logger)
    {
    }

    /// <summary>
    /// Crea una nueva entidad Ingrediente validando primero los parámetros
    /// </summary>
    /// <param name="parameters">Parámetros de creación como IngredienteCreationParameters</param>
    /// <returns>Resultado con la entidad creada o errores</returns>
    protected override Ingrediente? CrearEntidadInterno(object parameters)
    {
        if (parameters is not IngredienteCreationParameters parametros)
        {
            Logger.LogError("Los parámetros deben ser de tipo IngredienteCreationParameters");
            return null;
        }

        try
        {
            // Crear la entidad Ingrediente
            var id = parametros.Id ?? Guid.NewGuid();
            var ingrediente = Ingrediente.Crear(
                id,
                parametros.Nombre,
                parametros.Codigo,
                parametros.Descripcion,
                parametros.UnidadMedida,
                parametros.StockMinimo,
                parametros.StockActual,
                parametros.Rotacion,
                parametros.Temporada);

            // Asignar propiedades adicionales si se proporcionan
            if (parametros.ProveedorPrincipalId.HasValue)
            {
                ingrediente.AsociarProveedorPrincipal(parametros.ProveedorPrincipalId.Value);
            }

            if (parametros.CostoPromedio > 0)
            {
                ingrediente.ActualizarCostoPromedio(parametros.CostoPromedio);
            }

            Logger.LogInformation("Ingrediente {IngredienteId} creado exitosamente: {Nombre} ({UnidadMedida})", 
                ingrediente.Id, parametros.Nombre, parametros.UnidadMedida);

            return ingrediente;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al crear Ingrediente: {Message}", ex.Message);
            throw; // Re-lanzar para que EntityFactoryBase lo maneje
        }
    }

    /// <summary>
    /// Reconstruye una entidad Ingrediente desde datos persistidos
    /// </summary>
    /// <param name="id">ID de la entidad</param>
    /// <param name="data">Datos para reconstruir como IngredienteReconstructionData</param>
    /// <returns>Entidad reconstruida</returns>
    protected override Ingrediente? ReconstruirEntidadInterno(Guid id, object data)
    {
        if (data is not IngredienteReconstructionData datos)
        {
            Logger.LogError("Los datos deben ser de tipo IngredienteReconstructionData");
            return null;
        }

        try
        {
            // Reconstruir entidad usando el factory method básico
            var ingrediente = Ingrediente.Crear(
                id,
                datos.Nombre,
                datos.Codigo,
                datos.Descripcion,
                datos.UnidadMedida,
                datos.StockMinimo,
                datos.Stock,
                datos.Rotacion,
                datos.Temporada);

            // Usar reflection para establecer valores que no se pueden establecer a través de los métodos públicos
            if (datos.ProveedorPrincipalId.HasValue)
            {
                ingrediente.AsociarProveedorPrincipal(datos.ProveedorPrincipalId.Value);
            }

            if (datos.CostoPromedio > 0)
            {
                ingrediente.ActualizarCostoPromedio(datos.CostoPromedio);
            }

            // Usar reflection para propiedades que no tienen métodos públicos
            if (!datos.EstaActivo)
            {
                ingrediente.Desactivar();
            }

            Logger.LogDebug("Ingrediente {IngredienteId} reconstruido exitosamente", id);
            return ingrediente;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al reconstruir Ingrediente {IngredienteId}: {Message}", id, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Validaciones específicas para IngredienteCreationParameters
    /// </summary>
    /// <param name="parameters">Parámetros a validar</param>
    protected override void ValidarParametrosEspecificos(object parameters)
    {
        if (parameters is not IngredienteCreationParameters parametros)
        {
            AgregarError("TipoParametros", "Los parámetros deben ser de tipo IngredienteCreationParameters");
            return;
        }

        // Validar campos requeridos
        ValidarStringRequerido(parametros.Nombre, nameof(parametros.Nombre), 100);
        ValidarStringRequerido(parametros.Codigo, nameof(parametros.Codigo), 50);
        
        // Validar descripción (opcional)
        if (!string.IsNullOrWhiteSpace(parametros.Descripcion) && parametros.Descripcion.Length > 500)
        {
            AgregarError(nameof(parametros.Descripcion), "La descripción no puede exceder 500 caracteres");
        }

        // Validar stocks
        if (parametros.StockMinimo < 0)
        {
            AgregarError(nameof(parametros.StockMinimo), "El stock mínimo no puede ser negativo");
        }

        if (parametros.StockActual < 0)
        {
            AgregarError(nameof(parametros.StockActual), "El stock actual no puede ser negativo");
        }

        // Validar costo promedio
        if (parametros.CostoPromedio < 0)
        {
            AgregarError(nameof(parametros.CostoPromedio), "El costo promedio no puede ser negativo");
        }

        // Validar unidad de medida
        if (!Enum.IsDefined(typeof(UnidadMedida), parametros.UnidadMedida))
        {
            AgregarError(nameof(parametros.UnidadMedida), "La unidad de medida no es válida");
        }

        // Validar rotación
        if (!Enum.IsDefined(typeof(RotacionIngrediente), parametros.Rotacion))
        {
            AgregarError(nameof(parametros.Rotacion), "El nivel de rotación no es válido");
        }

        // Validar temporada
        if (!Enum.IsDefined(typeof(TemporadaIngrediente), parametros.Temporada))
        {
            AgregarError(nameof(parametros.Temporada), "La temporada no es válida");
        }

        // Validar código único (formato básico)
        if (!string.IsNullOrWhiteSpace(parametros.Codigo))
        {
            if (!Regex.IsMatch(parametros.Codigo, @"^[A-Z]{2,5}-\d{4,8}$"))
            {
                AgregarError(nameof(parametros.Codigo), 
                    "El código debe tener el formato: 2-5 letras mayúsculas, guión, 4-8 dígitos (ej: TOM-20241125)");
            }
        }

        // Validar coherencia de stocks - generar advertencia no crítica
        if (parametros.StockActual > 0 && parametros.StockMinimo > 0 && parametros.StockActual > parametros.StockMinimo * 10)
        {
            // Por ahora esto es solo una nota para el desarrollador
            Logger.LogWarning("El stock actual ({StockActual}) es significativamente mayor al stock mínimo ({StockMinimo}). Verificar si es correcto.", 
                parametros.StockActual, parametros.StockMinimo);
        }
    }

    /// <summary>
    /// Método de conveniencia para crear un Ingrediente con parámetros directos
    /// </summary>
    /// <param name="nombre">Nombre del ingrediente</param>
    /// <param name="codigo">Código del ingrediente</param>
    /// <param name="descripcion">Descripción del ingrediente</param>
    /// <param name="unidadMedida">Unidad de medida</param>
    /// <param name="stockMinimo">Stock mínimo</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="rotacion">Nivel de rotación</param>
    /// <param name="temporada">Temporada</param>
    /// <param name="proveedorPrincipalId">ID del proveedor principal</param>
    /// <param name="costoPromedio">Costo promedio</param>
    /// <returns>Resultado con el Ingrediente creado</returns>
    public Result<Ingrediente> CrearIngrediente(
        string nombre,
        string codigo,
        string descripcion,
        UnidadMedida unidadMedida,
        decimal stockMinimo,
        decimal stockActual,
        RotacionIngrediente rotacion = RotacionIngrediente.Media,
        TemporadaIngrediente temporada = TemporadaIngrediente.TodoElAño,
        Guid? proveedorPrincipalId = null,
        decimal costoPromedio = 0)
    {
        var parameters = new IngredienteCreationParameters
        {
            Nombre = nombre,
            Codigo = codigo,
            Descripcion = descripcion,
            UnidadMedida = unidadMedida,
            StockMinimo = stockMinimo,
            StockActual = stockActual,
            Rotacion = rotacion,
            Temporada = temporada,
            ProveedorPrincipalId = proveedorPrincipalId,
            CostoPromedio = costoPromedio
        };

        return Crear(parameters);
    }

    /// <summary>
    /// Método de conveniencia para crear ingrediente con código generado automáticamente
    /// </summary>
    /// <param name="nombre">Nombre del ingrediente</param>
    /// <param name="descripcion">Descripción del ingrediente</param>
    /// <param name="unidadMedida">Unidad de medida</param>
    /// <param name="stockMinimo">Stock mínimo</param>
    /// <param name="stockActual">Stock actual</param>
    /// <param name="rotacion">Nivel de rotación</param>
    /// <param name="temporada">Temporada</param>
    /// <returns>Resultado con el Ingrediente creado</returns>
    public Result<Ingrediente> CrearIngredienteConCodigoAutomatico(
        string nombre,
        string descripcion,
        UnidadMedida unidadMedida,
        decimal stockMinimo,
        decimal stockActual,
        RotacionIngrediente rotacion = RotacionIngrediente.Media,
        TemporadaIngrediente temporada = TemporadaIngrediente.TodoElAño)
    {
        // Generar código automático
        var prefijo = nombre.Length >= 3 
            ? nombre.Substring(0, 3).ToUpper() 
            : nombre.ToUpper().PadRight(3, 'X');
        var codigo = $"{prefijo}-{DateTime.Now:yyyyMMdd}";

        return CrearIngrediente(nombre, codigo, descripcion, unidadMedida, stockMinimo, stockActual, rotacion, temporada);
    }
}

/// <summary>
/// Parámetros para crear un nuevo Ingrediente
/// </summary>
public class IngredienteCreationParameters
{
    public Guid? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public UnidadMedida UnidadMedida { get; set; } = UnidadMedida.Unidad;
    public decimal StockMinimo { get; set; } = 0;
    public decimal StockActual { get; set; } = 0;
    public RotacionIngrediente Rotacion { get; set; } = RotacionIngrediente.Media;
    public TemporadaIngrediente Temporada { get; set; } = TemporadaIngrediente.TodoElAño;
    public Guid? ProveedorPrincipalId { get; set; }
    public decimal CostoPromedio { get; set; } = 0;
}

/// <summary>
/// Datos para reconstruir un Ingrediente existente
/// </summary>
public class IngredienteReconstructionData
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public UnidadMedida UnidadMedida { get; set; } = UnidadMedida.Unidad;
    public decimal StockMinimo { get; set; } = 0;
    public decimal Stock { get; set; } = 0;
    public bool EstaActivo { get; set; } = true;
    public RotacionIngrediente Rotacion { get; set; } = RotacionIngrediente.Media;
    public TemporadaIngrediente Temporada { get; set; } = TemporadaIngrediente.TodoElAño;
    public Guid? ProveedorPrincipalId { get; set; }
    public bool BloqueadoControlCalidad { get; set; } = false;
    public decimal CostoPromedio { get; set; } = 0;
} 