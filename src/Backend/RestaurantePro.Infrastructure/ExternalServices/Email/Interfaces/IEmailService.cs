using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.ExternalServices.Email.Interfaces
{
    /// <summary>
    /// Interfaz para envío de correos electrónicos
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un correo electrónico simple
        /// </summary>
        /// <param name="destinatario">Dirección de correo del destinatario</param>
        /// <param name="asunto">Asunto del correo</param>
        /// <param name="contenido">Contenido del correo (puede ser HTML)</param>
        /// <returns>Resultado del envío</returns>
        Task<bool> EnviarCorreoAsync(string destinatario, string asunto, string contenido);
        
        /// <summary>
        /// Envía un correo electrónico con archivos adjuntos
        /// </summary>
        /// <param name="destinatario">Dirección de correo del destinatario</param>
        /// <param name="asunto">Asunto del correo</param>
        /// <param name="contenido">Contenido del correo (puede ser HTML)</param>
        /// <param name="archivosAdjuntos">Diccionario con el nombre y contenido binario de los archivos adjuntos</param>
        /// <returns>Resultado del envío</returns>
        Task<bool> EnviarCorreoConAdjuntosAsync(string destinatario, string asunto, string contenido, Dictionary<string, byte[]> archivosAdjuntos);
        
        /// <summary>
        /// Envía un correo electrónico a múltiples destinatarios
        /// </summary>
        /// <param name="destinatarios">Lista de direcciones de correo de los destinatarios</param>
        /// <param name="asunto">Asunto del correo</param>
        /// <param name="contenido">Contenido del correo (puede ser HTML)</param>
        /// <returns>Resultado del envío</returns>
        Task<bool> EnviarCorreoMasivoAsync(List<string> destinatarios, string asunto, string contenido);
        
        /// <summary>
        /// Envía un correo electrónico basado en una plantilla
        /// </summary>
        /// <param name="destinatario">Dirección de correo del destinatario</param>
        /// <param name="asunto">Asunto del correo</param>
        /// <param name="nombrePlantilla">Nombre de la plantilla a utilizar</param>
        /// <param name="datosPlantilla">Datos para poblar la plantilla</param>
        /// <returns>Resultado del envío</returns>
        Task<bool> EnviarCorreoConPlantillaAsync(string destinatario, string asunto, string nombrePlantilla, Dictionary<string, string> datosPlantilla);
    }
} 