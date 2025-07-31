using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestaurantePro.Mobile.Controls;

public partial class TabNavigation : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty TabsProperty =
        BindableProperty.Create(nameof(Tabs), typeof(ObservableCollection<string>), typeof(TabNavigation), new ObservableCollection<string>(), propertyChanged: OnTabsChanged);

    public static readonly BindableProperty SelectedTabIndexProperty =
        BindableProperty.Create(nameof(SelectedTabIndex), typeof(int), typeof(TabNavigation), 0, propertyChanged: OnSelectedTabIndexChanged);

    public static readonly BindableProperty TabSelectedCommandProperty =
        BindableProperty.Create(nameof(TabSelectedCommand), typeof(Command<int>), typeof(TabNavigation));

    private ObservableCollection<TabButton> _tabButtons;

    public ObservableCollection<string> Tabs
    {
        get => (ObservableCollection<string>)GetValue(TabsProperty);
        set => SetValue(TabsProperty, value);
    }

    public int SelectedTabIndex
    {
        get => (int)GetValue(SelectedTabIndexProperty);
        set => SetValue(SelectedTabIndexProperty, value);
    }

    public Command<int> TabSelectedCommand
    {
        get => (Command<int>)GetValue(TabSelectedCommandProperty);
        set => SetValue(TabSelectedCommandProperty, value);
    }

    public TabNavigation()
    {
        InitializeComponent();
        _tabButtons = new ObservableCollection<TabButton>();
    }

    private static void OnTabsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TabNavigation tabNavigation)
        {
            tabNavigation.CreateTabs();
        }
    }

    private static void OnSelectedTabIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TabNavigation tabNavigation)
        {
            tabNavigation.UpdateSelectedTab();
        }
    }

    private void CreateTabs()
    {
        TabContainer.Children.Clear();
        _tabButtons.Clear();

        for (int i = 0; i < Tabs.Count; i++)
        {
            var tabButton = new TabButton
            {
                Text = Tabs[i],
                IsSelected = i == SelectedTabIndex
            };

            var index = i; // Capturar el índice para el closure
            tabButton.Clicked += (sender, e) => OnTabClicked(index);

            TabContainer.Children.Add(tabButton);
            _tabButtons.Add(tabButton);
        }
    }

    private void UpdateSelectedTab()
    {
        for (int i = 0; i < _tabButtons.Count; i++)
        {
            _tabButtons[i].IsSelected = i == SelectedTabIndex;
        }
    }

    private void OnTabClicked(int index)
    {
        SelectedTabIndex = index;
        TabSelectedCommand?.Execute(index);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 