using Bunit;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class ParametrosSistemaTests : TestContext
    {
        [Fact]
        public void Renderizar_ConParametrosNull_DeberiaMostrarSpinner()
        {
            // Arrange
            var componente = RenderComponent<ParametrosSistema>();

            // Act & Assert
            Assert.NotNull(componente.Find(".spinner-border"));
            Assert.Contains("Cargando parámetros...", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametrosVacios_DeberiaMostrarMensajeSinParametros()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>();

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.Contains("No hay parámetros configurados", componente.Markup);
            Assert.Contains("oi-cog", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarTitulo()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.Contains("Parámetros del Sistema", componente.Markup);
            Assert.Contains("oi-cog", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarEstructuraBasica()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.Find(".row"));
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarParametros()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.Contains("TestParam", componente.Markup);
            Assert.Contains("Parámetro de prueba", componente.Markup);
            Assert.Contains("General", componente.Markup);
            Assert.Contains("string", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametroEditable_DeberiaMostrarInputEditable()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.Contains("Editable", componente.Markup);
            Assert.NotNull(componente.Find("input[type='text']"));
            Assert.NotNull(componente.Find("button[class*='btn-outline-success']"));
            Assert.Contains("oi-check", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametroNoEditable_DeberiaMostrarInputReadonly()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = false
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.Contains("Solo lectura", componente.Markup);
            var input = componente.Find("input[readonly]");
            Assert.NotNull(input);
            Assert.Equal("TestValue", input.GetAttribute("value"));
        }

        [Fact]
        public void Renderizar_ConParametroEditable_DeberiaMostrarBadgePrimario()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.NotNull(componente.Find(".badge.bg-primary"));
            Assert.NotNull(componente.Find(".card.border-primary"));
        }

        [Fact]
        public void Renderizar_ConParametroNoEditable_DeberiaMostrarBadgeSecundario()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = false
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.NotNull(componente.Find(".badge.bg-secondary"));
            Assert.NotNull(componente.Find(".card.border-secondary"));
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarIconos()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.Contains("oi-cog", componente.Markup);
            Assert.Contains("oi-check", componente.Markup);
            Assert.Contains("oi-tag", componente.Markup);
            Assert.Contains("oi-code", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarInformacionDetallada()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.Contains("TestParam", componente.Markup);
            Assert.Contains("Parámetro de prueba", componente.Markup);
            Assert.Contains("General", componente.Markup);
            Assert.Contains("string", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarClasesCSS()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.Find(".row"));
            Assert.NotNull(componente.Find(".col-md-6"));
            Assert.NotNull(componente.Find(".input-group"));
            Assert.NotNull(componente.Find(".form-control"));
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarEstructuraCompleta()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            // Verificar estructura principal
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            
            // Verificar fila de parámetros
            Assert.NotNull(componente.Find(".row"));
            Assert.NotNull(componente.Find(".col-md-6"));
            
            // Verificar card de parámetro
            Assert.NotNull(componente.Find(".card.border-primary"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.Find(".card-title"));
            Assert.NotNull(componente.Find(".card-text"));
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarInputGroup()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.NotNull(componente.Find(".input-group"));
            Assert.NotNull(componente.Find("input[type='text']"));
            Assert.NotNull(componente.Find("button[class*='btn-outline-success']"));
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarInformacionAdicional()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.Contains("oi-tag", componente.Markup);
            Assert.Contains("oi-code", componente.Markup);
            Assert.Contains("General", componente.Markup);
            Assert.Contains("string", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarValoresCorrectos()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.Contains("TestParam", componente.Markup);
            Assert.Contains("TestValue", componente.Markup);
            Assert.Contains("Parámetro de prueba", componente.Markup);
            Assert.Contains("General", componente.Markup);
            Assert.Contains("string", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarTextosDescriptivos()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.Contains("Parámetros del Sistema", componente.Markup);
            Assert.Contains("Editable", componente.Markup);
            Assert.Contains("Parámetro de prueba", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarEstructuraResponsiva()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.NotNull(componente.Find(".col-md-6"));
            Assert.NotNull(componente.Find(".row"));
        }

        [Fact]
        public void Renderizar_ConParametros_DeberiaMostrarElementosInteractivos()
        {
            // Arrange
            var parametros = new List<ConfiguracionDto>
            {
                new ConfiguracionDto
                {
                    Id = Guid.NewGuid(),
                    Clave = "TestParam",
                    Valor = "TestValue",
                    Descripcion = "Parámetro de prueba",
                    Categoria = "General",
                    TipoDato = "string",
                    EsEditable = true
                }
            };

            var componente = RenderComponent<ParametrosSistema>(parameters => parameters
                .Add(p => p.parametros, parametros));

            // Act & Assert
            Assert.NotNull(componente.Find("input[type='text']"));
            Assert.NotNull(componente.Find("button[class*='btn-outline-success']"));
            Assert.Contains("oi-check", componente.Markup);
        }
    }
}
