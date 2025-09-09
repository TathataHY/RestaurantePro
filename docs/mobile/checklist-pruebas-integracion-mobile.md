## Checklist de mejora - Pruebas de integración (Frontend Móvil)

### Objetivo
Elevar la calidad y estabilidad de las pruebas de integración móviles asegurando flujos e2e reales, ejecución confiable en CI y mantenimiento simple.

### Alcance
- Proyecto: `tests/Frontend/RestaurantePro.Mobile.IntegrationTests`
- Base de servidor de pruebas: `MobileIntegrationTestFixture` (usa `WebApplicationFactory<Program>`)

### Estado rápido
- Compilación/host: OK en clases representativas tras correcciones de firmas y tipos.
- Integración real: OK (flujos E2E reales: Auth, Preparaciones, Comandas, Mesas, Facturación, Reservaciones, Productos).
- Semilla de datos: Fixture con seed global (OK). Se añadió `TestCacheService` para DI estable en tests.

### Resultados finales
- `DailyPreparationsEndToEndTests`: 6/6 pruebas correctas usando `ApiService` real.
- E2E añadidos y correctos: Comandas, Mesas, Facturación (desde comanda), Reservaciones (confirmación), Productos (smoke listar/detalle).
- Suite completa integración móvil: 246/246 OK en 15.7s (local). TRX: `tests/Frontend/RestaurantePro.Mobile.IntegrationTests/TestResults/TestResults.trx`.
- Pruebas nuevas: Expiración/refresh de token (2/2 OK).
- Seguridad/Roles: sin token 401 (OK); Mesero puede listar Facturas (200), acceso a `Usuarios` devuelve 403 (OK).

### Checklist (técnico)
- [x] Agregar `FakeNavigationService` en proyecto de integración y referenciar donde se construye `AuthService`.
- [x] Actualizar tests para pasar `INavigationService` al `AuthService` (todas las ocurrencias).
- [x] Corregir tipos en tests de Comandas a `Features.Operations.Comandas.Models.CrearComandaRequest`.
- [x] Añadir `TestCacheService` para resolver `ICacheService` en DI durante tests.
- [x] Reemplazar mocks HTTP por `HttpClient` del fixture en tests marcados como integración.
- [x] Refactorizar `DailyPreparationsEndToEndTests` para usar `ApiService` real del `HttpClient` del fixture.
- [x] Mejorar aserciones según xUnit analyzers (`Assert.Contains`, `Assert.Empty`, etc.).
- [x] Asegurar seed único y limpieza de estado por clase o caso donde aplique.
- [x] Ejecutar suite completa y capturar métricas (tiempo total, tests pasados/fallidos, estabilidad).

### Cobertura por flujo (según distribución funcional)
- Auth (login básico y no autorizado): cubierto (`AuthenticationTest`, `AuthService*IntegrationTests`).
- Preparaciones (cocina) E2E: cubierto (`DailyPreparationsEndToEndTests`).
- Mesas (asignar/liberar): E2E cubierto (`MesasEndToEndTests`).
- Comandas (crear, agregar producto, avanzar/finalizar): E2E cubierto (`ComandasEndToEndTests`).
- Facturación (generar factura desde comanda): E2E cubierto (`FacturacionEndToEndTests`).
- Reservaciones (confirmación): E2E cubierto (`ReservacionesEndToEndTests`).
- Productos (consulta menú): E2E smoke cubierto (`ProductosSmokeEndToEndTests`).
- Clientes (consulta básica): cubierto a nivel servicio.
- Tarjetas fidelización (uso/consulta): cubierto a nivel servicio; E2E opcional con factura.
- [x] Documentar cobertura de flujos críticos (Auth, Preparaciones, Comandas, Mesas, Productos) y huecos restantes.

### Métricas objetivo
- Tiempo total: ≤ 5-8 min local, ≤ 12 min CI (indicativo).
- Flujos críticos e2e: ≥ 1 test por flujo principal (Auth, Preparaciones, Comandas, Mesas, Productos).
- Pruebas frágiles (flaky): 0.

### Riesgos y mitigaciones
- Cambios en contratos de servicios: centralizar helpers de construcción (factory) para `AuthService` y servicios dependientes.
- InMemory per-test: revisar aislamiento y, si procede, datos por clase con fixture compartido.

### Siguientes pasos
1) Resiliencia cliente: simular 500/timeout y verificar manejo en ViewModels.
2) Datos deterministas: builders para crear datos mínimos por caso.
3) Limpieza: confirmar limpieza por clase de `FakeSecureStorageService` (instancia por test).
4) CI/CD: pipeline que ejecute suite y publique TRX/HTML.
3) Estados inválidos: transiciones no permitidas en Comandas/Mesas y mensajes de error.
4) Resiliencia cliente: simular 500/timeout y verificar manejo en ViewModels.
5) Datos deterministas: builders para crear datos mínimos por caso.
6) Limpieza: confirmar limpieza por clase de `FakeSecureStorageService` (instancia por test).
7) CI/CD: pipeline que ejecute suite y publique TRX/HTML.


