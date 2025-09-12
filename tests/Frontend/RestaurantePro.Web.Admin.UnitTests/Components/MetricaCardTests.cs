using Bunit;
using RestaurantePro.Web.Admin.Components;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class MetricaCardTests : TestContext
    {
        [Fact]
        public void RenderizaEstructuraBasica()
        {
            // Arrange
            var titulo = "Ventas Totales";
            var valor = "$1,250.00";

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, titulo)
                .Add(p => p.Valor, valor));

            // Assert
            Assert.Contains(titulo, componente.Markup);
            Assert.Contains(valor, componente.Markup);
            Assert.Contains("card", componente.Markup);
        }

        [Fact]
        public void RenderizaTituloCorrectamente()
        {
            // Arrange
            var titulo = "Productos Vendidos";

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, titulo)
                .Add(p => p.Valor, "100"));

            // Assert
            var tituloElement = componente.Find("h6.card-title");
            Assert.Equal(titulo, tituloElement.TextContent);
            Assert.Contains("text-muted", tituloElement.ClassName);
        }

        [Fact]
        public void RenderizaValorCorrectamente()
        {
            // Arrange
            var valor = "1,500";

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, valor));

            // Assert
            var valorElement = componente.Find("h4.mb-0");
            Assert.Equal(valor, valorElement.TextContent);
        }

        [Fact]
        public void RenderizaSubtextoCuandoSeProporciona()
        {
            // Arrange
            var subtexto = "Este mes";

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.Subtexto, subtexto));

            // Assert
            Assert.Contains(subtexto, componente.Markup);
            var subtextoElement = componente.Find("small.text-muted");
            Assert.Equal(subtexto, subtextoElement.TextContent);
        }

        [Fact]
        public void NoRenderizaSubtextoCuandoEsNull()
        {
            // Arrange & Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.Subtexto, (string?)null));

            // Assert
            var subtextoElements = componente.FindAll("small.text-muted");
            Assert.Empty(subtextoElements);
        }

        [Fact]
        public void NoRenderizaSubtextoCuandoEsVacio()
        {
            // Arrange & Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.Subtexto, ""));

            // Assert
            var subtextoElements = componente.FindAll("small.text-muted");
            Assert.Empty(subtextoElements);
        }

        [Fact]
        public void RenderizaIconoConClaseCorrecta()
        {
            // Arrange
            var icono = "oi oi-people";

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.Icono, icono));

            // Assert
            var iconoElement = componente.Find("i");
            Assert.Contains(icono, iconoElement.ClassName);
            Assert.Contains("fa-2x", iconoElement.ClassName);
        }

        [Fact]
        public void UsaIconoPorDefecto()
        {
            // Arrange & Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100"));

            // Assert
            var iconoElement = componente.Find("i");
            Assert.Contains("oi oi-bar-chart", iconoElement.ClassName);
        }

        [Fact]
        public void AplicaClasesCssCorrectas()
        {
            // Arrange
            var cardClass = "border-left-success";
            var valorClass = "text-success";
            var iconoClass = "text-success";

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.CardClass, cardClass)
                .Add(p => p.ValorClass, valorClass)
                .Add(p => p.IconoClass, iconoClass));

            // Assert
            var card = componente.Find(".card");
            Assert.Contains(cardClass, card.ClassName);

            var valorElement = componente.Find("h4.mb-0");
            Assert.Contains(valorClass, valorElement.ClassName);

            var iconoElement = componente.Find("i");
            Assert.Contains(iconoClass, iconoElement.ClassName);
        }

        [Fact]
        public void UsaClasesPorDefecto()
        {
            // Arrange & Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100"));

            // Assert
            var card = componente.Find(".card");
            Assert.Contains("border-left-primary", card.ClassName);

            var valorElement = componente.Find("h4.mb-0");
            Assert.Contains("text-primary", valorElement.ClassName);

            var iconoElement = componente.Find("i");
            Assert.Contains("text-primary", iconoElement.ClassName);
        }

        [Fact]
        public void NoMuestraCambioCuandoMostrarCambioEsFalse()
        {
            // Arrange & Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.MostrarCambio, false)
                .Add(p => p.Cambio, 5.5m));

            // Assert
            Assert.DoesNotContain("oi-arrow-top", componente.Markup);
            Assert.DoesNotContain("oi-arrow-bottom", componente.Markup);
            Assert.DoesNotContain("5.5%", componente.Markup);
        }

        [Fact]
        public void NoMuestraCambioCuandoCambioEsNull()
        {
            // Arrange & Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, (decimal?)null));

            // Assert
            Assert.DoesNotContain("oi-arrow-top", componente.Markup);
            Assert.DoesNotContain("oi-arrow-bottom", componente.Markup);
        }

        [Fact]
        public void MuestraCambioPositivoCorrectamente()
        {
            // Arrange
            var cambio = 12.5m;

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Assert
            Assert.Contains("oi-arrow-top", componente.Markup);
            Assert.Contains("text-success", componente.Markup);
            Assert.Contains("12.5%", componente.Markup);
        }

        [Fact]
        public void MuestraCambioNegativoCorrectamente()
        {
            // Arrange
            var cambio = -8.3m;

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Assert
            Assert.Contains("oi-arrow-bottom", componente.Markup);
            Assert.Contains("text-danger", componente.Markup);
            Assert.Contains("8.3%", componente.Markup);
        }

        [Fact]
        public void MuestraCambioCeroCorrectamente()
        {
            // Arrange
            var cambio = 0m;

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Assert
            Assert.Contains("oi-arrow-top", componente.Markup);
            Assert.Contains("text-success", componente.Markup);
            Assert.Contains("0.0%", componente.Markup);
        }

        [Fact]
        public void FormateaCambioConUnDecimal()
        {
            // Arrange
            var cambio = 15.67m;

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Assert
            Assert.Contains("15.7%", componente.Markup);
        }

        [Fact]
        public void RenderizaEstructuraCompletaConTodosLosParametros()
        {
            // Arrange
            var titulo = "Ingresos";
            var valor = "$5,000.00";
            var subtexto = "Este mes";
            var icono = "oi oi-dollar";
            var cardClass = "border-left-info";
            var valorClass = "text-info";
            var iconoClass = "text-info";
            var cambio = 25.5m;

            // Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, titulo)
                .Add(p => p.Valor, valor)
                .Add(p => p.Subtexto, subtexto)
                .Add(p => p.Icono, icono)
                .Add(p => p.CardClass, cardClass)
                .Add(p => p.ValorClass, valorClass)
                .Add(p => p.IconoClass, iconoClass)
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Assert
            Assert.Contains(titulo, componente.Markup);
            Assert.Contains(valor, componente.Markup);
            Assert.Contains(subtexto, componente.Markup);
            Assert.Contains(icono, componente.Markup);
            Assert.Contains(cardClass, componente.Markup);
            Assert.Contains(valorClass, componente.Markup);
            Assert.Contains(iconoClass, componente.Markup);
            Assert.Contains("25.5%", componente.Markup);
        }

        [Fact]
        public void AplicaClasesBootstrapCorrectas()
        {
            // Arrange & Act
            var componente = RenderComponent<MetricaCard>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100"));

            // Assert
            Assert.Contains("col-md-3", componente.Markup);
            Assert.Contains("mb-3", componente.Markup);
            Assert.Contains("card", componente.Markup);
            Assert.Contains("h-100", componente.Markup);
            Assert.Contains("card-body", componente.Markup);
            Assert.Contains("d-flex", componente.Markup);
            Assert.Contains("justify-content-between", componente.Markup);
            Assert.Contains("align-items-center", componente.Markup);
            Assert.Contains("text-end", componente.Markup);
        }
    }
}