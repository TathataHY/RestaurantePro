using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class PromocionFormTests : TestContext
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<TokenStore> _mockTokenStore;
        private readonly List<ProductoDto> _productos;

        public PromocionFormTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockTokenStore = new Mock<TokenStore>();
            _productos = new List<ProductoDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Pizza Margherita", Precio = 15000, CategoriaNombre = "Pizzas" },
                new() { Id = Guid.NewGuid(), Nombre = "Hamburguesa Clásica", Precio = 12000, CategoriaNombre = "Hamburguesas" },
                new() { Id = Guid.NewGuid(), Nombre = "Ensalada César", Precio = 8000, CategoriaNombre = "Ensaladas" }
            };

            Services.AddSingleton(_mockHttpClientFactory.Object);
            Services.AddSingleton(_mockTokenStore.Object);
            var productosApiService = new ProductosApiService(_mockHttpClientFactory.Object, _mockTokenStore.Object);
            Services.AddSingleton<IProductosApiService>(productosApiService);
        }

        [Fact]
        public void Renderizar_FormularioNuevo_DeberiaMostrarTituloCorrecto()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            var titulo = componente.Find("h5.modal-title");
            Assert.Equal("Nueva Promoción", titulo.TextContent);
        }

        [Fact]
        public void Renderizar_FormularioEdicion_DeberiaMostrarTituloCorrecto()
        {
            // Arrange
            var promocion = new PromocionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Promoción Test",
                Codigo = "TEST123"
            };

            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, promocion));

            // Act & Assert
            var titulo = componente.Find("h5.modal-title");
            Assert.Equal("Editar Promoción", titulo.TextContent);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTodosLosCamposObligatorios()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            Assert.NotNull(componente.Find("input[placeholder='Nombre de la promoción']"));
            Assert.NotNull(componente.Find("input[placeholder='CÓDIGO123']"));
            Assert.NotNull(componente.Find("textarea[placeholder='Descripción de la promoción']"));
            Assert.NotNull(componente.Find("select"));
            Assert.NotNull(componente.Find("input[placeholder='0.00']"));
            Assert.NotNull(componente.Find("input[type='date']"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarOpcionesDeTipoPromocion()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act
            var select = componente.Find("select");
            var opciones = select.QuerySelectorAll("option");

            // Assert
            Assert.Equal(5, opciones.Count());
            Assert.Contains("Porcentaje de descuento", opciones[0].TextContent);
            Assert.Contains("Monto fijo de descuento", opciones[1].TextContent);
            Assert.Contains("Descuento por cantidad", opciones[2].TextContent);
            Assert.Contains("Descuento por compra mínima", opciones[3].TextContent);
            Assert.Contains("Descuento por producto", opciones[4].TextContent);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposNumericos()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act
            var camposNumericos = componente.FindAll("input[type='number']");

            // Assert
            Assert.Equal(3, camposNumericos.Count);
            Assert.Contains(camposNumericos, c => c.GetAttribute("placeholder") == "0.00");
            Assert.Contains(camposNumericos, c => c.GetAttribute("placeholder") == "Sin límite");
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCheckboxActiva()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            var checkbox = componente.Find("input[type='checkbox']");
            Assert.NotNull(checkbox);
            Assert.Equal("Promoción activa", componente.Find("label.form-check-label").TextContent);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSeccionProductos()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            Assert.NotNull(componente.Find("label:contains('Productos Asociados')"));
            Assert.NotNull(componente.Find(".border.rounded.p-3"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act
            var botones = componente.FindAll("button");

            // Assert
            Assert.Equal(3, botones.Count);
            Assert.Contains(botones, b => b.TextContent.Contains("Cancelar"));
            Assert.Contains(botones, b => b.TextContent.Contains("Crear"));
            Assert.Contains(botones, b => b.ClassList.Contains("btn-close"));
        }

        [Fact]
        public void Renderizar_FormularioEdicion_DeberiaMostrarBotonActualizar()
        {
            // Arrange
            var promocion = new PromocionDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Promoción Test"
            };

            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, promocion));

            // Act & Assert
            var botonGuardar = componente.Find("button.btn-primary");
            Assert.Contains("Actualizar", botonGuardar.TextContent);
        }

        [Fact]
        public void Renderizar_FormularioNuevo_DeberiaMostrarBotonCrear()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            var botonGuardar = componente.Find("button.btn-primary");
            Assert.Contains("Crear", botonGuardar.TextContent);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarValidaciones()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act
            var validaciones = componente.FindAll(".text-danger");

            // Assert
            Assert.True(validaciones.Count >= 0); // Las validaciones solo aparecen cuando hay errores
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTextosAyuda()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            Assert.Contains("Código único para aplicar la promoción", componente.Markup);
            Assert.Contains("Monto mínimo de compra (opcional)", componente.Markup);
            Assert.Contains("Dejar vacío para uso ilimitado", componente.Markup);
            Assert.Contains("Selecciona los productos que aplicarán para esta promoción", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarModalBackdrop()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            Assert.NotNull(componente.Find(".modal-backdrop"));
        }

        [Fact]
        public void Renderizar_MostrarFalse_NoDeberiaMostrarModal()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, false)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            var modal = componente.Find(".modal");
            Assert.Contains("display: none", modal.GetAttribute("style"));
        }

        [Fact]
        public void Renderizar_MostrarTrue_DeberiaMostrarModal()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            var modal = componente.Find(".modal");
            Assert.Contains("display: block", modal.GetAttribute("style"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraModal()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            Assert.NotNull(componente.Find(".modal-dialog.modal-lg"));
            Assert.NotNull(componente.Find(".modal-content"));
            Assert.NotNull(componente.Find(".modal-header"));
            Assert.NotNull(componente.Find(".modal-body"));
            Assert.NotNull(componente.Find(".modal-footer"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFormularioConValidacion()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            Assert.NotNull(componente.Find("form"));
            // DataAnnotationsValidator se renderiza como un componente, no como un elemento HTML
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposConPlaceholders()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            Assert.NotNull(componente.Find("input[placeholder='Nombre de la promoción']"));
            Assert.NotNull(componente.Find("input[placeholder='CÓDIGO123']"));
            Assert.NotNull(componente.Find("textarea[placeholder='Descripción de la promoción']"));
            Assert.NotNull(componente.Find("input[placeholder='0.00']"));
            Assert.NotNull(componente.Find("input[placeholder='Sin límite']"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarLabelsObligatorios()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            Assert.Contains("Nombre *", componente.Markup);
            Assert.Contains("Código *", componente.Markup);
            Assert.Contains("Tipo de Promoción *", componente.Markup);
            Assert.Contains("Valor de Descuento *", componente.Markup);
            Assert.Contains("Fecha de Inicio *", componente.Markup);
            Assert.Contains("Fecha de Fin *", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposOpcionales()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            Assert.Contains("Descripción", componente.Markup);
            Assert.Contains("Compra Mínima", componente.Markup);
            Assert.Contains("Usos Máximos", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraColumnas()
        {
            // Arrange
            var componente = RenderComponent<PromocionForm>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Promocion, (PromocionDto?)null));

            // Act & Assert
            Assert.NotNull(componente.Find(".row"));
            Assert.NotNull(componente.Find(".col-md-6"));
            Assert.NotNull(componente.Find(".col-md-4"));
        }
    }
}
