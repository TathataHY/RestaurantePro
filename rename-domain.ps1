# Script para limpiar y reorganizar las carpetas de dominio

# Verificar la existencia y contenido de las carpetas
Write-Host "Verificando carpetas existentes..."
Get-ChildItem -Path "src" -Directory | Where-Object { $_.Name -like "RestaurantePro.Domain*" } | Select-Object Name

# Primero intentamos mover la carpeta .Final a un nombre temporal fuera de src
Write-Host "Moviendo Domain.Final a ubicación temporal..."
$tempPath = ".\temp_domain"
New-Item -Path $tempPath -ItemType Directory -Force
Copy-Item -Path "src\RestaurantePro.Domain.Final\*" -Destination $tempPath -Recurse -Force

# Intentamos eliminar todas las carpetas Domain existentes
Write-Host "Eliminando carpetas Domain existentes..."
Remove-Item -Path "src\RestaurantePro.Domain*" -Recurse -Force -ErrorAction SilentlyContinue

# Creamos la carpeta Domain definitiva
Write-Host "Creando nueva carpeta Domain definitiva..."
New-Item -Path "src\RestaurantePro.Domain" -ItemType Directory -Force

# Copiamos desde la ubicación temporal
Write-Host "Copiando archivos a la nueva ubicación..."
Copy-Item -Path "$tempPath\*" -Destination "src\RestaurantePro.Domain" -Recurse -Force

# Limpiamos la carpeta temporal
Write-Host "Limpiando carpeta temporal..."
Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Proceso completado. Verificando resultado final:"
Get-ChildItem -Path "src" -Directory | Select-Object Name 