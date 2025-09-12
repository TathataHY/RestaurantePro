using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class ContactosProveedorTests : TestContext
    {
        [Fact]
        public void Renderizar_ConMostrarModalFalse_DeberiaOcultarModal()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, false));

            // Act & Assert
            var modal = componente.Find(".modal");
            Assert.Contains("display: none", modal.GetAttribute("style"));
        }

        [Fact]
        public void Renderizar_ConMostrarModalTrue_DeberiaMostrarModal()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            var modal = componente.Find(".modal");
            Assert.Contains("display: block", modal.GetAttribute("style"));
            Assert.Contains("show", modal.GetAttribute("class"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTituloModal()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("Contactos de Proveedor Test", componente.Markup);
            Assert.Contains("oi-people", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProveedorNull_DeberiaMostrarModalVacio()
        {
            // Arrange
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, (ProveedorDto?)null)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.NotNull(componente.Find(".modal"));
            Assert.DoesNotContain("Información del Proveedor", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProveedor_DeberiaMostrarInformacionProveedor()
        {
            // Arrange
            var proveedor = new ProveedorDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Proveedor Test",
                Ruc = "12345678901",
                Ciudad = "Quito",
                TotalContactos = 5,
                TotalOrdenesCompra = 10,
                MontoTotalCompras = 15000.50m
            };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("Información del Proveedor", componente.Markup);
            Assert.Contains("Proveedor Test", componente.Markup);
            Assert.Contains("12345678901", componente.Markup);
            Assert.Contains("Quito", componente.Markup);
            Assert.Contains("5", componente.Markup);
            Assert.Contains("10", componente.Markup);
            Assert.Contains("$15,000.50", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProveedorSinRuc_DeberiaOcultarRuc()
        {
            // Arrange
            var proveedor = new ProveedorDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Proveedor Test",
                Ruc = null,
                Ciudad = "Quito"
            };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.DoesNotContain("RUC:", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProveedorSinCiudad_DeberiaOcultarCiudad()
        {
            // Arrange
            var proveedor = new ProveedorDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Proveedor Test",
                Ruc = "12345678901",
                Ciudad = null
            };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.DoesNotContain("Ciudad:", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonAgregarContacto()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("Agregar Contacto", componente.Markup);
            Assert.Contains("oi-plus", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConContactosVacios_DeberiaMostrarMensajeSinContactos()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("No hay contactos", componente.Markup);
            Assert.Contains("Este proveedor no tiene contactos registrados.", componente.Markup);
            Assert.Contains("Agregar Primer Contacto", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConContactos_DeberiaMostrarTabla()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>
            {
                new ContactoProveedorDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Cargo = "Gerente",
                    Telefono = "022345678",
                    Email = "juan@proveedor.com",
                    Celular = "0987654321",
                    EsContactoPrincipal = true,
                    FechaCreacion = DateTime.Now
                }
            };

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.NotNull(componente.Find("table"));
            Assert.NotNull(componente.Find(".table-responsive"));
            Assert.Contains("Nombre", componente.Markup);
            Assert.Contains("Cargo", componente.Markup);
            Assert.Contains("Teléfono", componente.Markup);
            Assert.Contains("Email", componente.Markup);
            Assert.Contains("Celular", componente.Markup);
            Assert.Contains("Principal", componente.Markup);
            Assert.Contains("Acciones", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConContactos_DeberiaMostrarDatosContacto()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>
            {
                new ContactoProveedorDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Cargo = "Gerente",
                    Telefono = "022345678",
                    Email = "juan@proveedor.com",
                    Celular = "0987654321",
                    EsContactoPrincipal = true,
                    FechaCreacion = DateTime.Now
                }
            };

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("Juan Pérez", componente.Markup);
            Assert.Contains("Gerente", componente.Markup);
            Assert.Contains("022345678", componente.Markup);
            Assert.Contains("juan@proveedor.com", componente.Markup);
            Assert.Contains("0987654321", componente.Markup);
            Assert.Contains("Principal", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConContactoSinCargo_DeberiaMostrarSinCargo()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>
            {
                new ContactoProveedorDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Cargo = null,
                    EsContactoPrincipal = false
                }
            };

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("Sin cargo", componente.Markup);
            Assert.Contains("Secundario", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConContactoSinTelefono_DeberiaMostrarSinTelefono()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>
            {
                new ContactoProveedorDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Telefono = null,
                    Email = null,
                    Celular = null
                }
            };

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("Sin teléfono", componente.Markup);
            Assert.Contains("Sin email", componente.Markup);
            Assert.Contains("Sin celular", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConContactos_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>
            {
                new ContactoProveedorDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Apellido = "Pérez"
                }
            };

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("oi-pencil", componente.Markup);
            Assert.Contains("oi-trash", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonCerrar()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("Cerrar", componente.Markup);
            Assert.Contains("oi-x", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConMostrarFormularioContactoFalse_DeberiaOcultarFormulario()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.DoesNotContain("Nuevo Contacto", componente.Markup);
            Assert.DoesNotContain("Editar Contacto", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraModal()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.NotNull(componente.Find(".modal"));
            Assert.NotNull(componente.Find(".modal-dialog"));
            Assert.NotNull(componente.Find(".modal-content"));
            Assert.NotNull(componente.Find(".modal-header"));
            Assert.NotNull(componente.Find(".modal-body"));
            Assert.NotNull(componente.Find(".modal-footer"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarClasesCSS()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>();

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.NotNull(componente.Find(".modal-xl"));
            Assert.NotNull(componente.Find(".btn-close"));
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-body"));
        }

        [Fact]
        public void Renderizar_ConContactos_DeberiaMostrarIconos()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>
            {
                new ContactoProveedorDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Telefono = "022345678",
                    Email = "juan@proveedor.com",
                    Celular = "0987654321",
                    EsContactoPrincipal = true
                }
            };

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("oi-people", componente.Markup);
            Assert.Contains("oi-plus", componente.Markup);
            Assert.Contains("oi-phone", componente.Markup);
            Assert.Contains("oi-envelope-closed", componente.Markup);
            Assert.Contains("oi-star", componente.Markup);
            Assert.Contains("oi-pencil", componente.Markup);
            Assert.Contains("oi-trash", componente.Markup);
            Assert.Contains("oi-x", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConContactos_DeberiaMostrarBadges()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>
            {
                new ContactoProveedorDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Cargo = "Gerente",
                    EsContactoPrincipal = true
                }
            };

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.NotNull(componente.Find(".badge.bg-secondary"));
            Assert.NotNull(componente.Find(".badge.bg-warning"));
        }

        [Fact]
        public void Renderizar_ConContactos_DeberiaMostrarAvatar()
        {
            // Arrange
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>
            {
                new ContactoProveedorDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Apellido = "Pérez"
                }
            };

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            var avatar = componente.Find(".bg-info.text-white.rounded-circle");
            Assert.NotNull(avatar);
            Assert.Contains("J", avatar.TextContent);
        }

        [Fact]
        public void Renderizar_ConContactos_DeberiaMostrarFechaCreacion()
        {
            // Arrange
            var fechaCreacion = new DateTime(2024, 1, 15);
            var proveedor = new ProveedorDto { Id = Guid.NewGuid(), Nombre = "Proveedor Test" };
            var contactos = new List<ContactoProveedorDto>
            {
                new ContactoProveedorDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    FechaCreacion = fechaCreacion
                }
            };

            var componente = RenderComponent<ContactosProveedor>(parameters => parameters
                .Add(p => p.Proveedor, proveedor)
                .Add(p => p.Contactos, contactos)
                .Add(p => p.MostrarModal, true));

            // Act & Assert
            Assert.Contains("15/01/2024", componente.Markup);
        }
    }
}
