using System.Collections.ObjectModel;
using RestaurantePro.Mobile.Controls;

namespace RestaurantePro.Mobile.Views;

public partial class ModernComponentsDemoPage : ContentPage
{
    public ModernComponentsDemoPage()
    {
        InitializeComponent();
        SetupTabNavigation();
        SetupBottomNavigation();
        SetupSkeletonNavigation();
        SetupFloatingActionButton();
    }

    private void SetupTabNavigation()
    {
        // Configurar tabs de demostración
        DemoTabNavigation.Tabs = new ObservableCollection<string>
        {
            "Pendientes",
            "En Progreso", 
            "Completadas"
        };

        // Configurar comando para manejar selección de tabs
        DemoTabNavigation.TabSelectedCommand = new Command<int>(OnTabSelected);
    }

    private void SetupBottomNavigation()
    {
        var tabs = new ObservableCollection<TabItem>
        {
            new TabItem("Inicio", "🏠"),
            new TabItem("Mesas", "🪑"),
            new TabItem("Comandas", "📋"),
            new TabItem("Productos", "🍽️"),
            new TabItem("Perfil", "👤")
        };

        DemoBottomNavigation.Tabs = tabs;
        DemoBottomNavigation.TabSelected += OnBottomTabSelected;
    }

    private void SetupSkeletonNavigation()
    {
        SkeletonTabNavigation.Tabs = new ObservableCollection<string> { "Card", "List", "Table" };
        SkeletonTabNavigation.TabSelectedCommand = new Command<int>(OnSkeletonTabSelected);
    }

    private void SetupFloatingActionButton()
    {
        // El FAB ya está configurado en XAML
    }

    private void OnTabSelected(int tabIndex)
    {
        TabStatusLabel.Text = $"Tab seleccionado: {tabIndex} - {DemoTabNavigation.Tabs[tabIndex]}";
    }

    private void OnBottomTabSelected(object sender, int index)
    {
        var tabNames = new[] { "Inicio", "Mesas", "Comandas", "Productos", "Perfil" };
        DisplayAlert("Navegación", $"Navegando a: {tabNames[index]}", "OK");
    }

    private void OnSkeletonTabSelected(int index)
    {
        var skeletonTypes = new[] { SkeletonType.Card, SkeletonType.List, SkeletonType.Table };
        DemoSkeletonLoader.SkeletonType = skeletonTypes[index];
    }
} 