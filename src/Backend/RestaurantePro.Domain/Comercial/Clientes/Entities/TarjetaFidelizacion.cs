
namespace RestaurantePro.Domain.Comercial.Clientes.Entities
{
    /// <summary>
    /// Entidad que representa una tarjeta de fidelización para clientes
    /// </summary>
    public class TarjetaFidelizacion : EntityBase
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

            tarjeta.AddDomainEvent(new TarjetaFidelizacionCreadaEvent(tarjeta.Id, clienteId, codigo));

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

            AddDomainEvent(new TarjetaFidelizacionActivadaEvent(Id, ClienteId, FechaActivacion.Value));
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

            AddDomainEvent(new TarjetaFidelizacionSuspendidaEvent(Id, ClienteId, motivo));
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

            AddDomainEvent(new TarjetaFidelizacionCanceladaEvent(Id, ClienteId, motivo));
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

            AddDomainEvent(new PuntosAgregadosATarjetaEvent(Id, puntos, PuntosAcumulados));
            
            // Automáticamente actualizamos el nivel según los puntos acumulados
            ActualizarNivelSegunPuntos();
            
            // Registramos en el historial
            return HistorialPuntos.CrearRegistroAgregados(Id, puntos, concepto);
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

            AddDomainEvent(new PuntosAgregadosATarjetaEvent(Id, puntos, PuntosAcumulados));
            
            // Automáticamente actualizamos el nivel según los puntos acumulados
            ActualizarNivelSegunPuntos();
            
            // Registramos en el historial
            return HistorialPuntos.CrearRegistroPorCompra(Id, montoCompra, factorConversion, concepto);
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

            AddDomainEvent(new PuntosCanjeadosEvent(Id, puntos, concepto, PuntosDisponibles));
            
            // Registramos en el historial
            return HistorialPuntos.CrearRegistroCanjeados(Id, puntos, concepto);
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
            
            // Registramos en el historial
            return HistorialPuntos.CrearRegistroVencidos(Id, puntos, concepto);
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

            AddDomainEvent(new NivelFidelizacionActualizadoEvent(Id, nivelAnterior, nuevoNivel));
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
    }
} 