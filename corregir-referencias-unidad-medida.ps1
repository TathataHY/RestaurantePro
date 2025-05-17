# Script para corregir problemas con referencias a UnidadMedida
Write-Host "Corrigiendo referencias a UnidadMedida..."

# Lista de archivos a corregir
$files = @(
    "tests/RestaurantePro.Domain.UnitTests/Inventario/Compras/OrdenesCompra/Repositories/OrdenCompraRepositoryTests.cs",
    "tests/RestaurantePro.Domain.UnitTests/Inventario/Ingredientes/Entities/IngredienteTests.cs",
    "tests/RestaurantePro.Domain.UnitTests/Inventario/Policies/StockBajoPolicyTests.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "Procesando $file..."
        
        # Leer contenido
        $content = Get-Content -Path $file -Raw
        
        # Corregir patrón de referencia a UnidadMedida
        $content = $content -replace "RestaurantePro\.Domain\.Inventario\.Ingredientes\.Enums\.RestaurantePro\.Domain\.Inventario\.Ingredientes\.Enums\.UnidadMedida", "RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida"
        
        # Corregir referencia a .RestaurantePro
        $content = $content -replace "\.RestaurantePro\.Domain\.Inventario\.Ingredientes\.Enums\.UnidadMedida", ".UnidadMedida"
        
        # Preservar las referencias correctas
        $content = $content -replace "UnidadMedida\.([A-Za-z]+)", "UnidadMedida.$1"
        
        # Guardar el archivo
        Set-Content -Path $file -Value $content
        
        Write-Host "  Archivo actualizado: $file"
    }
}

Write-Host "Correcciones completadas." 