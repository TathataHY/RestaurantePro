using RestaurantePro.Mobile.Core.Features.Operations.Comandas.ViewModels;
using RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;

namespace RestaurantePro.Mobile.Features.Operations.Comandas.Pages;

/// <summary>
/// Página para crear una nueva comanda
/// </summary>
public partial class CrearComandaPage : ContentPage, IQueryAttributable
{
    private readonly CrearComandaViewModel _viewModel;

    public CrearComandaPage(CrearComandaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Implementación de IQueryAttributable para recibir parámetros de navegación
    /// </summary>
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // Modo creación: llega mesaId. Modo edición: llega comandaId.
        if (query.TryGetValue("comandaId", out var comandaIdObj) && !string.IsNullOrEmpty(comandaIdObj?.ToString()))
        {
            await _viewModel.InitializeEdicionAsync(comandaIdObj.ToString()!);
            return;
        }

        if (query.TryGetValue("mesaId", out var mesaIdObj) && !string.IsNullOrEmpty(mesaIdObj?.ToString()))
        {
            await _viewModel.InitializeAsync(mesaIdObj.ToString()!);
        }
        else
        {
            // Si no recibimos mesaId, iniciar el flujo pidiendo seleccionar mesa
            await _viewModel.InitializeAsync(string.Empty);
        }
    }

    /// <summary>
    /// Se ejecuta cuando la página aparece
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadPreparacionesDiaCommand.ExecuteAsync(null);
    }

    #region Event Handlers para botones de productos disponibles

    private void OnDecrementarCantidadClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ProductoCarritoDto producto)
        {
            _viewModel.DecrementarCantidadCommand.Execute(producto);
        }
    }

    private void OnIncrementarCantidadClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ProductoCarritoDto producto)
        {
            _viewModel.IncrementarCantidadCommand.Execute(producto);
        }
    }

    private void OnAgregarAlCarritoClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ProductoCarritoDto producto)
        {
            _viewModel.AgregarAlCarritoCommand.Execute(producto);
        }
    }

    #endregion

    #region Event Handlers para botones del carrito

    private void OnDecrementarCantidadCarritoClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ProductoCarritoDto producto)
        {
            _viewModel.DecrementarCantidadCarritoCommand.Execute(producto);
        }
    }

    private void OnIncrementarCantidadCarritoClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ProductoCarritoDto producto)
        {
            _viewModel.IncrementarCantidadCarritoCommand.Execute(producto);
        }
    }

    private void OnEliminarDelCarritoClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ProductoCarritoDto producto)
        {
            _viewModel.EliminarDelCarritoCommand.Execute(producto);
        }
    }

    #endregion
}
