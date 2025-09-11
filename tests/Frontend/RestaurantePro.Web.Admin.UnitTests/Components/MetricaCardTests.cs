using Bunit;
using RestaurantePro.Web.Admin.Components;
using System;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class MetricaCardTests : TestContext
    {
        [Fact]
        public void Renderizar_ConParametrosBasicos_DeberiaMostrarEstructura()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.Find(".col-md-3"));
        }

        [Fact]
        public void Renderizar_ConTitulo_DeberiaMostrarTitulo()
        {
            // Arrange
            var titulo = "Ventas del Mes";
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, titulo)
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.Contains(titulo, componente.Markup);
            Assert.NotNull(componente.Find(".card-title"));
        }

        [Fact]
        public void Renderizar_ConValor_DeberiaMostrarValor()
        {
            // Arrange
            var valor = "2,500";
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, valor));

            // Act & Assert
            Assert.Contains(valor, componente.Markup);
            Assert.NotNull(componente.Find("h4"));
        }

        [Fact]
        public void Renderizar_ConSubtexto_DeberiaMostrarSubtexto()
        {
            // Arrange
            var subtexto = "vs mes anterior";
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.Subtexto, subtexto));

            // Act & Assert
            Assert.Contains(subtexto, componente.Markup);
            Assert.NotNull(componente.Find("small"));
        }

        [Fact]
        public void Renderizar_SinSubtexto_NoDeberiaMostrarSubtexto()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.DoesNotContain("small", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConIcono_DeberiaMostrarIcono()
        {
            // Arrange
            var icono = "oi oi-people";
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Clientes")
                .Add(p => p.Valor, "150")
                .Add(p => p.Icono, icono));

            // Act & Assert
            Assert.Contains(icono, componente.Markup);
            Assert.NotNull(componente.Find("i"));
        }

        [Fact]
        public void Renderizar_ConIconoPorDefecto_DeberiaMostrarIconoPorDefecto()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.Contains("oi oi-bar-chart", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCardClass_DeberiaAplicarClase()
        {
            // Arrange
            var cardClass = "border-left-success";
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.CardClass, cardClass));

            // Act & Assert
            Assert.Contains(cardClass, componente.Markup);
        }

        [Fact]
        public void Renderizar_ConValorClass_DeberiaAplicarClase()
        {
            // Arrange
            var valorClass = "text-success";
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.ValorClass, valorClass));

            // Act & Assert
            Assert.Contains(valorClass, componente.Markup);
        }

        [Fact]
        public void Renderizar_ConIconoClass_DeberiaAplicarClase()
        {
            // Arrange
            var iconoClass = "text-warning";
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.IconoClass, iconoClass));

            // Act & Assert
            Assert.Contains(iconoClass, componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambioPositivo_DeberiaMostrarCambio()
        {
            // Arrange
            var cambio = 15.5m;
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("15.5%", componente.Markup);
            Assert.Contains("text-success", componente.Markup);
            Assert.Contains("oi-arrow-top", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambioNegativo_DeberiaMostrarCambio()
        {
            // Arrange
            var cambio = -8.3m;
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("8.3%", componente.Markup);
            Assert.Contains("text-danger", componente.Markup);
            Assert.Contains("oi-arrow-bottom", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambioCero_DeberiaMostrarCambio()
        {
            // Arrange
            var cambio = 0m;
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("0.0%", componente.Markup);
            Assert.Contains("text-success", componente.Markup);
            Assert.Contains("oi-arrow-top", componente.Markup);
        }

        [Fact]
        public void Renderizar_SinMostrarCambio_NoDeberiaMostrarCambio()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, false)
                .Add(p => p.Cambio, 15.5m));

            // Act & Assert
            Assert.DoesNotContain("15.5%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambioNull_NoDeberiaMostrarCambio()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, null));

            // Act & Assert
            Assert.DoesNotContain("%", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarClasesBootstrap()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.NotNull(componente.Find(".col-md-3"));
            Assert.NotNull(componente.Find(".mb-3"));
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.Find(".d-flex"));
            Assert.NotNull(componente.Find(".justify-content-between"));
            Assert.NotNull(componente.Find(".align-items-center"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraFlexbox()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.NotNull(componente.Find(".d-flex"));
            Assert.NotNull(componente.Find(".justify-content-between"));
            Assert.NotNull(componente.Find(".align-items-center"));
            Assert.NotNull(componente.Find(".text-end"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarClasesTexto()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.NotNull(componente.Find(".text-muted"));
            Assert.NotNull(componente.Find(".mb-0"));
            Assert.NotNull(componente.Find(".mb-1"));
        }

        [Fact]
        public void Renderizar_ConIconoFa2x_DeberiaMostrarClase()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.Contains("fa-2x", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConH100_DeberiaMostrarClase()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.Contains("h-100", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambio_DeberiaMostrarMt2()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, 15.5m));

            // Act & Assert
            Assert.NotNull(componente.Find(".mt-2"));
        }

        [Fact]
        public void Renderizar_ConCambio_DeberiaMostrarSmall()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, 15.5m));

            // Act & Assert
            Assert.NotNull(componente.Find("small"));
        }

        [Fact]
        public void Renderizar_ConCambio_DeberiaMostrarIconoFlecha()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, 15.5m));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi-arrow-top"));
        }

        [Fact]
        public void Renderizar_ConCambioNegativo_DeberiaMostrarIconoFlechaAbajo()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, -15.5m));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi-arrow-bottom"));
        }

        [Fact]
        public void Renderizar_ConCambio_DeberiaMostrarValorAbsoluto()
        {
            // Arrange
            var cambio = -25.7m;
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("25.7%", componente.Markup);
            Assert.DoesNotContain("-25.7%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambio_DeberiaMostrarFormatoDecimal()
        {
            // Arrange
            var cambio = 12.345m;
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("12.3%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConParametrosCompletos_DeberiaMostrarTodo()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas Totales")
                .Add(p => p.Valor, "15,750")
                .Add(p => p.Subtexto, "Este mes")
                .Add(p => p.Icono, "oi oi-dollar")
                .Add(p => p.CardClass, "border-left-success")
                .Add(p => p.ValorClass, "text-success")
                .Add(p => p.IconoClass, "text-success")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, 12.5m));

            // Act & Assert
            Assert.Contains("Ventas Totales", componente.Markup);
            Assert.Contains("15,750", componente.Markup);
            Assert.Contains("Este mes", componente.Markup);
            Assert.Contains("oi oi-dollar", componente.Markup);
            Assert.Contains("border-left-success", componente.Markup);
            Assert.Contains("text-success", componente.Markup);
            Assert.Contains("12.5%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConValorVacio_DeberiaMostrarValorVacio()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, ""));

            // Act & Assert
            Assert.NotNull(componente.Find("h4"));
            Assert.Contains("h4", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConTituloVacio_DeberiaMostrarTituloVacio()
        {
            // Arrange
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.NotNull(componente.Find(".card-title"));
            Assert.Contains("h6", componente.Markup);
        }
    }
}
