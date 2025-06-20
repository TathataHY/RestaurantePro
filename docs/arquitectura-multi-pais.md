# Arquitectura Multi-País para RestaurantePro

Este documento detalla la **arquitectura multi-país** propuesta para que RestaurantePro pueda venderse en Chile, Perú y otros países latinoamericanos.

## 🌎 **Visión General**

### **📱 Aplicación Móvil/Web - Una Sola App**

```
RestaurantePro App
├── 🇨🇱 Modo Chile
├── 🇵🇪 Modo Perú  
├── 🇲🇽 Modo México (futuro)
└── 🇨🇴 Modo Colombia (futuro)
```

**Una sola aplicación** que se adapta según el país configurado.

## 🔧 **Sistema de Configuración por País**

### **1. Configuración en Base de Datos**

```sql
-- Tabla de configuración por país
CREATE TABLE ConfiguracionPais (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CodigoPais NVARCHAR(3) NOT NULL, -- 'CL', 'PE', 'MX'
    NombrePais NVARCHAR(100) NOT NULL,
    MonedaLocal NVARCHAR(3) NOT NULL, -- 'CLP', 'PEN', 'MXN'
    ZonaHoraria NVARCHAR(50) NOT NULL, -- 'America/Santiago'
    Idioma NVARCHAR(10) NOT NULL, -- 'es-CL', 'es-PE'
    TasaIVA DECIMAL(5,4) NOT NULL, -- 0.19 (Chile), 0.18 (Perú)
    FormatoRUT NVARCHAR(50), -- Patrón regex para validación
    SistemaFiscal NVARCHAR(20), -- 'SII', 'SUNAT', 'SAT'
    Activo BIT NOT NULL DEFAULT 1
);

-- Tabla de restaurantes con país
CREATE TABLE Restaurantes (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Nombre NVARCHAR(200) NOT NULL,
    CodigoPais NVARCHAR(3) NOT NULL, -- FK a ConfiguracionPais
    -- ... otros campos
);
```

### **2. Configuración en Código**

```csharp
// Enum de países soportados
public enum PaisSoportado
{
    Chile = 1,
    Peru = 2,
    Mexico = 3,
    Colombia = 4
}

// Configuración por país
public class ConfiguracionPais
{
    public string CodigoPais { get; set; } // "CL", "PE"
    public string NombrePais { get; set; }
    public string MonedaLocal { get; set; }
    public string ZonaHoraria { get; set; }
    public string Idioma { get; set; }
    public decimal TasaIVA { get; set; }
    public string FormatoIdentificacion { get; set; }
    public SistemaFiscal SistemaFiscal { get; set; }
    public List<string> MonedasAceptadas { get; set; }
}

// Factory para configuraciones
public static class ConfiguracionPaisFactory
{
    public static ConfiguracionPais ObtenerConfiguracion(string codigoPais)
    {
        return codigoPais.ToUpper() switch
        {
            "CL" => new ConfiguracionPais
            {
                CodigoPais = "CL",
                NombrePais = "Chile",
                MonedaLocal = "CLP",
                ZonaHoraria = "America/Santiago",
                Idioma = "es-CL",
                TasaIVA = 0.19m,
                FormatoIdentificacion = @"^[0-9]{7,8}-[0-9kK]$", // RUT
                SistemaFiscal = SistemaFiscal.SII,
                MonedasAceptadas = ["CLP", "USD"]
            },
            "PE" => new ConfiguracionPais
            {
                CodigoPais = "PE",
                NombrePais = "Perú",
                MonedaLocal = "PEN",
                ZonaHoraria = "America/Lima",
                Idioma = "es-PE",
                TasaIVA = 0.18m,
                FormatoIdentificacion = @"^[0-9]{8}$", // DNI o RUC
                SistemaFiscal = SistemaFiscal.SUNAT,
                MonedasAceptadas = ["PEN", "USD"]
            },
            _ => throw new ArgumentException($"País no soportado: {codigoPais}")
        };
    }
}
```

## 📱 **Experiencia de Usuario Multi-País**

### **Configuración Inicial**
```
┌─────────────────────────────────┐
│     Bienvenido a RestaurantePro │
│                                 │
│   Selecciona tu país:           │
│                                 │
│   🇨🇱 Chile                     │
│   🇵🇪 Perú                      │
│   🇲🇽 México (Próximamente)     │
│                                 │
│   [Continuar]                   │
└─────────────────────────────────┘
```

### **Adaptación Automática**
Una vez seleccionado el país, la app se adapta:

```csharp
// Servicio de localización
public class LocalizationService
{
    private readonly ConfiguracionPais _configuracion;
    
    public LocalizationService(string codigoPais)
    {
        _configuracion = ConfiguracionPaisFactory.ObtenerConfiguracion(codigoPais);
    }
    
    public string FormatearMoneda(decimal monto)
    {
        return _configuracion.CodigoPais switch
        {
            "CL" => $"${monto:N0} CLP",
            "PE" => $"S/ {monto:N2}",
            _ => $"{monto:C}"
        };
    }
    
    public bool ValidarIdentificacion(string identificacion)
    {
        return Regex.IsMatch(identificacion, _configuracion.FormatoIdentificacion);
    }
    
    public decimal CalcularIVA(decimal monto)
    {
        return monto * _configuracion.TasaIVA;
    }
}
```

## 🏗️ **Arquitectura de Implementación**

### **1. Patrón Strategy por País**

```csharp
// Interface para servicios específicos por país
public interface IServicioFiscalPais
{
    Task<bool> ValidarIdentificacionAsync(string identificacion);
    Task<FacturaElectronica> GenerarFacturaElectronicaAsync(Factura factura);
    Task<bool> EnviarFacturaAsync(FacturaElectronica factura);
}

// Implementación para Chile
public class ServicioFiscalChile : IServicioFiscalPais
{
    public async Task<bool> ValidarIdentificacionAsync(string rut)
    {
        // Validación de RUT chileno con dígito verificador
        return ValidarRUT(rut);
    }
    
    public async Task<FacturaElectronica> GenerarFacturaElectronicaAsync(Factura factura)
    {
        // Integración con SII Chile
        return await _siiService.GenerarFacturaAsync(factura);
    }
}

// Implementación para Perú
public class ServicioFiscalPeru : IServicioFiscalPais
{
    public async Task<bool> ValidarIdentificacionAsync(string documento)
    {
        // Validación de DNI/RUC peruano
        return ValidarDNI_RUC(documento);
    }
    
    public async Task<FacturaElectronica> GenerarFacturaElectronicaAsync(Factura factura)
    {
        // Integración con SUNAT Perú
        return await _sunatService.GenerarFacturaAsync(factura);
    }
}

// Factory para servicios fiscales
public class ServicioFiscalFactory
{
    public static IServicioFiscalPais CrearServicio(string codigoPais)
    {
        return codigoPais.ToUpper() switch
        {
            "CL" => new ServicioFiscalChile(),
            "PE" => new ServicioFiscalPeru(),
            _ => throw new ArgumentException($"País no soportado: {codigoPais}")
        };
    }
}
```

### **2. Configuración en DI Container**

```csharp
// En Program.cs o Startup.cs
services.AddScoped<ILocalizationService>(provider =>
{
    var httpContext = provider.GetService<IHttpContextAccessor>()?.HttpContext;
    var codigoPais = httpContext?.Request.Headers["X-Country-Code"].FirstOrDefault() ?? "CL";
    return new LocalizationService(codigoPais);
});

services.AddScoped<IServicioFiscalPais>(provider =>
{
    var localization = provider.GetService<ILocalizationService>();
    return ServicioFiscalFactory.CrearServicio(localization.CodigoPais);
});
```

## 📊 **Base de Datos Multi-Tenant por País**

### **Opción 1: Una BD con Separación por País**
```sql
-- Todas las tablas principales incluyen CodigoPais
ALTER TABLE Productos ADD CodigoPais NVARCHAR(3) NOT NULL DEFAULT 'CL';
ALTER TABLE Clientes ADD CodigoPais NVARCHAR(3) NOT NULL DEFAULT 'CL';
ALTER TABLE Facturas ADD CodigoPais NVARCHAR(3) NOT NULL DEFAULT 'CL';

-- Índices por país para performance
CREATE INDEX IX_Productos_Pais ON Productos(CodigoPais);
CREATE INDEX IX_Clientes_Pais ON Clientes(CodigoPais);
```

### **Opción 2: Base de Datos por País**
```
RestaurantePro_CL (Chile)
RestaurantePro_PE (Perú)
RestaurantePro_MX (México)
```

## 🔧 **Configuración de Deployment**

### **Variables de Entorno por País**

```json
// appsettings.Chile.json
{
  "Pais": {
    "Codigo": "CL",
    "Nombre": "Chile",
    "Moneda": "CLP",
    "ZonaHoraria": "America/Santiago",
    "Idioma": "es-CL"
  },
  "SistemaFiscal": {
    "Tipo": "SII",
    "ApiUrl": "https://api.sii.cl",
    "CertificadoPath": "/certificates/sii-chile.p12"
  }
}

// appsettings.Peru.json
{
  "Pais": {
    "Codigo": "PE",
    "Nombre": "Perú",
    "Moneda": "PEN",
    "ZonaHoraria": "America/Lima",
    "Idioma": "es-PE"
  },
  "SistemaFiscal": {
    "Tipo": "SUNAT",
    "ApiUrl": "https://api.sunat.gob.pe",
    "CertificadoPath": "/certificates/sunat-peru.p12"
  }
}
```

## 📱 **Interfaz de Usuario Adaptativa**

### **Componentes que Cambian por País**

```typescript
// En el frontend (React/Angular/etc)
interface ConfiguracionPais {
  codigo: string;
  nombre: string;
  moneda: string;
  simboloMoneda: string;
  formatoFecha: string;
  formatoIdentificacion: string;
}

// Componente de facturación adaptativo
const FacturacionComponent = () => {
  const { configuracionPais } = useLocalization();
  
  return (
    <div>
      <h2>Facturación - {configuracionPais.nombre}</h2>
      
      {/* Campo de identificación adaptativo */}
      <input 
        placeholder={
          configuracionPais.codigo === 'CL' ? 'RUT (12345678-9)' :
          configuracionPais.codigo === 'PE' ? 'DNI/RUC (12345678)' :
          'Identificación'
        }
        pattern={configuracionPais.formatoIdentificacion}
      />
      
      {/* Formato de moneda adaptativo */}
      <span>{formatearMoneda(precio, configuracionPais)}</span>
    </div>
  );
};
```

## 🚀 **Plan de Implementación Multi-País**

### **Fase 1: Arquitectura Base (Actual)**
- ✅ Configuración por país en código
- ✅ Factory patterns implementados
- ✅ Validaciones específicas por país

### **Fase 2: Chile + Perú**
- 🔄 Implementar configuraciones completas
- 🔄 Servicios fiscales SII y SUNAT
- 🔄 UI adaptativa por país

### **Fase 3: Expansión**
- ⏳ México, Colombia, etc.
- ⏳ Multi-tenancy completo
- ⏳ Gestión centralizada de países

## 💡 **Ventajas de Esta Arquitectura**

1. **Una sola aplicación** para todos los países
2. **Fácil expansión** a nuevos países
3. **Mantenimiento centralizado** del código
4. **Experiencia nativa** por país
5. **Cumplimiento fiscal** específico por país
6. **Escalabilidad** horizontal

## 📝 **Consideraciones Técnicas**

### **Performance**
- Caché de configuraciones por país
- Índices optimizados por código de país
- Carga lazy de componentes específicos

### **Seguridad**
- Validación de país en cada request
- Aislamiento de datos por país
- Certificados específicos por autoridad fiscal

### **Mantenimiento**
- Tests específicos por país
- CI/CD por configuración de país
- Monitoreo diferenciado por región

---

**Fecha de Creación**: Diciembre 2024  
**Versión**: RestaurantePro Multi-País v1.0  
**Estado**: Propuesta de Arquitectura 