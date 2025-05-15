using System;

namespace RestaurantePro.Mobile.Features.Comercial.Clientes.Models
{
    /// <summary>
    /// Modelo que representa un cliente en la aplicación móvil
    /// </summary>
    public class ClienteModel
    {
        /// <summary>
        /// Identificador único del cliente
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nombre del cliente
        /// </summary>
        public string Nombre { get; set; }
        
        /// <summary>
        /// Apellido del cliente
        /// </summary>
        public string Apellido { get; set; }
        
        /// <summary>
        /// Nombre completo (Nombre + Apellido)
        /// </summary>
        public string NombreCompleto => $"{Nombre} {Apellido}";
        
        /// <summary>
        /// Correo electrónico del cliente
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Teléfono del cliente
        /// </summary>
        public string Telefono { get; set; }
        
        /// <summary>
        /// Fecha de registro del cliente
        /// </summary>
        public DateTime FechaCreacion { get; set; }
        
        /// <summary>
        /// Indica si el cliente está activo
        /// </summary>
        public bool Activo { get; set; }
        
        /// <summary>
        /// Puntos acumulados en el programa de fidelización
        /// </summary>
        public int PuntosAcumulados { get; set; }
        
        /// <summary>
        /// Obtiene las iniciales del cliente para mostrar en avatares
        /// </summary>
        public string Iniciales
        {
            get
            {
                if (string.IsNullOrEmpty(Nombre) && string.IsNullOrEmpty(Apellido))
                    return "??";
                
                var inicialNombre = !string.IsNullOrEmpty(Nombre) ? Nombre[0].ToString() : "";
                var inicialApellido = !string.IsNullOrEmpty(Apellido) ? Apellido[0].ToString() : "";
                
                return $"{inicialNombre}{inicialApellido}".ToUpper();
            }
        }
        
        /// <summary>
        /// Tiempo transcurrido desde la creación del cliente en formato legible
        /// </summary>
        public string TiempoRegistro
        {
            get
            {
                var tiempo = DateTime.Now - FechaCreacion;
                
                if (tiempo.TotalDays > 365)
                    return $"{(int)(tiempo.TotalDays / 365)} año(s)";
                if (tiempo.TotalDays > 30)
                    return $"{(int)(tiempo.TotalDays / 30)} mes(es)";
                if (tiempo.TotalDays > 1)
                    return $"{(int)tiempo.TotalDays} día(s)";
                if (tiempo.TotalHours > 1)
                    return $"{(int)tiempo.TotalHours} hora(s)";
                
                return $"{(int)tiempo.TotalMinutes} minuto(s)";
            }
        }
        
        /// <summary>
        /// Nivel de fidelización basado en puntos acumulados
        /// </summary>
        public string NivelFidelizacion
        {
            get
            {
                if (PuntosAcumulados >= 1000)
                    return "Platinum";
                if (PuntosAcumulados >= 500)
                    return "Gold";
                if (PuntosAcumulados >= 200)
                    return "Silver";
                
                return "Standard";
            }
        }
    }
} 