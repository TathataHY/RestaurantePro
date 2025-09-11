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
    public class ClienteListTests : TestContext
    {
        [Fact]
        public void Renderizar_ConClientesNull_DeberiaMostrarSpinner()
        {
            // Arrange
            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, (List<ClienteDto>?)null));

            // Act & Assert
            Assert.NotNull(componente.Find(".spinner-border"));
            Assert.Contains("Cargando clientes...", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConListaVacia_DeberiaMostrarMensajeVacio()
        {
            // Arrange
            var clientes = new List<ClienteDto>();

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains("No hay clientes", componente.Markup);
            Assert.Contains("Comienza agregando tu primer cliente", componente.Markup);
            Assert.Contains("Agregar Cliente", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConClientes_DeberiaMostrarTabla()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    Email = "juan@email.com",
                    Telefono = "555-1234",
                    Ciudad = "Lima",
                    Pais = "Perú",
                    FechaNacimiento = DateTime.Today.AddYears(-30),
                    FechaRegistro = DateTime.Now.AddDays(-30),
                    TotalGastado = 1500,
                    TotalCompras = 5,
                    TotalVisitas = 8,
                    PromedioGasto = 300,
                    Segmento = "Frecuente",
                    PuntosFidelizacion = 150,
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.NotNull(componente.Find("table"));
            Assert.NotNull(componente.Find("thead"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEncabezadosTabla()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test", Apellidos = "User" }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains("Cliente", componente.Markup);
            Assert.Contains("Contacto", componente.Markup);
            Assert.Contains("Ubicación", componente.Markup);
            Assert.Contains("Estadísticas", componente.Markup);
            Assert.Contains("Segmento", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Acciones", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionCliente()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    Email = "juan@email.com",
                    Telefono = "555-1234",
                    Ciudad = "Lima",
                    Pais = "Perú",
                    FechaNacimiento = DateTime.Today.AddYears(-30),
                    FechaRegistro = DateTime.Now.AddDays(-30),
                    TotalGastado = 1500,
                    TotalCompras = 5,
                    TotalVisitas = 8,
                    PromedioGasto = 300,
                    Segmento = "Frecuente",
                    PuntosFidelizacion = 150,
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains("Juan Pérez", componente.Markup);
            Assert.Contains("juan@email.com", componente.Markup);
            Assert.Contains("555-1234", componente.Markup);
            Assert.Contains("Lima", componente.Markup);
            Assert.Contains("Perú", componente.Markup);
            Assert.Contains("30 años", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstadisticasCliente()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    TotalGastado = 1500,
                    TotalCompras = 5,
                    TotalVisitas = 8,
                    PromedioGasto = 300
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains("$1,500", componente.Markup);
            Assert.Contains("5 compras", componente.Markup);
            Assert.Contains("8 visitas", componente.Markup);
            Assert.Contains("$300", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSegmentoCliente()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    Segmento = "Frecuente",
                    PuntosFidelizacion = 150
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains("Frecuente", componente.Markup);
            Assert.Contains("150 pts", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstadoCliente()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    EstaActivo = true,
                    TotalGastado = 1500,
                    PuntosFidelizacion = 1500,
                    TotalVisitas = 10
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains("Activo", componente.Markup);
            Assert.Contains("VIP", componente.Markup);
            Assert.Contains("Frecuente", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act
            var botones = componente.FindAll("button");

            // Assert
            Assert.True(botones.Count >= 4); // Ver, Editar, Activar/Desactivar, Eliminar
            Assert.Contains("Ver detalles", componente.Markup);
            Assert.Contains("Editar", componente.Markup);
            Assert.Contains("Eliminar", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesHeader()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test", Apellidos = "User" }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains("Excel", componente.Markup);
            Assert.Contains("Nuevo Cliente", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarContadorClientes()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Cliente", Apellidos = "1" },
                new() { Id = Guid.NewGuid(), Nombre = "Cliente", Apellidos = "2" }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains("Lista de Clientes (2)", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConPaginacion_DeberiaMostrarPaginacion()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test", Apellidos = "User" }
            };

            var paginacion = new PaginatedList<ClienteDto>
            {
                Items = clientes,
                PageSize = 10,
                PageNumber = 1,
                TotalPages = 5,
                TotalCount = 50
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes)
                .Add(p => p.Paginacion, paginacion));

            // Act & Assert
            Assert.NotNull(componente.Find(".pagination"));
            Assert.Contains("Mostrando 1 de 5 páginas", componente.Markup);
            Assert.Contains("50 clientes en total", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarAvatarCliente()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez"
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.NotNull(componente.Find(".avatar-circle"));
            Assert.Contains("J", componente.Markup); // Primera letra del nombre
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    Email = "juan@email.com",
                    Telefono = "555-1234",
                    Ciudad = "Lima"
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi-people"));
            Assert.NotNull(componente.Find("i.oi-envelope-closed"));
            Assert.NotNull(componente.Find("i.oi-phone"));
            Assert.NotNull(componente.Find("i.oi-location"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadges()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    Segmento = "VIP",
                    EstaActivo = true,
                    TotalGastado = 1500,
                    PuntosFidelizacion = 1500,
                    TotalVisitas = 10
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act
            var badges = componente.FindAll(".badge");

            // Assert
            Assert.True(badges.Count >= 3); // Segmento, Estado, VIP, Frecuente
        }

        [Fact]
        public void Renderizar_ConUbicacionVacia_DeberiaMostrarMensaje()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    Ciudad = "",
                    Pais = ""
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains("No especificada", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraCard()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test", Apellidos = "User" }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTablaResponsive()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test", Apellidos = "User" }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-responsive"));
            Assert.NotNull(componente.Find(".table.table-hover"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstilosCSS()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test", Apellidos = "User" }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains(".avatar-circle", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarGrupoBotones()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.NotNull(componente.Find(".btn-group"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTitulosBotones()
        {
            // Arrange
            var clientes = new List<ClienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Juan",
                    Apellidos = "Pérez",
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<ClienteList>(parameters => parameters
                .Add(p => p.Clientes, clientes));

            // Act & Assert
            Assert.Contains("title=\"Ver detalles\"", componente.Markup);
            Assert.Contains("title=\"Editar\"", componente.Markup);
            Assert.Contains("title=\"Eliminar\"", componente.Markup);
        }
    }
}
