using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RestaurantePro.Mobile.Animations;

namespace RestaurantePro.Mobile.Controls;

public partial class ModernTabBar : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(ModernTabBar), 0, propertyChanged: OnSelectedIndexChanged);

    public static readonly BindableProperty TabsProperty =
        BindableProperty.Create(nameof(Tabs), typeof(ObservableCollection<TabItem>), typeof(ModernTabBar), 
            new ObservableCollection<TabItem>(), propertyChanged: OnTabsChanged);

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public ObservableCollection<TabItem> Tabs
    {
        get => (ObservableCollection<TabItem>)GetValue(TabsProperty);
        set => SetValue(TabsProperty, value);
    }

    public event EventHandler<int> TabSelected;

    private List<Button> _tabButtons = new List<Button>();

    public ModernTabBar()
    {
        InitializeComponent();
    }

    private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernTabBar tabBar)
        {
            tabBar.UpdateSelectedTab();
        }
    }

    private static void OnTabsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernTabBar tabBar)
        {
            tabBar.BuildTabs();
        }
    }

    private void BuildTabs()
    {
        TabContainer.Children.Clear();
        _tabButtons.Clear();

        if (Tabs == null) return;

        for (int i = 0; i < Tabs.Count; i++)
        {
            var tab = Tabs[i];
            var tabButton = CreateTabButton(tab, i);
            TabContainer.Children.Add(tabButton);
            _tabButtons.Add((Button)tabButton);
        }

        UpdateSelectedTab();
    }

    private View CreateTabButton(TabItem tab, int index)
    {
        var button = new Button
        {
            BackgroundColor = Colors.Transparent,
            CornerRadius = 0,
            HeightRequest = 64,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Fill,
            Padding = new Thickness(4, 8, 4, 8)
        };

        // Configurar layout del botón (icono arriba, texto abajo)
        button.ImageSource = tab.Icon;
        button.Text = tab.Title;
        button.FontSize = 12;
        button.TextColor = Color.FromArgb("#6C757D");

        button.Clicked += async (sender, e) =>
        {
            if (SelectedIndex != index)
            {
                SelectedIndex = index;
                TabSelected?.Invoke(this, index);
                
                // Feedback háptico
                HapticFeedback.Selection();
                
                // Animación del botón
                await button.ScaleTo(0.95, 100, Easing.CubicOut);
                await button.ScaleTo(1.0, 100, Easing.CubicIn);
            }
        };

        return button;
    }

    private async void UpdateSelectedTab()
    {
        if (_tabButtons.Count == 0) return;

        for (int i = 0; i < _tabButtons.Count; i++)
        {
            var button = _tabButtons[i];

            if (i == SelectedIndex)
            {
                // Tab seleccionado
                button.TextColor = Color.FromArgb("#FF6B35"); // PrimaryColor
                button.FontAttributes = FontAttributes.Bold;
                button.Opacity = 1.0;
            }
            else
            {
                // Tab no seleccionado
                button.TextColor = Color.FromArgb("#6C757D"); // TextSecondary
                button.FontAttributes = FontAttributes.None;
                button.Opacity = 0.8;
            }
        }

        // Animar indicador de selección
        await AnimateSelectionIndicator();
    }

    private async Task AnimateSelectionIndicator()
    {
        if (_tabButtons.Count == 0) return;

        var selectedButton = _tabButtons[SelectedIndex];
        var buttonWidth = selectedButton.Width;
        var buttonX = selectedButton.X;

        if (buttonWidth > 0)
        {
            var indicatorWidth = Math.Min(40, buttonWidth * 0.6);
            var indicatorX = buttonX + (buttonWidth - indicatorWidth) / 2;

            await SelectionIndicator.TranslateTo(indicatorX, 0, 300, Easing.CubicOut);
            SelectionIndicator.WidthRequest = indicatorWidth;
        }
    }
}

public class TabItem
{
    public string Title { get; set; }
    public string Icon { get; set; }
    public string Route { get; set; }

    public TabItem(string title, string icon, string route = null)
    {
        Title = title;
        Icon = icon;
        Route = route;
    }
} 