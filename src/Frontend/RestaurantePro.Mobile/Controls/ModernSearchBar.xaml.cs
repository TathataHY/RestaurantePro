using System.Windows.Input;
using RestaurantePro.Mobile.Animations;

namespace RestaurantePro.Mobile.Controls;

/// <summary>
/// Barra de búsqueda moderna con animaciones - V4
/// </summary>
public partial class ModernSearchBar : ContentView
{
    public static readonly BindableProperty SearchTextProperty =
        BindableProperty.Create(nameof(SearchText), typeof(string), typeof(ModernSearchBar), string.Empty, propertyChanged: OnSearchTextChanged);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(ModernSearchBar), "Buscar...");

    public static readonly BindableProperty SearchCommandProperty =
        BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(ModernSearchBar), null);

    public static readonly BindableProperty SearchCommandParameterProperty =
        BindableProperty.Create(nameof(SearchCommandParameter), typeof(object), typeof(ModernSearchBar), null);

    public static readonly BindableProperty IsSearchingProperty =
        BindableProperty.Create(nameof(IsSearching), typeof(bool), typeof(ModernSearchBar), false);

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public ICommand SearchCommand
    {
        get => (ICommand)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public object SearchCommandParameter
    {
        get => GetValue(SearchCommandParameterProperty);
        set => SetValue(SearchCommandParameterProperty, value);
    }

    public bool IsSearching
    {
        get => (bool)GetValue(IsSearchingProperty);
        set => SetValue(IsSearchingProperty, value);
    }

    public event EventHandler<string> SearchTextChanged;
    public event EventHandler<string> SearchSubmitted;

    public ModernSearchBar()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        // Los elementos se configuran en el XAML con binding
        // No necesitamos configurar event handlers manualmente
    }

    private static void OnSearchTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernSearchBar searchBar)
        {
            searchBar.UpdateClearButtonVisibility();
        }
    }

    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        SearchText = e.NewTextValue;
        SearchTextChanged?.Invoke(this, e.NewTextValue);
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            // Feedback háptico
            HapticFeedback.Selection();
            
            // Ejecutar comando de búsqueda
            if (SearchCommand?.CanExecute(SearchText) == true)
            {
                SearchCommand.Execute(SearchText);
            }
            
            SearchSubmitted?.Invoke(this, SearchText);
        }
    }

    private void OnClearButtonClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            // Feedback háptico
            HapticFeedback.Click();
            
            // Limpiar texto
            SearchText = string.Empty;
        }
    }

    private void UpdateClearButtonVisibility()
    {
        // La visibilidad se maneja en el XAML con binding
    }

    /// <summary>
    /// Inicia la búsqueda programáticamente
    /// </summary>
    public async Task StartSearchAsync(string query = null)
    {
        if (!string.IsNullOrWhiteSpace(query))
        {
            SearchText = query;
        }

        IsSearching = true;
        
        // Simular búsqueda
        await Task.Delay(1000);
        
        IsSearching = false;
    }

    /// <summary>
    /// Limpia la búsqueda
    /// </summary>
    public void ClearSearch()
    {
        SearchText = string.Empty;
    }

    /// <summary>
    /// Enfoca el campo de búsqueda
    /// </summary>
    public void Focus()
    {
        // El foco se maneja en el XAML con binding
    }

    /// <summary>
    /// Quita el foco del campo de búsqueda
    /// </summary>
    public void Unfocus()
    {
        // El foco se maneja en el XAML con binding
    }
} 