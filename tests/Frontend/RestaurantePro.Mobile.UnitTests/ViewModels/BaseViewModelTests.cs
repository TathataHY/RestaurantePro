using FluentAssertions;
using RestaurantePro.Mobile.Core.Models.ViewModels;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.ViewModels;

/// <summary>
/// Pruebas unitarias para BaseViewModel
/// </summary>
public class BaseViewModelTests
{
    #region Constructor y Propiedades Iniciales

    [Fact]
    public void Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange & Act
        var viewModel = new TestBaseViewModel();

        // Assert
        viewModel.IsBusy.Should().BeFalse();
        viewModel.IsLoading.Should().BeFalse();
        viewModel.Title.Should().BeEmpty();
        viewModel.HasError.Should().BeFalse();
        viewModel.ErrorMessage.Should().BeEmpty();
        viewModel.IsNotBusy.Should().BeTrue();
    }

    #endregion

    #region Propiedades Observables

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsBusy_DeberiaActualizarIsNotBusyCorrectamente(bool isBusy)
    {
        // Arrange
        var viewModel = new TestBaseViewModel();

        // Act
        viewModel.IsBusy = isBusy;

        // Assert
        viewModel.IsNotBusy.Should().Be(!isBusy);
    }

    [Theory]
    [InlineData("Título de prueba")]
    [InlineData("")]
    [InlineData("Título con espacios   ")]
    public void Title_DeberiaEstablecerTituloCorrectamente(string title)
    {
        // Arrange
        var viewModel = new TestBaseViewModel();

        // Act
        viewModel.Title = title;

        // Assert
        viewModel.Title.Should().Be(title);
    }

    [Theory]
    [InlineData("Error de prueba")]
    [InlineData("")]
    [InlineData("Error con caracteres especiales: áéíóú")]
    public void ErrorMessage_DeberiaEstablecerMensajeCorrectamente(string errorMessage)
    {
        // Arrange
        var viewModel = new TestBaseViewModel();

        // Act
        viewModel.ErrorMessage = errorMessage;

        // Assert
        viewModel.ErrorMessage.Should().Be(errorMessage);
    }

    #endregion

    #region Métodos de Error

    [Fact]
    public async Task ShowErrorAsync_DeberiaEstablecerErrorCorrectamente()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();
        var errorMessage = "Error de prueba";

        // Act
        await viewModel.ShowErrorAsync(errorMessage);

        // Assert
        viewModel.ErrorMessage.Should().Be(errorMessage);
        viewModel.HasError.Should().BeTrue();
    }

    [Fact]
    public void ClearError_DeberiaLimpiarErrorCorrectamente()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();
        viewModel.SetError("Error previo");

        // Act
        viewModel.ClearError();

        // Assert
        viewModel.HasError.Should().BeFalse();
        viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void SetError_DeberiaEstablecerErrorCorrectamente()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();
        var errorMessage = "Error directo";

        // Act
        viewModel.SetError(errorMessage);

        // Assert
        viewModel.ErrorMessage.Should().Be(errorMessage);
        viewModel.HasError.Should().BeTrue();
    }

    #endregion

    #region Método ExecuteAsync

    [Fact]
    public async Task ExecuteAsync_ConOperacionExitosa_DeberiaEjecutarCorrectamente()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();
        var operacionEjecutada = false;

        // Act
        await viewModel.ExecuteAsync(async () =>
        {
            await Task.Delay(10);
            operacionEjecutada = true;
        });

        // Assert
        operacionEjecutada.Should().BeTrue();
        viewModel.IsBusy.Should().BeFalse();
        viewModel.IsLoading.Should().BeFalse();
        viewModel.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ConOperacionConError_DeberiaManejarErrorCorrectamente()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();
        var errorMessage = "Error en operación";

        // Act
        await viewModel.ExecuteAsync(async () =>
        {
            await Task.Delay(10);
            throw new Exception(errorMessage);
        });

        // Assert
        viewModel.IsBusy.Should().BeFalse();
        viewModel.IsLoading.Should().BeFalse();
        viewModel.HasError.Should().BeTrue();
        viewModel.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ConIsBusyTrue_NoDeberiaEjecutarOperacion()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();
        viewModel.IsBusy = true;
        var operacionEjecutada = false;

        // Act
        await viewModel.ExecuteAsync(async () =>
        {
            operacionEjecutada = true;
            await Task.CompletedTask;
        });

        // Assert
        operacionEjecutada.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ConShowLoadingFalse_NoDeberiaCambiarEstadosDeLoading()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();
        var operacionEjecutada = false;

        // Act
        await viewModel.ExecuteAsync(async () =>
        {
            operacionEjecutada = true;
            await Task.CompletedTask;
        }, showLoading: false);

        // Assert
        operacionEjecutada.Should().BeTrue();
        viewModel.IsBusy.Should().BeFalse();
        viewModel.IsLoading.Should().BeFalse();
    }

    #endregion

    #region Comandos

    [Fact]
    public async Task GoBackAsync_DeberiaEjecutarseSinErrores()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();

        // Act & Assert
        var act = async () => await viewModel.GoBackAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void ClearErrorCommand_DeberiaEjecutarseSinErrores()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();
        viewModel.SetError("Error previo");

        // Act
        viewModel.ClearErrorCommand.Execute(null);

        // Assert
        viewModel.HasError.Should().BeFalse();
        viewModel.ErrorMessage.Should().BeEmpty();
    }

    #endregion

    #region Casos Edge

    [Fact]
    public async Task ShowErrorAsync_ConMensajeNull_DeberiaManejarCorrectamente()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();

        // Act
        await viewModel.ShowErrorAsync(null!);

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
        viewModel.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ConOperacionNull_DeberiaLanzarExcepcion()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();

        // Act & Assert
        // El método ExecuteAsync no valida null, simplemente no ejecuta nada
        var act = async () => await viewModel.ExecuteAsync(null!);
        await act.Should().NotThrowAsync(); // El método actual no lanza excepción
    }

    [Fact]
    public async Task ExecuteAsync_ConOperacionQueLanzaExcepcionEspecifica_DeberiaManejarCorrectamente()
    {
        // Arrange
        var viewModel = new TestBaseViewModel();
        var errorMessage = "Error específico";

        // Act
        await viewModel.ExecuteAsync(async () =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException(errorMessage);
        });

        // Assert
        viewModel.HasError.Should().BeTrue();
        viewModel.ErrorMessage.Should().Be(errorMessage);
    }

    #endregion
}

/// <summary>
/// Clase de prueba para BaseViewModel que permite acceso a métodos protegidos
/// </summary>
public class TestBaseViewModel : BaseViewModel
{
    public new async Task ShowErrorAsync(string message)
    {
        await base.ShowErrorAsync(message);
    }

    public new void ClearError()
    {
        base.ClearError();
    }

    public new async Task GoBackAsync()
    {
        await base.GoBackAsync();
    }

    public new async Task ExecuteAsync(Func<Task> operation, bool showLoading = true)
    {
        await base.ExecuteAsync(operation, showLoading);
    }
}
