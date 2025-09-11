using Bunit;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class PromocionListTests : TestContext
    {
        [Fact]
        public void Renderizar_ConPromocionesNull_DeberiaMostrarSpinner()
        {
            // Arrange
            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, (List<PromocionDto>?)null));

            // Act & Assert
            Assert.NotNull(componente.Find(".spinner-border"));
            Assert.Contains("Cargando promociones...", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConListaVacia_DeberiaMostrarMensajeVacio()
        {
            // Arrange
            var promociones = new List<PromocionDto>();

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("No hay promociones", componente.Markup);
            Assert.Contains("Crea tu primera promoción para empezar a atraer clientes", componente.Markup);
            Assert.Contains("Crear Promoción", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConPromociones_DeberiaMostrarTabla()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Descuento 20%",
                    Codigo = "DESC20",
                    Tipo = TipoPromocion.Porcentaje,
                    ValorDescuento = 20,
                    FechaInicio = DateTime.Now,
                    FechaFin = DateTime.Now.AddDays(30),
                    EstaActiva = true,
                    UsosRealizados = 5
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.NotNull(componente.Find("table"));
            Assert.NotNull(componente.Find("thead"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEncabezadosTabla()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("Nombre", componente.Markup);
            Assert.Contains("Código", componente.Markup);
            Assert.Contains("Tipo", componente.Markup);
            Assert.Contains("Descuento", componente.Markup);
            Assert.Contains("Período", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Usos", componente.Markup);
            Assert.Contains("Acciones", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionPromocion()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Descuento 20%",
                    Descripcion = "Descuento especial para clientes VIP",
                    Codigo = "DESC20",
                    Tipo = TipoPromocion.Porcentaje,
                    ValorDescuento = 20,
                    ValorMinimoCompra = 100,
                    FechaInicio = new DateTime(2024, 1, 1),
                    FechaFin = new DateTime(2024, 1, 31),
                    EstaActiva = true,
                    UsosRealizados = 5,
                    CantidadMaximaUsos = 100
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("Descuento 20%", componente.Markup);
            Assert.Contains("Descuento especial para clientes VIP", componente.Markup);
            Assert.Contains("DESC20", componente.Markup);
            Assert.Contains("20%", componente.Markup);
            Assert.Contains("Compra mín: $100", componente.Markup);
            Assert.Contains("01/01/2024", componente.Markup);
            Assert.Contains("31/01/2024", componente.Markup);
            Assert.Contains("5", componente.Markup);
            Assert.Contains("/ 100", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTipoPromocion()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Tipo = TipoPromocion.Porcentaje
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("Porcentaje", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarDescuentoPorcentaje()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Tipo = TipoPromocion.Porcentaje,
                    ValorDescuento = 25
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("25%", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarDescuentoMontoFijo()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Tipo = TipoPromocion.MontoFijo,
                    ValorDescuento = 50
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("$50", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadgesEstado()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    EstaActiva = true
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act
            var badges = componente.FindAll(".badge");

            // Assert
            Assert.True(badges.Count >= 2); // Tipo y Estado
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    EstaActiva = true
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act
            var botones = componente.FindAll("button");

            // Assert
            Assert.True(botones.Count >= 3); // Editar, Activar/Desactivar, Eliminar
            Assert.Contains("Editar", componente.Markup);
            Assert.Contains("Eliminar", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesHeader()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("Filtrar", componente.Markup);
            Assert.Contains("Nueva Promoción", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarContadorPromociones()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test 1" },
                new() { Id = Guid.NewGuid(), Nombre = "Test 2" }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("Promociones (2", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConPaginacion_DeberiaMostrarPaginacion()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var paginacion = new PaginatedList<PromocionDto>
            {
                Items = promociones,
                PageSize = 10,
                PageNumber = 1,
                TotalPages = 5,
                TotalCount = 50
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones)
                .Add(p => p.Paginacion, paginacion));

            // Act & Assert
            Assert.NotNull(componente.Find(".pagination"));
            Assert.Contains("Anterior", componente.Markup);
            Assert.Contains("Siguiente", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Codigo = "TEST"
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi-badge"));
            Assert.NotNull(componente.Find("i.oi-pencil"));
            Assert.NotNull(componente.Find("i.oi-trash"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraCard()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTablaResponsive()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-responsive"));
            Assert.NotNull(componente.Find(".table.table-hover"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarGrupoBotones()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    EstaActiva = true
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.NotNull(componente.Find(".btn-group"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTitulosBotones()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    EstaActiva = true
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("title=\"Editar\"", componente.Markup);
            Assert.Contains("title=\"Eliminar\"", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCodigoEnCode()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Codigo = "DESC20"
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.NotNull(componente.Find("code"));
            Assert.Contains("DESC20", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarUsosInfinitos()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    UsosRealizados = 5,
                    CantidadMaximaUsos = null
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("5", componente.Markup);
            Assert.Contains("/ ∞", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBarraProgreso()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    UsosRealizados = 5,
                    CantidadMaximaUsos = 100
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.NotNull(componente.Find(".progress"));
            Assert.NotNull(componente.Find(".progress-bar"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesToggleActivar()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    EstaActiva = true
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("Desactivar", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFechasFormateadas()
        {
            // Arrange
            var fechaInicio = new DateTime(2024, 1, 15);
            var fechaFin = new DateTime(2024, 2, 15);
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("15/01/2024", componente.Markup);
            Assert.Contains("15/02/2024", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarValorMinimoCompra()
        {
            // Arrange
            var promociones = new List<PromocionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    ValorMinimoCompra = 150
                }
            };

            var componente = RenderComponent<PromocionList>(parameters => parameters
                .Add(p => p.Promociones, promociones));

            // Act & Assert
            Assert.Contains("Compra mín: $150", componente.Markup);
        }
    }
}
