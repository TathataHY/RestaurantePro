using System.Windows.Input;

namespace RestaurantePro.Mobile.Controls;

/// <summary>
/// Elemento de navegación accesible con soporte completo para lectores de pantalla
/// </summary>
public partial class AccessibleNavigationItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(AccessibleNavigationItem), string.Empty);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(AccessibleNavigationItem), string.Empty);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(AccessibleNavigationItem), Colors.Black);

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(AccessibleNavigationItem), Colors.Black);

    public static readonly BindableProperty BackgroundColorProperty =
        BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(AccessibleNavigationItem), Colors.Transparent);

    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(AccessibleNavigationItem), Colors.Transparent);

    public static readonly BindableProperty BorderWidthProperty =
        BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(AccessibleNavigationItem), 0.0);

    public static readonly BindableProperty FontAttributesProperty =
        BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(AccessibleNavigationItem), FontAttributes.None);

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(AccessibleNavigationItem), false, propertyChanged: OnIsSelectedChanged);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(AccessibleNavigationItem), null);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(AccessibleNavigationItem), null);

    // Propiedades de Accesibilidad
    public static readonly BindableProperty AccessibilityNameProperty =
        BindableProperty.Create(nameof(AccessibilityName), typeof(string), typeof(AccessibleNavigationItem), string.Empty);

    public static readonly BindableProperty AccessibilityHelpTextProperty =
        BindableProperty.Create(nameof(AccessibilityHelpText), typeof(string), typeof(AccessibleNavigationItem), string.Empty);

    public static readonly BindableProperty IconDescriptionProperty =
        BindableProperty.Create(nameof(IconDescription), typeof(string), typeof(AccessibleNavigationItem), string.Empty);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public double BorderWidth
    {
        get => (double)GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }

    public FontAttributes FontAttributes
    {
        get => (FontAttributes)GetValue(FontAttributesProperty);
        set => SetValue(FontAttributesProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
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

    public event EventHandler? Tapped;

    public AccessibleNavigationItem()
    {
        InitializeComponent();
        SetupGestures();
        SetupDefaultAccessibility();
    }

    private void SetupGestures()
    {
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnItemTapped;
        this.GestureRecognizers.Add(tapGesture);
    }

    private void SetupDefaultAccessibility()
    {
        // Configurar accesibilidad por defecto
        if (string.IsNullOrEmpty(AccessibilityName) && !string.IsNullOrEmpty(Text))
        {
            AccessibilityName = Text;
        }

        if (string.IsNullOrEmpty(AccessibilityHelpText))
        {
            AccessibilityHelpText = $"Elemento de navegación {Text}. Toque para navegar.";
        }

        if (string.IsNullOrEmpty(IconDescription) && !string.IsNullOrEmpty(Icon))
        {
            IconDescription = GetIconDescription(Icon);
        }
    }

    private string GetIconDescription(string icon)
    {
        return icon switch
        {
            "🏠" => "Icono de inicio",
            "📊" => "Icono de dashboard",
            "🍽️" => "Icono de comandas",
            "📦" => "Icono de inventario",
            "👥" => "Icono de clientes",
            "💰" => "Icono de ventas",
            "⚙️" => "Icono de configuración",
            "👤" => "Icono de perfil",
            "📈" => "Icono de reportes",
            "🔍" => "Icono de búsqueda",
            _ => "Icono descriptivo"
        };
    }

    private async void OnItemTapped(object? sender, EventArgs e)
    {
        // Feedback háptico
        HapticFeedback.Click();

        // Animación de presión
        await this.ScaleTo(0.95, 100);
        await this.ScaleTo(1.0, 100);

        // Ejecutar comando si existe
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }

        // Disparar evento Tapped
        Tapped?.Invoke(this, EventArgs.Empty);
    }

    private static void OnIsSelectedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AccessibleNavigationItem item)
        {
            var isSelected = (bool)newValue;
            item.UpdateVisualState(isSelected);
        }
    }

    private void UpdateVisualState(bool isSelected)
    {
        if (isSelected)
        {
            // Estado seleccionado
            var primaryColor = Application.Current?.Resources["PrimaryColor"] as Color ?? Colors.Blue;
            BackgroundColor = primaryColor;
            TextColor = Colors.White;
            IconColor = Colors.White;
            BorderColor = primaryColor;
            BorderWidth = 2;
            FontAttributes = FontAttributes.Bold;
            
            // Actualizar accesibilidad para estado seleccionado
            AccessibilityHelpText = $"{Text} - Seleccionado actualmente. Toque para navegar.";
        }
        else
        {
            // Estado normal
            BackgroundColor = Colors.Transparent;
            var textPrimary = Application.Current?.Resources["TextPrimary"] as Color ?? Colors.Black;
            var textSecondary = Application.Current?.Resources["TextSecondary"] as Color ?? Colors.Gray;
            TextColor = textPrimary;
            IconColor = textSecondary;
            BorderColor = Colors.Transparent;
            BorderWidth = 0;
            FontAttributes = FontAttributes.None;
            
            // Restaurar accesibilidad normal
            AccessibilityHelpText = $"Elemento de navegación {Text}. Toque para navegar.";
        }
    }

    // Métodos públicos para configuración rápida
    public void ApplyPrimaryStyle()
    {
        var primaryColor = Application.Current?.Resources["PrimaryColor"] as Color ?? Colors.Blue;
        BackgroundColor = primaryColor;
        TextColor = Colors.White;
        IconColor = Colors.White;
        BorderColor = primaryColor;
        BorderWidth = 2;
    }

    public void ApplySecondaryStyle()
    {
        BackgroundColor = Colors.Transparent;
        var primaryColor = Application.Current?.Resources["PrimaryColor"] as Color ?? Colors.Blue;
        TextColor = primaryColor;
        IconColor = primaryColor;
        BorderColor = primaryColor;
        BorderWidth = 1;
    }

    public void ApplySuccessStyle()
    {
        var successColor = Application.Current?.Resources["SuccessColor"] as Color ?? Colors.Green;
        BackgroundColor = successColor;
        TextColor = Colors.White;
        IconColor = Colors.White;
        BorderColor = successColor;
        BorderWidth = 2;
    }
} 