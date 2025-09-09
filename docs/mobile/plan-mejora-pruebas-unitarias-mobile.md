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
- Reservaciones / Preparaciones / Ingredientes / Categorías / Clientes / Tarjetas / Analytics / Login
  - [x] `IsBusy`/`CanExecute` y `PropertyChanged` (Clientes, Analytics y Login: no reentrancia)
  - [x] Paginación (donde aplique) y 204/empty states (Clientes y Analytics: estados vacíos cubiertos)
  - [x] Errores por comando (Clientes, Analytics y Login: errores mostrados con diálogo)
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
- [x] Reactivar o reemplazar `tests/Frontend/RestaurantePro.Mobile.UnitTests/ViewModels/DailyPreparationsViewModelTests.cs` (reactivado con mocks compatibles)

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

#### Fase 2.3 - Services Commercial: Facturas
- [x] 401/403/429 (propagación de `StatusCode` y mensaje)
- [x] 204/empty body y `null Data`
- [x] Cancelación (`CancellationToken`) y cancelación temprana (sin invocar API)

Estado Fase 2.3: Completada (suite de tests Facturas Service/ViewModel en verde).

#### Fase 2.4 - Services Commercial: Clientes
- [x] 401/403/429 (propagación de `StatusCode` y mensaje)
- [x] 204/empty body y `null Data`
- [x] Cancelación (`CancellationToken`) y cancelación temprana (si aplica)

Estado Fase 2.4: Completada (suite de tests Clientes Service/ViewModel en verde).

### Fase 3 - ViewModels (Otras features)

#### Reservaciones
- [x] IsLoading y `PropertyChanged` básicos (término de búsqueda, estado, fecha)
- [x] Errores en carga (diálogo de error coherente)
- [x] Filtros: estado + búsqueda (aplicación sobre colección)
- [x] CanExecute/debounce (paginación N/A) y estado vacío
- [x] Estadísticas y confirmar; ver/editar completados

Estado parcial Fase 3 (Reservaciones): Completada para núcleo + CanExecute/debounce y estado vacío; ver/editar completados.

#### Preparaciones
- [x] IsLoading y `PropertyChanged` (término de búsqueda, categoría, solo disponibles)
- [x] Errores en carga/búsqueda (diálogo de error coherente)
- [x] Búsqueda: actualización de `PreparacionesFiltradas`
- [x] Cambiar disponibilidad: éxito y error (feedback de diálogo)
- [x] CanExecute/debounce y paginación/estado vacío
- [x] Comandos secundarios: iniciar/completar/cancelar; ver/crear completados

Estado parcial Fase 3 (Preparaciones): Completada para comandos principales y no reentrancia; ver/crear completados.

#### Ingredientes
- [x] CanExecute/debounce en cargas y búsquedas (no reentrancia con ExecuteAsync)
- [x] Estados vacíos en carga/búsqueda; no duplicación de resultados
- [x] Errores por comando: buscar, alertas stock; validaciones de null en ver

Estado parcial Fase 3 (Ingredientes): Completada (suite de tests de ViewModel en verde).

#### Categorías
- [x] CanExecute/debounce en carga y búsqueda (no reentrancia)
- [x] Estados vacíos y métricas en cero
- [x] Errores/validaciones por comando (carga/búsqueda, filtros, activar/desactivar)

Estado parcial Fase 3 (Categorías): Completada (no reentrancia, estados vacíos y errores/validaciones en verde).

#### Clientes
- [x] CanExecute/debounce en carga y búsqueda (no reentrancia con IsBusy)
- [x] Estados vacíos en carga/búsqueda; TotalClientes en cero
- [x] Errores/validaciones: filtro vacío no invoca servicio; confirmación/cancelación en desactivar; éxito y error muestran diálogo adecuado

Estado parcial Fase 3 (Clientes): Completada para no reentrancia, estados vacíos y errores/validaciones.

#### Analytics
- [x] CanExecute/debounce en comandos de carga (no reentrancia con IsBusy)
- [x] Estados vacíos (TopProductos sin datos, colecciones sin errores)
- [x] Errores por comando (métricas/tiempos/ventas), tolerancia a mensajes

Estado parcial Fase 3 (Analytics): Completada (no reentrancia, estados vacíos y errores en verde).

#### Login
- [x] CanExecute/debounce en `LoginCommand` (no reentrancia con `IsLoading`)
- [x] Errores/validaciones: email/contraseña vacíos; error de servicio muestra mensaje; éxito navega a dashboard

Estado parcial Fase 3 (Login): Completada (no reentrancia y errores/validaciones en verde).

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


