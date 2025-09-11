using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RestaurantePro.Application.Common.Services;

/// <summary>
/// Implementación del servicio de sanitización HTML
/// </summary>
public class HtmlSanitizerService : IHtmlSanitizerService
{
    private static readonly string[] DangerousTags = {
        "script", "iframe", "object", "embed", "form", "input", "button", "link", "meta", "style",
        "applet", "base", "body", "frame", "frameset", "head", "html", "title", "img"
    };

    private static readonly string[] DangerousAttributes = {
        "onload", "onerror", "onclick", "onmouseover", "onfocus", "onblur", "onchange", "onsubmit",
        "onreset", "onselect", "onkeydown", "onkeyup", "onkeypress", "onmousedown", "onmouseup",
        "onmousemove", "onmouseout", "onmouseover", "onabort", "onbeforeunload", "onerror",
        "onhashchange", "onload", "onpageshow", "onpagehide", "onresize", "onscroll", "onunload"
    };

    /// <summary>
    /// Sanitiza el contenido HTML removiendo elementos peligrosos
    /// </summary>
    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var result = input;

        // Remover tags peligrosos pero mantener su contenido
        foreach (var tag in DangerousTags)
        {
            // Patrón para tags con contenido: <tag>contenido</tag> -> contenido
            var pattern = $@"<{tag}[^>]*>(.*?)</{tag}>";
            result = Regex.Replace(result, pattern, "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // Patrón para tags auto-cerrados: <tag /> -> (vacío)
            pattern = $@"<{tag}[^>]*/?>";
            result = Regex.Replace(result, pattern, "", RegexOptions.IgnoreCase);
        }

        // Remover atributos peligrosos de cualquier tag restante
        foreach (var attr in DangerousAttributes)
        {
            // Patrón más robusto para atributos: attr="valor" o attr='valor'
            var pattern = $@"\s+{attr}\s*=\s*[""'][^""']*[""']";
            result = Regex.Replace(result, pattern, "", RegexOptions.IgnoreCase);
        }

        // Remover javascript: y data: URLs
        result = Regex.Replace(result, @"javascript\s*:", "", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"data\s*:", "", RegexOptions.IgnoreCase);

        // Remover expresiones de estilo peligrosas
        result = Regex.Replace(result, @"expression\s*\(", "", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"url\s*\(\s*javascript\s*:", "", RegexOptions.IgnoreCase);

        return result.Trim();
    }

    /// <summary>
    /// Sanitiza texto plano removiendo elementos peligrosos pero manteniendo el texto legible
    /// </summary>
    public string SanitizeText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // Si no hay elementos HTML peligrosos, mantener el texto original
        if (!ContainsDangerousContent(input))
        {
            return input;
        }

        // Si hay elementos peligrosos, usar SanitizeHtml para removerlos
        return SanitizeHtml(input);
    }

    /// <summary>
    /// Verifica si el contenido contiene elementos peligrosos
    /// </summary>
    public bool ContainsDangerousContent(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Verificar tags peligrosos
        foreach (var tag in DangerousTags)
        {
            if (Regex.IsMatch(input, $@"<{tag}[^>]*>", RegexOptions.IgnoreCase))
                return true;
        }

        // Verificar atributos peligrosos
        foreach (var attr in DangerousAttributes)
        {
            if (Regex.IsMatch(input, $@"\s{attr}\s*=", RegexOptions.IgnoreCase))
                return true;
        }

        // Verificar javascript: y data: URLs
        if (Regex.IsMatch(input, @"javascript\s*:", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(input, @"data\s*:", RegexOptions.IgnoreCase))
            return true;

        // Verificar expresiones de estilo peligrosas
        if (Regex.IsMatch(input, @"expression\s*\(", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(input, @"url\s*\(\s*javascript\s*:", RegexOptions.IgnoreCase))
            return true;

        return false;
    }
}
