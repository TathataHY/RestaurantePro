using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Domain.Operaciones.Services
{
    /// <summary>
    /// Implementación de la fachada de servicios para el contexto de Operaciones
    /// </summary>
    public class OperacionesServiceFacade : IOperacionesServiceFacade
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly IReservacionRepository _reservacionRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IProductoRepository _productoRepository;
        
        public OperacionesServiceFacade(
            IComandaRepository comandaRepository,
            IReservacionRepository reservacionRepository,
            IMesaRepository mesaRepository,
            IProductoRepository productoRepository)
        {
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
        }
        
        #region Comandas

        /// <inheritdoc />
        public async Task<Comanda> CrearNuevaComandaAsync(
            Guid? clienteId, 
            Guid? mesaId, 
            Guid meseroId, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            // Crear la comanda
            var comanda = Comanda.Crear(
                meseroId,
                clienteId, 
                mesaId.HasValue ? mesaId.Value : (Guid?)null, 
                observaciones);
            
            // Persistir la comanda
            await _comandaRepository.AgregarAsync(comanda);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);
            
            return comanda;
        }

        /// <inheritdoc />
        public async Task<Comanda?> AgregarProductoAComandaAsync(
            Guid comandaId, 
            Guid productoId, 
            int cantidad, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            // Obtener la comanda
            var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
            if (comanda == null)
            {
                return null;
            }
            
            // Obtener el producto
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
            if (producto == null)
            {
                throw new InvalidOperationException($"No se encontró el producto con ID {productoId}");
            }
            
            // Agregar el producto a la comanda
            comanda.AgregarItem(productoId, producto.Nombre, cantidad, producto.Precio.Valor, observaciones);
            
            // Persistir cambios
            await _comandaRepository.ActualizarAsync(comanda);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);
            
            return comanda;
        }

        /// <inheritdoc />
        public async Task<bool> AgregarPersonalizacionExtraAItemAsync(
            Guid comandaId, 
            Guid itemId, 
            Guid ingredienteId, 
            string nombreIngrediente, 
            decimal cantidad, 
            decimal precioAdicional = 0, 
            CancellationToken cancellationToken = default)
        {
            // Obtener la comanda
            var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
            if (comanda == null)
            {
                return false;
            }
            
            // Agregar personalización de tipo "extra"
            var personalizado = comanda.AgregarPersonalizacionExtra(
                itemId, 
                ingredienteId, 
                nombreIngrediente, 
                cantidad, 
                precioAdicional);
            
            if (!personalizado)
            {
                return false;
            }
            
            // Persistir cambios
            await _comandaRepository.ActualizarAsync(comanda);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> AgregarPersonalizacionQuitarAItemAsync(
            Guid comandaId, 
            Guid itemId, 
            Guid ingredienteId, 
            string nombreIngrediente, 
            CancellationToken cancellationToken = default)
        {
            // Obtener la comanda
            var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
            if (comanda == null)
            {
                return false;
            }
            
            // Agregar personalización de tipo "quitar"
            var personalizado = comanda.AgregarPersonalizacionQuitar(
                itemId, 
                ingredienteId, 
                nombreIngrediente);
            
            if (!personalizado)
            {
                return false;
            }
            
            // Persistir cambios
            await _comandaRepository.ActualizarAsync(comanda);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> AgregarPersonalizacionSustituirAItemAsync(
            Guid comandaId, 
            Guid itemId, 
            Guid ingredienteId, 
            string nombreIngrediente, 
            Guid ingredienteSustitucionId, 
            string nombreIngredienteSustitucion, 
            decimal cantidad = 1, 
            decimal precioAdicional = 0, 
            CancellationToken cancellationToken = default)
        {
            // Obtener la comanda
            var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
            if (comanda == null)
            {
                return false;
            }
            
            // Agregar personalización de tipo "sustituir"
            var personalizado = comanda.AgregarPersonalizacionSustituir(
                itemId, 
                ingredienteId, 
                nombreIngrediente, 
                ingredienteSustitucionId, 
                nombreIngredienteSustitucion, 
                cantidad, 
                precioAdicional);
            
            if (!personalizado)
            {
                return false;
            }
            
            // Persistir cambios
            await _comandaRepository.ActualizarAsync(comanda);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> ActualizarEstadoComandaAsync(
            Guid comandaId, 
            EstadoComanda nuevoEstado, 
            CancellationToken cancellationToken = default)
        {
            // Obtener la comanda
            var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
            if (comanda == null)
            {
                return false;
            }
            
            // Actualizar estado según el tipo
            switch (nuevoEstado)
            {
                case EstadoComanda.EnProceso:
                    comanda.MarcarEnPreparacion();
                    break;
                case EstadoComanda.Lista:
                    comanda.MarcarLista();
                    break;
                case EstadoComanda.Entregada:
                    comanda.MarcarEntregada();
                    break;
                case EstadoComanda.Finalizada:
                    comanda.MarcarPagada();
                    break;
                case EstadoComanda.Cancelada:
                    comanda.Cancelar("Cancelada desde servicio de operaciones");
                    break;
                default:
                    throw new InvalidOperationException($"No se puede actualizar al estado {nuevoEstado}");
            }
            
            // Persistir cambios
            await _comandaRepository.ActualizarAsync(comanda);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> AplicarDescuentoComandaAsync(
            Guid comandaId, 
            decimal montoDescuento, 
            string motivo, 
            CancellationToken cancellationToken = default)
        {
            // Obtener la comanda
            var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
            if (comanda == null)
            {
                return false;
            }
            
            // Aplicar descuento
            comanda.AplicarDescuento(montoDescuento, motivo);
            
            // Persistir cambios
            await _comandaRepository.ActualizarAsync(comanda);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }

        #endregion
        
        #region Reservaciones

        /// <inheritdoc />
        public async Task<Reservacion> CrearReservacionAsync(
            Guid clienteId, 
            DateTime fecha, 
            int cantidadPersonas, 
            string observaciones = "", 
            CancellationToken cancellationToken = default)
        {
            // Buscar mesa disponible
            var mesasDisponibles = await _reservacionRepository.ObtenerMesasDisponiblesAsync(
                fecha.Date, 
                fecha.TimeOfDay, 
                cantidadPersonas, 
                90, // Duración predeterminada en minutos
                cancellationToken);
            
            if (!mesasDisponibles.Any())
            {
                throw new InvalidOperationException("No hay mesas disponibles para la fecha y cantidad de personas especificadas");
            }
            
            // Seleccionar la primera mesa disponible
            var mesaId = mesasDisponibles.First();
            
            // Crear la reservación
            var reservacion = Reservacion.Crear(
                mesaId,
                clienteId,
                fecha,
                TimeSpan.FromMinutes(90), // Duración predeterminada
                cantidadPersonas,
                "", // Teléfono vacío (se debe actualizar después)
                "", // Email vacío (se debe actualizar después)
                observaciones);
            
            // Persistir la reservación
            await _reservacionRepository.AgregarAsync(reservacion);
            await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
            
            return reservacion;
        }

        /// <inheritdoc />
        public async Task<bool> AsignarMesaAReservacionAsync(
            Guid reservacionId, 
            Guid mesaId, 
            CancellationToken cancellationToken = default)
        {
            // Obtener la reservación
            var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
            if (reservacion == null)
            {
                return false;
            }
            
            // Verificar que la mesa exista
            var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
            if (mesa == null)
            {
                throw new InvalidOperationException($"No se encontró la mesa con ID {mesaId}");
            }
            
            // Verificar disponibilidad
            var disponible = await _reservacionRepository.VerificarDisponibilidadMesaAsync(
                mesaId,
                reservacion.FechaReservacion,
                reservacion.FechaReservacion.TimeOfDay,
                (int)reservacion.DuracionEstimada.TotalMinutes,
                cancellationToken);
            
            if (!disponible)
            {
                throw new InvalidOperationException("La mesa seleccionada no está disponible en el horario de la reservación");
            }
            
            // Asignar mesa
            reservacion.CambiarMesa(mesaId);
            
            // Persistir cambios
            await _reservacionRepository.ActualizarAsync(reservacion);
            await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> ActualizarEstadoReservacionAsync(
            Guid reservacionId, 
            EstadoReservacion nuevoEstado, 
            CancellationToken cancellationToken = default)
        {
            // Obtener la reservación
            var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
            if (reservacion == null)
            {
                return false;
            }
            
            // Actualizar estado según el tipo
            switch (nuevoEstado)
            {
                case EstadoReservacion.Confirmada:
                    reservacion.Confirmar();
                    break;
                case EstadoReservacion.Completada:
                    reservacion.Completar();
                    break;
                case EstadoReservacion.Cancelada:
                    reservacion.Cancelar("Cancelada desde servicio de operaciones");
                    break;
                case EstadoReservacion.NoShow:
                    reservacion.MarcarNoAsistio();
                    break;
                default:
                    throw new InvalidOperationException($"No se puede actualizar al estado {nuevoEstado}");
            }
            
            // Persistir cambios
            await _reservacionRepository.ActualizarAsync(reservacion);
            await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
            
            return true;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Guid>> VerificarDisponibilidadMesasAsync(
            DateTime fecha, 
            int cantidadPersonas, 
            CancellationToken cancellationToken = default)
        {
            // Obtener mesas disponibles
            var mesasDisponibles = await _reservacionRepository.ObtenerMesasDisponiblesAsync(
                fecha.Date,
                fecha.TimeOfDay,
                cantidadPersonas,
                90, // Duración predeterminada en minutos
                cancellationToken);
            
            return mesasDisponibles;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Reservacion>> ObtenerReservacionesPorRangoFechasAsync(
            DateTime fechaInicio, 
            DateTime fechaFin, 
            CancellationToken cancellationToken = default)
        {
            return await _reservacionRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Comanda?> ConvertirReservacionAComandaAsync(
            Guid reservacionId, 
            Guid meseroId, 
            CancellationToken cancellationToken = default)
        {
            // Obtener la reservación
            var reservacion = await _reservacionRepository.ObtenerPorIdAsync(reservacionId, cancellationToken);
            if (reservacion == null)
            {
                return null;
            }
            
            // Verificar que la reservación esté confirmada
            if (reservacion.Estado != EstadoReservacion.Confirmada)
            {
                throw new InvalidOperationException("Solo se pueden convertir a comanda las reservaciones confirmadas");
            }
            
            // Crear la comanda
            var comanda = Comanda.Crear(
                meseroId,
                reservacion.ClienteId,
                reservacion.MesaId,
                $"Comanda generada desde reservación #{reservacionId}");
            
            // Marcar la reservación como completada
            reservacion.Completar();
            
            // Persistir cambios
            await _comandaRepository.AgregarAsync(comanda);
            await _reservacionRepository.ActualizarAsync(reservacion);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);
            await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
            
            return comanda;
        }

        #endregion
    }
} 