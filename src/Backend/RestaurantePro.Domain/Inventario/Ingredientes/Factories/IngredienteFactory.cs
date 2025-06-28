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
            var ingrediente = Ingrediente.Crear(
                parametros.Id ?? Guid.NewGuid(),
                parametros.Nombre,
                parametros.Codigo,
                parametros.Descripcion,
                parametros.UnidadMedida,
                parametros.StockMinimo,
                parametros.StockActual,
                parametros.Rotacion,
                parametros.Temporada
            );

            // Aplicar configuraciones adicionales si se proporcionan
            if (parametros.ProveedorPrincipalId.HasValue)
            {
                ingrediente.AsociarProveedorPrincipal(parametros.ProveedorPrincipalId.Value);
            }

            if (parametros.CostoPromedio > 0)
            {
                ingrediente.ActualizarCostoPromedio(parametros.CostoPromedio);
            }

            Logger.LogInformation("Ingrediente creado exitosamente: {Nombre} con código {Codigo}", 
                parametros.Nombre, parametros.Codigo);
            return ingrediente;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Excepción al crear ingrediente {Nombre}", parametros.Nombre);
            AgregarError("Excepcion", "Error interno al crear el ingrediente");
            return null;
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
        if (data is not IngredienteReconstructionData datosReconstruccion)
        {
            Logger.LogError("Los datos deben ser de tipo IngredienteReconstructionData");
            AgregarError("TipoParametros", "Los datos deben ser de tipo IngredienteReconstructionData");
            return null;
        }

        try
        {
            var ingrediente = Ingrediente.Crear(
                id,
                datosReconstruccion.Nombre,
                datosReconstruccion.Codigo,
                datosReconstruccion.Descripcion,
                datosReconstruccion.UnidadMedida,
                datosReconstruccion.StockMinimo,
                datosReconstruccion.Stock,
                datosReconstruccion.Rotacion,
                datosReconstruccion.Temporada
            );

            // Aplicar configuraciones adicionales si se proporcionan
            if (datosReconstruccion.ProveedorPrincipalId.HasValue)
            {
                ingrediente.AsociarProveedorPrincipal(datosReconstruccion.ProveedorPrincipalId.Value);
            }

            if (datosReconstruccion.CostoPromedio > 0)
            {
                ingrediente.ActualizarCostoPromedio(datosReconstruccion.CostoPromedio);
            }

            if (!datosReconstruccion.EstaActivo)
            {
                ingrediente.Desactivar();
            }

            Logger.LogInformation("Ingrediente {IngredienteId} reconstruido exitosamente: {Nombre}", 
                id, datosReconstruccion.Nombre);

            return ingrediente;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al reconstruir Ingrediente {IngredienteId}: {Message}", id, ex.Message);
            AgregarError("Reconstruccion", $"Error al reconstruir el ingrediente: {ex.Message}");
            return null;
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
        
        // Validar código - debe estar presente y no vacío
        if (string.IsNullOrWhiteSpace(parametros.Codigo))
        {
            AgregarError(nameof(parametros.Codigo), "Codigo es requerido");
        }
        else
        {
            if (parametros.Codigo.Length > 50)
            {
                AgregarError(nameof(parametros.Codigo), "Codigo no puede exceder 50 caracteres");
            }
            
            // Validar formato de código - permitir caracteres acentuados para códigos generados automáticamente
            // Formato 1: PREFIJO-YYYYMMDD (ej: ING-20250628) - formato automático
            // Formato 2: PREFIJO-XXXXXXXX (ej: ING-EB0AA216) - formato de test con caracteres hexadecimales
            if (!Regex.IsMatch(parametros.Codigo, @"^[A-ZÀ-ÿ]{2,5}-[0-9A-F]{8}$"))
            {
                AgregarError(nameof(parametros.Codigo), 
                    "Codigo debe tener el formato: 2-5 letras mayúsculas (con o sin acentos), guión, 8 caracteres hexadecimales (ej: TOM-20241125, ING-EB0AA216)");
            }
        }

        // Validar descripción
        if (!string.IsNullOrWhiteSpace(parametros.Descripcion) && parametros.Descripcion.Length > 500)
        {
            AgregarError(nameof(parametros.Descripcion), "Descripcion no puede exceder 500 caracteres");
        }

        // Validar valores numéricos
        ValidarNoNegativo(parametros.StockMinimo, nameof(parametros.StockMinimo));
        ValidarNoNegativo(parametros.StockActual, nameof(parametros.StockActual));
        
        // Validar costo promedio - debe ser NO negativo (puede ser cero)
        if (parametros.CostoPromedio < 0)
        {
            AgregarError(nameof(parametros.CostoPromedio), "CostoPromedio no puede ser negativo");
        }

        // Validar enums
        if (!Enum.IsDefined(typeof(UnidadMedida), parametros.UnidadMedida))
        {
            AgregarError(nameof(parametros.UnidadMedida), "UnidadMedida no es válida");
        }

        if (!Enum.IsDefined(typeof(RotacionIngrediente), parametros.Rotacion))
        {
            AgregarError(nameof(parametros.Rotacion), "Rotacion no es válida");
        }

        if (!Enum.IsDefined(typeof(TemporadaIngrediente), parametros.Temporada))
        {
            AgregarError(nameof(parametros.Temporada), "Temporada no es válida");
        }

        // Advertencia si el stock actual es muy alto comparado con el mínimo
        if (parametros.StockActual > parametros.StockMinimo * 10 && parametros.StockMinimo > 0)
        {
            // Se podría agregar un log de advertencia aquí si fuera necesario
            Logger.LogWarning("El stock actual ({StockActual}) es significativamente mayor que el mínimo ({StockMinimo}) para el ingrediente {Nombre}", 
                parametros.StockActual, parametros.StockMinimo, parametros.Nombre);
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
        // Generar código automáticamente preservando acentos
        var prefijo = nombre.Length >= 3 
            ? nombre.Substring(0, 3).ToUpper() 
            : nombre.ToUpper().PadRight(3, 'X');
        var codigo = $"{prefijo}-{DateTime.Now:yyyyMMdd}";

        return CrearIngrediente(nombre, codigo, descripcion, unidadMedida, stockMinimo, stockActual, rotacion, temporada);
    }

    /// <summary>
    /// Normaliza texto eliminando acentos y caracteres especiales para generar códigos válidos
    /// </summary>
    /// <param name="texto">Texto a normalizar</param>
    /// <returns>Texto normalizado solo con caracteres A-Z</returns>
    private static string NormalizarTextoParaCodigo(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return "XXX";

        // Normalizar caracteres acentuados a su equivalente ASCII
        var stringNormalizada = texto.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (char caracter in stringNormalizada)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);
            if (categoria != UnicodeCategory.NonSpacingMark)
            {
                if (char.IsLetter(caracter))
                {
                    stringBuilder.Append(caracter);
                }
            }
        }

        var resultado = stringBuilder.ToString();
        
        // Si después de la normalización no quedan caracteres válidos, usar XXX
        if (string.IsNullOrWhiteSpace(resultado))
        {
            return "XXX";
        }

        // Asegurar que solo contenga letras A-Z
        return new string(resultado.Where(c => char.IsLetter(c)).ToArray());
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