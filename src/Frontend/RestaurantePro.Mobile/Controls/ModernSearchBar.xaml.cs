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
        SearchEntry.TextChanged += OnEntryTextChanged;
        SearchEntry.Completed += OnEntryCompleted;
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
        UpdateClearButtonVisibility();
        SearchTextChanged?.Invoke(this, e.NewTextValue);
    }

    private async void OnEntryCompleted(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            // Feedback háptico
            HapticFeedback.Selection();
            
            // Animación de búsqueda
            await SearchIcon.ScaleTo(1.2, 100, Easing.CubicOut);
            await SearchIcon.ScaleTo(1.0, 100, Easing.CubicIn);
            
            // Ejecutar comando de búsqueda
            if (SearchCommand?.CanExecute(SearchText) == true)
            {
                SearchCommand.Execute(SearchText);
            }
            
            SearchSubmitted?.Invoke(this, SearchText);
        }
    }

    private async void OnClearButtonClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            // Feedback háptico
            HapticFeedback.Click();
            
            // Animación de limpieza
            await ClearButton.ScaleTo(0.8, 100, Easing.CubicOut);
            await ClearButton.ScaleTo(1.0, 100, Easing.CubicIn);
            
            // Limpiar texto
            SearchText = string.Empty;
            SearchEntry.Text = string.Empty;
            
            // Ocultar botón de limpiar
            await ClearButton.FadeTo(0, 200, Easing.CubicOut);
            ClearButton.IsVisible = false;
            
            // Enfocar el campo de entrada
            SearchEntry.Focus();
        }
    }

    private void UpdateClearButtonVisibility()
    {
        var shouldShow = !string.IsNullOrWhiteSpace(SearchText);
        
        if (shouldShow && !ClearButton.IsVisible)
        {
            ClearButton.IsVisible = true;
            ClearButton.FadeTo(1, 200, Easing.CubicOut);
        }
        else if (!shouldShow && ClearButton.IsVisible)
        {
            ClearButton.FadeTo(0, 200, Easing.CubicOut);
            ClearButton.IsVisible = false;
        }
    }

    /// <summary>
    /// Inicia la búsqueda programáticamente
    /// </summary>
    public async Task StartSearchAsync(string query = null)
    {
        if (!string.IsNullOrWhiteSpace(query))
        {
            SearchText = query;
            SearchEntry.Text = query;
        }

        IsSearching = true;
        
        // Animación de búsqueda
        await SearchIcon.RotateTo(360, 1000, Easing.Linear);
        SearchIcon.Rotation = 0;
        
        IsSearching = false;
    }

    /// <summary>
    /// Limpia la búsqueda
    /// </summary>
    public void ClearSearch()
    {
        SearchText = string.Empty;
        SearchEntry.Text = string.Empty;
        UpdateClearButtonVisibility();
    }

    /// <summary>
    /// Enfoca el campo de búsqueda
    /// </summary>
    public void Focus()
    {
        SearchEntry.Focus();
    }

    /// <summary>
    /// Quita el foco del campo de búsqueda
    /// </summary>
    public void Unfocus()
    {
        SearchEntry.Unfocus();
    }
} 