using RestaurantePro.Application.Proveedores.Proveedores.DTOs;

namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.CrearProveedor;

/// <summary>
/// Comando para crear un nuevo proveedor
/// Incluye validaciones de negocio y factory methods para diferentes escenarios
/// </summary>
public class CrearProveedorCommand : IRequest<Result<ProveedorDto>>
{
    /// <summary>
    /// Nombre del proveedor (obligatorio)
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del contacto principal (obligatorio)
    /// </summary>
    public string NombreContacto { get; set; } = string.Empty;

    /// <summary>
    /// Email del proveedor (obligatorio)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del proveedor (obligatorio)
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Dirección del proveedor (obligatorio)
    /// </summary>
    public string Direccion { get; set; } = string.Empty;

    /// <summary>
    /// Ciudad del proveedor (obligatorio)
    /// </summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>
    /// Código postal (opcional)
    /// </summary>
    public string CodigoPostal { get; set; } = string.Empty;

    /// <summary>
    /// País (opcional, por defecto México)
    /// </summary>
    public string Pais { get; set; } = "México";

    /// <summary>
    /// RFC del proveedor (obligatorio)
    /// </summary>
    public string RFC { get; set; } = string.Empty;

    /// <summary>
    /// Información bancaria (opcional)
    /// </summary>
    public string InformacionBancaria { get; set; } = string.Empty;

    /// <summary>
    /// Días de crédito (opcional, por defecto 0 = contado)
    /// </summary>
    public int DiasCredito { get; set; } = 0;

    /// <summary>
    /// Usuario que crea el proveedor
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public CrearProveedorCommand() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public CrearProveedorCommand(
        string nombre,
        string nombreContacto,
        string email,
        string telefono,
        string direccion,
        string ciudad,
        string rfc,
        Guid usuarioId)
    {
        Nombre = nombre;
        NombreContacto = nombreContacto;
        Email = email;
        Telefono = telefono;
        Direccion = direccion;
        Ciudad = ciudad;
        RFC = rfc;
        UsuarioId = usuarioId;
    }

    /// <summary>
    /// Factory method para proveedor básico (solo datos esenciales)
    /// </summary>
    public static CrearProveedorCommand CrearBasico(
        string nombre,
        string nombreContacto,
        string email,
        string telefono,
        string ciudad,
        string rfc,
        Guid usuarioId)
    {
        return new CrearProveedorCommand
        {
            Nombre = nombre,
            NombreContacto = nombreContacto,
            Email = email,
            Telefono = telefono,
            Direccion = $"Dirección en {ciudad}",
            Ciudad = ciudad,
            RFC = rfc,
            Pais = "México",
            DiasCredito = 0,
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method para proveedor con crédito
    /// </summary>
    public static CrearProveedorCommand CrearConCredito(
        string nombre,
        string nombreContacto,
        string email,
        string telefono,
        string direccion,
        string ciudad,
        string rfc,
        int diasCredito,
        string informacionBancaria,
        Guid usuarioId)
    {
        return new CrearProveedorCommand
        {
            Nombre = nombre,
            NombreContacto = nombreContacto,
            Email = email,
            Telefono = telefono,
            Direccion = direccion,
            Ciudad = ciudad,
            RFC = rfc,
            DiasCredito = diasCredito,
            InformacionBancaria = informacionBancaria,
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method para proveedor internacional
    /// </summary>
    public static CrearProveedorCommand CrearInternacional(
        string nombre,
        string nombreContacto,
        string email,
        string telefono,
        string direccion,
        string ciudad,
        string pais,
        string identificacionFiscal,
        Guid usuarioId)
    {
        return new CrearProveedorCommand
        {
            Nombre = nombre,
            NombreContacto = nombreContacto,
            Email = email,
            Telefono = telefono,
            Direccion = direccion,
            Ciudad = ciudad,
            Pais = pais,
            RFC = identificacionFiscal, // En otros países puede no ser RFC
            DiasCredito = 30, // Default para internacionales
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method desde DTO
    /// </summary>
    public static CrearProveedorCommand DesdeDto(ProveedorCreateDto dto, Guid usuarioId)
    {
        return new CrearProveedorCommand
        {
            Nombre = dto.Nombre,
            NombreContacto = dto.NombreContacto,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            Ciudad = dto.Ciudad,
            CodigoPostal = dto.CodigoPostal,
            Pais = dto.Pais,
            RFC = dto.RFC,
            InformacionBancaria = dto.InformacionBancaria,
            DiasCredito = dto.DiasCredito,
            UsuarioId = usuarioId
        };
    }
} 