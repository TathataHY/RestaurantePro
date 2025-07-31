using RestaurantePro.Mobile.Animations;

namespace RestaurantePro.Mobile.Controls;

/// <summary>
/// Indicador de carga moderno con animaciones - V4
/// </summary>
public partial class ModernLoadingIndicator : ContentView
{
    private bool _isAnimating = false;
    private CancellationTokenSource _animationCancellationToken;

    public static readonly BindableProperty LoadingMessageProperty =
        BindableProperty.Create(nameof(LoadingMessage), typeof(string), typeof(ModernLoadingIndicator), "Cargando...");

    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(ModernLoadingIndicator), 0.0);

    public static readonly BindableProperty ShowProgressProperty =
        BindableProperty.Create(nameof(ShowProgress), typeof(bool), typeof(ModernLoadingIndicator), false);

    public string LoadingMessage
    {
        get => (string)GetValue(LoadingMessageProperty);
        set => SetValue(LoadingMessageProperty, value);
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public bool ShowProgress
    {
        get => (bool)GetValue(ShowProgressProperty);
        set => SetValue(ShowProgressProperty, value);
    }

    public ModernLoadingIndicator()
    {
        InitializeComponent();
        _animationCancellationToken = new CancellationTokenSource();
    }

    /// <summary>
    /// Muestra el indicador de carga con animación
    /// </summary>
    public async Task ShowAsync(string message = null, bool showProgress = false)
    {
        if (_isAnimating) return;

        _isAnimating = true;
        
        if (!string.IsNullOrEmpty(message))
            LoadingMessage = message;
            
        ShowProgress = showProgress;
        
        LoadingContainer.IsVisible = true;
        await LoadingContainer.FadeInAsync(300);
        
        // Iniciar animación del spinner
        _ = StartSpinnerAnimationAsync();
    }

    /// <summary>
    /// Oculta el indicador de carga con animación
    /// </summary>
    public async Task HideAsync()
    {
        if (!_isAnimating) return;

        _isAnimating = false;
        
        // Detener animación del spinner
        _animationCancellationToken.Cancel();
        _animationCancellationToken = new CancellationTokenSource();
        
        await LoadingContainer.FadeOutAsync(300);
        LoadingContainer.IsVisible = false;
    }

    /// <summary>
    /// Actualiza el progreso
    /// </summary>
    public void UpdateProgress(double progress)
    {
        Progress = Math.Max(0, Math.Min(1, progress));
    }

    /// <summary>
    /// Animación continua del spinner
    /// </summary>
    private async Task StartSpinnerAnimationAsync()
    {
        try
        {
            while (_isAnimating && !_animationCancellationToken.Token.IsCancellationRequested)
            {
                await SpinnerImage.RotateTo(360, 1000, Easing.Linear);
                SpinnerImage.Rotation = 0; // Reset para evitar overflow
                
                if (_animationCancellationToken.Token.IsCancellationRequested)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Animación cancelada, es normal
        }
    }

    /// <summary>
    /// Muestra mensaje de éxito
    /// </summary>
    public async Task ShowSuccessAsync(string message = "¡Completado!")
    {
        LoadingMessage = message;
        await SpinnerFrame.SuccessAnimationAsync();
        await Task.Delay(1000); // Mostrar por 1 segundo
        await HideAsync();
    }

    /// <summary>
    /// Muestra mensaje de error
    /// </summary>
    public async Task ShowErrorAsync(string message = "Error")
    {
        LoadingMessage = message;
        await SpinnerFrame.ShakeAsync();
        await Task.Delay(2000); // Mostrar por 2 segundos
        await HideAsync();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        
        if (Handler == null)
        {
            _animationCancellationToken?.Cancel();
            _animationCancellationToken?.Dispose();
        }
    }
} 