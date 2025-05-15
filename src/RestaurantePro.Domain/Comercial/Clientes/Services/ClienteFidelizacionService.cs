using System;
using System.Collections.Generic;
using RestaurantePro.Domain.Comercial.Clientes.Aggregates;
using RestaurantePro.Domain.Comercial.Clientes.Specifications;

namespace RestaurantePro.Domain.Comercial.Clientes.Services
{
    /// <summary>
    /// Servicio de dominio para operaciones que involucran múltiples agregados
    /// o que no pertenecen naturalmente a ninguna entidad.
    /// </summary>
    public class ClienteFidelizacionService
    {
        /// <summary>
        /// Calcula nivel de fidelización basado en patrones de compra y puntos
        /// </summary>
        public string CalcularNivelFidelizacion(ClienteAggregate cliente, List<DateTime> fechasCompras, decimal totalCompras)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            if (!cliente.Activo)
                return "Inactivo";

            // Calcular frecuencia de compras
            var comprasPorMes = CalcularFrecuenciaCompras(fechasCompras);
            
            // Aplicar reglas de negocio
            if (cliente.PuntosAcumulados >= 1000 && comprasPorMes >= 4 && totalCompras >= 5000)
                return "Platinum";
            
            if (cliente.PuntosAcumulados >= 500 && comprasPorMes >= 2 && totalCompras >= 2000)
                return "Gold";
            
            if (cliente.PuntosAcumulados >= 200 && comprasPorMes >= 1)
                return "Silver";
            
            return "Standard";
        }

        /// <summary>
        /// Verifica si un cliente califica para promociones específicas basadas en especificaciones
        /// </summary>
        public IEnumerable<string> ObtenerPromocionesDisponibles(ClienteAggregate cliente)
        {
            var promociones = new List<string>();
            
            // Evaluar diferentes especificaciones
            var especClientePreferencial = new ClientePreferencialSpecification(500);
            if (especClientePreferencial.IsSatisfiedBy(cliente))
            {
                promociones.Add("Descuento Preferencial 10%");
            }
            
            var especClienteAntiguo = new ClienteElegiblePromocionSpecification(90);
            if (especClienteAntiguo.IsSatisfiedBy(cliente))
            {
                promociones.Add("Promoción Cliente Fiel");
            }
            
            // Reglas adicionales
            if (cliente.PuntosAcumulados >= 1000)
            {
                promociones.Add("Premio Milestone 1000 puntos");
            }
            
            return promociones;
        }
        
        /// <summary>
        /// Método auxiliar para calcular frecuencia de compras
        /// </summary>
        private double CalcularFrecuenciaCompras(List<DateTime> fechasCompras)
        {
            if (fechasCompras == null || fechasCompras.Count == 0)
                return 0;
            
            // Tomar las compras de los últimos 6 meses
            var fechaLimite = DateTime.Now.AddMonths(-6);
            var comprasRecientes = fechasCompras.FindAll(f => f >= fechaLimite);
            
            return (double)comprasRecientes.Count / 6;
        }
    }
}
