using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using RestaurantePro.Domain.Comercial.Promociones.Events;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantePro.Domain.Comercial.Promociones.Entities
{
    /// <summary>
    /// Entidad que representa una promoción en el sistema
    /// </summary>
    public class Promocion : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Código único de la promoción (para usar en el punto de venta)
        /// </summary>
        public string Codigo { get; private set; }

        /// <summary>
        /// Nombre descriptivo de la promoción
        /// </summary>
        public string Nombre { get; private set; }

        /// <summary>
        /// Descripción detallada de la promoción
        /// </summary>
        public string Descripcion { get; private set; }

        /// <summary>
        /// Tipo de promoción
        /// </summary>
        public TipoPromocion Tipo { get; private set; }

        /// <summary>
        /// Valor del descuento (monto fijo o porcentaje según el tipo)
        /// </summary>
        public decimal ValorDescuento { get; private set; }

        /// <summary>
        /// Monto mínimo de compra para aplicar la promoción
        /// </summary>
        public decimal MontoMinimo { get; private set; }

        /// <summary>
        /// Puntos de fidelización requeridos para canjear la promoción
        /// </summary>
        public int PuntosRequeridos { get; private set; }

        /// <summary>
        /// Fecha de inicio de la promoción
        /// </summary>
        public DateTime FechaInicio { get; private set; }

        /// <summary>
        /// Fecha de fin de la promoción
        /// </summary>
        public DateTime FechaFin { get; private set; }

        /// <summary>
        /// Número máximo de veces que se puede usar la promoción (null = ilimitado)
        /// </summary>
        public int? MaximoUsos { get; private set; }

        /// <summary>
        /// Número de veces que se ha usado la promoción
        /// </summary>
        public int VecesUsada { get; private set; }

        /// <summary>
        /// Estado actual de la promoción
        /// </summary>
        public EstadoPromocion Estado { get; private set; }

        /// <summary>
        /// Indica si la promoción es acumulable con otras
        /// </summary>
        public bool EsAcumulable { get; private set; }

        /// <summary>
        /// Días de la semana en que es válida la promoción (si es null, es válida todos los días)
        /// Se almacena como un valor de bits donde cada bit representa un día de la semana (domingo = 1, lunes = 2, etc.)
        /// </summary>
        public int? DiasValidos { get; private set; }

        /// <summary>
        /// IDs de los productos a los que aplica la promoción
        /// </summary>
        private readonly List<Guid> _productosAplicablesIds = new();

        /// <summary>
        /// IDs de las categorías a las que aplica la promoción
        /// </summary>
        private readonly List<Guid> _categoriasAplicablesIds = new();

        /// <summary>
        /// IDs de los clientes que han usado la promoción
        /// </summary>
        private readonly List<Guid> _clientesQueUsaronIds = new();

        /// <summary>
        /// Lista de IDs de productos a los que aplica la promoción
        /// </summary>
        public IReadOnlyCollection<Guid> ProductosAplicablesIds => _productosAplicablesIds.AsReadOnly();

        /// <summary>
        /// Lista de IDs de categorías a las que aplica la promoción
        /// </summary>
        public IReadOnlyCollection<Guid> CategoriasAplicablesIds => _categoriasAplicablesIds.AsReadOnly();

        /// <summary>
        /// Lista de IDs de clientes que han usado la promoción
        /// </summary>
        public IReadOnlyCollection<Guid> ClientesQueUsaronIds => _clientesQueUsaronIds.AsReadOnly();

        /// <summary>
        /// Constructor protegido para EF Core
        /// </summary>
        protected Promocion() { }

        /// <summary>
        /// Constructor privado para crear una promoción
        /// </summary>
        private Promocion(
            string codigo, 
            string nombre, 
            string descripcion, 
            TipoPromocion tipo, 
            decimal valorDescuento, 
            DateTime fechaInicio, 
            DateTime fechaFin, 
            decimal montoMinimo = 0, 
            int puntosRequeridos = 0, 
            int? maximoUsos = null, 
            bool esAcumulable = false)
        {
            Codigo = codigo;
            Nombre = nombre;
            Descripcion = descripcion;
            Tipo = tipo;
            ValorDescuento = valorDescuento;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
            MontoMinimo = montoMinimo;
            PuntosRequeridos = puntosRequeridos;
            MaximoUsos = maximoUsos;
            VecesUsada = 0;
            Estado = EstadoPromocion.Creada;
            EsAcumulable = esAcumulable;
            
            AddDomainEvent(new PromocionCreada(Id, Codigo, Nombre, Tipo, ValorDescuento, FechaInicio, FechaFin));
            
            ValidarInvariantes();
        }

        /// <summary>
        /// Factory method para crear una nueva promoción
        /// </summary>
        public static Promocion Crear(
            string codigo, 
            string nombre,
            string descripcion,
            TipoPromocion tipo, 
            decimal valorDescuento, 
            DateTime fechaInicio, 
            DateTime fechaFin, 
            decimal montoMinimo = 0, 
            int puntosRequeridos = 0, 
            int? maximoUsos = null, 
            bool esAcumulable = false)
        {
            ValidarArgumentos(codigo, nombre, tipo, valorDescuento, fechaInicio, fechaFin, montoMinimo, puntosRequeridos);
            
            return new Promocion(
                codigo, 
                nombre, 
                descripcion, 
                tipo, 
                valorDescuento, 
                fechaInicio, 
                fechaFin, 
                montoMinimo, 
                puntosRequeridos, 
                maximoUsos, 
                esAcumulable);
        }
        
        /// <summary>
        /// Valida los argumentos para crear una promoción
        /// </summary>
        private static void ValidarArgumentos(
            string codigo, 
            string nombre, 
            TipoPromocion tipo, 
            decimal valorDescuento, 
            DateTime fechaInicio, 
            DateTime fechaFin,
            decimal montoMinimo,
            int puntosRequeridos)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("El código de la promoción es obligatorio", nameof(codigo));
                
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la promoción es obligatorio", nameof(nombre));
                
            if (fechaInicio >= fechaFin)
                throw new ArgumentException("La fecha de inicio debe ser anterior a la fecha de fin", nameof(fechaInicio));
                
            if (fechaFin < DateTime.Now)
                throw new ArgumentException("La fecha de fin no puede ser en el pasado", nameof(fechaFin));
                
            if (valorDescuento <= 0)
                throw new ArgumentException("El valor del descuento debe ser mayor que cero", nameof(valorDescuento));
                
            // Para promociones de porcentaje, validar que sea entre 0 y 100
            if ((tipo == TipoPromocion.PorcentajeTotal || tipo == TipoPromocion.PorcentajeProducto) && valorDescuento > 100)
                throw new ArgumentException("El porcentaje de descuento no puede ser mayor a 100%", nameof(valorDescuento));
                
            if (montoMinimo < 0)
                throw new ArgumentException("El monto mínimo no puede ser negativo", nameof(montoMinimo));
                
            if (puntosRequeridos < 0)
                throw new ArgumentException("Los puntos requeridos no pueden ser negativos", nameof(puntosRequeridos));
                
            // Validar coherencia entre tipo y configuración
            if (tipo == TipoPromocion.CanjePuntos && puntosRequeridos <= 0)
                throw new ArgumentException("Para promociones de canje de puntos, debe especificar los puntos requeridos", nameof(puntosRequeridos));
        }

        /// <summary>
        /// Activa la promoción
        /// </summary>
        public void Activar()
        {
            if (Estado == EstadoPromocion.Activa)
                return;
                
            if (Estado == EstadoPromocion.Finalizada || Estado == EstadoPromocion.Cancelada)
                throw new InvalidOperationException($"No se puede activar una promoción en estado {Estado}");
                
            var estadoAnterior = Estado;
            Estado = EstadoPromocion.Activa;
            
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new PromocionEstadoActualizado(Id, Codigo, Nombre, estadoAnterior, Estado));
        }

        /// <summary>
        /// Pausa la promoción
        /// </summary>
        public void Pausar()
        {
            if (Estado == EstadoPromocion.Pausada)
                return;
                
            if (Estado != EstadoPromocion.Activa)
                throw new InvalidOperationException("Solo se pueden pausar promociones activas");
                
            var estadoAnterior = Estado;
            Estado = EstadoPromocion.Pausada;
            
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new PromocionEstadoActualizado(Id, Codigo, Nombre, estadoAnterior, Estado));
        }

        /// <summary>
        /// Finaliza la promoción
        /// </summary>
        public void Finalizar()
        {
            if (Estado == EstadoPromocion.Finalizada)
                return;
                
            if (Estado == EstadoPromocion.Cancelada)
                throw new InvalidOperationException("No se puede finalizar una promoción cancelada");
                
            var estadoAnterior = Estado;
            Estado = EstadoPromocion.Finalizada;
            
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new PromocionEstadoActualizado(Id, Codigo, Nombre, estadoAnterior, Estado));
        }

        /// <summary>
        /// Cancela la promoción
        /// </summary>
        /// <param name="motivo">Motivo de la cancelación</param>
        public void Cancelar(string motivo)
        {
            if (Estado == EstadoPromocion.Cancelada)
                return;
                
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo de cancelación es obligatorio", nameof(motivo));
                
            var estadoAnterior = Estado;
            Estado = EstadoPromocion.Cancelada;
            
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new PromocionCancelada(Id, Codigo, Nombre, motivo));
        }
        
        /// <summary>
        /// Registra el uso de la promoción por un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="montoAplicado">Monto del descuento aplicado</param>
        public void RegistrarUso(Guid clienteId, Guid comandaId, decimal montoAplicado)
        {
            if (Estado != EstadoPromocion.Activa)
                throw new InvalidOperationException($"No se puede usar una promoción en estado {Estado}");
                
            // Verificar límite de usos
            if (MaximoUsos.HasValue && VecesUsada >= MaximoUsos.Value)
                throw new InvalidOperationException("Se ha alcanzado el máximo de usos para esta promoción");
                
            // Verificar fecha de validez
            if (DateTime.Now < FechaInicio || DateTime.Now > FechaFin)
                throw new InvalidOperationException("La promoción no está vigente en la fecha actual");
                
            // Registrar uso
            VecesUsada++;
            _clientesQueUsaronIds.Add(clienteId);
            
            MarkAsModified();
            ValidarInvariantes();
            
            // Verificar si alcanzó el máximo de usos
            if (MaximoUsos.HasValue && VecesUsada >= MaximoUsos.Value)
            {
                var estadoAnterior = Estado;
                Estado = EstadoPromocion.Finalizada;
                AddDomainEvent(new PromocionEstadoActualizado(Id, Codigo, Nombre, estadoAnterior, Estado));
            }
            
            AddDomainEvent(new PromocionUsada(Id, Codigo, Nombre, clienteId, comandaId, montoAplicado));
        }
    
    /// <summary>
        /// Agrega un producto a la lista de productos aplicables
    /// </summary>
        /// <param name="productoId">ID del producto</param>
        public void AgregarProductoAplicable(Guid productoId)
        {
            if (_productosAplicablesIds.Contains(productoId))
                return;
                
            _productosAplicablesIds.Add(productoId);
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new PromocionProductoAgregado(Id, Codigo, Nombre, productoId));
        }
        
        /// <summary>
        /// Elimina un producto de la lista de productos aplicables
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        public void EliminarProductoAplicable(Guid productoId)
        {
            if (!_productosAplicablesIds.Contains(productoId))
                return;
                
            _productosAplicablesIds.Remove(productoId);
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new PromocionProductoEliminado(Id, Codigo, Nombre, productoId));
    }
    
    /// <summary>
        /// Agrega una categoría a la lista de categorías aplicables
    /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        public void AgregarCategoriaAplicable(Guid categoriaId)
        {
            if (_categoriasAplicablesIds.Contains(categoriaId))
                return;
                
            _categoriasAplicablesIds.Add(categoriaId);
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new PromocionCategoriaAgregada(Id, Codigo, Nombre, categoriaId));
        }
        
        /// <summary>
        /// Elimina una categoría de la lista de categorías aplicables
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        public void EliminarCategoriaAplicable(Guid categoriaId)
        {
            if (!_categoriasAplicablesIds.Contains(categoriaId))
                return;
                
            _categoriasAplicablesIds.Remove(categoriaId);
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new PromocionCategoriaEliminada(Id, Codigo, Nombre, categoriaId));
    }
    
    /// <summary>
        /// Establece los días de la semana en que la promoción es válida
    /// </summary>
        /// <param name="dias">Array con los días de la semana válidos</param>
        public void EstablecerDiasValidos(params DayOfWeek[] dias)
        {
            if (dias == null || !dias.Any())
            {
                DiasValidos = null; // Válida todos los días
            }
            else
            {
                int valorBits = 0;
                foreach (var dia in dias)
                {
                    valorBits |= (1 << (int)dia);
                }
                DiasValidos = valorBits;
            }
            
            MarkAsModified();
            ValidarInvariantes();
        }
        
        /// <summary>
        /// Verifica si la promoción es válida para un día específico de la semana
        /// </summary>
        /// <param name="dia">Día de la semana a verificar</param>
        /// <returns>True si la promoción es válida para ese día</returns>
        public bool EsValidaParaDia(DayOfWeek dia)
        {
            // Si DiasValidos es null, la promoción es válida todos los días
            if (!DiasValidos.HasValue)
                return true;
                
            // Verificar si el bit correspondiente al día está activo
            return (DiasValidos.Value & (1 << (int)dia)) != 0;
    }
    
    /// <summary>
        /// Actualiza la información de la promoción
    /// </summary>
        public void ActualizarInformacion(
            string nombre, 
            string descripcion, 
            decimal valorDescuento, 
            DateTime fechaInicio, 
            DateTime fechaFin, 
            decimal montoMinimo = 0, 
            int puntosRequeridos = 0, 
            int? maximoUsos = null, 
            bool esAcumulable = false)
        {
            if (Estado == EstadoPromocion.Finalizada || Estado == EstadoPromocion.Cancelada)
                throw new InvalidOperationException($"No se puede actualizar una promoción en estado {Estado}");
                
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la promoción es obligatorio", nameof(nombre));
                
            if (fechaInicio >= fechaFin)
                throw new ArgumentException("La fecha de inicio debe ser anterior a la fecha de fin", nameof(fechaInicio));
                
            if (valorDescuento <= 0)
                throw new ArgumentException("El valor del descuento debe ser mayor que cero", nameof(valorDescuento));
                
            // Para promociones de porcentaje, validar que sea entre 0 y 100
            if ((Tipo == TipoPromocion.PorcentajeTotal || Tipo == TipoPromocion.PorcentajeProducto) && valorDescuento > 100)
                throw new ArgumentException("El porcentaje de descuento no puede ser mayor a 100%", nameof(valorDescuento));
                
            // Verificar que el número de usos máximo no sea menor a los usos actuales
            if (maximoUsos.HasValue && maximoUsos.Value < VecesUsada)
                throw new ArgumentException($"El máximo de usos no puede ser menor a los usos actuales ({VecesUsada})", nameof(maximoUsos));
                
            Nombre = nombre;
            Descripcion = descripcion;
            ValorDescuento = valorDescuento;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
            MontoMinimo = montoMinimo;
            PuntosRequeridos = puntosRequeridos;
            MaximoUsos = maximoUsos;
            EsAcumulable = esAcumulable;
            
            MarkAsModified();
            ValidarInvariantes();
            
            AddDomainEvent(new PromocionActualizada(Id, Codigo, Nombre, ValorDescuento, FechaInicio, FechaFin));
        }
        
        /// <summary>
        /// Verifica si la promoción es aplicable a un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="categoriaId">ID de la categoría del producto</param>
        /// <returns>True si la promoción aplica al producto</returns>
        public bool EsAplicableAProducto(Guid productoId, Guid? categoriaId = null)
        {
            // Si no hay productos ni categorías definidas, la promoción aplica a todos los productos
            if (_productosAplicablesIds.Count == 0 && _categoriasAplicablesIds.Count == 0)
                return true;
                
            // Si el producto está en la lista de productos aplicables
            if (_productosAplicablesIds.Contains(productoId))
                return true;
                
            // Si la categoría está en la lista de categorías aplicables
            if (categoriaId.HasValue && _categoriasAplicablesIds.Contains(categoriaId.Value))
                return true;
                
            return false;
    }
    
    /// <summary>
        /// Verifica si la promoción está vigente en la fecha actual
    /// </summary>
        /// <returns>True si la promoción está vigente</returns>
        public bool EstaVigente()
        {
            if (Estado != EstadoPromocion.Activa)
                return false;
                
            var now = DateTime.Now;
            return now >= FechaInicio && now <= FechaFin && (!MaximoUsos.HasValue || VecesUsada < MaximoUsos.Value);
        }
        
        /// <summary>
        /// Verifica si la promoción es válida para un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="puntosDisponibles">Puntos disponibles del cliente (para promociones de canje)</param>
        /// <param name="montoTotal">Monto total de la compra</param>
        /// <returns>True si la promoción es válida para el cliente</returns>
        public bool EsValidaParaCliente(Guid clienteId, int puntosDisponibles, decimal montoTotal)
        {
            // Verificar si la promoción está vigente
            if (!EstaVigente())
                return false;
                
            // Verificar monto mínimo
            if (montoTotal < MontoMinimo)
                return false;
                
            // Verificar puntos requeridos (para promociones de canje)
            if (Tipo == TipoPromocion.CanjePuntos && puntosDisponibles < PuntosRequeridos)
                return false;
                
            return true;
    }
    
    /// <summary>
        /// Calcula el descuento aplicable a un monto
    /// </summary>
        /// <param name="montoOriginal">Monto original sin descuento</param>
        /// <returns>Monto del descuento a aplicar</returns>
        public decimal CalcularDescuento(decimal montoOriginal)
        {
            if (!EstaVigente())
                return 0;
                
            if (montoOriginal < MontoMinimo)
                return 0;
                
            switch (Tipo)
            {
                case TipoPromocion.PorcentajeTotal:
                    return Math.Round(montoOriginal * (ValorDescuento / 100), 2);
                    
                case TipoPromocion.MontoFijoTotal:
                    return Math.Min(ValorDescuento, montoOriginal); // No puede ser mayor al monto original
                    
                default:
                    // Para otros tipos, el cálculo se hace en el servicio de aplicación
                    return 0;
            }
        }
        
        /// <summary>
        /// Valida todas las invariantes del agregado
        /// </summary>
        private void ValidarInvariantes()
        {
            if (string.IsNullOrWhiteSpace(Codigo))
                throw new InvalidOperationException("El código de la promoción no puede estar vacío");
                
            if (string.IsNullOrWhiteSpace(Nombre))
                throw new InvalidOperationException("El nombre de la promoción no puede estar vacío");
                
            if (FechaInicio >= FechaFin)
                throw new InvalidOperationException("La fecha de inicio debe ser anterior a la fecha de fin");
                
            if (ValorDescuento <= 0)
                throw new InvalidOperationException("El valor del descuento debe ser mayor que cero");
                
            // Para promociones de porcentaje, validar que sea entre 0 y 100
            if ((Tipo == TipoPromocion.PorcentajeTotal || Tipo == TipoPromocion.PorcentajeProducto) && ValorDescuento > 100)
                throw new InvalidOperationException($"El porcentaje de descuento no puede ser mayor a 100%. Valor actual: {ValorDescuento}%");
                
            if (MontoMinimo < 0)
                throw new InvalidOperationException("El monto mínimo no puede ser negativo");
                
            if (PuntosRequeridos < 0)
                throw new InvalidOperationException("Los puntos requeridos no pueden ser negativos");
                
            if (VecesUsada < 0)
                throw new InvalidOperationException("El número de veces usada no puede ser negativo");
                
            // Verificar coherencia entre tipo y configuración
            if (Tipo == TipoPromocion.CanjePuntos && PuntosRequeridos <= 0)
                throw new InvalidOperationException("Para promociones de canje de puntos, debe especificar los puntos requeridos");
                
            // Verificar que el número de usos máximo no sea menor a los usos actuales
            if (MaximoUsos.HasValue && MaximoUsos.Value < VecesUsada)
                throw new InvalidOperationException($"El máximo de usos no puede ser menor a los usos actuales ({VecesUsada})");
        }
    }
} 
