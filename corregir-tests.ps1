# Script para corregir problemas comunes en los tests
Write-Host "Iniciando correcciones en archivos de test..."

# Crear directorio para backups si no existe
if (-not (Test-Path -Path "backups")) {
    New-Item -ItemType Directory -Path "backups" | Out-Null
    Write-Host "Directorio de backups creado"
}

# Lista de todos los archivos de test a revisar
$testFiles = Get-ChildItem -Path "tests/RestaurantePro.Domain.UnitTests" -Recurse -Filter "*Tests.cs"

foreach ($file in $testFiles) {
    Write-Host "Procesando $($file.FullName)..."
    
    # Hacer backup del archivo
    $filename = $file.Name
    Copy-Item -Path $file.FullName -Destination "backups/$filename.bak" -Force
    
    # Leer contenido
    $content = Get-Content -Path $file.FullName -Raw
    
    # 1. Corregir problemas de árboles de expresión
    $content = $content -replace "It\.IsAny<CancellationToken>\(\)", "CancellationToken.None"
    
    # 2. Corregir problemas de nulabilidad
    $content = $content -replace "Task<([^>?]+)>", "Task<`$1?>"
    $content = $content -replace "\(([^)]+)\)null", "(`$1?)null"
    
    # 3. Corregir problemas con ReturnsAsync y tipos nulables
    $content = $content -replace "ReturnsAsync\(null\)", "ReturnsAsync((TarjetaFidelizacion?)null)"
    
    # 4. Agregar variable estándar para CancellationToken si no existe
    if (-not ($content -match "private readonly CancellationToken _cancellationToken")) {
        $content = $content -replace "(^\s*public\s+\w+Tests\s*\(\))", "private readonly CancellationToken _cancellationToken = CancellationToken.None;`n`n`$1"
    }
    
    # 5. Corregir problemas con IEnumerable
    $content = $content -replace "Task<IEnumerable<([^>?]+)>>", "Task<IEnumerable<`$1?>>"
    $content = $content -replace "new List<([^>?]+)>\(\)", "new List<`$1?>()"
    
    # 6. Corregir problemas con ambigüedad de tipos
    $content = $content -replace "UnidadMedida\.([A-Za-z]+)", "RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.`$1"
    
    # Guardar los cambios
    Set-Content -Path $file.FullName -Value $content
    Write-Host "  Archivo actualizado: $($file.Name)"
}

Write-Host "Correcciones aplicadas a todos los archivos de test."
Write-Host "Intentando compilar..."

# Compilar el proyecto para ver si se han resuelto los errores
dotnet build tests/RestaurantePro.Domain.UnitTests

Write-Host "Proceso completado." 