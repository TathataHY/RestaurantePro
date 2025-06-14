namespace RestaurantePro.Infrastructure.ExternalServices.Email.Models
{
    /// <summary>
    /// Configuración para servicios de correo electrónico
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// Servidor SMTP
        /// </summary>
        public string ServidorSMTP { get; set; }
        
        /// <summary>
        /// Puerto SMTP
        /// </summary>
        public int PuertoSMTP { get; set; }
        
        /// <summary>
        /// Usuario para autenticación SMTP
        /// </summary>
        public string Usuario { get; set; }
        
        /// <summary>
        /// Contraseña para autenticación SMTP
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Dirección de correo del remitente
        /// </summary>
        public string DireccionRemitente { get; set; }
        
        /// <summary>
        /// Nombre del remitente
        /// </summary>
        public string NombreRemitente { get; set; }
        
        /// <summary>
        /// Indica si se debe usar SSL
        /// </summary>
        public bool UsarSSL { get; set; }
        
        /// <summary>
        /// Directorio donde se encuentran las plantillas de correo
        /// </summary>
        public string DirectorioPlantillas { get; set; }
        
        /// <summary>
        /// API Key de SendGrid (si se usa ese servicio)
        /// </summary>
        public string SendGridApiKey { get; set; }
    }
} 