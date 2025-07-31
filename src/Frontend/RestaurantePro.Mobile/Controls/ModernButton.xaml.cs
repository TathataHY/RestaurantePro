using RestaurantePro.Mobile.Animations;
using System.Windows.Input;

namespace RestaurantePro.Mobile.Controls;

/// <summary>
/// Botón moderno con feedback háptico y animaciones - V4
/// </summary>
public partial class ModernButton : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ModernButton), string.Empty);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(ModernButton), Colors.White);

    public static readonly BindableProperty BackgroundColorProperty =
        BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(ModernButton), Colors.Transparent);

    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(ModernButton), Colors.Transparent);

    public static readonly BindableProperty BorderWidthProperty =
        BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(ModernButton), 0.0);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(ModernButton), 16.0);

    public static readonly BindableProperty FontAttributesProperty =
        BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(ModernButton), FontAttributes.Bold);

    public static readonly BindableProperty HeightRequestProperty =
        BindableProperty.Create(nameof(HeightRequest), typeof(double), typeof(ModernButton), 44.0);

    public static readonly BindableProperty PaddingProperty =
        BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(ModernButton), new Thickness(24, 12));

    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(nameof(IconSource), typeof(string), typeof(ModernButton), string.Empty);

    public static readonly BindableProperty ShowIconProperty =
        BindableProperty.Create(nameof(ShowIcon), typeof(bool), typeof(ModernButton), false);

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(ModernButton), false);

    public static readonly BindableProperty IsEnabledProperty =
        BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(ModernButton), true);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ModernButton), null);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ModernButton), null);

    // Propiedades de Accesibilidad
    public static readonly BindableProperty AccessibilityNameProperty =
        BindableProperty.Create(nameof(AccessibilityName), typeof(string), typeof(ModernButton), string.Empty);

    public static readonly BindableProperty AccessibilityHelpTextProperty =
        BindableProperty.Create(nameof(AccessibilityHelpText), typeof(string), typeof(ModernButton), string.Empty);

    public static readonly BindableProperty IconDescriptionProperty =
        BindableProperty.Create(nameof(IconDescription), typeof(string), typeof(ModernButton), string.Empty);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
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

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontAttributes FontAttributes
    {
        get => (FontAttributes)GetValue(FontAttributesProperty);
        set => SetValue(FontAttributesProperty, value);
    }

    public double HeightRequest
    {
        get => (double)GetValue(HeightRequestProperty);
        set => SetValue(HeightRequestProperty, value);
    }

    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public string IconSource
    {
        get => (string)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
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

    public event EventHandler Clicked;

    public ModernButton()
    {
        InitializeComponent();
        SetupGestures();
        SetupDefaultAccessibility();
    }

    private void SetupDefaultAccessibility()
    {
        // Configurar accesibilidad por defecto basada en el texto
        if (string.IsNullOrEmpty(AccessibilityName) && !string.IsNullOrEmpty(Text))
        {
            AccessibilityName = Text;
        }

        // Configurar ayuda de accesibilidad por defecto
        if (string.IsNullOrEmpty(AccessibilityHelpText))
        {
            AccessibilityHelpText = $"Botón {Text}. Toque para ejecutar la acción.";
        }

        // Configurar descripción del icono
        if (string.IsNullOrEmpty(IconDescription) && !string.IsNullOrEmpty(IconSource))
        {
            IconDescription = GetIconDescription(IconSource);
        }
    }

    private string GetIconDescription(string iconSource)
    {
        return iconSource switch
        {
            "add" => "Icono de agregar",
            "edit" => "Icono de editar",
            "delete" => "Icono de eliminar",
            "save" => "Icono de guardar",
            "cancel" => "Icono de cancelar",
            "search" => "Icono de búsqueda",
            "filter" => "Icono de filtro",
            "refresh" => "Icono de actualizar",
            "settings" => "Icono de configuración",
            "help" => "Icono de ayuda",
            _ => "Icono descriptivo"
        };
    }

    private void SetupGestures()
    {
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnButtonTapped;
        this.GestureRecognizers.Add(tapGesture);
    }

    private async void OnButtonTapped(object? sender, EventArgs e)
    {
        if (!IsEnabled || IsLoading)
            return;

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

        // Disparar evento Clicked
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Aplicar estilo primario
    /// </summary>
    public void ApplyPrimaryStyle()
    {
        BackgroundColor = Color.FromArgb("#FF6B35"); // PrimaryColor
        TextColor = Colors.White;
        BorderColor = Colors.Transparent;
        BorderWidth = 0;
    }

    /// <summary>
    /// Aplicar estilo secundario
    /// </summary>
    public void ApplySecondaryStyle()
    {
        BackgroundColor = Colors.Transparent;
        TextColor = Color.FromArgb("#FF6B35"); // PrimaryColor
        BorderColor = Color.FromArgb("#FF6B35"); // PrimaryColor
        BorderWidth = 2;
    }

    /// <summary>
    /// Aplicar estilo de éxito
    /// </summary>
    public void ApplySuccessStyle()
    {
        BackgroundColor = Color.FromArgb("#27AE60"); // SuccessColor
        TextColor = Colors.White;
        BorderColor = Colors.Transparent;
        BorderWidth = 0;
    }

    /// <summary>
    /// Aplicar estilo de advertencia
    /// </summary>
    public void ApplyWarningStyle()
    {
        BackgroundColor = Color.FromArgb("#F39C12"); // WarningColor
        TextColor = Colors.White;
        BorderColor = Colors.Transparent;
        BorderWidth = 0;
    }

    /// <summary>
    /// Aplicar estilo de peligro
    /// </summary>
    public void ApplyDangerStyle()
    {
        BackgroundColor = Color.FromArgb("#E74C3C"); // ErrorColor
        TextColor = Colors.White;
        BorderColor = Colors.Transparent;
        BorderWidth = 0;
    }

    /// <summary>
    /// Mostrar estado de carga
    /// </summary>
    public void ShowLoading()
    {
        IsLoading = true;
        IsEnabled = false;
    }

    /// <summary>
    /// Ocultar estado de carga
    /// </summary>
    public void HideLoading()
    {
        IsLoading = false;
        IsEnabled = true;
    }

    /// <summary>
    /// Animación de éxito
    /// </summary>
    public async Task ShowSuccessAsync()
    {
        var originalColor = BackgroundColor;
        BackgroundColor = Color.FromArgb("#27AE60"); // SuccessColor
        await ButtonBorder.SuccessAnimationAsync();
        BackgroundColor = originalColor;
    }

    /// <summary>
    /// Animación de error
    /// </summary>
    public async Task ShowErrorAsync()
    {
        await ButtonBorder.ShakeAsync();
    }
} 