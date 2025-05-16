namespace RestaurantePro.Domain.Operaciones.Comandas.Entities
{
    /// <summary>
    /// Aggregate Root que representa una comanda en el restaurante
    /// </summary>
    public class Comanda : EntityBase, IAggregateRoot
    {
        private readonly List<ItemComanda> _items = new List<ItemComanda>();

        /// <summary>
        /// ID de la mesa asociada a la comanda
        /// </summary>
        public Guid MesaId { get; private set; }

        /// <summary>
        /// ID del usuario (mesero) que creó la comanda
        /// </summary>
        public Guid MeseroId { get; private set; }

        /// <summary>
        /// ID del cliente asociado a la comanda
        /// </summary>
        public Guid? ClienteId { get; private set; }

        /// <summary>
        /// Fecha de creación de la comanda
        /// </summary>
        public new DateTime FechaCreacion { get; private set; }

        /// <summary>
        /// Fecha de actualización de la comanda
        /// </summary>
        public new DateTime? FechaActualizacion { get; private set; }

        /// <summary>
        /// Estado actual de la comanda
        /// </summary>
        public EstadoComanda Estado { get; private set; }

        /// <summary>
        /// Observaciones adicionales para la comanda
        /// </summary>
        public string Observaciones { get; private set; }

        /// <summary>
        /// Total de la comanda
        /// </summary>
        public TotalComanda Total { get; private set; }

        /// <summary>
        /// Detalles de los productos incluidos en la comanda
        /// </summary>
        public IReadOnlyCollection<ItemComanda> Items => _items.AsReadOnly();

        /// <summary>
        /// Constructor privado para EF Core
        /// </summary>
        private Comanda() { }

        /// <summary>
        /// Constructor para crear una nueva comanda
        /// </summary>
        public static Comanda Crear(Guid mesaId, Guid meseroId, Guid? clienteId = null, string observaciones = null)
        {
            var comanda = new Comanda
            {
                Id = Guid.NewGuid(),
                MesaId = mesaId,
                MeseroId = meseroId,
                ClienteId = clienteId,
                FechaCreacion = DateTime.Now,
                Estado = EstadoComanda.Creada,
                Observaciones = observaciones,
                Total = TotalComanda.Crear(0, 0)
            };

            comanda.AddDomainEvent(new ComandaCreada(comanda.Id, mesaId, meseroId));

            return comanda;
        }

        /// <summary>
        /// Método para agregar un producto a la comanda
        /// </summary>
        public void AgregarProducto(Guid productoId, int cantidad, decimal precioUnitario, string observaciones = null)
        {
            ValidarComandaActiva();

            var item = new ItemComanda(Id, productoId, cantidad, precioUnitario, observaciones);
            _items.Add(item);

            RecalcularTotal();
            ActualizarFecha();

            AddDomainEvent(new ProductoAgregadoAComanda(Id, productoId, cantidad));
        }

        /// <summary>
        /// Método para actualizar el estado de la comanda
        /// </summary>
        public void ActualizarEstado(EstadoComanda nuevoEstado)
        {
            // Validar transición de estado válida
            if (!EsTransicionEstadoValida(nuevoEstado))
            {
                throw new InvalidOperationException($"No se puede cambiar el estado de {Estado} a {nuevoEstado}");
            }

            // Validar que una comanda no pueda finalizarse sin productos
            if (nuevoEstado == EstadoComanda.Finalizada && !Items.Any())
            {
                throw new InvalidOperationException("No se puede finalizar una comanda sin productos");
            }

            var estadoAnterior = Estado;
            Estado = nuevoEstado;
            ActualizarFecha();

            AddDomainEvent(new EstadoComandaActualizado(Id, estadoAnterior, nuevoEstado));

            // Si la comanda se finaliza, agregar evento específico
            if (nuevoEstado == EstadoComanda.Finalizada)
            {
                AddDomainEvent(new ComandaFinalizada(Id, Total.Total));
            }
        }

        /// <summary>
        /// Método para cancelar la comanda
        /// </summary>
        public void Cancelar(string motivo)
        {
            ValidarComandaActiva();

            Estado = EstadoComanda.Cancelada;
            Observaciones = string.IsNullOrEmpty(Observaciones)
                ? $"Cancelada: {motivo}"
                : $"{Observaciones} | Cancelada: {motivo}";

            ActualizarFecha();

            AddDomainEvent(new ComandaCancelada(Id, motivo));
        }

        /// <summary>
        /// Métodos privados para validaciones y lógica interna
        /// </summary>
        private void ValidarComandaActiva()
        {
            if (Estado != EstadoComanda.Creada && Estado != EstadoComanda.EnProceso)
            {
                throw new InvalidOperationException($"No se pueden realizar cambios en una comanda con estado {Estado}");
            }
        }

        private bool EsTransicionEstadoValida(EstadoComanda nuevoEstado)
        {
            return (Estado, nuevoEstado) switch
            {
                (EstadoComanda.Creada, EstadoComanda.EnProceso) => true,
                (EstadoComanda.EnProceso, EstadoComanda.Lista) => true,
                (EstadoComanda.Lista, EstadoComanda.Entregada) => true,
                (EstadoComanda.Entregada, EstadoComanda.Finalizada) => true,
                _ => false
            };
        }

        private void RecalcularTotal()
        {
            decimal subtotal = _items.Sum(i => i.Subtotal);
            decimal impuesto = subtotal * 0.16m; // IVA del 16%

            Total = TotalComanda.Crear(subtotal, impuesto);
        }

        private void ActualizarFecha()
        {
            FechaActualizacion = DateTime.Now;
        }
    }
}
