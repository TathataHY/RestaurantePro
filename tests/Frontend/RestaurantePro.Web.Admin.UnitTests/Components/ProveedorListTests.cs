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
    public class ProveedorListTests : TestContext
    {
        [Fact]
        public void Renderizar_ConProveedoresNull_DeberiaMostrarMensajeVacio()
        {
            // Arrange
            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, (List<ProveedorDto>?)null));

            // Act & Assert
            Assert.Contains("No hay proveedores", componente.Markup);
            Assert.Contains("No se encontraron proveedores con los filtros aplicados", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConListaVacia_DeberiaMostrarMensajeVacio()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>();

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("No hay proveedores", componente.Markup);
            Assert.Contains("No se encontraron proveedores con los filtros aplicados", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProveedores_DeberiaMostrarTabla()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Proveedor Test",
                    Ruc = "12345678901",
                    Ciudad = "Lima",
                    Pais = "Perú",
                    Telefono = "555-1234",
                    Email = "test@proveedor.com",
                    FechaCreacion = DateTime.Now,
                    TotalOrdenesCompra = 10,
                    MontoTotalCompras = 5000,
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.NotNull(componente.Find("table"));
            Assert.NotNull(componente.Find("thead"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEncabezadosTabla()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("Proveedor", componente.Markup);
            Assert.Contains("RUC", componente.Markup);
            Assert.Contains("Ubicación", componente.Markup);
            Assert.Contains("Contacto", componente.Markup);
            Assert.Contains("Rendimiento", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Acciones", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionProveedor()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Proveedor Test",
                    Ruc = "12345678901",
                    Ciudad = "Lima",
                    Pais = "Perú",
                    Telefono = "555-1234",
                    Email = "test@proveedor.com",
                    FechaCreacion = new DateTime(2024, 1, 15),
                    TotalOrdenesCompra = 10,
                    MontoTotalCompras = 5000,
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("Proveedor Test", componente.Markup);
            Assert.Contains("12345678901", componente.Markup);
            Assert.Contains("Lima", componente.Markup);
            Assert.Contains("Perú", componente.Markup);
            Assert.Contains("555-1234", componente.Markup);
            Assert.Contains("test@proveedor.com", componente.Markup);
            Assert.Contains("15/01/2024", componente.Markup);
            Assert.Contains("10", componente.Markup);
            Assert.Contains("$5,000", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarAvatarConPrimeraLetra()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Proveedor Test"
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("P", componente.Markup); // Primera letra del nombre
        }

        [Fact]
        public void Renderizar_DeberiaMostrarRuc()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Ruc = "12345678901"
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("12345678901", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSinRuc()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Ruc = ""
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("Sin RUC", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarUbicacion()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Ciudad = "Lima",
                    Pais = "Perú"
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("Lima", componente.Markup);
            Assert.Contains("Perú", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSinUbicacion()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Ciudad = "",
                    Pais = ""
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("Sin ubicación", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarContacto()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Telefono = "555-1234",
                    Email = "test@proveedor.com"
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("555-1234", componente.Markup);
            Assert.Contains("test@proveedor.com", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSinContacto()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Telefono = "",
                    Email = ""
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("Sin contacto", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarRendimiento()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    TotalOrdenesCompra = 15,
                    MontoTotalCompras = 7500
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("15", componente.Markup);
            Assert.Contains("$7,500", componente.Markup);
            Assert.Contains("Órdenes", componente.Markup);
            Assert.Contains("Total", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstadoActivo()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("Activo", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstadoInactivo()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    EstaActivo = false
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("Inactivo", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test"
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act
            var botones = componente.FindAll("button");

            // Assert
            Assert.True(botones.Count >= 4); // Ver, Editar, Gestionar contactos, Eliminar
            Assert.Contains("Ver detalles", componente.Markup);
            Assert.Contains("Editar", componente.Markup);
            Assert.Contains("Gestionar contactos", componente.Markup);
            Assert.Contains("Eliminar", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConPaginacion_DeberiaMostrarPaginacion()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var paginacion = new PaginatedList<ProveedorDto>
            {
                Items = proveedores,
                PageSize = 10,
                PageNumber = 1,
                TotalPages = 5,
                TotalCount = 50
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores)
                .Add(p => p.Paginacion, paginacion));

            // Act & Assert
            Assert.NotNull(componente.Find(".pagination"));
            Assert.Contains("Mostrando 1 a 10 de 50 proveedores", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Telefono = "555-1234",
                    Email = "test@proveedor.com"
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi-people"));
            Assert.NotNull(componente.Find("i.oi-phone"));
            Assert.NotNull(componente.Find("i.oi-envelope-closed"));
            Assert.NotNull(componente.Find("i.oi-eye"));
            Assert.NotNull(componente.Find("i.oi-pencil"));
            Assert.NotNull(componente.Find("i.oi-trash"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTablaResponsive()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-responsive"));
            Assert.NotNull(componente.Find(".table.table-hover"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarGrupoBotones()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test"
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.NotNull(componente.Find(".btn-group"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTitulosBotones()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test"
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("title=\"Ver detalles\"", componente.Markup);
            Assert.Contains("title=\"Editar\"", componente.Markup);
            Assert.Contains("title=\"Gestionar contactos\"", componente.Markup);
            Assert.Contains("title=\"Eliminar\"", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadges()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Ruc = "12345678901",
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act
            var badges = componente.FindAll(".badge");

            // Assert
            Assert.True(badges.Count >= 2); // RUC y Estado
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFechaFormateada()
        {
            // Arrange
            var fecha = new DateTime(2024, 1, 15);
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    FechaCreacion = fecha
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("15/01/2024", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarMontoFormateado()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    MontoTotalCompras = 12345
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("$12,345", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraTabla()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.NotNull(componente.Find("thead.table-dark"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarAvatarCircular()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test"
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.NotNull(componente.Find(".bg-primary.text-white.rounded-circle"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionRendimiento()
        {
            // Arrange
            var proveedores = new List<ProveedorDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    TotalOrdenesCompra = 25,
                    MontoTotalCompras = 15000
                }
            };

            var componente = RenderComponent<ProveedorList>(parameters => parameters
                .Add(p => p.Proveedores, proveedores));

            // Act & Assert
            Assert.Contains("25", componente.Markup);
            Assert.Contains("$15,000", componente.Markup);
            Assert.Contains("Órdenes", componente.Markup);
            Assert.Contains("Total", componente.Markup);
        }
    }
}
