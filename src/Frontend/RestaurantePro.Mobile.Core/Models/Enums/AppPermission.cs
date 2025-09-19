namespace RestaurantePro.Mobile.Core.Models.Enums;

/// <summary>
/// Permisos específicos disponibles en la aplicación móvil
/// Basado en la matriz de distribución de funcionalidades por rol
/// </summary>
public enum AppPermission
{
    // ===== COMANDAS (CORE PARA MESEROS) =====
    /// <summary>
    /// Crear nuevas comandas
    /// </summary>
    CrearComandas = 1,
    
    /// <summary>
    /// Ver comandas existentes
    /// </summary>
    VerComandas = 2,
    
    /// <summary>
    /// Modificar comandas existentes
    /// </summary>
    ModificarComandas = 3,
    
    /// <summary>
    /// Cerrar comandas completadas
    /// </summary>
    CerrarComandas = 4,
    
    // ===== MESAS (CORE PARA MESEROS) =====
    /// <summary>
    /// Ver estado de todas las mesas
    /// </summary>
    VerEstadoMesas = 5,
    
    /// <summary>
    /// Cambiar estado de las mesas
    /// </summary>
    CambiarEstadoMesas = 6,
    
    /// <summary>
    /// Asignar mesas a comandas
    /// </summary>
    AsignarMesas = 7,
    
    // ===== PREPARACIONES (CORE PARA COCINEROS) =====
    /// <summary>
    /// Ver órdenes pendientes de preparación
    /// </summary>
    VerPreparacionesPendientes = 8,
    
    /// <summary>
    /// Actualizar estado de preparaciones
    /// </summary>
    ActualizarEstadoPreparaciones = 9,
    
    /// <summary>
    /// Marcar preparaciones como completadas
    /// </summary>
    CompletarPreparaciones = 10,
    
    // ===== FACTURACIÓN (CORE PARA CAJEROS) =====
    /// <summary>
    /// Generar facturas de venta
    /// </summary>
    GenerarFacturas = 11,
    
    /// <summary>
    /// Procesar pagos
    /// </summary>
    ProcesarPagos = 12,
    
    /// <summary>
    /// Aplicar promociones básicas
    /// </summary>
    AplicarPromociones = 13,
    
    // ===== PRODUCTOS/MENÚ (CONSULTA GENERAL) =====
    /// <summary>
    /// Consultar productos del menú
    /// </summary>
    ConsultarProductos = 14,
    
    /// <summary>
    /// Ver información detallada de productos
    /// </summary>
    VerDetalleProductos = 15,
    
    // ===== CLIENTES (BÁSICO) =====
    /// <summary>
    /// Consultar información básica de clientes
    /// </summary>
    ConsultarClientes = 16,
    
    /// <summary>
    /// Gestionar tarjetas de fidelización (uso)
    /// </summary>
    UsarTarjetasFidelizacion = 17,
    
    // ===== INVENTARIO (CONSULTA BÁSICA) =====
    /// <summary>
    /// Consultar disponibilidad de ingredientes
    /// </summary>
    ConsultarDisponibilidadIngredientes = 18,
    
    /// <summary>
    /// Ver alertas de inventario crítico
    /// </summary>
    VerAlertasInventario = 19,
    
    // ===== RESERVACIONES (LIMITADO) =====
    /// <summary>
    /// Consultar reservaciones del día
    /// </summary>
    ConsultarReservaciones = 20,
    
    /// <summary>
    /// Confirmar llegada de reservaciones
    /// </summary>
    ConfirmarReservaciones = 21,
    
    // ===== ADMINISTRACIÓN (SUPERVISORES) =====
    /// <summary>
    /// Acceder a reportes básicos
    /// </summary>
    VerReportesBasicos = 22,
    
    /// <summary>
    /// Gestionar personal del turno
    /// </summary>
    GestionarPersonalTurno = 23,
    
    /// <summary>
    /// Supervisar operaciones generales
    /// </summary>
    SupervisarOperaciones = 24,
    
    // ===== CONFIGURACIÓN =====
    /// <summary>
    /// Cambiar configuraciones personales
    /// </summary>
    ConfiguracionPersonal = 25,
    
    /// <summary>
    /// Ver información del perfil
    /// </summary>
    VerPerfil = 26
}
