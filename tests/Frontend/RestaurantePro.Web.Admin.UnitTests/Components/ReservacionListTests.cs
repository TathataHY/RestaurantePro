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
    public class ReservacionListTests : TestContext
    {
        [Fact]
        public void Renderizar_ConReservacionesNull_DeberiaMostrarSpinner()
        {
            // Arrange
            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, (List<ReservacionDto>?)null));

            // Act & Assert
            Assert.NotNull(componente.Find(".spinner-border"));
            Assert.Contains("Cargando reservaciones...", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConListaVacia_DeberiaMostrarMensajeVacio()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>();

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("No hay reservaciones", componente.Markup);
            Assert.Contains("Comienza creando tu primera reservación", componente.Markup);
            Assert.Contains("Crear Reservación", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReservaciones_DeberiaMostrarTabla()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    ClienteNombre = "Juan Pérez",
                    MesaNombre = "Mesa 1",
                    FechaReservacion = DateTime.Now,
                    HoraReservacion = TimeSpan.FromHours(19),
                    NumeroPersonas = 4,
                    Estado = "Pendiente",
                    CanalReservacion = "Teléfono"
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.NotNull(componente.Find("table"));
            Assert.NotNull(componente.Find("thead"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEncabezadosTabla()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES-001" }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("Reservación", componente.Markup);
            Assert.Contains("Cliente", componente.Markup);
            Assert.Contains("Mesa", componente.Markup);
            Assert.Contains("Fecha y Hora", componente.Markup);
            Assert.Contains("Personas", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Tiempo", componente.Markup);
            Assert.Contains("Canal", componente.Markup);
            Assert.Contains("Acciones", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionReservacion()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    ClienteNombre = "Juan Pérez",
                    ClienteTelefono = "555-1234",
                    ClienteEmail = "juan@email.com",
                    MesaNombre = "Mesa 1",
                    MesaUbicacion = "Terraza",
                    MesaCapacidad = 6,
                    FechaReservacion = DateTime.Now,
                    HoraReservacion = TimeSpan.FromHours(19),
                    NumeroPersonas = 4,
                    Estado = "Pendiente",
                    CanalReservacion = "Teléfono",
                    FuenteReservacion = "Llamada directa"
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("RES-001", componente.Markup);
            Assert.Contains("Juan Pérez", componente.Markup);
            Assert.Contains("555-1234", componente.Markup);
            Assert.Contains("juan@email.com", componente.Markup);
            Assert.Contains("Mesa 1", componente.Markup);
            Assert.Contains("Terraza", componente.Markup);
            Assert.Contains("Cap: 6", componente.Markup);
            Assert.Contains("Teléfono", componente.Markup);
            Assert.Contains("Llamada directa", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFechaYHoraFormateada()
        {
            // Arrange
            var fecha = new DateTime(2024, 1, 15);
            var hora = TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30));
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    FechaReservacion = fecha,
                    HoraReservacion = hora
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("15/01/2024", componente.Markup);
            Assert.Contains("19:30", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarNumeroPersonas()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    NumeroPersonas = 4
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("4", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadgesEstado()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act
            var badges = componente.FindAll(".badge");

            // Assert
            Assert.True(badges.Count >= 1); // Estado
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act
            var botones = componente.FindAll("button");

            // Assert
            Assert.True(botones.Count >= 4); // Ver, Editar, Confirmar, etc.
            Assert.Contains("Ver detalles", componente.Markup);
            Assert.Contains("Editar", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesHeader()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES-001" }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("Excel", componente.Markup);
            Assert.Contains("Nueva Reservación", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarContadorReservaciones()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES-001" },
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES-002" }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("Lista de Reservaciones (2)", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConPaginacion_DeberiaMostrarPaginacion()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES-001" }
            };

            var paginacion = new PaginatedList<ReservacionDto>
            {
                Items = reservaciones,
                PageSize = 10,
                PageNumber = 1,
                TotalPages = 5,
                TotalCount = 50
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones)
                .Add(p => p.Paginacion, paginacion));

            // Act & Assert
            Assert.NotNull(componente.Find(".pagination"));
            Assert.Contains("Mostrando 1 de 5 páginas", componente.Markup);
            Assert.Contains("50 reservaciones en total", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    MesaNombre = "Mesa 1"
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi-calendar"));
            Assert.NotNull(componente.Find("i.oi-eye"));
            Assert.NotNull(componente.Find("i.oi-pencil"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraCard()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES-001" }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTablaResponsive()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() { Id = Guid.NewGuid(), NumeroReservacion = "RES-001" }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-responsive"));
            Assert.NotNull(componente.Find(".table.table-hover"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarGrupoBotones()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".btn-group"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTitulosBotones()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("title=\"Ver detalles\"", componente.Markup);
            Assert.Contains("title=\"Editar\"", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadgesEspeciales()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    EsUrgente = true,
                    EsVIP = true,
                    EsGrupo = true,
                    EsRecurrente = true
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("Urgente", componente.Markup);
            Assert.Contains("VIP", componente.Markup);
            Assert.Contains("Grupo", componente.Markup);
            Assert.Contains("Recurrente", componente.Markup);
            Assert.Contains("Requiere Atención", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFilaUrgente()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    EsUrgente = true
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-warning"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFilaRequiereAtencion()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    EsUrgente = true // Esto hará que se aplique table-warning
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-warning"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTamañoGrupo()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    EsGrupo = true,
                    TamañoGrupo = 12
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("Grupo: 12", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadgesTiempo()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    FechaReservacion = DateTime.Today,
                    HoraReservacion = TimeSpan.FromHours(19)
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("Hoy", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesCondicionales()
        {
            // Arrange
            var reservaciones = new List<ReservacionDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroReservacion = "RES-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<ReservacionList>(parameters => parameters
                .Add(p => p.Reservaciones, reservaciones));

            // Act & Assert
            Assert.Contains("Confirmar", componente.Markup);
            Assert.Contains("Reasignar mesa", componente.Markup);
            Assert.Contains("Cambiar hora", componente.Markup);
            Assert.Contains("Duplicar", componente.Markup);
            Assert.Contains("Cancelar", componente.Markup);
        }
    }
}
