using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestaurantePro.Mobile.Controls;

public partial class TabButton : Button, INotifyPropertyChanged
{
    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(TabButton), false, propertyChanged: OnIsSelectedChanged);

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public TabButton()
    {
        InitializeComponent();
        UpdateVisualState();
    }

    private static void OnIsSelectedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TabButton tabButton)
        {
            tabButton.UpdateVisualState();
        }
    }

    private void UpdateVisualState()
    {
        if (IsSelected)
        {
            VisualStateManager.GoToState(this, "Selected");
        }
        else
        {
            VisualStateManager.GoToState(this, "Normal");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 