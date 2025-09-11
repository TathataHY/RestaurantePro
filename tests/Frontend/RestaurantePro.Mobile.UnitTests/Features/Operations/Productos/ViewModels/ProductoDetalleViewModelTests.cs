using AutoFixture;
using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Operations.Productos.ViewModels;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Dialog;
using RestaurantePro.Mobile.Core.Services.Navigation;
using RestaurantePro.Mobile.Core.Services.Productos;

namespace RestaurantePro.Mobile.UnitTests.Features.Operations.Productos.ViewModels;

public class ProductoDetalleViewModelTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IProductosService> _mockProductosService = new();
    private readonly Mock<INavigationService> _mockNavigationService = new();
    private readonly Mock<IDialogService> _mockDialogService = new();

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        
        vm.Title.Should().Be("Detalle de Producto");
        vm.Producto.Should().BeNull();
        vm.IsEditing.Should().BeFalse();
        vm.DescripcionEditada.Should().BeEmpty();
        vm.PrecioEditado.Should().Be(0);
        vm.CategoriaSeleccionada.Should().BeNull();
        vm.Categorias.Should().NotBeNull();
        vm.Categorias.Should().BeEmpty();
        vm.CantidadParaComanda.Should().Be(1);
        vm.ObservacionesComanda.Should().BeEmpty();
        vm.CanEdit.Should().BeFalse();
        vm.CanAddToComanda.Should().BeFalse();
        vm.CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task LoadProductoAsync_WithValidId_ShouldLoadProductoAndCategorias()
    {
        var productoId = Guid.NewGuid();
        var producto = new ProductoDto 
        { 
            Id = productoId, 
            Nombre = "Test Product", 
            CategoriaNombre = "Bebidas",
            Descripcion = "Test description",
            Precio = 10.50m,
            Activo = true,
            CantidadDisponible = 10
        };
        
        _mockProductosService.Setup(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.SuccessResponse(producto));
        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>()));

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        await vm.LoadProductoCommand.ExecuteAsync(productoId);

        vm.Producto.Should().Be(producto);
        vm.Title.Should().Be("Producto: Test Product - Bebidas");
        vm.CanEdit.Should().BeTrue();
        vm.CanAddToComanda.Should().BeTrue();
        vm.CanDelete.Should().BeTrue();
    }

    [Fact]
    public async Task LoadProductoAsync_WithErrorResponse_ShouldShowError()
    {
        var productoId = Guid.NewGuid();
        _mockProductosService.Setup(x => x.ObtenerProductoPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductoDto>.ErrorResponse(new List<string> { "Error" }, "Error", 500));

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        await vm.LoadProductoCommand.ExecuteAsync(productoId);

        _mockDialogService.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoadCategoriasAsync_WithValidResponse_ShouldLoadCategorias()
    {
        var categorias = new List<CategoriaProductoDto>
        {
            new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Bebidas" },
            new CategoriaProductoDto { Id = Guid.NewGuid(), Nombre = "Platos" }
        };
        
        _mockProductosService.Setup(x => x.ObtenerCategoriasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias));

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        await vm.LoadCategoriasCommand.ExecuteAsync(null);

        vm.Categorias.Should().HaveCount(2);
        vm.Categorias.Should().Contain(c => c.Nombre == "Bebidas");
        vm.Categorias.Should().Contain(c => c.Nombre == "Platos");
    }

    [Fact]
    public void StartEdit_WithValidProducto_ShouldEnableEditing()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product", 
            Descripcion = "Original description", 
            Precio = 15.00m 
        };

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.IsBusy = false;
        
        vm.StartEditCommand.Execute(null);

        vm.IsEditing.Should().BeTrue();
        vm.DescripcionEditada.Should().Be("Original description");
        vm.PrecioEditado.Should().Be(15.00m);
    }

    [Fact]
    public void StartEdit_WhenBusy_ShouldNotEnableEditing()
    {
        var producto = new ProductoDto { Nombre = "Test Product" };

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.IsBusy = true;
        
        vm.StartEditCommand.Execute(null);

        vm.IsEditing.Should().BeFalse();
    }

    [Fact]
    public void CancelEdit_ShouldDisableEditingAndResetValues()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product", 
            Descripcion = "Original description", 
            Precio = 15.00m 
        };

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.IsEditing = true;
        vm.DescripcionEditada = "Modified description";
        vm.PrecioEditado = 20.00m;
        
        vm.CancelEditCommand.Execute(null);

        vm.IsEditing.Should().BeFalse();
        vm.DescripcionEditada.Should().Be("Original description");
        vm.PrecioEditado.Should().Be(15.00m);
    }

    [Fact]
    public async Task SaveChangesAsync_WithValidData_ShouldSaveChanges()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product", 
            Descripcion = "Original description", 
            Precio = 15.00m 
        };

        _mockDialogService.Setup(x => x.ShowConfirmAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.IsEditing = true;
        vm.DescripcionEditada = "New description";
        vm.PrecioEditado = 20.00m;
        
        await vm.SaveChangesCommand.ExecuteAsync(null);

        vm.IsEditing.Should().BeFalse();
        vm.Producto.Descripcion.Should().Be("New description");
        vm.Producto.Precio.Should().Be(20.00m);
        _mockDialogService.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_WithEmptyDescription_ShouldShowError()
    {
        var producto = new ProductoDto { Nombre = "Test Product" };

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.IsEditing = true;
        vm.DescripcionEditada = "";
        vm.PrecioEditado = 20.00m;
        
        await vm.SaveChangesCommand.ExecuteAsync(null);

        vm.IsEditing.Should().BeTrue(); // No se guardó
        _mockDialogService.Verify(x => x.ShowAlertAsync(
            "Error", "La descripción no puede estar vacía", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_WithInvalidPrice_ShouldShowError()
    {
        var producto = new ProductoDto { Nombre = "Test Product" };

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.IsEditing = true;
        vm.DescripcionEditada = "Valid description";
        vm.PrecioEditado = 0;
        
        await vm.SaveChangesCommand.ExecuteAsync(null);

        vm.IsEditing.Should().BeTrue(); // No se guardó
        _mockDialogService.Verify(x => x.ShowAlertAsync(
            "Error", "El precio debe ser mayor a cero", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenUserCancels_ShouldNotSave()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product", 
            Descripcion = "Original description", 
            Precio = 15.00m 
        };

        _mockDialogService.Setup(x => x.ShowConfirmAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.IsEditing = true;
        vm.DescripcionEditada = "New description";
        vm.PrecioEditado = 20.00m;
        
        await vm.SaveChangesCommand.ExecuteAsync(null);

        vm.IsEditing.Should().BeTrue(); // No se guardó
        vm.Producto.Descripcion.Should().Be("Original description"); // No cambió
        vm.Producto.Precio.Should().Be(15.00m); // No cambió
    }

    [Fact]
    public async Task ToggleDisponibilidadAsync_WithConfirmation_ShouldToggleAvailability()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product", 
            Activo = true,
            CantidadDisponible = 10
        };

        _mockDialogService.Setup(x => x.ShowConfirmAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        
        await vm.ToggleDisponibilidadCommand.ExecuteAsync(null);

        _mockDialogService.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ToggleDisponibilidadAsync_WhenUserCancels_ShouldNotToggle()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product", 
            Activo = true,
            CantidadDisponible = 10
        };

        _mockDialogService.Setup(x => x.ShowConfirmAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        
        await vm.ToggleDisponibilidadCommand.ExecuteAsync(null);

        _mockDialogService.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AgregarAComandaAsync_WithValidQuantity_ShouldAddToComanda()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product", 
            Precio = 10.00m,
            Activo = true,
            CantidadDisponible = 10
        };

        _mockDialogService.Setup(x => x.ShowConfirmAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.CantidadParaComanda = 3;
        vm.ObservacionesComanda = "Sin hielo";
        
        await vm.AgregarAComandaCommand.ExecuteAsync(null);

        vm.CantidadParaComanda.Should().Be(1); // Se reseteó
        vm.ObservacionesComanda.Should().BeEmpty(); // Se reseteó
        _mockDialogService.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AgregarAComandaAsync_WithInvalidQuantity_ShouldShowError()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product",
            Activo = true,
            CantidadDisponible = 10
        };

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.CantidadParaComanda = 0;
        
        await vm.AgregarAComandaCommand.ExecuteAsync(null);

        _mockDialogService.Verify(x => x.ShowAlertAsync(
            "Error", "La cantidad debe ser mayor a cero", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AgregarAComandaAsync_WhenNotAvailable_ShouldNotAdd()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product",
            Activo = false,
            CantidadDisponible = 0
        };

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        
        await vm.AgregarAComandaCommand.ExecuteAsync(null);

        _mockDialogService.Verify(x => x.ShowConfirmAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task VerHistorialAsync_WithValidProducto_ShouldShowHistorial()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product",
            Popularidad = 8,
            FechaModificacion = DateTime.Now,
            TiempoPreparacion = 15,
            Activo = true,
            CantidadDisponible = 10
        };

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        
        await vm.VerHistorialCommand.ExecuteAsync(null);

        _mockDialogService.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task EliminarProductoAsync_WithConfirmation_ShouldDeleteProducto()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product" 
        };

        _mockDialogService.Setup(x => x.ShowConfirmAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        
        await vm.EliminarProductoCommand.ExecuteAsync(null);

        _mockDialogService.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _mockNavigationService.Verify(x => x.GoBackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarProductoAsync_WhenUserCancels_ShouldNotDelete()
    {
        var producto = new ProductoDto 
        { 
            Nombre = "Test Product" 
        };

        _mockDialogService.Setup(x => x.ShowConfirmAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        
        await vm.EliminarProductoCommand.ExecuteAsync(null);

        _mockDialogService.Verify(x => x.ShowAlertAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockNavigationService.Verify(x => x.GoBackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_WithValidProducto_ShouldReloadData()
    {
        var producto = new ProductoDto 
        { 
            Id = Guid.NewGuid(),
            Nombre = "Test Product" 
        };

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.IsBusy = false; // Asegurar que no esté ocupado
        
        await vm.RefreshCommand.ExecuteAsync(null);

        // Verificar que se mostró el mensaje de éxito
        _mockDialogService.Verify(x => x.ShowAlertAsync(
            "Éxito", "Datos actualizados correctamente", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WhenBusy_ShouldNotReload()
    {
        var producto = new ProductoDto { Id = Guid.NewGuid() };

        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = producto;
        vm.IsBusy = true;
        
        await vm.RefreshCommand.ExecuteAsync(null);

        _mockProductosService.Verify(x => x.ObtenerProductoPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Cleanup_ShouldClearAllData()
    {
        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        vm.Producto = new ProductoDto { Nombre = "Test" };
        vm.IsEditing = true;
        vm.Categorias.Add(new CategoriaProductoDto { Nombre = "Test" });
        vm.CantidadParaComanda = 5;
        vm.ObservacionesComanda = "Test";
        
        vm.Cleanup();
        
        vm.Producto.Should().BeNull();
        vm.IsEditing.Should().BeFalse();
        vm.Categorias.Should().BeEmpty();
        vm.CantidadParaComanda.Should().Be(1);
        vm.ObservacionesComanda.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true, false, true, 10, true, true)]   // Producto disponible, no ocupado
    [InlineData(true, true, true, 10, false, false)]  // Producto disponible, ocupado
    [InlineData(false, false, false, 0, false, false)] // Producto no disponible
    [InlineData(true, false, true, 0, true, false)]   // Producto activo pero sin stock
    public void CanEdit_CanAddToComanda_CanDelete_ShouldReturnCorrectValues(
        bool productoNotNull, bool isBusy, bool activo, int cantidadDisponible, bool expectedCanEdit, bool expectedCanAdd)
    {
        var vm = new ProductoDetalleViewModel(_mockProductosService.Object, _mockNavigationService.Object, _mockDialogService.Object);
        
        if (productoNotNull)
        {
            vm.Producto = new ProductoDto { Activo = activo, CantidadDisponible = cantidadDisponible };
        }
        vm.IsBusy = isBusy;
        
        vm.CanEdit.Should().Be(expectedCanEdit);
        vm.CanAddToComanda.Should().Be(expectedCanAdd);
        vm.CanDelete.Should().Be(expectedCanEdit);
    }
} 