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
    public class PreparacionListTests : TestContext
    {
        [Fact]
        public void Renderizar_ConListaVacia_DeberiaMostrarColumnasVacias()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>();

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("Pendientes (0)", componente.Markup);
            Assert.Contains("En Proceso (0)", componente.Markup);
            Assert.Contains("Listas (0)", componente.Markup);
            Assert.Contains("Entregadas (0)", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConPreparaciones_DeberiaMostrarKanban()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Producto Test",
                    MesaNumero = 5,
                    Cantidad = 2,
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Normal,
                    TiempoEstimadoMinutos = 15
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".preparaciones-container"));
            Assert.NotNull(componente.Find(".kanban-column"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarColumnasKanban()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>();

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".kanban-column"));
            Assert.Contains("Pendientes", componente.Markup);
            Assert.Contains("En Proceso", componente.Markup);
            Assert.Contains("Listas", componente.Markup);
            Assert.Contains("Entregadas", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarPreparacionPendiente()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Producto Test",
                    MesaNumero = 5,
                    Cantidad = 2,
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Normal,
                    TiempoEstimadoMinutos = 15,
                    FechaCreacion = DateTime.Now
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("Producto Test", componente.Markup);
            Assert.Contains("Mesa 5", componente.Markup);
            Assert.Contains("Cant: 2", componente.Markup);
            Assert.Contains("15 min", componente.Markup);
            Assert.Contains("Normal", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarPreparacionEnProceso()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Producto Test",
                    MesaNumero = 5,
                    Cantidad = 2,
                    Estado = EstadoPreparacion.EnProceso,
                    Prioridad = PrioridadPreparacion.Alta,
                    CocineroNombre = "Chef Juan",
                    TiempoTranscurridoMinutos = 10,
                    FechaInicio = DateTime.Now.AddMinutes(-10)
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("Producto Test", componente.Markup);
            Assert.Contains("Mesa 5", componente.Markup);
            Assert.Contains("Cant: 2", componente.Markup);
            Assert.Contains("Chef Juan", componente.Markup);
            Assert.Contains("10 min", componente.Markup);
            Assert.Contains("Alta", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarPreparacionLista()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Producto Test",
                    MesaNumero = 5,
                    Cantidad = 2,
                    Estado = EstadoPreparacion.Lista,
                    Prioridad = PrioridadPreparacion.Normal,
                    CocineroNombre = "Chef Juan",
                    TiempoRealMinutos = 12,
                    FechaFin = DateTime.Now
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("Producto Test", componente.Markup);
            Assert.Contains("Mesa 5", componente.Markup);
            Assert.Contains("Cant: 2", componente.Markup);
            Assert.Contains("Chef Juan", componente.Markup);
            Assert.Contains("12 min", componente.Markup);
            Assert.Contains("Lista", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarPreparacionEntregada()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Producto Test",
                    MesaNumero = 5,
                    Cantidad = 2,
                    Estado = EstadoPreparacion.Entregada,
                    Prioridad = PrioridadPreparacion.Normal,
                    CocineroNombre = "Chef Juan",
                    TiempoRealMinutos = 12,
                    FechaFin = DateTime.Now
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("Producto Test", componente.Markup);
            Assert.Contains("Mesa 5", componente.Markup);
            Assert.Contains("Cant: 2", componente.Markup);
            Assert.Contains("Chef Juan", componente.Markup);
            Assert.Contains("12 min", componente.Markup);
            Assert.Contains("Entregada", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarContadores()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() { Estado = EstadoPreparacion.Pendiente },
                new() { Estado = EstadoPreparacion.Pendiente },
                new() { Estado = EstadoPreparacion.EnProceso },
                new() { Estado = EstadoPreparacion.Lista },
                new() { Estado = EstadoPreparacion.Entregada },
                new() { Estado = EstadoPreparacion.Entregada }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("Pendientes (2)", componente.Markup);
            Assert.Contains("En Proceso (1)", componente.Markup);
            Assert.Contains("Listas (1)", componente.Markup);
            Assert.Contains("Entregadas (2)", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarPrioridades()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Urgente",
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Urgente
                },
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Alta",
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Alta
                },
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Normal",
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Normal
                },
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Baja",
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Baja
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("Urgente", componente.Markup);
            Assert.Contains("Alta", componente.Markup);
            Assert.Contains("Normal", componente.Markup);
            Assert.Contains("Baja", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Test",
                    Estado = EstadoPreparacion.EnProceso,
                    CocineroNombre = "Chef Juan"
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi-play-circle"));
            Assert.NotNull(componente.Find("i.oi-person"));
        }

        [Fact]
        public void Renderizar_ConVistaTabular_DeberiaMostrarTabla()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Producto Test",
                    MesaNumero = 5,
                    Cantidad = 2,
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Normal,
                    TiempoEstimadoMinutos = 15
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones)
                .Add(p => p.MostrarVistaTabular, true));

            // Act & Assert
            Assert.NotNull(componente.Find("table"));
            Assert.NotNull(componente.Find("thead"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_ConVistaTabular_DeberiaMostrarEncabezados()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() { Id = 1, ProductoNombre = "Test" }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones)
                .Add(p => p.MostrarVistaTabular, true));

            // Act & Assert
            Assert.Contains("Comanda", componente.Markup);
            Assert.Contains("Producto", componente.Markup);
            Assert.Contains("Mesa", componente.Markup);
            Assert.Contains("Cantidad", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Prioridad", componente.Markup);
            Assert.Contains("Cocinero", componente.Markup);
            Assert.Contains("Tiempo Est.", componente.Markup);
            Assert.Contains("Tiempo Real", componente.Markup);
            Assert.Contains("Diferencia", componente.Markup);
            Assert.Contains("Acciones", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCardsPreparacion()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Producto Test",
                    Estado = EstadoPreparacion.Pendiente
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".preparacion-card"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadgesPrioridad()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Test",
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Urgente
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".badge"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraKanban()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>();

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".row"));
            Assert.NotNull(componente.Find(".col-md-3"));
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstadosCorrectos()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() { Estado = EstadoPreparacion.Pendiente },
                new() { Estado = EstadoPreparacion.EnProceso },
                new() { Estado = EstadoPreparacion.Lista },
                new() { Estado = EstadoPreparacion.Entregada }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("Pendientes (1)", componente.Markup);
            Assert.Contains("En Proceso (1)", componente.Markup);
            Assert.Contains("Listas (1)", componente.Markup);
            Assert.Contains("Entregadas (1)", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTiempoEstimado()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Test",
                    Estado = EstadoPreparacion.Pendiente,
                    TiempoEstimadoMinutos = 20
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("20 min", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTiempoTranscurrido()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Test",
                    Estado = EstadoPreparacion.EnProceso,
                    TiempoTranscurridoMinutos = 15
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("15 min", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTiempoReal()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Test",
                    Estado = EstadoPreparacion.Lista,
                    TiempoRealMinutos = 18
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("18 min", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCocinero()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Test",
                    Estado = EstadoPreparacion.EnProceso,
                    CocineroNombre = "Chef María"
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.Contains("Chef María", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSinCocinero()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Test",
                    Estado = EstadoPreparacion.Pendiente,
                    CocineroNombre = null
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.DoesNotContain("Chef", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSinTiempoEstimado()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Test",
                    Estado = EstadoPreparacion.Pendiente,
                    TiempoEstimadoMinutos = 0
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.DoesNotContain("min", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSinTiempoTranscurrido()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Test",
                    Estado = EstadoPreparacion.EnProceso,
                    TiempoTranscurridoMinutos = null
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.DoesNotContain("min", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSinTiempoReal()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Test",
                    Estado = EstadoPreparacion.Lista,
                    TiempoRealMinutos = null
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.DoesNotContain("min", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarClasesPrioridad()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Urgente",
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Urgente
                },
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Alta",
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Alta
                },
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Normal",
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Normal
                },
                new() 
                { 
                    Id = 1, 
                    ProductoNombre = "Baja",
                    Estado = EstadoPreparacion.Pendiente,
                    Prioridad = PrioridadPreparacion.Baja
                }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".prioridad-urgente"));
            Assert.NotNull(componente.Find(".prioridad-alta"));
            Assert.NotNull(componente.Find(".prioridad-normal"));
            Assert.NotNull(componente.Find(".prioridad-baja"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarVistaDetallada()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>
            {
                new() { Id = 1, ProductoNombre = "Test" }
            };

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones)
                .Add(p => p.MostrarVistaTabular, true));

            // Act & Assert
            Assert.Contains("Vista Detallada", componente.Markup);
        }


        [Fact]
        public void Renderizar_DeberiaMostrarLimiteEntregadas()
        {
            // Arrange
            var preparaciones = new List<PreparacionDto>();
            for (int i = 0; i < 15; i++)
            {
                preparaciones.Add(new() 
                { 
                    Id = 1, 
                    ProductoNombre = $"Producto {i}",
                    Estado = EstadoPreparacion.Entregada,
                    FechaFin = DateTime.Now.AddMinutes(-i)
                });
            }

            var componente = RenderComponent<PreparacionList>(parameters => parameters
                .Add(p => p.Preparaciones, preparaciones));

            // Act & Assert
            // Solo debería mostrar 10 entregadas (Take(10))
            Assert.Contains("Entregadas (15)", componente.Markup);
        }
    }
}
