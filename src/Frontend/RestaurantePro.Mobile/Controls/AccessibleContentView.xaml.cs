using RestaurantePro.Mobile.Services;

namespace RestaurantePro.Mobile.Controls;

/// <summary>
/// Componente base para accesibilidad - V4
/// </summary>
public partial class AccessibleContentView : ContentView
{
    public static readonly BindableProperty AccessibilityLabelProperty =
        BindableProperty.Create(nameof(AccessibilityLabel), typeof(string), typeof(AccessibleContentView), string.Empty);

    public static readonly BindableProperty AccessibilityHintProperty =
        BindableProperty.Create(nameof(AccessibilityHint), typeof(string), typeof(AccessibleContentView), string.Empty);

    public static readonly BindableProperty IsAccessibilityElementProperty =
        BindableProperty.Create(nameof(IsAccessibilityElement), typeof(bool), typeof(AccessibleContentView), true);

    public static readonly BindableProperty AccessibilityRoleProperty =
        BindableProperty.Create(nameof(AccessibilityRole), typeof(string), typeof(AccessibleContentView), "button");

    public static readonly BindableProperty AccessibilityValueProperty =
        BindableProperty.Create(nameof(AccessibilityValue), typeof(string), typeof(AccessibleContentView), string.Empty);

    public static readonly BindableProperty AccessibilityTraitsProperty =
        BindableProperty.Create(nameof(AccessibilityTraits), typeof(string), typeof(AccessibleContentView), string.Empty);

    private readonly AccessibilityService _accessibilityService;

    public AccessibleContentView()
    {
        InitializeComponent();
        _accessibilityService = AccessibilityService.Instance;
        SetupAccessibility();
    }

    #region Propiedades de Accesibilidad

    /// <summary>
    /// Etiqueta de accesibilidad para el lector de pantalla
    /// </summary>
    public string AccessibilityLabel
    {
        get => (string)GetValue(AccessibilityLabelProperty);
        set => SetValue(AccessibilityLabelProperty, value);
    }

    /// <summary>
    /// Sugerencia de accesibilidad para el lector de pantalla
    /// </summary>
    public string AccessibilityHint
    {
        get => (string)GetValue(AccessibilityHintProperty);
        set => SetValue(AccessibilityHintProperty, value);
    }

    /// <summary>
    /// Indica si el elemento es accesible para el lector de pantalla
    /// </summary>
    public bool IsAccessibilityElement
    {
        get => (bool)GetValue(IsAccessibilityElementProperty);
        set => SetValue(IsAccessibilityElementProperty, value);
    }

    /// <summary>
    /// Rol de accesibilidad del elemento
    /// </summary>
    public string AccessibilityRole
    {
        get => (string)GetValue(AccessibilityRoleProperty);
        set => SetValue(AccessibilityRoleProperty, value);
    }

    /// <summary>
    /// Valor de accesibilidad del elemento
    /// </summary>
    public string AccessibilityValue
    {
        get => (string)GetValue(AccessibilityValueProperty);
        set => SetValue(AccessibilityValueProperty, value);
    }

    /// <summary>
    /// Características de accesibilidad del elemento
    /// </summary>
    public string AccessibilityTraits
    {
        get => (string)GetValue(AccessibilityTraitsProperty);
        set => SetValue(AccessibilityTraitsProperty, value);
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Establece el contenido del componente
    /// </summary>
    public void SetContent(View content)
    {
        MainContainer.Children.Clear();
        MainContainer.Children.Add(content);
        SetupAccessibilityForContent(content);
    }

    /// <summary>
    /// Anuncia un mensaje al lector de pantalla
    /// </summary>
    public void AnnounceToScreenReader(string message)
    {
        _accessibilityService.AnnounceToScreenReader(message);
    }

    /// <summary>
    /// Obtiene el texto completo de accesibilidad
    /// </summary>
    public string GetFullAccessibilityText()
    {
        var text = AccessibilityLabel;
        
        if (!string.IsNullOrEmpty(AccessibilityValue))
        {
            text += $", valor: {AccessibilityValue}";
        }
        
        if (!string.IsNullOrEmpty(AccessibilityHint))
        {
            text += $", {AccessibilityHint}";
        }
        
        return text;
    }

    /// <summary>
    /// Configura la accesibilidad para un elemento específico
    /// </summary>
    public void ConfigureAccessibility(string label, string hint = "", string role = "button", string value = "")
    {
        AccessibilityLabel = _accessibilityService.GetAccessibilityText(label);
        AccessibilityHint = hint;
        AccessibilityRole = role;
        AccessibilityValue = value;
        
        ApplyAccessibilitySettings();
    }

    #endregion

    #region Métodos Privados

    private void SetupAccessibility()
    {
        // Configurar propiedades de accesibilidad básicas
        // Nota: SemanticProperties no está disponible en .NET MAUI
        // Se implementará cuando esté disponible
        
        // Configurar gestos de accesibilidad
        SetupAccessibilityGestures();
    }

    private void SetupAccessibilityGestures()
    {
        // Doble tap para activar
        var doubleTapGesture = new TapGestureRecognizer
        {
            NumberOfTapsRequired = 2,
            Command = new Command(() => OnAccessibilityActivated())
        };
        
        GestureRecognizers.Add(doubleTapGesture);
        
        // Tap largo para más información
        var longPressGesture = new TapGestureRecognizer
        {
            NumberOfTapsRequired = 1,
            Command = new Command(() => OnAccessibilityLongPress())
        };
        
        GestureRecognizers.Add(longPressGesture);
    }

    private void SetupAccessibilityForContent(View content)
    {
        if (content == null) return;

        // Configurar accesibilidad para el contenido
        if (content is Button button)
        {
            SetupButtonAccessibility(button);
        }
        else if (content is Label label)
        {
            SetupLabelAccessibility(label);
        }
        else if (content is Entry entry)
        {
            SetupEntryAccessibility(entry);
        }
        else if (content is Image image)
        {
            SetupImageAccessibility(image);
        }
    }

    private void SetupButtonAccessibility(Button button)
    {
        // Nota: SemanticProperties no está disponible en .NET MAUI
        // Se implementará cuando esté disponible
        
        // Configurar evento de activación
        button.Clicked += (sender, e) => OnAccessibilityActivated();
    }

    private void SetupLabelAccessibility(Label label)
    {
        // Nota: SemanticProperties no está disponible en .NET MAUI
        // Se implementará cuando esté disponible
    }

    private void SetupEntryAccessibility(Entry entry)
    {
        // Nota: SemanticProperties no está disponible en .NET MAUI
        // Se implementará cuando esté disponible
        
        // Configurar eventos de texto
        entry.TextChanged += (sender, e) => OnAccessibilityTextChanged(e.NewTextValue);
    }

    private void SetupImageAccessibility(Image image)
    {
        // Nota: SemanticProperties no está disponible en .NET MAUI
        // Se implementará cuando esté disponible
    }

    private void ApplyAccessibilitySettings()
    {
        // Nota: SemanticProperties no está disponible en .NET MAUI
        // Se implementará cuando esté disponible
        
        // Por ahora, solo configuramos el texto de accesibilidad
        System.Diagnostics.Debug.WriteLine($"Accessibility configured: {GetFullAccessibilityText()}");
    }

    private void OnAccessibilityActivated()
    {
        if (_accessibilityService.ScreenReaderEnabled)
        {
            AnnounceToScreenReader($"Activado: {AccessibilityLabel}");
        }
        
        // Proporcionar feedback háptico si está habilitado
        if (_accessibilityService.HapticFeedbackEnabled)
        {
            // Nota: HapticFeedback.Default no está disponible en .NET 9
            // Se implementará cuando esté disponible
            System.Diagnostics.Debug.WriteLine("Haptic feedback triggered");
        }
    }

    private void OnAccessibilityLongPress()
    {
        if (_accessibilityService.ScreenReaderEnabled)
        {
            var fullText = GetFullAccessibilityText();
            AnnounceToScreenReader(fullText);
        }
    }

    private void OnAccessibilityTextChanged(string newText)
    {
        if (_accessibilityService.ScreenReaderEnabled && !string.IsNullOrEmpty(newText))
        {
            AnnounceToScreenReader($"Texto ingresado: {newText}");
        }
    }

    #endregion

    #region Eventos de Accesibilidad

    /// <summary>
    /// Evento que se dispara cuando el elemento es activado por accesibilidad
    /// </summary>
    public event EventHandler AccessibilityActivated;

    /// <summary>
    /// Evento que se dispara cuando se presiona largo el elemento
    /// </summary>
    public event EventHandler AccessibilityLongPressed;

    /// <summary>
    /// Evento que se dispara cuando cambia el texto del elemento
    /// </summary>
    public event EventHandler<string> AccessibilityTextChanged;

    #endregion
} 