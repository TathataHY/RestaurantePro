# Script para crear la estructura de carpetas de RestaurantePro.Domain
$basePath = "src\RestaurantePro.Domain.New"

# Crear la carpeta base
New-Item -Path $basePath -ItemType Directory -Force

# Lista de módulos
$modules = @(
    "Base",
    "Comandas", 
    "Productos", 
    "Ingredientes", 
    "Inventario", 
    "Proveedores", 
    "Compras", 
    "Clientes", 
    "Promociones", 
    "Reservaciones", 
    "Pagos", 
    "Usuarios"
)

# Crear carpetas para cada módulo
foreach ($module in $modules) {
    # Crear carpeta principal del módulo
    $modulePath = Join-Path -Path $basePath -ChildPath $module
    New-Item -Path $modulePath -ItemType Directory -Force
    
    # Crear subcarpetas por tipo
    New-Item -Path (Join-Path -Path $modulePath -ChildPath "Entities") -ItemType Directory -Force
    New-Item -Path (Join-Path -Path $modulePath -ChildPath "Enums") -ItemType Directory -Force
    New-Item -Path (Join-Path -Path $modulePath -ChildPath "Interfaces") -ItemType Directory -Force
}

Write-Host "Estructura de carpetas creada correctamente." 