# Arquitectura de la Aplicación Web - RestaurantePro (Parte 1)

## Visión General

La aplicación web de RestaurantePro está desarrollada con Blazor WebAssembly, permitiendo ejecutar C# directamente en el navegador. Esta aplicación está destinada principalmente a la gestión administrativa y analítica del restaurante, proporcionando dashboards, reportes y funcionalidades de configuración para propietarios y gerentes.

## Tecnologías Principales

- **Framework Frontend**: Blazor WebAssembly (.NET 8)
- **Biblioteca de UI**: MudBlazor
- **Gráficos**: ChartJS.Blazor
- **API Cliente**: HttpClient con System.Net.Http.Json
- **Autenticación**: JWT con almacenamiento en localStorage
- **Gestión de Estado**: Flux con StateHasChanged

## Arquitectura MVVM en Blazor

Blazor WebAssembly utiliza un patrón similar a MVVM adaptado al contexto web:

![Diagrama Arquitectura Blazor](../diagramas/blazor-architecture.png)

### Models

Modelos que representan los datos:

```csharp
public class DashboardStats
{
    public decimal VentasDiarias { get; set; }
    public decimal VentasSemanales { get; set; }
    public decimal VentasMensuales { get; set; }
    public int ComandaCompletadas { get; set; }
    public int ClientesAtendidos { get; set; }
    public decimal TicketPromedio { get; set; }
    public List<VentasPorHora> VentasPorHora { get; set; } = new();
    public List<ProductoMasVendido> ProductosMasVendidos { get; set; } = new();
}

public class VentasPorHora
{
    public int Hora { get; set; }
    public decimal Monto { get; set; }
}

public class ProductoMasVendido
{
    public string Nombre { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}
```

### ViewModels (Componentes Base)

En Blazor, los componentes actúan como ViewModels:

```csharp
public partial class DashboardBase : ComponentBase
{
    [Inject] protected IDashboardService DashboardService { get; set; }
    [Inject] protected ISnackbar Snackbar { get; set; }
    
    protected DashboardStats Stats { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected DateRange SelectedDateRange { get; set; } = 
        new(DateTime.Now.AddDays(-7), DateTime.Now);
    
    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardDataAsync();
    }
    
    protected async Task LoadDashboardDataAsync()
    {
        try
        {
            IsLoading = true;
            Stats = await DashboardService.GetDashboardStatsAsync(
                SelectedDateRange.Start,
                SelectedDateRange.End);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error cargando datos: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    
    protected async Task DateRangeChanged(DateRange range)
    {
        SelectedDateRange = range;
        await LoadDashboardDataAsync();
    }
}
```

### Views (Razor Components)

Interfaz de usuario usando Razor y componentes MudBlazor:

```razor
@page "/dashboard"
@inherits DashboardBase

<PageTitle>Dashboard - RestaurantePro</PageTitle>

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">Dashboard</MudText>
    
    <MudPaper Elevation="3" Class="pa-4 mb-4">
        <MudGrid>
            <MudItem xs="12" md="6">
                <MudText Typo="Typo.h6">Periodo de Análisis</MudText>
            </MudItem>
            <MudItem xs="12" md="6" Class="d-flex justify-end">
                <MudDateRangePicker @bind-DateRange="SelectedDateRange" 
                                   Label="Rango de fechas"
                                   AdornmentColor="Color.Primary"
                                   OnRangeSelect="DateRangeChanged" />
            </MudItem>
        </MudGrid>
    </MudPaper>
    
    @if (IsLoading)
    {
        <MudProgressCircular Color="Color.Primary" Indeterminate="true" Size="Size.Large" Class="my-7" />
    }
    else
    {
        <MudGrid>
            <!-- Tarjetas de resumen -->
            <MudItem xs="12" sm="6" md="3">
                <MudPaper Elevation="2" Class="pa-4 mud-height-full">
                    <MudText Typo="Typo.subtitle1">Ventas Diarias</MudText>
                    <MudText Typo="Typo.h5">@Stats.VentasDiarias.ToString("C")</MudText>
                </MudPaper>
            </MudItem>
            <MudItem xs="12" sm="6" md="3">
                <MudPaper Elevation="2" Class="pa-4 mud-height-full">
                    <MudText Typo="Typo.subtitle1">Ventas Semanales</MudText>
                    <MudText Typo="Typo.h5">@Stats.VentasSemanales.ToString("C")</MudText>
                </MudPaper>
            </MudItem>
            <MudItem xs="12" sm="6" md="3">
                <MudPaper Elevation="2" Class="pa-4 mud-height-full">
                    <MudText Typo="Typo.subtitle1">Comandas Completadas</MudText>
                    <MudText Typo="Typo.h5">@Stats.ComandaCompletadas</MudText>
                </MudPaper>
            </MudItem>
            <MudItem xs="12" sm="6" md="3">
                <MudPaper Elevation="2" Class="pa-4 mud-height-full">
                    <MudText Typo="Typo.subtitle1">Ticket Promedio</MudText>
                    <MudText Typo="Typo.h5">@Stats.TicketPromedio.ToString("C")</MudText>
                </MudPaper>
            </MudItem>
            
            <!-- Gráficos -->
            <MudItem xs="12" md="8">
                <MudPaper Elevation="2" Class="pa-4">
                    <MudText Typo="Typo.h6" Class="mb-4">Ventas por Hora</MudText>
                    <MudChart ChartType="ChartType.Line" ChartSeries="@GetVentasPorHoraSeries()"
                             XAxisLabels="@GetVentasPorHoraLabels()" Width="100%" Height="300px" />
                </MudPaper>
            </MudItem>
            
            <MudItem xs="12" md="4">
                <MudPaper Elevation="2" Class="pa-4">
                    <MudText Typo="Typo.h6" Class="mb-4">Productos Más Vendidos</MudText>
                    <MudChart ChartType="ChartType.Pie" ChartSeries="@GetProductosMasVendidosSeries()"
                             Width="100%" Height="300px">
                        <CustomGraphics>
                            <MudText Typo="Typo.subtitle2" Align="Align.Center">Productos</MudText>
                        </CustomGraphics>
                    </MudChart>
                </MudPaper>
            </MudItem>
        </MudGrid>
    }
</MudContainer>

@code {
    private List<ChartSeries> GetVentasPorHoraSeries()
    {
        var series = new ChartSeries() 
        { 
            Name = "Ventas", 
            Data = Stats.VentasPorHora.Select(v => (double)v.Monto).ToArray() 
        };
        return new List<ChartSeries> { series };
    }
    
    private string[] GetVentasPorHoraLabels() => 
        Stats.VentasPorHora.Select(v => $"{v.Hora}:00").ToArray();
        
    private List<ChartSeries> GetProductosMasVendidosSeries()
    {
        return Stats.ProductosMasVendidos.Select(p => 
            new ChartSeries { Name = p.Nombre, Data = new double[] { (double)p.Cantidad } })
            .ToList();
    }
} 