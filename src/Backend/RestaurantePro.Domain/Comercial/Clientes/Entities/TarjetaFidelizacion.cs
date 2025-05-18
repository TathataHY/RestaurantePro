namespace RestaurantePro.Domain.Comercial.Clientes.Entities
{
    /// <summary>
    /// Agregado que representa una tarjeta de fidelización para clientes.
    /// 
    /// Invariantes:
    /// - Los puntos acumulados y disponibles nunca pueden ser negativos
    /// - Solo las tarjetas activas pueden acumular o canjear puntos
    /// - Cada movimiento de puntos debe registrarse en el historial interno
    /// - Una vez cancelada, una tarjeta no puede ser reactivada
    /// - Una tarjeta debe estar vinculada a un cliente válido
    /// 
    /// Ciclo de vida:
    /// - Creación/Emitida → [Activa ↔ Suspendida] → [Cancelada | Expirada | Reemplazada]
    /// 
    /// Reglas de negocio:
    /// - La acumulación de puntos actualiza automáticamente el nivel de fidelización
    /// - Los niveles de fidelización se determinan por la cantidad total de puntos acumulados
    /// - Las tarjetas tienen una fecha de expiración y pueden marcarse como expiradas
    /// - Cada operación de puntos genera un registro en el historial y eventos de dominio
    /// </summary>
    public class TarjetaFidelizacion : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Código único de la tarjeta (visible para el cliente)
        /// </summary>
        public string Codigo { get; private set; }

        /// <summary>
        /// Identificador del cliente propietario de la tarjeta
        /// </summary>
        public Guid ClienteId { get; private set; }

        /// <summary>
        /// Estado actual de la tarjeta
        /// </summary>
        public EstadoTarjeta Estado { get; private set; }

        /// <summary>
        /// Nivel de fidelización actual
        /// </summary>
        public NivelFidelizacion NivelFidelizacion { get; private set; }

        /// <summary>
        /// Fecha en que se emitió la tarjeta
        /// </summary>
        public DateTime FechaEmision { get; private set; }

        /// <summary>
        /// Fecha en que se activó la tarjeta (null si no está activada)
        /// </summary>
        public DateTime? FechaActivacion { get; private set; }

        /// <summary>
        /// Fecha de expiración de la tarjeta (habitualmente un año después de la emisión)
        /// </summary>
        public DateTime? FechaExpiracion { get; private set; }

        /// <summary>
        /// Total de puntos acumulados en la tarjeta (histórico)
        /// </summary>
        public int PuntosAcumulados { get; private set; }

        /// <summary>
        /// Puntos disponibles para canjear
        /// </summary>
        public int PuntosDisponibles { get; private set; }

        /// <summary>
        /// Historial de operaciones con puntos de esta tarjeta
        /// </summary>
        private readonly List<HistorialPuntos> _historialPuntos = new();

        /// <summary>
        /// Acceso de solo lectura al historial de puntos
        /// </summary>
        public IReadOnlyCollection<HistorialPuntos> HistorialPuntos => _historialPuntos.AsReadOnly();

        // Constructor privado para EF Core
        private TarjetaFidelizacion() { }

        /// <summary>
        /// Crea una nueva tarjeta de fidelización
        /// </summary>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="codigo">Código único de la tarjeta</param>
        /// <returns>Nueva instancia de tarjeta</returns>
        public static TarjetaFidelizacion Crear(Guid clienteId, string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("El código de la tarjeta no puede estar vacío", nameof(codigo));

            var tarjeta = new TarjetaFidelizacion
            {
                ClienteId = clienteId,
                Codigo = codigo,
                Estado = EstadoTarjeta.Emitida,
                NivelFidelizacion = NivelFidelizacion.Basico,
                FechaEmision = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddYears(1),
                PuntosAcumulados = 0,
                PuntosDisponibles = 0
            };

            tarjeta.AddDomainEvent(new TarjetaFidelizacionCreada(tarjeta.Id, clienteId, codigo));
            
            // Validar invariantes al crear la tarjeta
            tarjeta.ValidarInvariantes();

            return tarjeta;
        }

        /// <summary>
        /// Activa la tarjeta de fidelización
        /// </summary>
        public void Activar()
        {
            if (Estado == EstadoTarjeta.Activa)
                return;

            if (Estado == EstadoTarjeta.Cancelada || Estado == EstadoTarjeta.Expirada || Estado == EstadoTarjeta.Reemplazada)
                throw new InvalidOperationException($"No se puede activar una tarjeta en estado {Estado}");

            Estado = EstadoTarjeta.Activa;
            FechaActivacion = DateTime.Now;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new TarjetaFidelizacionActivada(Id, ClienteId, FechaActivacion.Value));
        }

        /// <summary>
        /// Suspende temporalmente la tarjeta
        /// </summary>
        /// <param name="motivo">Motivo de la suspensión</param>
        public void Suspender(string motivo)
        {
            if (Estado != EstadoTarjeta.Activa)
                throw new InvalidOperationException("Solo se pueden suspender tarjetas activas");

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo de suspensión no puede estar vacío", nameof(motivo));

            Estado = EstadoTarjeta.Suspendida;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new TarjetaFidelizacionSuspendida(Id, ClienteId, motivo));
        }

        /// <summary>
        /// Cancela permanentemente la tarjeta
        /// </summary>
        /// <param name="motivo">Motivo de la cancelación</param>
        public void Cancelar(string motivo)
        {
            if (Estado == EstadoTarjeta.Cancelada)
                return;

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo de cancelación no puede estar vacío", nameof(motivo));

            Estado = EstadoTarjeta.Cancelada;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new TarjetaFidelizacionCancelada(Id, ClienteId, motivo));
        }

        /// <summary>
        /// Agrega puntos a la tarjeta
        /// </summary>
        /// <param name="puntos">Cantidad de puntos a agregar</param>
        /// <param name="concepto">Concepto o razón de los puntos</param>
        /// <returns>Registro del historial creado</returns>
        public HistorialPuntos AgregarPuntos(int puntos, string concepto = "Puntos por compra")
        {
            if (Estado != EstadoTarjeta.Activa)
                throw new InvalidOperationException("Solo se pueden agregar puntos a tarjetas activas");

            if (puntos <= 0)
                throw new ArgumentException("La cantidad de puntos debe ser mayor a cero", nameof(puntos));

            PuntosAcumulados += puntos;
            PuntosDisponibles += puntos;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new PuntosAgregadosATarjeta(Id, puntos, PuntosAcumulados));

            // Automáticamente actualizamos el nivel según los puntos acumulados
            ActualizarNivelSegunPuntos();

            // Registramos en el historial
            var historial = Comercial.Clientes.Entities.HistorialPuntos.CrearRegistroAgregados(Id, puntos, concepto);
            _historialPuntos.Add(historial);
            
            return historial;
        }

        /// <summary>
        /// Agrega puntos a la tarjeta basados en el monto de compra
        /// </summary>
        /// <param name="montoCompra">Monto de la compra</param>
        /// <param name="factorConversion">Factor de conversión (monto por punto)</param>
        /// <param name="concepto">Concepto de la compra</param>
        /// <returns>Registro del historial creado</returns>
        public HistorialPuntos AgregarPuntosPorCompra(decimal montoCompra, int factorConversion, string concepto)
        {
            if (Estado != EstadoTarjeta.Activa)
                throw new InvalidOperationException("Solo se pueden agregar puntos a tarjetas activas");

            if (montoCompra <= 0)
                throw new ArgumentException("El monto de la compra debe ser mayor a cero", nameof(montoCompra));

            if (factorConversion <= 0)
                throw new ArgumentException("El factor de conversión debe ser mayor a cero", nameof(factorConversion));

            // Calculamos puntos en base al monto y factor
            int puntos = (int)(montoCompra / factorConversion);

            if (puntos <= 0)
                puntos = 1; // Mínimo un punto por compra

            PuntosAcumulados += puntos;
            PuntosDisponibles += puntos;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new PuntosAgregadosATarjeta(Id, puntos, PuntosAcumulados));

            // Automáticamente actualizamos el nivel según los puntos acumulados
            ActualizarNivelSegunPuntos();

            // Registramos en el historial
            var historial = Comercial.Clientes.Entities.HistorialPuntos.CrearRegistroPorCompra(Id, montoCompra, factorConversion, concepto);
            _historialPuntos.Add(historial);
            
            return historial;
        }

        /// <summary>
        /// Canjea puntos de la tarjeta por algún beneficio
        /// </summary>
        /// <param name="puntos">Cantidad de puntos a canjear</param>
        /// <param name="concepto">Concepto o razón del canje</param>
        /// <returns>Registro del historial creado</returns>
        public HistorialPuntos CanjearPuntos(int puntos, string concepto)
        {
            if (Estado != EstadoTarjeta.Activa)
                throw new InvalidOperationException("Solo se pueden canjear puntos de tarjetas activas");

            if (puntos <= 0)
                throw new ArgumentException("La cantidad de puntos debe ser mayor a cero", nameof(puntos));

            if (PuntosDisponibles < puntos)
                throw new InvalidOperationException($"Puntos insuficientes. Disponibles: {PuntosDisponibles}, Solicitados: {puntos}");

            if (string.IsNullOrWhiteSpace(concepto))
                throw new ArgumentException("El concepto del canje no puede estar vacío", nameof(concepto));

            PuntosDisponibles -= puntos;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new RestaurantePro.Domain.Comercial.Clientes.Events.TarjetaFidelizacion.PuntosCanjeados(Id, puntos, concepto, PuntosDisponibles));

            // Registramos en el historial
            var historial = Comercial.Clientes.Entities.HistorialPuntos.CrearRegistroCanjeados(Id, puntos, concepto);
            _historialPuntos.Add(historial);
            
            return historial;
        }

        /// <summary>
        /// Registra expiración de puntos
        /// </summary>
        /// <param name="puntos">Cantidad de puntos a expirar</param>
        /// <param name="concepto">Motivo de la expiración</param>
        /// <returns>Registro del historial creado</returns>
        public HistorialPuntos ExpirarPuntos(int puntos, string concepto = "Expiración por tiempo")
        {
            if (Estado != EstadoTarjeta.Activa)
                throw new InvalidOperationException("Solo se pueden expirar puntos de tarjetas activas");

            if (puntos <= 0)
                throw new ArgumentException("La cantidad de puntos debe ser mayor a cero", nameof(puntos));

            if (PuntosDisponibles < puntos)
                throw new InvalidOperationException($"Puntos insuficientes. Disponibles: {PuntosDisponibles}, Solicitados: {puntos}");

            PuntosDisponibles -= puntos;
            MarkAsModified();
            ValidarInvariantes();

            // Registramos en el historial
            var historial = Comercial.Clientes.Entities.HistorialPuntos.CrearRegistroVencidos(Id, puntos, concepto);
            _historialPuntos.Add(historial);
            
            return historial;
        }

        /// <summary>
        /// Actualiza manualmente el nivel de fidelización
        /// </summary>
        /// <param name="nuevoNivel">Nuevo nivel de fidelización</param>
        public void ActualizarNivel(NivelFidelizacion nuevoNivel)
        {
            if (NivelFidelizacion == nuevoNivel)
                return;

            var nivelAnterior = NivelFidelizacion;
            NivelFidelizacion = nuevoNivel;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new NivelFidelizacionActualizado(Id, ClienteId, nivelAnterior, nuevoNivel));
        }

        /// <summary>
        /// Actualiza automáticamente el nivel según los puntos acumulados
        /// </summary>
        private void ActualizarNivelSegunPuntos()
        {
            var nuevoNivel = NivelFidelizacion;

            if (PuntosAcumulados >= 10000)
                nuevoNivel = NivelFidelizacion.Platino;
            else if (PuntosAcumulados >= 5000)
                nuevoNivel = NivelFidelizacion.Oro;
            else if (PuntosAcumulados >= 1000)
                nuevoNivel = NivelFidelizacion.Plata;

            if (nuevoNivel != NivelFidelizacion)
                ActualizarNivel(nuevoNivel);
        }
        
        /// <summary>
        /// Valida todas las invariantes del agregado TarjetaFidelizacion.
        /// Se llama después de cada operación que modifica el estado para asegurar la consistencia.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si alguna invariante se viola</exception>
        private void ValidarInvariantes()
        {
            // Validar que los puntos nunca sean negativos
            if (PuntosAcumulados < 0)
                throw new InvalidOperationException($"Los puntos acumulados no pueden ser negativos. Valor actual: {PuntosAcumulados}");
                
            if (PuntosDisponibles < 0)
                throw new InvalidOperationException($"Los puntos disponibles no pueden ser negativos. Valor actual: {PuntosDisponibles}");
                
            // Validar que los puntos disponibles nunca sean mayores que los acumulados
            if (PuntosDisponibles > PuntosAcumulados)
                throw new InvalidOperationException($"Los puntos disponibles ({PuntosDisponibles}) no pueden ser mayores que los acumulados ({PuntosAcumulados})");
            
            // Validar que el código no esté vacío
            if (string.IsNullOrWhiteSpace(Codigo))
                throw new InvalidOperationException("El código de la tarjeta no puede estar vacío");
                
            // Validar que la tarjeta esté asociada a un cliente válido
            if (ClienteId == Guid.Empty)
                throw new InvalidOperationException("La tarjeta debe estar asociada a un cliente válido");
                
            // Validar consistencia de fechas
            if (FechaExpiracion.HasValue && FechaEmision > FechaExpiracion.Value)
                throw new InvalidOperationException("La fecha de expiración no puede ser anterior a la fecha de emisión");
                
            if (FechaActivacion.HasValue && FechaEmision > FechaActivacion.Value)
                throw new InvalidOperationException("La fecha de activación no puede ser anterior a la fecha de emisión");
                
            // Validar estado
            if (!Enum.IsDefined(typeof(EstadoTarjeta), Estado))
                throw new InvalidOperationException($"Estado de tarjeta no válido: {Estado}");
                
            if (!Enum.IsDefined(typeof(NivelFidelizacion), NivelFidelizacion))
                throw new InvalidOperationException($"Nivel de fidelización no válido: {NivelFidelizacion}");
        }
    }
}
