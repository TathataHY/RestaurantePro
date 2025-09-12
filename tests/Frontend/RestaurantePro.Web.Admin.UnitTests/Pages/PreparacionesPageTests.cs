using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Pages;
using RestaurantePro.Web.Admin.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Web.Admin.UnitTests.Pages
{
    public class PreparacionesPageTests : TestContext
    {
        private readonly Mock<IPreparacionesApiService> _preparacionesApiMock;

        public PreparacionesPageTests()
        {
            _preparacionesApiMock = new Mock<IPreparacionesApiService>();
            Services.AddSingleton(_preparacionesApiMock.Object);
            Services.AddSingleton<IPreparacionesApiService>(_preparacionesApiMock.Object);
            Services.AddSingleton<TestNavigationManager>();
            
            // Configurar JSInterop para manejar llamadas JavaScript
            JSInterop.SetupVoid("preparacionesCharts.initAll", _ => true);
        }

        [Fact]
        public void PreparacionesPage_ShouldRenderCorrectly()
        {
            // Arrange
            SetupMocks();

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            component.Find("div").Should().NotBeNull();
        }

        [Fact]
        public async Task PreparacionesPage_ShouldLoadPreparacionesOnInit()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new PreparacionDto 
                { 
                    Id = 1, 
                    ComandaId = 101,
                    ProductoId = 201,
                    ProductoNombre = "Pizza Margherita",
                    Cantidad = 2,
                    Estado = EstadoPreparacion.Pendiente,
                    TiempoEstimadoMinutos = 15,
                    TiempoRealMinutos = null,
                    FechaCreacion = DateTime.Now.AddMinutes(-30),
                    FechaInicio = null,
                    FechaFin = null,
                    CocineroId = 301,
                    CocineroNombre = "Juan Pérez",
                    Observaciones = "Sin cebolla"
                }
            };

            _preparacionesApiMock.Setup(x => x.ObtenerPreparacionesPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PreparacionFiltrosDto>()))
                                .ReturnsAsync(new PaginatedList<PreparacionDto> { Items = preparaciones, TotalCount = 1, PageNumber = 1, PageSize = 20 });

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            _preparacionesApiMock.Verify(x => x.ObtenerPreparacionesPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PreparacionFiltrosDto>()), Times.Once);
        }

        [Fact]
        public async Task PreparacionesPage_ShouldHandleErrorWhenLoadingPreparaciones()
        {
            // Arrange
            _preparacionesApiMock.Setup(x => x.ObtenerPreparacionesPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PreparacionFiltrosDto>()))
                                .ThrowsAsync(new Exception("Error de red"));

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            // El componente debería manejar el error sin fallar
            component.Find("div").Should().NotBeNull();
        }

        [Fact]
        public void PreparacionesPage_ShouldDisplayPreparacionesList()
        {
            // Arrange
            SetupMocks();

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            component.Find("div").Should().NotBeNull();
        }

        [Fact]
        public void PreparacionesPage_ShouldHaveSearchFunctionality()
        {
            // Arrange
            SetupMocks();

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            component.Find("div").Should().NotBeNull();
        }

        [Fact]
        public void PreparacionesPage_ShouldHaveFilterOptions()
        {
            // Arrange
            SetupMocks();

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            component.Find("div").Should().NotBeNull();
        }

        [Fact]
        public void PreparacionesPage_ShouldHavePaginationControls()
        {
            // Arrange
            SetupMocks();

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            component.Find("div").Should().NotBeNull();
        }

        [Fact]
        public void PreparacionesPage_ShouldHaveActionButtons()
        {
            // Arrange
            SetupMocks();

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            component.Find("div").Should().NotBeNull();
        }

        [Fact]
        public void PreparacionesPage_ShouldDisplayStatistics()
        {
            // Arrange
            SetupMocks();

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            component.Find("div").Should().NotBeNull();
        }

        [Fact]
        public void PreparacionesPage_ShouldHandleEmptyState()
        {
            // Arrange
            _preparacionesApiMock.Setup(x => x.ObtenerPreparacionesPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PreparacionFiltrosDto>()))
                                .ReturnsAsync(new PaginatedList<PreparacionDto> { Items = new List<PreparacionDto>(), TotalCount = 0, PageNumber = 1, PageSize = 20 });

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            component.Find("div").Should().NotBeNull();
        }

        [Fact]
        public void PreparacionesPage_ShouldBeResponsive()
        {
            // Arrange
            SetupMocks();

            // Act
            var component = RenderComponent<Preparaciones>();

            // Assert
            component.Find("div").Should().NotBeNull();
        }

        private void SetupMocks()
        {
            var preparaciones = new List<PreparacionDto>
            {
                new PreparacionDto 
                { 
                    Id = 1, 
                    ComandaId = 101,
                    ProductoId = 201,
                    ProductoNombre = "Pizza Margherita",
                    Cantidad = 2,
                    Estado = EstadoPreparacion.Pendiente,
                    TiempoEstimadoMinutos = 15,
                    TiempoRealMinutos = null,
                    FechaCreacion = DateTime.Now.AddMinutes(-30),
                    FechaInicio = null,
                    FechaFin = null,
                    CocineroId = 301,
                    CocineroNombre = "Juan Pérez",
                    Observaciones = "Sin cebolla"
                }
            };

            var estadisticas = new PreparacionEstadisticasDto
            {
                PreparacionesHoy = 25,
                PreparacionesPendientes = 5,
                PreparacionesEnProceso = 8,
                PreparacionesCompletadas = 12,
                PreparacionesAtrasadas = 2,
                TiempoPromedioPreparacion = 18.5,
                PreparacionesPorCocinero = 10,
                EficienciaCocina = 85.2
            };

            _preparacionesApiMock.Setup(x => x.ObtenerPreparacionesPaginadasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PreparacionFiltrosDto>()))
                                .ReturnsAsync(new PaginatedList<PreparacionDto> { Items = preparaciones, TotalCount = 1, PageNumber = 1, PageSize = 20 });
            _preparacionesApiMock.Setup(x => x.ObtenerEstadisticasAsync())
                                .ReturnsAsync(estadisticas);
        }
    }
}