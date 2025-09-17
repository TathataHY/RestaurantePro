using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestaurantePro.Mobile.Controls;

public partial class FloatingActionButton : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(FloatingActionButton), "+", propertyChanged: OnTextChanged);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(FloatingActionButton), null, propertyChanged: OnIconChanged);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(System.Windows.Input.ICommand), typeof(FloatingActionButton), null);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(FloatingActionButton), null);

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

    public System.Windows.Input.ICommand Command
    {
        get => (System.Windows.Input.ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public event EventHandler Clicked;

    public FloatingActionButton()
    {
        InitializeComponent();
        FabButton.Clicked += OnButtonClicked;
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FloatingActionButton fab)
        {
            fab.FabButton.Text = newValue?.ToString() ?? "+";
        }
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FloatingActionButton fab)
        {
            fab.FabButton.ImageSource = newValue?.ToString();
        }
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("🔍 [FloatingActionButton] OnButtonClicked - FAB tocado");
        
        Clicked?.Invoke(this, e);
        
        if (Command != null)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 [FloatingActionButton] Command disponible: {Command.GetType().Name}");
            if (Command.CanExecute(CommandParameter))
            {
                System.Diagnostics.Debug.WriteLine("🔍 [FloatingActionButton] Ejecutando comando...");
                Command.Execute(CommandParameter);
                System.Diagnostics.Debug.WriteLine("✅ [FloatingActionButton] Comando ejecutado");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ [FloatingActionButton] Comando no puede ejecutarse (CanExecute = false)");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("❌ [FloatingActionButton] No hay comando configurado");
        }
    }
} 