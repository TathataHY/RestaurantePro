# Script para reorganizar en módulos principales

$domainPath = "src\RestaurantePro.Domain"
$tempPath = ".\temp_domain_modules"

# Crear carpeta temporal
Write-Host "Creando directorio temporal..."
New-Item -Path $tempPath -ItemType Directory -Force

# Crear estructura de módulos principales
Write-Host "Creando estructura de módulos principales..."
$modulosPrincipales = @(
    "Core",
    "Operaciones",
    "Inventario",
    "Comercial",
    "Proveedores"
)

foreach ($modulo in $modulosPrincipales) {
    New-Item -Path "$tempPath\$modulo" -ItemType Directory -Force
}

# Mover módulos a sus correspondientes módulos principales
Write-Host "Moviendo módulos a sus categorías principales..."

# 1. Core
Write-Host "Organizando módulo Core..."
Copy-Item -Path "$domainPath\Base" -Destination "$tempPath\Core" -Recurse -Force
Copy-Item -Path "$domainPath\Productos" -Destination "$tempPath\Core" -Recurse -Force
Copy-Item -Path "$domainPath\Usuarios" -Destination "$tempPath\Core" -Recurse -Force

# 2. Operaciones
Write-Host "Organizando módulo Operaciones..."
Copy-Item -Path "$domainPath\Comandas" -Destination "$tempPath\Operaciones" -Recurse -Force
Copy-Item -Path "$domainPath\Reservaciones" -Destination "$tempPath\Operaciones" -Recurse -Force

# 3. Inventario
Write-Host "Organizando módulo Inventario..."
Copy-Item -Path "$domainPath\Inventario" -Destination "$tempPath\Inventario" -Recurse -Force
Copy-Item -Path "$domainPath\Ingredientes" -Destination "$tempPath\Inventario" -Recurse -Force
Copy-Item -Path "$domainPath\Compras" -Destination "$tempPath\Inventario" -Recurse -Force

# 4. Comercial
Write-Host "Organizando módulo Comercial..."
Copy-Item -Path "$domainPath\Clientes" -Destination "$tempPath\Comercial" -Recurse -Force
Copy-Item -Path "$domainPath\Pagos" -Destination "$tempPath\Comercial" -Recurse -Force
Copy-Item -Path "$domainPath\Promociones" -Destination "$tempPath\Comercial" -Recurse -Force

# 5. Proveedores
Write-Host "Organizando módulo Proveedores..."
Copy-Item -Path "$domainPath\Proveedores" -Destination "$tempPath\Proveedores" -Recurse -Force

# Copiar archivos de proyecto y README
Copy-Item -Path "$domainPath\RestaurantePro.Domain.csproj" -Destination "$tempPath" -Force
Copy-Item -Path "$domainPath\*.md" -Destination "$tempPath" -Force

# Limpiar directorio Domain y mover la nueva estructura
Write-Host "Limpiando directorio Domain y aplicando nueva estructura..."
Remove-Item -Path "$domainPath\*" -Recurse -Force
Copy-Item -Path "$tempPath\*" -Destination $domainPath -Recurse -Force

# Limpiar carpeta temporal
Write-Host "Limpiando carpeta temporal..."
Remove-Item -Path $tempPath -Recurse -Force

# Crear nuevo README con la estructura actualizada
Write-Host "Creando README actualizado..."
$readmeContent = @"
# RestaurantePro.Domain - Estructura Modular por Categorías

Este directorio contiene la capa de dominio del sistema RestaurantePro, organizada siguiendo los principios de Screaming Architecture (Arquitectura Gritante) junto con Clean Architecture, agrupada en categorías funcionales.

## Estructura de carpetas

La estructura está organizada por categorías principales que agrupan módulos de negocio relacionados:

```
Domain/
├── Core/                         # Componentes esenciales del sistema
│   ├── Base/                     # Componentes base compartidos
│   ├── Productos/                # Catálogo de productos/platillos
│   └── Usuarios/                 # Usuarios del sistema
│
├── Operaciones/                  # Operaciones diarias del restaurante
│   ├── Comandas/                 # Gestión de comandas y pedidos
│   └── Reservaciones/            # Gestión de reservaciones y mesas
│
├── Inventario/                   # Gestión de stock y suministros
│   ├── Inventario/               # Control de inventario
│   ├── Ingredientes/             # Gestión de ingredientes
│   └── Compras/                  # Órdenes de compra
│
├── Comercial/                    # Aspectos comerciales y financieros
│   ├── Clientes/                 # Gestión de clientes y fidelización
│   ├── Pagos/                    # Sistema de pagos
│   └── Promociones/              # Sistema de promociones
│
└── Proveedores/                  # Gestión de proveedores
```

Cada módulo mantiene su estructura interna con:
- Entities/: Entidades de dominio
- Enums/: Enumeraciones
- Interfaces/: Interfaces específicas del módulo

## Beneficios de esta estructura

1. **Organización conceptual clara**: La estructura refleja las áreas funcionales del negocio
2. **Mayor cohesión**: Los componentes relacionados están agrupados en diferentes niveles
3. **Límites contextuales definidos**: Cada módulo principal representa un contexto del negocio
4. **Escalabilidad**: Facilita agregar nuevas funcionalidades en el contexto adecuado
5. **Facilita TDD**: Estructura ideal para aplicar desarrollo guiado por pruebas por contextos
"@

Set-Content -Path "$domainPath\README.md" -Value $readmeContent

Write-Host "Reorganización completada exitosamente."
Get-ChildItem -Path $domainPath -Directory | Select-Object Name 