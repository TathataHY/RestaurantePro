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
        public Email Email { get; private set; }

        /// <summary>
        /// Teléfono del cliente.
        /// Medio de contacto alternativo para notificaciones.
        /// </summary>
        public PhoneNumber Telefono { get; private set; }

        /// <summary>
        /// Fecha de nacimiento del cliente
        /// </summary>
        public DateTime FechaNacimiento { get; private set; }

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
        /// <param name="id">Identificador único del cliente</param>
        /// <param name="nombre">Nombre completo del cliente</param>
        /// <param name="email">Email del cliente como ValueObject</param>
        /// <param name="telefono">Teléfono del cliente como ValueObject</param>
        /// <param name="fechaNacimiento">Fecha de nacimiento del cliente</param>
        /// <param name="estaActivo">Estado inicial del cliente (activo por defecto)</param>
        /// <returns>Una nueva instancia de Cliente</returns>
        /// <exception cref="ArgumentNullException">Si algún parámetro requerido es nulo</exception>
        /// <exception cref="ArgumentException">Si los parámetros no son válidos</exception>
        public static Cliente Crear(
            Guid id, 
            ClienteNombre nombre, 
            Email email, 
            PhoneNumber telefono,
            DateTime fechaNacimiento,
            bool estaActivo = true)
        {
            // Usar Guard clauses para validaciones
            Guard.AgainstEmpty(id, nameof(id));
            Guard.AgainstNull(nombre, nameof(nombre));
            Guard.AgainstNull(email, nameof(email));
            Guard.AgainstNull(telefono, nameof(telefono));
            Guard.AgainstMinValue(fechaNacimiento, nameof(fechaNacimiento));
            
            // Validar edad mínima (18 años)
            if (fechaNacimiento > DateTime.Now.AddYears(-18))
            {
                throw BusinessRuleViolationException.ForOperationNotAllowed(
                    "Crear cliente menor de edad",
                    "Cliente",
                    "El cliente debe ser mayor de 18 años",
                    "Comercial",
                    id);
            }

            var cliente = new Cliente
            {
                Id = id,
                Nombre = nombre,
                Email = email,
                Telefono = telefono,
                FechaNacimiento = fechaNacimiento,
                EstaActivo = estaActivo,
                PuntosAcumulados = 0,
                CantidadVisitas = 0,
                Segmento = SegmentoCliente.SinClasificar
            };

            cliente.MarkAsModified();
            cliente.AddDomainEvent(new ClienteCreado(cliente.Id, nombre.NombreCompleto, email, telefono));

            return cliente;
        }

        /// <summary>
        /// Factory Method alternativo para crear una nueva instancia de cliente a partir de strings.
        /// Este método es una conveniencia para casos donde solo están disponibles los valores como string.
        /// </summary>
        /// <param name="nombre">Nombre completo del cliente</param>
        /// <param name="email">Email del cliente como string</param>
        /// <param name="telefono">Teléfono del cliente como string</param>
        /// <param name="fechaNacimiento">Fecha de nacimiento del cliente</param>
        /// <returns>Una nueva instancia de Cliente</returns>
        public static Cliente Crear(ClienteNombre nombre, string email, string telefono, DateTime fechaNacimiento)
        {
            var emailVO = Email.Create(email);
            var telefonoVO = PhoneNumber.Create(telefono);
            
            return Crear(Guid.NewGuid(), nombre, emailVO, telefonoVO, fechaNacimiento);
        }

        /// <summary>
        /// Agrega puntos al cliente en el programa de fidelización.
        /// Se verifica que el cliente esté activo antes de realizar la operación.
        /// </summary>
        /// <param name="puntos">Cantidad de puntos a agregar</param>
        /// <exception cref="ClienteInactivoException">Si el cliente está inactivo</exception>
        /// <exception cref="ArgumentException">Si la cantidad de puntos es menor o igual a cero</exception>
        public void AgregarPuntos(int puntos)
        {
            // Usar nuestras excepciones específicas
            if (!EstaActivo)
                throw ClienteInactivoException.ParaAcumulacionPuntos(Id, puntos);

            Guard.AgainstNegativeOrZero(puntos, nameof(puntos), "La cantidad de puntos debe ser mayor a cero");

            PuntosAcumulados += puntos;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new PuntosAgregados(Id, puntos, PuntosAcumulados));
        }

        /// <summary>
        /// Resta puntos al cliente con motivo especificado.
        /// Se verifica que el cliente esté activo y tenga puntos suficientes.
        /// </summary>
        /// <param name="puntos">Cantidad de puntos a restar</param>
        /// <param name="motivo">Motivo de la resta de puntos</param>
        /// <exception cref="ClienteInactivoException">Si el cliente está inactivo</exception>
        /// <exception cref="BusinessRuleViolationException">Si no tiene puntos suficientes</exception>
        public void RestarPuntos(int puntos, string motivo)
        {
            if (!EstaActivo)
                throw new ClienteInactivoException(Id, "restar puntos");

            Guard.AgainstNegativeOrZero(puntos, nameof(puntos));
            Guard.AgainstNullOrWhiteSpace(motivo, nameof(motivo));

            if (PuntosAcumulados < puntos)
            {
                throw BusinessRuleViolationException.ForOperationNotAllowed(
                    "Restar puntos",
                    "Cliente",
                    $"El cliente no tiene puntos suficientes. Disponibles: {PuntosAcumulados}, Solicitados: {puntos}",
                    "Comercial",
                    Id)
                    .WithData("PuntosDisponibles", PuntosAcumulados)
                    .WithData("PuntosSolicitados", puntos)
                    .WithData("Motivo", motivo) as BusinessRuleViolationException;
            }

            PuntosAcumulados -= puntos;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new PuntosRestados(Id, puntos, PuntosAcumulados, motivo));
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
        /// <param name="email">Nuevo email como ValueObject</param>
        /// <param name="telefono">Nuevo teléfono como ValueObject</param>
        public void ActualizarInformacionContacto(Email email, PhoneNumber telefono)
        {
            Guard.AgainstNull(email, nameof(email));
            Guard.AgainstNull(telefono, nameof(telefono));
            
            if (Email.Value == email.Value && Telefono.Value == telefono.Value)
                return;

            var emailAnterior = Email;
            var telefonoAnterior = Telefono;

            Email = email;
            Telefono = telefono;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new InformacionContactoActualizada(Id, email, telefono));
        }

        /// <summary>
        /// Registra una visita del cliente al restaurante.
        /// Incrementa el contador de visitas y genera el evento correspondiente.
        /// </summary>
        /// <exception cref="ClienteInactivoException">Si el cliente está inactivo</exception>
        public void RegistrarVisita()
        {
            if (!EstaActivo)
                throw ClienteInactivoException.ParaRegistroVisita(Id);

            CantidadVisitas++;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new VisitaRegistrada(Id, CantidadVisitas));
        }

        /// <summary>
        /// Asocia una tarjeta de fidelización al cliente.
        /// Un cliente solo puede tener una tarjeta principal activa.
        /// </summary>
        /// <param name="tarjetaId">ID de la tarjeta de fidelización</param>
        /// <exception cref="ClienteInactivoException">Si el cliente está inactivo</exception>
        /// <exception cref="BusinessRuleViolationException">Si ya tiene una tarjeta asociada</exception>
        public void AsociarTarjetaFidelizacion(Guid tarjetaId)
        {
            if (!EstaActivo)
                throw ClienteInactivoException.ParaAsociacionTarjeta(Id, tarjetaId);

            Guard.AgainstEmpty(tarjetaId, nameof(tarjetaId));

            if (TarjetaFidelizacionPrincipalId.HasValue)
            {
                throw BusinessRuleViolationException.ForOperationNotAllowed(
                    "Asociar tarjeta",
                    "Cliente",
                    "El cliente ya tiene una tarjeta de fidelización asociada",
                    "Comercial",
                    Id)
                    .WithData("TarjetaExistente", TarjetaFidelizacionPrincipalId.Value)
                    .WithData("TarjetaNueva", tarjetaId) as BusinessRuleViolationException;
            }

            TarjetaFidelizacionPrincipalId = tarjetaId;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new TarjetaFidelizacionAsociada(Id, tarjetaId));
        }

        /// <summary>
        /// Actualiza el segmento del cliente según su comportamiento.
        /// </summary>
        /// <param name="nuevoSegmento">Nuevo segmento del cliente</param>
        public void ActualizarSegmento(SegmentoCliente nuevoSegmento)
        {
            if (Segmento == nuevoSegmento)
                return;

            var segmentoAnterior = Segmento;
            Segmento = nuevoSegmento;
            MarkAsModified();
            ValidarInvariantes();

            AddDomainEvent(new SegmentoClienteActualizado(Id, segmentoAnterior, nuevoSegmento));
        }

        /// <summary>
        /// Valida las invariantes del agregado Cliente.
        /// Se ejecuta después de cada operación que modifica el estado.
        /// </summary>
        /// <exception cref="BusinessRuleViolationException">Si alguna invariante es violada</exception>
        private void ValidarInvariantes()
        {
            // Validar que los puntos nunca sean negativos
            if (PuntosAcumulados < 0)
            {
                throw BusinessRuleViolationException.ForInvalidState(
                    "Cliente",
                    $"PuntosAcumulados = {PuntosAcumulados}",
                    "PuntosAcumulados >= 0",
                    "Comercial",
                    Id);
            }

            // Validar que las visitas nunca sean negativas
            if (CantidadVisitas < 0)
            {
                throw BusinessRuleViolationException.ForInvalidState(
                    "Cliente",
                    $"CantidadVisitas = {CantidadVisitas}",
                    "CantidadVisitas >= 0",
                    "Comercial",
                    Id);
            }

            // Validar que un cliente inactivo no puede tener puntos positivos
            if (!EstaActivo && PuntosAcumulados > 0)
            {
                throw BusinessRuleViolationException.ForInvalidState(
                    "Cliente",
                    $"Cliente inactivo con {PuntosAcumulados} puntos",
                    "Cliente inactivo debe tener 0 puntos",
                    "Comercial",
                    Id);
            }
        }

        /// <summary>
        /// Determina si el cliente tiene tarjeta de fidelización
        /// </summary>
        public bool TieneTarjetaFidelizacion() => TarjetaFidelizacionPrincipalId.HasValue;

        /// <summary>
        /// Obtiene los puntos disponibles para utilizar
        /// </summary>
        public int ObtenerPuntosDisponibles() => PuntosAcumulados;

        /// <summary>
        /// Verifica si el cliente es elegible para un descuento
        /// </summary>
        /// <param name="puntosRequeridos">Puntos requeridos para el descuento</param>
        /// <returns>True si es elegible</returns>
        public bool EsElegibleParaDescuento(int puntosRequeridos)
        {
            Guard.AgainstNegativeOrZero(puntosRequeridos, nameof(puntosRequeridos));
            return EstaActivo && PuntosAcumulados >= puntosRequeridos;
        }

        /// <summary>
        /// Calcula la edad actual del cliente
        /// </summary>
        public int CalcularEdad()
        {
            var edad = DateTime.Now.Year - FechaNacimiento.Year;
            if (DateTime.Now.DayOfYear < FechaNacimiento.DayOfYear)
                edad--;
            return edad;
        }
    }
}
