namespace RestaurantePro.Api.Configuration;

public class EnvironmentConfig
{
    public string Environment { get; set; } = string.Empty;
    public bool IsDemo { get; set; }
    public bool IsDevelopment { get; set; }
    public bool IsProduction { get; set; }
    public ClienteConfig? ClienteConfig { get; set; }
    public DemoSettings? DemoSettings { get; set; }
}

public class ClienteConfig
{
    public string NombreRestaurante { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string ColorPrimario { get; set; } = "#1976D2";
    public string ColorSecundario { get; set; } = "#FFC107";
    public string ColorAcento { get; set; } = "#FF5722";
    public List<string> CaracteristicasEspeciales { get; set; } = new();
    public bool HabilitarModoDemo { get; set; }
    public int LimiteComandasPorDia { get; set; } = 100;
}

public class DemoSettings
{
    public string NombreRestaurante { get; set; } = "La Buena Mesa Demo";
    public int MaxComandasPerSession { get; set; } = 10;
    public int AutoResetHours { get; set; } = 24;
    public bool HabilitarResetAutomatico { get; set; } = true;
    public List<string> UsuariosDemo { get; set; } = new()
    {
        "admin@demo.com",
        "mesero@demo.com", 
        "cocinero@demo.com"
    };
    public int LimiteUsuariosConcurrentes { get; set; } = 50;
} 