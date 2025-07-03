using System;

namespace RestaurantePro.Infrastructure.DTOs.SignalR
{
    /// <summary>
    /// DTO para alertas de inventario por SignalR
    /// Optimizado para comunicación en tiempo real de alertas de stock
    /// </summary>
    public class AlertaInventarioSignalRDto
    {
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; set; }

        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string NombreIngrediente { get; set; } = string.Empty;

        /// <summary>
        /// Stock actual disponible
        /// </summary>
        public decimal StockActual { get; set; }

        /// <summary>
        /// Stock mínimo requerido
        /// </summary>
        public decimal StockMinimo { get; set; }

        /// <summary>
        /// Unidad de medida del ingrediente
        /// </summary>
        public string UnidadMedida { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de alerta (StockBajo, StockAgotado, PorVencer)
        /// </summary>
        public string TipoAlerta { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de la alerta
        /// </summary>
        public DateTime FechaAlerta { get; set; }

        /// <summary>
        /// Fecha de vencimiento (si aplica)
        /// </summary>
        public DateTime? FechaVencimiento { get; set; }

        /// <summary>
        /// Días restantes hasta el vencimiento (si aplica)
        /// </summary>
        public int? DiasRestantes { get; set; }

        /// <summary>
        /// ID único de la alerta
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nivel de urgencia (Bajo, Medio, Alto, Crítico)
        /// </summary>
        public string NivelUrgencia { get; set; } = "Bajo";

        /// <summary>
        /// Mensaje descriptivo de la alerta
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la alerta requiere acción inmediata
        /// </summary>
        public bool RequiereAccionInmediata { get; set; }

        /// <summary>
        /// ID del proveedor recomendado para reabastecimiento
        /// </summary>
        public Guid? ProveedorRecomendadoId { get; set; }

        /// <summary>
        /// Nombre del proveedor recomendado
        /// </summary>
        public string? NombreProveedor { get; set; }

        /// <summary>
        /// Constructor por defecto requerido para serialización
        /// </summary>
        public AlertaInventarioSignalRDto()
        {
            Id = Guid.NewGuid();
            FechaAlerta = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor para alerta de stock bajo
        /// </summary>
        public AlertaInventarioSignalRDto(
            Guid ingredienteId,
            string nombreIngrediente,
            decimal stockActual,
            decimal stockMinimo,
            string unidadMedida)
        {
            Id = Guid.NewGuid();
            IngredienteId = ingredienteId;
            NombreIngrediente = nombreIngrediente;
            StockActual = stockActual;
            StockMinimo = stockMinimo;
            UnidadMedida = unidadMedida;
            TipoAlerta = TiposAlerta.StockBajo;
            FechaAlerta = DateTime.UtcNow;
            NivelUrgencia = CalcularNivelUrgencia(stockActual, stockMinimo);
            Mensaje = GenerarMensajeStockBajo(stockActual, stockMinimo, unidadMedida);
        }

        /// <summary>
        /// Constructor para alerta de stock agotado
        /// </summary>
        public AlertaInventarioSignalRDto(
            Guid ingredienteId,
            string nombreIngrediente,
            string unidadMedida)
        {
            Id = Guid.NewGuid();
            IngredienteId = ingredienteId;
            NombreIngrediente = nombreIngrediente;
            StockActual = 0;
            StockMinimo = 0;
            UnidadMedida = unidadMedida;
            TipoAlerta = TiposAlerta.StockAgotado;
            FechaAlerta = DateTime.UtcNow;
            NivelUrgencia = "Crítico";
            RequiereAccionInmediata = true;
            Mensaje = $"Stock agotado: {nombreIngrediente}";
        }

        /// <summary>
        /// Constructor para alerta de vencimiento
        /// </summary>
        public AlertaInventarioSignalRDto(
            Guid ingredienteId,
            string nombreIngrediente,
            DateTime fechaVencimiento,
            int diasRestantes)
        {
            Id = Guid.NewGuid();
            IngredienteId = ingredienteId;
            NombreIngrediente = nombreIngrediente;
            TipoAlerta = TiposAlerta.PorVencer;
            FechaAlerta = DateTime.UtcNow;
            FechaVencimiento = fechaVencimiento;
            DiasRestantes = diasRestantes;
            NivelUrgencia = CalcularNivelUrgenciaVencimiento(diasRestantes);
            RequiereAccionInmediata = diasRestantes <= 3;
            Mensaje = GenerarMensajeVencimiento(nombreIngrediente, diasRestantes);
        }

        /// <summary>
        /// Constructor completo con todos los parámetros
        /// </summary>
        public AlertaInventarioSignalRDto(
            Guid ingredienteId,
            string nombreIngrediente,
            decimal stockActual,
            decimal stockMinimo,
            string unidadMedida,
            string tipoAlerta,
            DateTime? fechaVencimiento = null,
            int? diasRestantes = null,
            Guid? proveedorRecomendadoId = null,
            string? nombreProveedor = null)
        {
            Id = Guid.NewGuid();
            IngredienteId = ingredienteId;
            NombreIngrediente = nombreIngrediente;
            StockActual = stockActual;
            StockMinimo = stockMinimo;
            UnidadMedida = unidadMedida;
            TipoAlerta = tipoAlerta;
            FechaAlerta = DateTime.UtcNow;
            FechaVencimiento = fechaVencimiento;
            DiasRestantes = diasRestantes;
            ProveedorRecomendadoId = proveedorRecomendadoId;
            NombreProveedor = nombreProveedor;

            // Calcular nivel de urgencia y mensaje según el tipo
            switch (tipoAlerta)
            {
                case TiposAlerta.StockBajo:
                    NivelUrgencia = CalcularNivelUrgencia(stockActual, stockMinimo);
                    Mensaje = GenerarMensajeStockBajo(stockActual, stockMinimo, unidadMedida);
                    break;
                case TiposAlerta.StockAgotado:
                    NivelUrgencia = "Crítico";
                    RequiereAccionInmediata = true;
                    Mensaje = $"Stock agotado: {nombreIngrediente}";
                    break;
                case TiposAlerta.PorVencer:
                    NivelUrgencia = CalcularNivelUrgenciaVencimiento(diasRestantes ?? 0);
                    RequiereAccionInmediata = (diasRestantes ?? 0) <= 3;
                    Mensaje = GenerarMensajeVencimiento(nombreIngrediente, diasRestantes ?? 0);
                    break;
            }
        }

        /// <summary>
        /// Calcula el nivel de urgencia basado en el stock actual vs mínimo
        /// </summary>
        private static string CalcularNivelUrgencia(decimal stockActual, decimal stockMinimo)
        {
            if (stockActual <= 0) return "Crítico";
            if (stockActual <= stockMinimo * 0.25m) return "Alto";
            if (stockActual <= stockMinimo * 0.5m) return "Medio";
            return "Bajo";
        }

        /// <summary>
        /// Calcula el nivel de urgencia basado en días restantes para vencimiento
        /// </summary>
        private static string CalcularNivelUrgenciaVencimiento(int diasRestantes)
        {
            if (diasRestantes <= 1) return "Crítico";
            if (diasRestantes <= 3) return "Alto";
            if (diasRestantes <= 7) return "Medio";
            return "Bajo";
        }

        /// <summary>
        /// Genera mensaje para alerta de stock bajo
        /// </summary>
        private static string GenerarMensajeStockBajo(decimal stockActual, decimal stockMinimo, string unidadMedida)
        {
            return $"Stock bajo: {stockActual} {unidadMedida} (mínimo: {stockMinimo} {unidadMedida})";
        }

        /// <summary>
        /// Genera mensaje para alerta de vencimiento
        /// </summary>
        private static string GenerarMensajeVencimiento(string nombreIngrediente, int diasRestantes)
        {
            return $"Vencimiento próximo: {nombreIngrediente} - {diasRestantes} días restantes";
        }
    }

    /// <summary>
    /// Tipos de alerta disponibles
    /// </summary>
    public static class TiposAlerta
    {
        public const string StockBajo = "StockBajo";
        public const string StockAgotado = "StockAgotado";
        public const string PorVencer = "PorVencer";
        public const string StockExcesivo = "StockExcesivo";
        public const string MovimientoInusual = "MovimientoInusual";
    }

    /// <summary>
    /// Niveles de urgencia disponibles
    /// </summary>
    public static class NivelesUrgencia
    {
        public const string Bajo = "Bajo";
        public const string Medio = "Medio";
        public const string Alto = "Alto";
        public const string Critico = "Crítico";
    }
} 