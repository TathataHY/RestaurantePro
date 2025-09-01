using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestaurantePro.Mobile.Controls;

public partial class ModernEntry : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ModernEntry), string.Empty, 
            BindingMode.TwoWay, propertyChanged: OnTextChanged);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(ModernEntry), string.Empty, propertyChanged: OnPlaceholderChanged);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(ModernEntry), string.Empty, propertyChanged: OnIconChanged);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(ModernEntry), false, propertyChanged: OnIsPasswordChanged);

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(ModernEntry), Keyboard.Default, propertyChanged: OnKeyboardChanged);

    // Propiedades de Accesibilidad
    public static readonly BindableProperty AccessibilityNameProperty =
        BindableProperty.Create(nameof(AccessibilityName), typeof(string), typeof(ModernEntry), string.Empty);

    public static readonly BindableProperty AccessibilityHelpTextProperty =
        BindableProperty.Create(nameof(AccessibilityHelpText), typeof(string), typeof(ModernEntry), string.Empty);

    public static readonly BindableProperty IconDescriptionProperty =
        BindableProperty.Create(nameof(IconDescription), typeof(string), typeof(ModernEntry), string.Empty);

    // Propiedades de UX
    public static readonly BindableProperty IsValidProperty =
        BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(ModernEntry), true, propertyChanged: OnIsValidChanged);

    public static readonly BindableProperty ValidationMessageProperty =
        BindableProperty.Create(nameof(ValidationMessage), typeof(string), typeof(ModernEntry), string.Empty);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    // Propiedades de Accesibilidad
    public string AccessibilityName
    {
        get => (string)GetValue(AccessibilityNameProperty);
        set => SetValue(AccessibilityNameProperty, value);
    }

    public string AccessibilityHelpText
    {
        get => (string)GetValue(AccessibilityHelpTextProperty);
        set => SetValue(AccessibilityHelpTextProperty, value);
    }

    public string IconDescription
    {
        get => (string)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    // Propiedades de UX
    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }

    public string ValidationMessage
    {
        get => (string)GetValue(ValidationMessageProperty);
        set => SetValue(ValidationMessageProperty, value);
    }

    public ModernEntry()
    {
        InitializeComponent();
        
        // Configurar accesibilidad por defecto
        SetupDefaultAccessibility();
    }

    private void SetupDefaultAccessibility()
    {
        // Configurar nombres de accesibilidad basados en el placeholder
        if (string.IsNullOrEmpty(AccessibilityName) && !string.IsNullOrEmpty(Placeholder))
        {
            AccessibilityName = Placeholder;
        }

        // Configurar ayuda de accesibilidad basada en el tipo de campo
        if (string.IsNullOrEmpty(AccessibilityHelpText))
        {
            if (IsPassword)
            {
                AccessibilityHelpText = "Campo de contraseña. Ingrese su contraseña de forma segura.";
            }
            else if (Keyboard == Keyboard.Email)
            {
                AccessibilityHelpText = "Campo de correo electrónico. Ingrese su dirección de email.";
            }
            else
            {
                AccessibilityHelpText = "Campo de texto. Ingrese la información solicitada.";
            }
        }
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernEntry modernEntry)
        {
            var newText = newValue?.ToString() ?? string.Empty;
            modernEntry.InnerEntry.Text = newText;
        }
    }

    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernEntry modernEntry)
        {
            modernEntry.InnerEntry.Placeholder = newValue?.ToString() ?? string.Empty;
            modernEntry.SetupDefaultAccessibility();
        }
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernEntry modernEntry)
        {
            var icon = newValue?.ToString() ?? string.Empty;
            modernEntry.LabelIcon.Text = icon;
            modernEntry.LabelIcon.IsVisible = !string.IsNullOrEmpty(icon);
            
            // Configurar descripción del icono para accesibilidad
            modernEntry.IconDescription = GetIconDescription(icon);
        }
    }

    private static string GetIconDescription(string icon)
    {
        return icon switch
        {
            "📧" => "Icono de correo electrónico",
            "🔒" => "Icono de contraseña segura",
            "👤" => "Icono de usuario",
            "🔍" => "Icono de búsqueda",
            "📱" => "Icono de teléfono",
            "📍" => "Icono de ubicación",
            _ => "Icono descriptivo"
        };
    }

    private static void OnIsPasswordChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernEntry modernEntry)
        {
            modernEntry.InnerEntry.IsPassword = (bool)newValue;
            modernEntry.SetupDefaultAccessibility();
        }
    }

    private static void OnKeyboardChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernEntry modernEntry)
        {
            modernEntry.InnerEntry.Keyboard = (Keyboard)newValue;
            modernEntry.SetupDefaultAccessibility();
        }
    }

    private static void OnIsValidChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernEntry modernEntry)
        {
            // Cambiar el color del borde basado en la validación
            var isValid = (bool)newValue;
            modernEntry.UpdateVisualState(isValid);
        }
    }

    private void UpdateVisualState(bool isValid)
    {
        // Cambiar el color del borde para indicar validación
        var border = this.FindByName<Border>("ButtonBorder");
        if (border != null)
        {
            var borderColor = isValid ? 
                Application.Current?.Resources["BorderColor"] as Color ?? Colors.Gray : 
                Application.Current?.Resources["ErrorColor"] as Color ?? Colors.Red;
            border.Stroke = borderColor;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 