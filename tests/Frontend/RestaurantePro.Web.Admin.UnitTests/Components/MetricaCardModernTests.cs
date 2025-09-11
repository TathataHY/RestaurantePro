using Bunit;
using RestaurantePro.Web.Admin.Components;
using System;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class MetricaCardModernTests : TestContext
    {
        [Fact]
        public void Renderizar_ConParametrosBasicos_DeberiaMostrarEstructura()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.NotNull(componente.Find(".metric-card"));
            Assert.NotNull(componente.Find(".metric-header"));
            Assert.NotNull(componente.Find(".metric-title"));
            Assert.NotNull(componente.Find(".metric-value"));
        }

        [Fact]
        public void Renderizar_ConTitulo_DeberiaMostrarTitulo()
        {
            // Arrange
            var titulo = "Ventas del Mes";

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, titulo)
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.Contains(titulo, componente.Markup);
        }

        [Fact]
        public void Renderizar_ConValor_DeberiaMostrarValor()
        {
            // Arrange
            var valor = "2,500";

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, valor));

            // Act & Assert
            Assert.Contains(valor, componente.Markup);
        }

        [Fact]
        public void Renderizar_ConSubtexto_DeberiaMostrarSubtexto()
        {
            // Arrange
            var subtexto = "vs mes anterior";

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.Subtexto, subtexto));

            // Act & Assert
            Assert.Contains(subtexto, componente.Markup);
        }

        [Fact]
        public void Renderizar_SinSubtexto_NoDeberiaMostrarSubtexto()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250"));

            // Act & Assert
            Assert.DoesNotContain("metric-subtitle", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConIcono_DeberiaMostrarIcono()
        {
            // Arrange
            var icono = "oi-dollar";

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.Icono, icono));

            // Act & Assert
            Assert.Contains(icono, componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCardClass_DeberiaAplicarClase()
        {
            // Arrange
            var cardClass = "success";

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.CardClass, cardClass));

            // Act & Assert
            Assert.Contains($"metric-card {cardClass}", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConValorClass_DeberiaAplicarClase()
        {
            // Arrange
            var valorClass = "text-success";

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.ValorClass, valorClass));

            // Act & Assert
            Assert.Contains($"metric-value {valorClass}", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConIconoClass_DeberiaAplicarClase()
        {
            // Arrange
            var iconoClass = "success";

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.Icono, "oi-dollar")
                .Add(p => p.IconoClass, iconoClass));

            // Act & Assert
            Assert.Contains($"metric-icon {iconoClass}", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConMostrarCambioFalse_NoDeberiaMostrarCambio()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, false)
                .Add(p => p.Cambio, 0.15m));

            // Act & Assert
            Assert.DoesNotContain("metric-change", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConMostrarCambioTrueYCambioPositivo_DeberiaMostrarCambioPositivo()
        {
            // Arrange
            var cambio = 0.15m;

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("metric-change positive", componente.Markup);
            Assert.Contains("oi-arrow-top", componente.Markup);
            Assert.Contains("15.0%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConMostrarCambioTrueYCambioNegativo_DeberiaMostrarCambioNegativo()
        {
            // Arrange
            var cambio = -0.08m;

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("metric-change negative", componente.Markup);
            Assert.Contains("oi-arrow-bottom", componente.Markup);
            Assert.Contains("8.0%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConMostrarCambioTrueYCambioCero_DeberiaMostrarCambioPositivo()
        {
            // Arrange
            var cambio = 0m;

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("metric-change positive", componente.Markup);
            Assert.Contains("oi-arrow-top", componente.Markup);
            Assert.Contains("0.0%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConMostrarCambioTrueYCambioNull_NoDeberiaMostrarCambio()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, (decimal?)null));

            // Act & Assert
            Assert.DoesNotContain("metric-change", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambioDecimal_DeberiaFormatearCorrectamente()
        {
            // Arrange
            var cambio = 0.1234m;

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("12.3%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambioGrande_DeberiaFormatearCorrectamente()
        {
            // Arrange
            var cambio = 1.5m;

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("150.0%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambioMuyPequeno_DeberiaFormatearCorrectamente()
        {
            // Arrange
            var cambio = 0.001m;

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("0.1%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConTodosLosParametros_DeberiaMostrarTodo()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas Mensuales")
                .Add(p => p.Valor, "15,750")
                .Add(p => p.Subtexto, "vs mes anterior")
                .Add(p => p.Icono, "oi-dollar")
                .Add(p => p.CardClass, "success")
                .Add(p => p.ValorClass, "text-success")
                .Add(p => p.IconoClass, "success")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, 0.12m));

            // Act & Assert
            Assert.Contains("Ventas Mensuales", componente.Markup);
            Assert.Contains("15,750", componente.Markup);
            Assert.Contains("vs mes anterior", componente.Markup);
            Assert.Contains("oi-dollar", componente.Markup);
            Assert.Contains("metric-card success", componente.Markup);
            Assert.Contains("metric-value text-success", componente.Markup);
            Assert.Contains("metric-icon success", componente.Markup);
            Assert.Contains("metric-change positive", componente.Markup);
            Assert.Contains("12.0%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConValoresPorDefecto_DeberiaUsarValoresPorDefecto()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>();

            // Act & Assert
            Assert.Contains("metric-card primary", componente.Markup);
            Assert.Contains("metric-value text-primary", componente.Markup);
            Assert.Contains("metric-icon primary", componente.Markup);
            Assert.DoesNotContain("metric-subtitle", componente.Markup);
            Assert.DoesNotContain("metric-change", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConSubtextoVacio_NoDeberiaMostrarSubtexto()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.Subtexto, ""));

            // Act & Assert
            Assert.DoesNotContain("metric-subtitle", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConSubtextoNull_NoDeberiaMostrarSubtexto()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.Subtexto, (string)null));

            // Act & Assert
            Assert.DoesNotContain("metric-subtitle", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConIconoVacio_DeberiaMostrarSpanVacio()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.Icono, ""));

            // Act & Assert
            Assert.Contains("<span class=\"\"></span>", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambioExactamenteCero_DeberiaMostrarComoPositivo()
        {
            // Arrange
            var cambio = 0.0m;

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("metric-change positive", componente.Markup);
            Assert.Contains("oi-arrow-top", componente.Markup);
            Assert.Contains("0.0%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConCambioMuyNegativo_DeberiaFormatearCorrectamente()
        {
            // Arrange
            var cambio = -0.999m;

            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Ventas")
                .Add(p => p.Valor, "1,250")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, cambio));

            // Act & Assert
            Assert.Contains("metric-change negative", componente.Markup);
            Assert.Contains("oi-arrow-bottom", componente.Markup);
            Assert.Contains("99.9%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConClasesPersonalizadas_DeberiaAplicarTodasLasClases()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.CardClass, "custom-card")
                .Add(p => p.ValorClass, "custom-value")
                .Add(p => p.IconoClass, "custom-icon"));

            // Act & Assert
            Assert.Contains("metric-card custom-card", componente.Markup);
            Assert.Contains("metric-value custom-value", componente.Markup);
            Assert.Contains("metric-icon custom-icon", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConEstructuraCompleta_DeberiaTenerEstructuraCorrecta()
        {
            // Arrange
            var componente = RenderComponent<MetricaCardModern>(parameters => parameters
                .Add(p => p.Titulo, "Test")
                .Add(p => p.Valor, "100")
                .Add(p => p.Subtexto, "test")
                .Add(p => p.Icono, "oi-test")
                .Add(p => p.MostrarCambio, true)
                .Add(p => p.Cambio, 0.1m));

            // Act & Assert
            var metricCard = componente.Find(".metric-card");
            Assert.NotNull(metricCard);

            var metricHeader = componente.Find(".metric-header");
            Assert.NotNull(metricHeader);

            var metricTitle = componente.Find(".metric-title");
            Assert.NotNull(metricTitle);

            var metricIcon = componente.Find(".metric-icon");
            Assert.NotNull(metricIcon);

            var metricValue = componente.Find(".metric-value");
            Assert.NotNull(metricValue);

            var metricSubtitle = componente.Find(".metric-subtitle");
            Assert.NotNull(metricSubtitle);

            var metricChange = componente.Find(".metric-change");
            Assert.NotNull(metricChange);
        }
    }
}
