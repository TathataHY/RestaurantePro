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
        /// <returns>Resultado con la comanda creada</returns>
        Task<Result<Comanda>> CrearNuevaComandaAsync(Guid? clienteId, Guid? mesaId, Guid meseroId, string observaciones = "", CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un producto a una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidad">Cantidad a agregar</param>
        /// <param name="observaciones">Observaciones para el ítem</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la comanda actualizada</returns>
        Task<Result<Comanda>> AgregarProductoAComandaAsync(Guid comandaId, Guid productoId, int cantidad, string observaciones = "", CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 🍳 Crea una nueva comanda con productos usando flujo híbrido de preparaciones
        /// Verifica preparaciones diarias primero, luego inventario si es necesario
        /// </summary>
        /// <param name="clienteId">ID del cliente (opcional)</param>
        /// <param name="mesaId">ID de la mesa (opcional)</param>
        /// <param name="meseroId">ID del mesero</param>
        /// <param name="productos">Lista de productos con cantidad y observaciones</param>
        /// <param name="observacionesComanda">Observaciones generales de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la comanda creada incluyendo estadísticas del flujo</returns>
        Task<Result<Comanda>> CrearComandaConProductosAsync(
            Guid? clienteId,
            Guid? mesaId,
            Guid meseroId,
            IEnumerable<(Guid ProductoId, int Cantidad, string Observaciones)> productos,
            string observacionesComanda = "",
            CancellationToken cancellationToken = default);
        
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
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> AgregarPersonalizacionExtraAItemAsync(Guid comandaId, Guid itemId, Guid ingredienteId, string nombreIngrediente, decimal cantidad, decimal precioAdicional = 0, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega una personalización de tipo "quitar" a un ítem de comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="itemId">ID del ítem</param>
        /// <param name="ingredienteId">ID del ingrediente a quitar</param>
        /// <param name="nombreIngrediente">Nombre del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> AgregarPersonalizacionQuitarAItemAsync(Guid comandaId, Guid itemId, Guid ingredienteId, string nombreIngrediente, CancellationToken cancellationToken = default);
        
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
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> AgregarPersonalizacionSustituirAItemAsync(Guid comandaId, Guid itemId, Guid ingredienteId, string nombreIngrediente, Guid ingredienteSustitucionId, string nombreIngredienteSustitucion, decimal cantidad = 1, decimal precioAdicional = 0, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza el estado de una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> ActualizarEstadoComandaAsync(Guid comandaId, EstadoComanda nuevoEstado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Aplica un descuento a una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="montoDescuento">Monto del descuento</param>
        /// <param name="motivo">Motivo del descuento</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> AplicarDescuentoComandaAsync(Guid comandaId, decimal montoDescuento, string motivo, CancellationToken cancellationToken = default);
        
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
        /// <returns>Resultado con la reservación creada</returns>
        Task<Result<Reservacion>> CrearReservacionAsync(Guid clienteId, DateTime fecha, int cantidadPersonas, string observaciones = "", CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene una reservación por su ID
        /// </summary>
        /// <param name="reservacionId">ID de la reservación a obtener</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la reservación solicitada</returns>
        Task<Result<Reservacion>> ObtenerReservacionAsync(Guid reservacionId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Asigna una mesa a una reservación
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> AsignarMesaAReservacionAsync(Guid reservacionId, Guid mesaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza el estado de una reservación
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> ActualizarEstadoReservacionAsync(Guid reservacionId, EstadoReservacion nuevoEstado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica la disponibilidad de mesas para una fecha y cantidad de personas específicas
        /// </summary>
        /// <param name="fecha">Fecha y hora deseada</param>
        /// <param name="cantidadPersonas">Cantidad de personas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la lista de IDs de mesas disponibles</returns>
        Task<Result<IEnumerable<Guid>>> VerificarDisponibilidadMesasAsync(DateTime fecha, int cantidadPersonas, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las reservaciones para un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la lista de reservaciones en el rango</returns>
        Task<Result<IEnumerable<Reservacion>>> ObtenerReservacionesPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Convierte una reservación en una comanda
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="meseroId">ID del mesero que atiende</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la comanda creada</returns>
        Task<Result<Comanda>> ConvertirReservacionAComandaAsync(Guid reservacionId, Guid meseroId, CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Mesas
        
        /// <summary>
        /// Registra una nueva mesa en el restaurante
        /// </summary>
        /// <param name="numero">Número de la mesa</param>
        /// <param name="capacidad">Capacidad de personas</param>
        /// <param name="ubicacion">Ubicación de la mesa</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la mesa registrada</returns>
        Task<Result<Mesa>> RegistrarMesaAsync(int numero, int capacidad, string ubicacion, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza la información de una mesa existente
        /// </summary>
        /// <param name="mesaId">ID de la mesa a actualizar</param>
        /// <param name="capacidad">Nueva capacidad</param>
        /// <param name="ubicacion">Nueva ubicación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la mesa actualizada</returns>
        Task<Result<Mesa>> ActualizarMesaAsync(Guid mesaId, int capacidad, string ubicacion, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Cambia el estado de una mesa (Disponible, Ocupada, FueraDeServicio)
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="nuevoEstado">Nuevo estado de la mesa</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> CambiarEstadoMesaAsync(Guid mesaId, EstadoMesa nuevoEstado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Pone una mesa fuera de servicio con motivo
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="motivo">Motivo por el cual se pone fuera de servicio</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> PonerMesaFueraDeServicioAsync(Guid mesaId, string motivo, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Libera una mesa y la marca como disponible
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> LiberarMesaAsync(Guid mesaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las mesas disponibles que cumplen con la capacidad mínima
        /// </summary>
        /// <param name="capacidadMinima">Capacidad mínima requerida</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la lista de mesas disponibles</returns>
        Task<Result<IEnumerable<Mesa>>> ObtenerMesasDisponiblesAsync(int capacidadMinima = 1, CancellationToken cancellationToken = default);
        
        #endregion

        #region Preparaciones Diarias

        /// <summary>
        /// Prepara un producto con una cantidad específica para el día
        /// </summary>
        /// <param name="productoId">ID del producto a preparar</param>
        /// <param name="cantidad">Cantidad a preparar</param>
        /// <param name="chefId">ID del chef que realiza la preparación</param>
        /// <param name="fechaVencimiento">Fecha de vencimiento</param>
        /// <param name="observaciones">Observaciones de la preparación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la preparación creada</returns>
        Task<Result<PreparacionDiaria>> PrepararProductoAsync(Guid productoId, int cantidad, Guid chefId, DateTime fechaVencimiento, string? observaciones = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las preparaciones del día actual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la lista de preparaciones del día</returns>
        Task<Result<IEnumerable<PreparacionDiaria>>> ObtenerPreparacionesDelDiaAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las preparaciones de un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con las preparaciones del producto</returns>
        Task<Result<IEnumerable<PreparacionDiaria>>> ObtenerPreparacionesPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si hay suficiente cantidad preparada de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cantidadRequerida">Cantidad requerida</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado indicando si hay disponibilidad</returns>
        Task<Result<bool>> VerificarDisponibilidadPreparacionAsync(Guid productoId, int cantidadRequerida, CancellationToken cancellationToken = default);

        #endregion
    }
} 