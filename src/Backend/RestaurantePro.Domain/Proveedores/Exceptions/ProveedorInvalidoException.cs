namespace RestaurantePro.Domain.Proveedores.Exceptions;

/// <summary>
/// Excepción que se lanza cuando se intenta realizar una operación inválida con un proveedor.
/// </summary>
public class ProveedorInvalidoException : BusinessRuleViolationException
{
    /// <summary>
    /// ID del proveedor involucrado
    /// </summary>
    public Guid ProveedorId { get; }

    /// <summary>
    /// Estado actual del proveedor
    /// </summary>
    public bool EstaActivo { get; }

    /// <summary>
    /// Constructor principal para proveedor inválido
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <param name="estaActivo">Estado actual del proveedor</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <param name="razon">Razón por la cual la operación es inválida</param>
    public ProveedorInvalidoException(
        Guid proveedorId,
        bool estaActivo,
        string operacion,
        string razon)
        : base(
            "ProveedorInvalido",
            "Proveedor",
            $"No se puede realizar '{operacion}' con proveedor {(estaActivo ? "activo" : "inactivo")}: {razon}",
            "Proveedores",
            proveedorId)
    {
        ProveedorId = proveedorId;
        EstaActivo = estaActivo;
        
        WithData("EstaActivo", EstaActivo)
            .WithData("Operacion", operacion)
            .WithData("Razon", razon);
    }

    /// <summary>
    /// Crea excepción para proveedor inactivo
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <param name="operacion">Operación que se intentó realizar</param>
    /// <returns>Nueva instancia de ProveedorInvalidoException</returns>
    public static ProveedorInvalidoException ParaProveedorInactivo(Guid proveedorId, string operacion = "generar orden de compra")
    {
        return new ProveedorInvalidoException(
            proveedorId,
            false,
            operacion,
            "El proveedor está inactivo");
    }

    /// <summary>
    /// Crea excepción para RFC duplicado
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <param name="rfc">RFC duplicado</param>
    /// <returns>Nueva instancia de ProveedorInvalidoException</returns>
    public static ProveedorInvalidoException ParaRFCDuplicado(Guid proveedorId, string rfc)
    {
        return new ProveedorInvalidoException(
            proveedorId,
            true,
            "registrar RFC",
            $"Ya existe otro proveedor registrado con el RFC '{rfc}'")
            .WithData("RFC", rfc) as ProveedorInvalidoException;
    }

    /// <summary>
    /// Crea excepción para contacto duplicado
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <param name="email">Email duplicado</param>
    /// <returns>Nueva instancia de ProveedorInvalidoException</returns>
    public static ProveedorInvalidoException ParaContactoDuplicado(Guid proveedorId, string email)
    {
        return new ProveedorInvalidoException(
            proveedorId,
            true,
            "agregar contacto",
            $"Ya existe un contacto con el email '{email}' en este proveedor")
            .WithData("EmailDuplicado", email) as ProveedorInvalidoException;
    }

    /// <summary>
    /// Crea excepción para categoría no válida
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <param name="categoria">Categoría inválida</param>
    /// <param name="razon">Razón específica del rechazo</param>
    /// <returns>Nueva instancia de ProveedorInvalidoException</returns>
    public static ProveedorInvalidoException ParaCategoriaInvalida(
        Guid proveedorId, 
        ProveedorCategoria categoria, 
        string razon)
    {
        return new ProveedorInvalidoException(
            proveedorId,
            true,
            "asignar categoría",
            $"Categoría '{categoria}' no válida: {razon}")
            .WithData("Categoria", categoria.ToString())
            .WithData("RazonRechazo", razon) as ProveedorInvalidoException;
    }

    /// <summary>
    /// Crea excepción para descuento inválido
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <param name="descuentoSolicitado">Descuento solicitado</param>
    /// <param name="descuentoMaximo">Descuento máximo permitido</param>
    /// <returns>Nueva instancia de ProveedorInvalidoException</returns>
    public static ProveedorInvalidoException ParaDescuentoInvalido(
        Guid proveedorId, 
        decimal descuentoSolicitado, 
        decimal descuentoMaximo)
    {
        return new ProveedorInvalidoException(
            proveedorId,
            true,
            "establecer descuento",
            $"El descuento solicitado ({descuentoSolicitado:P}) excede el máximo permitido ({descuentoMaximo:P})")
            .WithData("DescuentoSolicitado", descuentoSolicitado)
            .WithData("DescuentoMaximo", descuentoMaximo) as ProveedorInvalidoException;
    }

    /// <summary>
    /// Crea excepción para días de crédito inválidos
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <param name="diasCredito">Días de crédito solicitados</param>
    /// <param name="maximoPermitido">Máximo permitido</param>
    /// <returns>Nueva instancia de ProveedorInvalidoException</returns>
    public static ProveedorInvalidoException ParaDiasCreditoInvalidos(
        Guid proveedorId, 
        int diasCredito, 
        int maximoPermitido)
    {
        return new ProveedorInvalidoException(
            proveedorId,
            true,
            "establecer días de crédito",
            $"Los días de crédito solicitados ({diasCredito}) exceden el máximo permitido ({maximoPermitido})")
            .WithData("DiasCreditoSolicitados", diasCredito)
            .WithData("MaximoPermitido", maximoPermitido) as ProveedorInvalidoException;
    }

    /// <summary>
    /// Crea excepción para información bancaria faltante
    /// </summary>
    /// <param name="proveedorId">ID del proveedor</param>
    /// <returns>Nueva instancia de ProveedorInvalidoException</returns>
    public static ProveedorInvalidoException ParaInformacionBancariaFaltante(Guid proveedorId)
    {
        return new ProveedorInvalidoException(
            proveedorId,
            true,
            "procesar pago",
            "No se puede procesar el pago porque falta la información bancaria del proveedor");
    }
} 