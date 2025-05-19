namespace RestaurantePro.Domain.Comercial.Clientes.Entities
{
    /// <summary>
    /// Agregado que representa a un cliente del restaurante.
    /// 
    /// Invariantes:
    /// - Un cliente inactivo no puede acumular puntos ni asociar tarjetas de fidelización
    /// - El email y teléfono deben ser válidos según las reglas de validación
    /// - La cantidad de visitas y puntos acumulados nunca pueden ser negativos
    /// - Un cliente puede tener solo una tarjeta de fidelización principal a la vez
    /// 
    /// Ciclo de vida:
    /// - Creación → Activo → [Desactivado ↔ Activado] → Eliminado lógico
    /// 
    /// Reglas de negocio:
    /// - Cuando un cliente se desactiva, se genera un evento ClienteDesactivado
    /// - Las visitas incrementan el contador de visitas y generan eventos
    /// - La relación con TarjetaFidelizacion se mantiene por ID para preservar límites de agregados
    /// </summary>
    public class Cliente : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Nombre completo del cliente
        /// </summary>
        public ClienteNombre Nombre { get; private set; }

        /// <summary>
        /// Email del cliente.
        /// Utilizado como medio de contacto principal y para identificación.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Teléfono del cliente.
        /// Medio de contacto alternativo para notificaciones.
        /// </summary>
        public string Telefono { get; private set; }

        /// <summary>
        /// Indica si el cliente está activo en el sistema.
        /// Un cliente inactivo no puede acumular puntos ni asociar tarjetas.
        /// </summary>
        public bool EstaActivo { get; private set; }

        /// <summary>
        /// Puntos acumulados por el cliente en el programa de fidelización.
        /// Se actualiza mediante operaciones de acumulación y utilización de puntos.
        /// </summary>
        public int PuntosAcumulados { get; private set; }

        /// <summary>
        /// Cantidad de visitas registradas del cliente.
        /// Se incrementa cada vez que el cliente realiza una visita al restaurante.
        /// </summary>
        public int CantidadVisitas { get; private set; }

        /// <summary>
        /// ID de la tarjeta de fidelización principal del cliente.
        /// La relación se mantiene por ID para preservar los límites del agregado.
        /// </summary>
        public Guid? TarjetaFidelizacionPrincipalId { get; private set; }

        /// <summary>
        /// Segmento al que pertenece el cliente según su comportamiento y patrones de consumo.
        /// Se actualiza mediante análisis de comportamiento de compra y visitación.
        /// </summary>
        public SegmentoCliente Segmento { get; private set; }

        // Constructor privado para EF Core
        private Cliente() { }

        /// <summary>
        /// Factory Method para crear una nueva instancia de cliente.
        /// Este es el único punto de entrada para crear instancias válidas de Cliente.
        /// </summary>
        /// <param name="nombre">Nombre completo del cliente</param>
        /// <param name="email">Email del cliente</param>
        /// <param name="telefono">Teléfono del cliente</param>
        /// <returns>Una nueva instancia de Cliente</returns>
        public static Cliente Crear(ClienteNombre nombre, string email, string telefono)
        {
            var cliente = new Cliente
            {
                Nombre = nombre,
                Email = email,
                Telefono = telefono,
                EstaActivo = true,
                PuntosAcumulados = 0
            };

            cliente.AddDomainEvent(new ClienteCreado(cliente.Id, nombre.NombreCompleto));

            return cliente;
        }

        /// <summary>
        /// Agrega puntos al cliente en el programa de fidelización.
        /// Se verifica que el cliente esté activo antes de realizar la operación.
        /// </summary>
        /// <param name="puntos">Cantidad de puntos a agregar</param>
        /// <exception cref="InvalidOperationException">Si el cliente está inactivo</exception>
        /// <exception cref="ArgumentException">Si la cantidad de puntos es menor o igual a cero</exception>
        public void AgregarPuntos(int puntos)
        {
            if (!EstaActivo)
                throw new InvalidOperationException("No se pueden agregar puntos a un cliente inactivo");

            if (puntos <= 0)
                throw new ArgumentException("La cantidad de puntos debe ser mayor a cero", nameof(puntos));

            PuntosAcumulados += puntos;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new PuntosAgregados(Id, puntos, PuntosAcumulados));
        }

        /// <summary>
        /// Desactiva al cliente en el sistema.
        /// Cuando un cliente se desactiva, se genera un evento ClienteDesactivado
        /// que puede desencadenar otras acciones como cancelación de reservaciones.
        /// </summary>
        public void Desactivar()
        {
            if (!EstaActivo)
                return;

            EstaActivo = false;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new ClienteDesactivado(Id, Nombre.NombreCompleto));
        }

        /// <summary>
        /// Reactiva al cliente en el sistema.
        /// Permite que un cliente previamente desactivado vuelva a utilizar los servicios.
        /// </summary>
        public void Reactivar()
        {
            if (EstaActivo)
                return;

            EstaActivo = true;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new ClienteReactivado(Id, Nombre.NombreCompleto));
        }

        /// <summary>
        /// Actualiza la información de contacto del cliente.
        /// Solo se genera un evento de actualización si alguno de los valores cambia.
        /// </summary>
        /// <param name="email">Nuevo email</param>
        /// <param name="telefono">Nuevo teléfono</param>
        public void ActualizarInformacionContacto(string email, string telefono)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede estar vacío", nameof(email));
                
            if (string.IsNullOrWhiteSpace(telefono))
                throw new ArgumentException("El teléfono no puede estar vacío", nameof(telefono));

            if (Email == email && Telefono == telefono)
                return;

            Email = email;
            Telefono = telefono;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new InformacionContactoActualizada(Id, Email, Telefono));
        }

        /// <summary>
        /// Registra una nueva visita del cliente.
        /// Incrementa el contador de visitas y emite un evento VisitaRegistrada.
        /// Este método es utilizado para análisis de frecuencia de clientes.
        /// </summary>
        public void RegistrarVisita()
        {
            CantidadVisitas++;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new VisitaRegistrada(Id, CantidadVisitas));
        }

        /// <summary>
        /// Asocia una tarjeta de fidelización al cliente.
        /// La relación se mantiene por ID para preservar los límites del agregado.
        /// </summary>
        /// <param name="tarjetaId">ID de la tarjeta a asociar</param>
        /// <exception cref="InvalidOperationException">Si el cliente está inactivo</exception>
        public void AsociarTarjetaFidelizacion(Guid tarjetaId)
        {
            if (!EstaActivo)
                throw new InvalidOperationException("No se puede asociar una tarjeta a un cliente inactivo");

            TarjetaFidelizacionPrincipalId = tarjetaId;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new TarjetaFidelizacionAsociada(Id, tarjetaId));
        }
        
        /// <summary>
        /// Resta puntos al cliente en el programa de fidelización.
        /// Útil para registrar canje de puntos por beneficios.
        /// </summary>
        /// <param name="puntos">Cantidad de puntos a restar</param>
        /// <param name="motivo">Motivo del descuento de puntos</param>
        /// <exception cref="InvalidOperationException">Si el cliente está inactivo o no tiene suficientes puntos</exception>
        public void RestarPuntos(int puntos, string motivo)
        {
            if (!EstaActivo)
                throw new InvalidOperationException("No se pueden restar puntos a un cliente inactivo");
                
            if (puntos <= 0)
                throw new ArgumentException("La cantidad de puntos debe ser mayor a cero", nameof(puntos));
                
            if (puntos > PuntosAcumulados)
                throw new InvalidOperationException($"No hay suficientes puntos. Disponibles: {PuntosAcumulados}, Solicitados: {puntos}");
                
            PuntosAcumulados -= puntos;
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new RestaurantePro.Domain.Comercial.Clientes.Events.Cliente.PuntosFidelizacionCanjeados(Id, puntos, PuntosAcumulados, motivo));
        }
        
        /// <summary>
        /// Actualiza el segmento al que pertenece el cliente según su comportamiento.
        /// Este método es utilizado por análisis de comportamiento y segmentación automática.
        /// </summary>
        /// <param name="nuevoSegmento">Nuevo segmento del cliente</param>
        public void ActualizarSegmento(SegmentoCliente nuevoSegmento)
        {
            if (Segmento == nuevoSegmento)
                return;
                
            var segmentoAnterior = Segmento;
            Segmento = nuevoSegmento;
            MarkAsModified();
            
            AddDomainEvent(new SegmentoClienteActualizado(Id, segmentoAnterior, nuevoSegmento));
        }
        
        /// <summary>
        /// Valida todas las invariantes del agregado Cliente.
        /// Se llama después de cada operación que modifica el estado para asegurar la consistencia.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si alguna invariante se viola</exception>
        private void ValidarInvariantes()
        {
            // Validar que los puntos acumulados no sean negativos
            if (PuntosAcumulados < 0)
                throw new InvalidOperationException($"Los puntos acumulados del cliente no pueden ser negativos. Valor actual: {PuntosAcumulados}");
            
            // Validar que la cantidad de visitas no sea negativa
            if (CantidadVisitas < 0)
                throw new InvalidOperationException($"La cantidad de visitas del cliente no puede ser negativa. Valor actual: {CantidadVisitas}");
            
            // Validar que el nombre no sea nulo
            if (Nombre == null)
                throw new InvalidOperationException("El nombre del cliente no puede ser nulo");
            
            // Validar que el email no esté vacío y tenga un formato válido
            if (string.IsNullOrWhiteSpace(Email))
                throw new InvalidOperationException("El email del cliente no puede estar vacío");
                
            if (!Email.Contains("@") || !Email.Contains("."))
                throw new InvalidOperationException($"El email '{Email}' no tiene un formato válido");
            
            // Validar que el teléfono no esté vacío
            if (string.IsNullOrWhiteSpace(Telefono))
                throw new InvalidOperationException("El teléfono del cliente no puede estar vacío");
        }
    }
}
