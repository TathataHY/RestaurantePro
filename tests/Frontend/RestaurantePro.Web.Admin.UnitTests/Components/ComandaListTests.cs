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
    public class ComandaListTests : TestContext
    {
        [Fact]
        public void Renderizar_ConComandasNull_DeberiaMostrarSpinner()
        {
            // Arrange
            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, (List<ComandaDto>?)null));

            // Act & Assert
            Assert.NotNull(componente.Find(".spinner-border"));
            Assert.Contains("Cargando comandas...", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConListaVacia_DeberiaMostrarMensajeVacio()
        {
            // Arrange
            var comandas = new List<ComandaDto>();

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("No hay comandas", componente.Markup);
            Assert.Contains("Comienza creando tu primera comanda", componente.Markup);
            Assert.Contains("Crear Comanda", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConComandas_DeberiaMostrarTabla()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    MesaNombre = "Mesa 1",
                    ClienteNombre = "Juan Pérez",
                    MeseroNombre = "Carlos",
                    Estado = "Pendiente",
                    Prioridad = "Normal",
                    Total = 150.50m,
                    Detalles = new List<ComandaDetalleDto>
                    {
                        new() { Cantidad = 1 },
                        new() { Cantidad = 1 },
                        new() { Cantidad = 1 }
                    }
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.NotNull(componente.Find("table"));
            Assert.NotNull(componente.Find("thead"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEncabezadosTabla()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-001" }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("Comanda", componente.Markup);
            Assert.Contains("Mesa", componente.Markup);
            Assert.Contains("Cliente", componente.Markup);
            Assert.Contains("Mesero", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Prioridad", componente.Markup);
            Assert.Contains("Tiempo", componente.Markup);
            Assert.Contains("Total", componente.Markup);
            Assert.Contains("Acciones", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionComanda()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    MesaNombre = "Mesa 1",
                    MesaUbicacion = "Terraza",
                    ClienteNombre = "Juan Pérez",
                    ClienteTelefono = "555-1234",
                    MeseroNombre = "Carlos",
                    Estado = "Pendiente",
                    Prioridad = "Normal",
                    Total = 150.50m,
                    Detalles = new List<ComandaDetalleDto>
                    {
                        new() { Cantidad = 1 },
                        new() { Cantidad = 1 },
                        new() { Cantidad = 1 }
                    }
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("CMD-001", componente.Markup);
            Assert.Contains("Mesa 1", componente.Markup);
            Assert.Contains("Terraza", componente.Markup);
            Assert.Contains("Juan Pérez", componente.Markup);
            Assert.Contains("555-1234", componente.Markup);
            Assert.Contains("Carlos", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTotalYItems()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    Total = 150.50m,
                    Detalles = new List<ComandaDetalleDto>
                    {
                        new() { Cantidad = 1 },
                        new() { Cantidad = 1 },
                        new() { Cantidad = 1 }
                    }
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("$150.50", componente.Markup);
            Assert.Contains("3 items", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadgesEstado()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    Estado = "Pendiente",
                    Prioridad = "Normal"
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act
            var badges = componente.FindAll(".badge");

            // Assert
            Assert.True(badges.Count >= 2); // Estado y Prioridad
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act
            var botones = componente.FindAll("button");

            // Assert
            Assert.True(botones.Count >= 4); // Ver, Editar, Cambiar Estado, etc.
            Assert.Contains("Ver detalles", componente.Markup);
            Assert.Contains("Editar", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesHeader()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-001" }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("Excel", componente.Markup);
            Assert.Contains("Nueva Comanda", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarContadorComandas()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-001" },
                new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-002" }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("Lista de Comandas (2)", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConPaginacion_DeberiaMostrarPaginacion()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-001" }
            };

            var paginacion = new PaginatedList<ComandaDto>
            {
                Items = comandas,
                PageSize = 10,
                PageNumber = 1,
                TotalPages = 5,
                TotalCount = 50
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas)
                .Add(p => p.Paginacion, paginacion));

            // Act & Assert
            Assert.NotNull(componente.Find(".pagination"));
            Assert.Contains("Mostrando 1 de 5 páginas", componente.Markup);
            Assert.Contains("50 comandas en total", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    MesaNombre = "Mesa 1"
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi-clipboard"));
            Assert.NotNull(componente.Find("i.oi-eye"));
            Assert.NotNull(componente.Find("i.oi-pencil"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraCard()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-001" }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTablaResponsive()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroComanda = "CMD-001" }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-responsive"));
            Assert.NotNull(componente.Find(".table.table-hover"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarGrupoBotones()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.NotNull(componente.Find(".btn-group"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTitulosBotones()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("title=\"Ver detalles\"", componente.Markup);
            Assert.Contains("title=\"Editar\"", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConClienteGeneral_DeberiaMostrarMensaje()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    ClienteNombre = ""
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("Cliente General", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTiempoTranscurrido()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    FechaCreacion = DateTime.Now.AddMinutes(-15)
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("15 min", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTiempoProcesamiento()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    FechaInicio = DateTime.Now.AddMinutes(-20),
                    FechaFinalizacion = DateTime.Now.AddMinutes(-10)
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("Proc: 10 min", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadgesEspeciales()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    EsUrgente = true,
                    EsDomicilio = true,
                    FechaCreacion = DateTime.Now.AddMinutes(-45),
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.Contains("Urgente", componente.Markup);
            Assert.Contains("Domicilio", componente.Markup);
            Assert.Contains("Lenta", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFilaUrgente()
        {
            // Arrange
            var comandas = new List<ComandaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroComanda = "CMD-001",
                    EsUrgente = true
                }
            };

            var componente = RenderComponent<ComandaList>(parameters => parameters
                .Add(p => p.Comandas, comandas));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-warning"));
        }
    }
}
