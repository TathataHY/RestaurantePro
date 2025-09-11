using System;

namespace RestaurantePro.Application.Common.Services;

/// <summary>
/// Servicio para sanitización de contenido HTML y prevención de ataques XSS
/// </summary>
public interface IHtmlSanitizerService
{
    /// <summary>
    /// Sanitiza el contenido HTML removiendo elementos peligrosos
    /// </summary>
    /// <param name="input">Contenido HTML a sanitizar</param>
    /// <returns>Contenido sanitizado</returns>
    string SanitizeHtml(string input);

    /// <summary>
    /// Sanitiza texto plano removiendo caracteres peligrosos
    /// </summary>
    /// <param name="input">Texto a sanitizar</param>
    /// <returns>Texto sanitizado</returns>
    string SanitizeText(string input);

    /// <summary>
    /// Verifica si el contenido contiene elementos peligrosos
    /// </summary>
    /// <param name="input">Contenido a verificar</param>
    /// <returns>True si contiene elementos peligrosos</returns>
    bool ContainsDangerousContent(string input);
}
