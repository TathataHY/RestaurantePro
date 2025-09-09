### Plan de mejora de pruebas unitarias - RestaurantePro Mobile (MAUI)

#### Objetivo
Elevar la calidad y robustez de los tests unitarios del frontend móvil (MAUI) asegurando cobertura funcional, resiliencia y mantenibilidad.

#### Alcance
- Proyecto: `tests/Frontend/RestaurantePro.Mobile.UnitTests`
- Áreas: Services, ViewModels (Operations, Inventory, Commercial, Auth, Analytics), Core Services (Auth, Comandas).

### Estado actual (resumen)
- Cobertura sólida de happy paths, errores de API y excepciones en muchos Services.
- ViewModels con buena cobertura de comandos principales, navegación, y estados calculados.
- Core Services con validaciones y errores cubiertos.
- Deuda detectada en concurrencia (`IsBusy`/`CanExecute`), `PropertyChanged`, cancelación/timeout, 401/403/429, 204/empty states, paginación/debounce, cultura/formateo, resiliencia/offline.

### Brechas transversales (checklist)
- [ ] `IsBusy`/`CanExecute` consistente en todos los comandos (evitar reentrancia/doble-tap)
- [ ] Verificación de `INotifyPropertyChanged` por propiedad relevante
- [ ] `CancellationToken` + tests de cancelación/timeout
- [ ] Manejo de 401/403/429 + reintentos exponenciales cuando aplique
- [ ] Manejo de 204/empty body y mensajes de “sin datos” coherentes
- [ ] Paginación/infinite scroll (primera/siguiente/última, datasets vacíos)
- [ ] Cultura/formateo (fechas, monedas, localización) en ViewModels
- [ ] Resiliencia/offline (caché y reintentos diferidos)
- [ ] Mutation testing (Stryker) para validar calidad de aserciones

### Plan por áreas (checklist)

#### ViewModels - Operations
- ProductosViewModel / ProductoDetalleViewModel
  - [ ] `IsBusy`/`CanExecute` en todos los comandos
  - [ ] `PropertyChanged` (p. ej. `TextoBusqueda`, `SelectedCategoria`, `MostrarSoloDisponibles`, `Productos`, `Categorias`, métricas)
  - [ ] Errores/diálogos por comando (carga, búsqueda, filtros, populares, navegación)
  - [ ] Paginación/scroll; estado vacío y 204
  - [ ] Debounce de búsqueda (si aplica)
  - [ ] Navegación con parámetros requeridos/nulos
  - [ ] Concurrencia (doble ejecución)

- MesasViewModel / MesaDetalleViewModel
  - [x] `IsBusy`/`CanExecute` y `PropertyChanged`
  - [x] Errores en asignar/liberar/cambiar estado; confirmaciones
  - [x] Filtros (Apply/Clear), búsqueda local y estado vacío/204; navegación y parámetros

- ComandasViewModel / ComandaDetalleViewModel
  - [x] `IsBusy`/`CanExecute` en cargas y estadísticas (ComandasViewModel)
  - [x] Errores en cambiar estado/finalizar/cancelar (ComandasViewModel)
  - [x] Estados sin transición y toggle activas (ComandasViewModel)
  - [x] Búsqueda: texto vacío y fallback por número (ComandasViewModel)
  - [x] Revisar errores y confirmaciones adicionales en ComandaDetalleViewModel

#### ViewModels - Otras features
- Reservaciones / Preparaciones / Ingredientes / Categorías / Tarjetas / Analytics / Login
  - [ ] `IsBusy`/`CanExecute` y `PropertyChanged`
  - [ ] Paginación (donde aplique) y 204/empty states
  - [ ] Errores por comando (no solo carga)
  - [ ] Validación de rangos/filtros; cultura/formateo

#### Services (Productos, Mesas, Preparaciones, Ingredientes, Categorías, Comercial, Facturas, Clientes, Analytics)
- [ ] `CancellationToken` en métodos y tests de cancelación/timeout
- [ ] Casos 401/403/429 (y política de reintentos donde aplique)
- [ ] 204/empty body y `null Data`
- [ ] Validaciones de parámetros y composición de query (fechas/culturas)
- [ ] Mapeos con nulos/parciales
- [ ] Verificar propagación de token en todas las llamadas

#### Core Services (Auth, Comandas)
- [ ] Ampliar errores (red, auth expirado), reintentos (si aplica)
- [ ] Casos límite de datos (listas vacías, cantidades límite)

#### Deuda puntual
- [ ] Reactivar o reemplazar `tests/Frontend/RestaurantePro.Mobile.UnitTests/ViewModels/DailyPreparationsViewModelTests.cs` (actualmente comentado)

### Prioridades y orden de ejecución
1. ViewModels Operations (Productos → Mesas → Comandas)
2. Services de esas mismas áreas
3. ViewModels restantes (Reservaciones, Preparaciones, Inventory, Commercial, Auth, Analytics)
4. Brechas transversales
5. Mutation testing

### Grupo inicial
Fase 1.1 - ViewModels Operations: Productos
- [x] Añadir tests `IsBusy`/`CanExecute` en todos los comandos de `ProductosViewModel`
- [x] Tests de `PropertyChanged` (claves y colecciones)
- [x] Paginación/infinite scroll + 204/empty (parcial: verificación de paginado por defecto y estado vacío)
- [ ] Debounce de búsqueda
- [x] Debounce de búsqueda
- [x] Errores/diálogos por comando (carga, búsqueda, filtros, populares)
- [x] Navegación con parámetros requeridos y nulos (cubierto en tests existentes)

Estado Fase 1.1: Completada (queda pendiente cubrir infinite scroll en una fase posterior).

Fase 1.2 - ViewModels Operations: Mesas
- [x] Infinite scroll (apéndice de páginas y bloqueo con IsBusy)
- [x] Apply/Clear filters mapping (mapeo de UI → backend y reset)
- [x] Cambiar estado: cancelación por usuario y error de servicio
- [x] Filtro local por SearchText (número/ubicación/zona)

Fase 1.3 - ViewModels Operations: Comandas
- [x] IsBusy/CanExecute en cargas y estadísticas (ComandasViewModel)
- [x] Errores en cambiar estado/finalizar/cancelar (ComandasViewModel)
- [x] Estados sin transición y toggle activas (ComandasViewModel)
- [x] Búsqueda: texto vacío y fallback por número (ComandasViewModel)
- [x] Errores/confirmaciones en ComandaDetalle: finalizar/cancelar/cambiar estado

Estado Fase 1.3: Completada (suite de tests Comandas ViewModels en verde).

### Fase 2 - Services

#### Fase 2.1 - Services Operations: Comandas
- [x] 204/empty y `null Data` (mapeo coherente a estado vacío o error informativo)
- [x] 401/403/429 (propagación de `StatusCode` y mensaje)
- [x] Cancelación (`CancellationToken`) y cancelación temprana (sin invocar API)
- [x] Paginación: verificación de parámetros `pageNumber/pageSize` en query

Estado Fase 2.1: Completada (suite de tests Comandas Services en verde).

#### Fase 2.2 - Services Operations: Mesas y Productos
- [x] 204/empty y `null Data` en MesasService y ProductosService
- [x] 401/403/429 en MesasService y ProductosService
- [x] Cancelación (`CancellationToken`) y cancelación temprana (sin invocar API)
- [x] Paginación: verificación de parámetros `pageNumber/pageSize` (si aplica)

Estado Fase 2.2: Completada (suite de tests Mesas/Productos Services en verde).

### Métricas e inspección (opcional)
- Cobertura (Coverlet + ReportGenerator)
  - PowerShell:
    ```powershell
    cd tests; dotnet test "Frontend/RestaurantePro.Mobile.UnitTests/RestaurantePro.Mobile.UnitTests.csproj" /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura; dotnet tool install -g dotnet-reportgenerator-globaltool; reportgenerator -reports:".\tests\Frontend\RestaurantePro.Mobile.UnitTests\TestResults\*\coverage.cobertura.xml" -targetdir:".\coverage-mobile-unit"
    ```
- Mutation testing (Stryker.NET)
  - PowerShell:
    ```powershell
    dotnet tool install -g dotnet-stryker; cd tests/Frontend/RestaurantePro.Mobile.UnitTests; dotnet stryker
    ```


