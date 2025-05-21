namespace RestaurantePro.Domain.Operaciones.Services
{
    /// <summary>
    /// Fachada de servicios para el contexto de Operaciones
    /// Esta interfaz expone operaciones compuestas para ser utilizadas por la capa de Aplicación
    /// </summary>
    public interface IOperacionesServiceFacade
    {
        #region Comandas
        
        /// <summary>
        /// Crea una nueva comanda
        /// </summary>
        /// <param name="clienteId">ID del cliente (opcional)</param>
        /// <param name="mesaId">ID de la mesa (opcional)</param>
        /// <param name="meseroId">ID del mesero</param>
        /// <param name="observaciones">Observaciones generales</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Comanda creada</returns>
        Task<Comanda> CrearNuevaComandaAsync(Guid? clienteId, Guid? mesaId, Guid meseroId, string observaciones = "", CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un producto a una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidad">Cantidad a agregar</param>
        /// <param name="observaciones">Observaciones para el ítem</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Comanda actualizada o null si no se encontró</returns>
        Task<Comanda?> AgregarProductoAComandaAsync(Guid comandaId, Guid productoId, int cantidad, string observaciones = "", CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega una personalización de tipo "agregar extra" a un ítem de comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="itemId">ID del ítem</param>
        /// <param name="ingredienteId">ID del ingrediente a agregar</param>
        /// <param name="nombreIngrediente">Nombre del ingrediente</param>
        /// <param name="cantidad">Cantidad a agregar</param>
        /// <param name="precioAdicional">Precio adicional por el ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se agregó correctamente, False si no se encontró el ítem</returns>
        Task<bool> AgregarPersonalizacionExtraAItemAsync(Guid comandaId, Guid itemId, Guid ingredienteId, string nombreIngrediente, decimal cantidad, decimal precioAdicional = 0, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega una personalización de tipo "quitar" a un ítem de comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="itemId">ID del ítem</param>
        /// <param name="ingredienteId">ID del ingrediente a quitar</param>
        /// <param name="nombreIngrediente">Nombre del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se agregó correctamente, False si no se encontró el ítem</returns>
        Task<bool> AgregarPersonalizacionQuitarAItemAsync(Guid comandaId, Guid itemId, Guid ingredienteId, string nombreIngrediente, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega una personalización de tipo "sustituir" a un ítem de comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="itemId">ID del ítem</param>
        /// <param name="ingredienteId">ID del ingrediente a sustituir</param>
        /// <param name="nombreIngrediente">Nombre del ingrediente a sustituir</param>
        /// <param name="ingredienteSustitucionId">ID del ingrediente de sustitución</param>
        /// <param name="nombreIngredienteSustitucion">Nombre del ingrediente de sustitución</param>
        /// <param name="cantidad">Cantidad a sustituir</param>
        /// <param name="precioAdicional">Precio adicional por la sustitución</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se agregó correctamente, False si no se encontró el ítem</returns>
        Task<bool> AgregarPersonalizacionSustituirAItemAsync(Guid comandaId, Guid itemId, Guid ingredienteId, string nombreIngrediente, Guid ingredienteSustitucionId, string nombreIngredienteSustitucion, decimal cantidad = 1, decimal precioAdicional = 0, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza el estado de una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se actualizó correctamente, False si no se encontró la comanda</returns>
        Task<bool> ActualizarEstadoComandaAsync(Guid comandaId, EstadoComanda nuevoEstado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Aplica un descuento a una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="montoDescuento">Monto del descuento</param>
        /// <param name="motivo">Motivo del descuento</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se aplicó correctamente, False si no se encontró la comanda</returns>
        Task<bool> AplicarDescuentoComandaAsync(Guid comandaId, decimal montoDescuento, string motivo, CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Reservaciones
        
        /// <summary>
        /// Crea una nueva reservación
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="fecha">Fecha y hora de la reservación</param>
        /// <param name="cantidadPersonas">Cantidad de personas</param>
        /// <param name="observaciones">Observaciones</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Reservación creada</returns>
        Task<Reservacion> CrearReservacionAsync(Guid clienteId, DateTime fecha, int cantidadPersonas, string observaciones = "", CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Asigna una mesa a una reservación
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se asignó correctamente, False si no se encontró la reservación</returns>
        Task<bool> AsignarMesaAReservacionAsync(Guid reservacionId, Guid mesaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza el estado de una reservación
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se actualizó correctamente, False si no se encontró la reservación</returns>
        Task<bool> ActualizarEstadoReservacionAsync(Guid reservacionId, EstadoReservacion nuevoEstado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica la disponibilidad de mesas para una fecha y cantidad de personas específicas
        /// </summary>
        /// <param name="fecha">Fecha y hora deseada</param>
        /// <param name="cantidadPersonas">Cantidad de personas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de IDs de mesas disponibles</returns>
        Task<IEnumerable<Guid>> VerificarDisponibilidadMesasAsync(DateTime fecha, int cantidadPersonas, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las reservaciones para un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de reservaciones en el rango</returns>
        Task<IEnumerable<Reservacion>> ObtenerReservacionesPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Convierte una reservación en una comanda
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="meseroId">ID del mesero que atiende</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Comanda creada o null si no se encontró la reservación</returns>
        Task<Comanda?> ConvertirReservacionAComandaAsync(Guid reservacionId, Guid meseroId, CancellationToken cancellationToken = default);
        
        #endregion
    }
} 