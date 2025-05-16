
namespace RestaurantePro.Domain.Comercial.Clientes.Entities
{
    /// <summary>
    /// Entidad que registra el historial de operaciones con puntos en una tarjeta de fidelización
    /// </summary>
    public class HistorialPuntos : EntityBase
    {
        /// <summary>
        /// Identificador de la tarjeta de fidelización asociada
        /// </summary>
        public Guid TarjetaFidelizacionId { get; private set; }

        /// <summary>
        /// Cantidad de puntos de la operación
        /// </summary>
        public int Puntos { get; private set; }

        /// <summary>
        /// Tipo de operación realizada
        /// </summary>
        public TipoOperacionPuntos TipoOperacion { get; private set; }

        /// <summary>
        /// Fecha y hora de la operación
        /// </summary>
        public DateTime FechaOperacion { get; private set; }

        /// <summary>
        /// Concepto o descripción de la operación
        /// </summary>
        public string Concepto { get; private set; }

        /// <summary>
        /// Monto de la compra que generó los puntos (si aplica)
        /// </summary>
        public decimal? MontoCompra { get; private set; }

        /// <summary>
        /// Factor de conversión usado para calcular puntos (si aplica)
        /// </summary>
        public int? FactorConversion { get; private set; }

        // Constructor privado para EF Core
        private HistorialPuntos() { }

        private HistorialPuntos(Guid tarjetaId, int puntos, TipoOperacionPuntos tipoOperacion, string concepto)
        {
            ValidarDatos(tarjetaId, puntos, concepto);

            TarjetaFidelizacionId = tarjetaId;
            Puntos = puntos;
            TipoOperacion = tipoOperacion;
            FechaOperacion = DateTime.Now;
            Concepto = concepto;
        }

        /// <summary>
        /// Crea un registro de puntos agregados a una tarjeta
        /// </summary>
        public static HistorialPuntos CrearRegistroAgregados(Guid tarjetaId, int puntos, string concepto)
        {
            return new HistorialPuntos(tarjetaId, puntos, TipoOperacionPuntos.Agregados, concepto);
        }

        /// <summary>
        /// Crea un registro de puntos calculados por una compra
        /// </summary>
        public static HistorialPuntos CrearRegistroPorCompra(Guid tarjetaId, decimal monto, int factorConversion, string concepto)
        {
            if (monto <= 0)
                throw new ArgumentException("El monto de la compra debe ser mayor a cero", nameof(monto));

            if (factorConversion <= 0)
                throw new ArgumentException("El factor de conversión debe ser mayor a cero", nameof(factorConversion));

            // Calcula puntos según el monto y factor (ej: $1000 / 10 = 100 puntos)
            int puntos = (int)(monto / factorConversion);

            var historial = new HistorialPuntos(tarjetaId, puntos, TipoOperacionPuntos.Agregados, concepto)
            {
                MontoCompra = monto,
                FactorConversion = factorConversion
            };

            return historial;
        }

        /// <summary>
        /// Crea un registro de puntos canjeados
        /// </summary>
        public static HistorialPuntos CrearRegistroCanjeados(Guid tarjetaId, int puntos, string concepto)
        {
            return new HistorialPuntos(tarjetaId, puntos, TipoOperacionPuntos.Canjeados, concepto);
        }

        /// <summary>
        /// Crea un registro de puntos vencidos
        /// </summary>
        public static HistorialPuntos CrearRegistroVencidos(Guid tarjetaId, int puntos, string concepto)
        {
            return new HistorialPuntos(tarjetaId, puntos, TipoOperacionPuntos.Vencidos, concepto);
        }

        /// <summary>
        /// Crea un registro de ajuste manual de puntos
        /// </summary>
        public static HistorialPuntos CrearRegistroAjuste(Guid tarjetaId, int puntos, string concepto)
        {
            return new HistorialPuntos(tarjetaId, puntos, TipoOperacionPuntos.Ajuste, concepto);
        }

        private static void ValidarDatos(Guid tarjetaId, int puntos, string concepto)
        {
            if (tarjetaId == Guid.Empty)
                throw new ArgumentException("El identificador de la tarjeta no puede estar vacío", nameof(tarjetaId));

            if (puntos <= 0)
                throw new ArgumentException("La cantidad de puntos debe ser mayor a cero", nameof(puntos));

            if (string.IsNullOrWhiteSpace(concepto))
                throw new ArgumentException("El concepto no puede estar vacío", nameof(concepto));
        }
    }
}
