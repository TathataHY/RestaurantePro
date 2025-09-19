using RestaurantePro.Mobile.Core.Models.Enums;

namespace RestaurantePro.Mobile.Core.Core.Attributes;

/// <summary>
/// Atributo para decorar ViewModels y páginas que requieren acceso a funcionalidades específicas
/// Utilizado para validación automática de autorización a nivel de funcionalidad
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireFeatureAttribute : Attribute
{
    /// <summary>
    /// Funcionalidad requerida para acceder al elemento decorado
    /// </summary>
    public AppFeature RequiredFeature { get; }
    
    /// <summary>
    /// Mensaje de error personalizado cuando no se tiene acceso a la funcionalidad
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="feature">Funcionalidad requerida</param>
    public RequireFeatureAttribute(AppFeature feature)
    {
        RequiredFeature = feature;
    }
}
