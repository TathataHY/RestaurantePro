using System.Collections;
using System.Windows.Input;

namespace RestaurantePro.Mobile.Controls;

/// <summary>
/// Lista optimizada con virtualización - V4
/// </summary>
public partial class OptimizedListView : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(OptimizedListView), null, propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(OptimizedListView), null);

    public static readonly BindableProperty HeaderProperty =
        BindableProperty.Create(nameof(Header), typeof(View), typeof(OptimizedListView), null, propertyChanged: OnHeaderChanged);

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(OptimizedListView), false, propertyChanged: OnIsLoadingChanged);

    public static readonly BindableProperty LoadMoreCommandProperty =
        BindableProperty.Create(nameof(LoadMoreCommand), typeof(ICommand), typeof(OptimizedListView), null);

    public static readonly BindableProperty EmptyStateTextProperty =
        BindableProperty.Create(nameof(EmptyStateText), typeof(string), typeof(OptimizedListView), "No hay elementos para mostrar");

    public static readonly BindableProperty EmptyStateDescriptionProperty =
        BindableProperty.Create(nameof(EmptyStateDescription), typeof(string), typeof(OptimizedListView), "Los elementos aparecerán aquí cuando estén disponibles");

    public OptimizedListView()
    {
        InitializeComponent();
        SetupBindings();
    }

    #region Properties

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public View Header
    {
        get => (View)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public ICommand LoadMoreCommand
    {
        get => (ICommand)GetValue(LoadMoreCommandProperty);
        set => SetValue(LoadMoreCommandProperty, value);
    }

    public string EmptyStateText
    {
        get => (string)GetValue(EmptyStateTextProperty);
        set => SetValue(EmptyStateTextProperty, value);
    }

    public string EmptyStateDescription
    {
        get => (string)GetValue(EmptyStateDescriptionProperty);
        set => SetValue(EmptyStateDescriptionProperty, value);
    }

    #endregion

    #region Private Methods

    private void SetupBindings()
    {
        // Configurar binding context para el CollectionView
        MainCollectionView.SetBinding(CollectionView.ItemsSourceProperty, new Binding(nameof(ItemsSource), source: this));
        MainCollectionView.SetBinding(CollectionView.ItemTemplateProperty, new Binding(nameof(ItemTemplate), source: this));
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is OptimizedListView listView)
        {
            listView.UpdateEmptyState();
        }
    }

    private static void OnHeaderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is OptimizedListView listView)
        {
            listView.UpdateHeader();
        }
    }

    private static void OnIsLoadingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is OptimizedListView listView)
        {
            listView.UpdateLoadingState();
        }
    }

    private void UpdateEmptyState()
    {
        var hasItems = ItemsSource != null && ItemsSource.Cast<object>().Any();
        
        MainCollectionView.IsVisible = hasItems;
        EmptyStateContainer.IsVisible = !hasItems;
        
        if (!hasItems)
        {
            // Actualizar texto del estado vacío
            var emptyStateLabel = EmptyStateContainer.Children.OfType<Label>().FirstOrDefault();
            if (emptyStateLabel != null)
            {
                emptyStateLabel.Text = EmptyStateText;
            }
            
            var descriptionLabel = EmptyStateContainer.Children.OfType<Label>().LastOrDefault();
            if (descriptionLabel != null)
            {
                descriptionLabel.Text = EmptyStateDescription;
            }
        }
    }

    private void UpdateHeader()
    {
        HeaderContainer.Children.Clear();
        
        if (Header != null)
        {
            HeaderContainer.Children.Add(Header);
            HeaderContainer.IsVisible = true;
        }
        else
        {
            HeaderContainer.IsVisible = false;
        }
    }

    private void UpdateLoadingState()
    {
        LoadingIndicator.IsVisible = IsLoading;
    }

    private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        if (LoadMoreCommand?.CanExecute(null) == true)
        {
            LoadMoreCommand.Execute(null);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Refresca la lista
    /// </summary>
    public void Refresh()
    {
        UpdateEmptyState();
        MainCollectionView.ItemsSource = null;
        MainCollectionView.ItemsSource = ItemsSource;
    }

    /// <summary>
    /// Desplaza la lista al elemento especificado
    /// </summary>
    public void ScrollToItem(object item, bool animate = true)
    {
        try
        {
            MainCollectionView.ScrollTo(item, animate: animate);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scrolling to item: {ex.Message}");
        }
    }

    /// <summary>
    /// Desplaza la lista al índice especificado
    /// </summary>
    public void ScrollToIndex(int index, bool animate = true)
    {
        try
        {
            MainCollectionView.ScrollTo(index, animate: animate);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scrolling to index: {ex.Message}");
        }
    }

    #endregion
} 