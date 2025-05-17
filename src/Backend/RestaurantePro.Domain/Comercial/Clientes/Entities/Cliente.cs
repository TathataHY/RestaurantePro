namespace RestaurantePro.Domain.Comercial.Clientes.Entities
{
    /// <summary>
    /// Entidad que representa a un cliente del restaurante
    /// </summary>
    public class Cliente : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Nombre completo del cliente
        /// </summary>
        public ClienteNombre Nombre { get; private set; }

        /// <summary>
        /// Email del cliente
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Teléfono del cliente
        /// </summary>
        public string Telefono { get; private set; }

        /// <summary>
        /// Indica si el cliente está activo en el sistema
        /// </summary>
        public bool EstaActivo { get; private set; }

        /// <summary>
        /// Puntos acumulados por el cliente en el programa de fidelización
        /// </summary>
        public int PuntosAcumulados { get; private set; }

        /// <summary>
        /// Cantidad de visitas registradas del cliente
        /// </summary>
        public int CantidadVisitas { get; private set; }

        /// <summary>
        /// ID de la tarjeta de fidelización principal del cliente
        /// </summary>
        public Guid? TarjetaFidelizacionPrincipalId { get; private set; }

        // Constructor privado para EF Core
        private Cliente() { }

        /// <summary>
        /// Crea una nueva instancia de cliente
        /// </summary>
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
        /// Agrega puntos al cliente en el programa de fidelización
        /// </summary>
        public void AgregarPuntos(int puntos)
        {
            if (!EstaActivo)
                throw new InvalidOperationException("No se pueden agregar puntos a un cliente inactivo");

            if (puntos <= 0)
                throw new ArgumentException("La cantidad de puntos debe ser mayor a cero", nameof(puntos));

            PuntosAcumulados += puntos;
            MarkAsModified();

            AddDomainEvent(new PuntosAgregados(Id, puntos, PuntosAcumulados));
        }

        /// <summary>
        /// Desactiva al cliente en el sistema
        /// </summary>
        public void Desactivar()
        {
            if (!EstaActivo)
                return;

            EstaActivo = false;
            MarkAsModified();

            AddDomainEvent(new ClienteDesactivado(Id, Nombre.NombreCompleto));
        }

        /// <summary>
        /// Reactiva al cliente en el sistema
        /// </summary>
        public void Reactivar()
        {
            if (EstaActivo)
                return;

            EstaActivo = true;
            MarkAsModified();

            AddDomainEvent(new ClienteReactivado(Id, Nombre.NombreCompleto));
        }

        /// <summary>
        /// Actualiza la información de contacto del cliente
        /// </summary>
        public void ActualizarInformacionContacto(string email, string telefono)
        {
            if (Email == email && Telefono == telefono)
                return;

            Email = email;
            Telefono = telefono;
            MarkAsModified();

            AddDomainEvent(new InformacionContactoActualizada(Id, Email, Telefono));
        }

        /// <summary>
        /// Registra una nueva visita del cliente
        /// </summary>
        public void RegistrarVisita()
        {
            CantidadVisitas++;
            MarkAsModified();

            AddDomainEvent(new VisitaRegistrada(Id, CantidadVisitas));
        }

        /// <summary>
        /// Asocia una tarjeta de fidelización al cliente
        /// </summary>
        /// <param name="tarjetaId">ID de la tarjeta a asociar</param>
        public void AsociarTarjetaFidelizacion(Guid tarjetaId)
        {
            if (!EstaActivo)
                throw new InvalidOperationException("No se puede asociar una tarjeta a un cliente inactivo");

            TarjetaFidelizacionPrincipalId = tarjetaId;
            MarkAsModified();

            AddDomainEvent(new TarjetaFidelizacionAsociada(Id, tarjetaId));
        }
    }
}
