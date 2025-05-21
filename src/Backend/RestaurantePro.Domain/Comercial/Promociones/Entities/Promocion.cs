using RestaurantePro.Domain.Core.Base;

namespace RestaurantePro.Domain.Comercial.Promociones.Entities
{
    /// <summary>
    /// Entidad que representa una promoción o descuento aplicable a productos
    /// </summary>
    public class Promocion : EntityBase
    {
        #region Propiedades

        /// <summary>
        /// Nombre descriptivo de la promoción
        /// </summary>
        public string Nombre { get; private set; }

        /// <summary>
        /// Descripción detallada de la promoción
        /// </summary>
        public string Descripcion { get; private set; }

        /// <summary>
        /// Código de la promoción que puede ser aplicado por el usuario
        /// </summary>
        public string? CodigoPromocion { get; private set; }

        /// <summary>
        /// Indica si la promoción está activa actualmente
        /// </summary>
        public bool Activa { get; private set; }

        /// <summary>
        /// Fecha de inicio de la promoción
        /// </summary>
        public DateTime? FechaInicio { get; private set; }

        /// <summary>
        /// Fecha de fin de la promoción
        /// </summary>
        public DateTime? FechaFin { get; private set; }

        /// <summary>
        /// Cantidad de usos disponibles de la promoción (null = ilimitado)
        /// </summary>
        public int? CantidadDisponible { get; private set; }

        /// <summary>
        /// Valor que representa los días válidos para la promoción (bitmap)
        /// Cada bit representa un día: bit 0 = Domingo, bit 1 = Lunes, etc.
        /// </summary>
        public int? DiasValidos { get; private set; }

        /// <summary>
        /// Porcentaje de descuento a aplicar
        /// </summary>
        public decimal PorcentajeDescuento { get; private set; }

        /// <summary>
        /// Valor de descuento fijo a aplicar
        /// </summary>
        public decimal? ValorDescuentoFijo { get; private set; }

        /// <summary>
        /// Valor mínimo de compra para que la promoción sea aplicable
        /// </summary>
        public decimal? ValorMinimoCompra { get; private set; }

        #endregion

        #region Constructores

        // Constructor privado para EF
        private Promocion() { }

        /// <summary>
        /// Crea una nueva promoción
        /// </summary>
        public static Promocion Crear(
            string nombre,
            string descripcion,
            decimal porcentajeDescuento,
            decimal? valorDescuentoFijo = null,
            string? codigoPromocion = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? cantidadDisponible = null,
            int? diasValidos = null,
            decimal? valorMinimoCompra = null)
        {
            var promocion = new Promocion
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = descripcion,
                CodigoPromocion = codigoPromocion,
                Activa = true,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                CantidadDisponible = cantidadDisponible,
                DiasValidos = diasValidos,
                PorcentajeDescuento = porcentajeDescuento,
                ValorDescuentoFijo = valorDescuentoFijo,
                ValorMinimoCompra = valorMinimoCompra
            };

            promocion.AddDomainEvent(new PromocionCreada(promocion.Id, promocion.Nombre));
            return promocion;
        }

        #endregion

        #region Métodos públicos

        /// <summary>
        /// Activa la promoción
        /// </summary>
        public void Activar()
        {
            if (Activa) return;
            
            Activa = true;
            AddDomainEvent(new PromocionActivada(Id, Nombre));
        }

        /// <summary>
        /// Desactiva la promoción
        /// </summary>
        public void Desactivar()
        {
            if (!Activa) return;
            
            Activa = false;
            AddDomainEvent(new PromocionDesactivada(Id, Nombre));
        }

        /// <summary>
        /// Actualiza los datos de la promoción
        /// </summary>
        public void Actualizar(
            string nombre,
            string descripcion, 
            decimal porcentajeDescuento,
            decimal? valorDescuentoFijo = null,
            string? codigoPromocion = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? cantidadDisponible = null,
            int? diasValidos = null,
            decimal? valorMinimoCompra = null)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            CodigoPromocion = codigoPromocion;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
            CantidadDisponible = cantidadDisponible;
            DiasValidos = diasValidos;
            PorcentajeDescuento = porcentajeDescuento;
            ValorDescuentoFijo = valorDescuentoFijo;
            ValorMinimoCompra = valorMinimoCompra;

            AddDomainEvent(new PromocionActualizada(Id, Nombre));
        }

        /// <summary>
        /// Registra un uso de la promoción, disminuyendo la cantidad disponible
        /// </summary>
        public void RegistrarUso()
        {
            if (CantidadDisponible.HasValue)
            {
                if (CantidadDisponible <= 0)
                {
                    throw new InvalidOperationException("La promoción no tiene usos disponibles");
                }
                
                CantidadDisponible--;
                
                if (CantidadDisponible == 0)
                {
                    Activa = false;
                    AddDomainEvent(new PromocionAgotada(Id, Nombre));
                }
            }
            
            AddDomainEvent(new PromocionUsada(Id, Nombre));
        }

        #endregion
    }
    
    #region Eventos
    
    /// <summary>
    /// Evento que se genera cuando se crea una promoción
    /// </summary>
    public class PromocionCreada : DomainEvent
    {
        public Guid PromocionId { get; }
        public string NombrePromocion { get; }
        
        public PromocionCreada(Guid promocionId, string nombrePromocion)
        {
            PromocionId = promocionId;
            NombrePromocion = nombrePromocion;
        }
    }
    
    /// <summary>
    /// Evento que se genera cuando se actualiza una promoción
    /// </summary>
    public class PromocionActualizada : DomainEvent
    {
        public Guid PromocionId { get; }
        public string NombrePromocion { get; }
        
        public PromocionActualizada(Guid promocionId, string nombrePromocion)
        {
            PromocionId = promocionId;
            NombrePromocion = nombrePromocion;
        }
    }
    
    /// <summary>
    /// Evento que se genera cuando se activa una promoción
    /// </summary>
    public class PromocionActivada : DomainEvent
    {
        public Guid PromocionId { get; }
        public string NombrePromocion { get; }
        
        public PromocionActivada(Guid promocionId, string nombrePromocion)
        {
            PromocionId = promocionId;
            NombrePromocion = nombrePromocion;
        }
    }
    
    /// <summary>
    /// Evento que se genera cuando se desactiva una promoción
    /// </summary>
    public class PromocionDesactivada : DomainEvent
    {
        public Guid PromocionId { get; }
        public string NombrePromocion { get; }
        
        public PromocionDesactivada(Guid promocionId, string nombrePromocion)
        {
            PromocionId = promocionId;
            NombrePromocion = nombrePromocion;
        }
    }
    
    /// <summary>
    /// Evento que se genera cuando se usa una promoción
    /// </summary>
    public class PromocionUsada : DomainEvent
    {
        public Guid PromocionId { get; }
        public string NombrePromocion { get; }
        
        public PromocionUsada(Guid promocionId, string nombrePromocion)
        {
            PromocionId = promocionId;
            NombrePromocion = nombrePromocion;
        }
    }
    
    /// <summary>
    /// Evento que se genera cuando se agotan los usos de una promoción
    /// </summary>
    public class PromocionAgotada : DomainEvent
    {
        public Guid PromocionId { get; }
        public string NombrePromocion { get; }
        
        public PromocionAgotada(Guid promocionId, string nombrePromocion)
        {
            PromocionId = promocionId;
            NombrePromocion = nombrePromocion;
        }
    }
    
    #endregion
} 