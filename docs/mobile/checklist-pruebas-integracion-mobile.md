## Checklist de mejora - Pruebas de integración (Frontend Móvil)

### Objetivo
Elevar la calidad y estabilidad de las pruebas de integración móviles asegurando flujos e2e reales, ejecución confiable en CI y mantenimiento simple.

### Alcance
- Proyecto: `tests/Frontend/RestaurantePro.Mobile.IntegrationTests`
- Base de servidor de pruebas: `MobileIntegrationTestFixture` (usa `WebApplicationFactory<Program>`)

### Estado rápido
- Compilación/host: OK en clases representativas tras correcciones de firmas y tipos.
- Integración real: Mixto (algunas usan `MockHttpMessageHandler`). Pendiente migración completa.
- Semilla de datos: Fixture con seed global (OK). Se añadió `TestCacheService` para DI estable en tests.

### Resultados parciales
- `DailyPreparationsEndToEndTests`: 6/6 pruebas correctas usando `ApiService` real.
- Suite completa integración móvil: 242/242 OK en ~10.4s (local).

### Checklist (técnico)
- [x] Agregar `FakeNavigationService` en proyecto de integración y referenciar donde se construye `AuthService`.
- [x] Actualizar tests para pasar `INavigationService` al `AuthService` (todas las ocurrencias).
- [x] Corregir tipos en tests de Comandas a `Features.Operations.Comandas.Models.CrearComandaRequest`.
- [x] Añadir `TestCacheService` para resolver `ICacheService` en DI durante tests.
- [x] Reemplazar mocks HTTP por `HttpClient` del fixture en tests marcados como integración.
- [x] Refactorizar `DailyPreparationsEndToEndTests` para usar `ApiService` real del `HttpClient` del fixture.
- [x] Mejorar aserciones según xUnit analyzers (`Assert.Contains`, `Assert.Empty`, etc.).
- [ ] Asegurar seed único y limpieza de estado por clase o caso donde aplique.
- [x] Ejecutar suite completa y capturar métricas (tiempo total, tests pasados/fallidos, estabilidad).

### Cobertura por flujo (según distribución funcional)
- Auth (login básico y no autorizado): cubierto (`AuthenticationTest`, `AuthService*IntegrationTests`).
- Preparaciones (cocina) E2E: cubierto (`DailyPreparationsEndToEndTests`).
- Mesas (listar, disponibles, estado ocupación): cubierto a nivel servicio; falta E2E de asignar/liberar.
- Comandas (crear, agregar producto, avanzar estado): cubierto a nivel servicio; falta E2E completo hasta facturar.
- Facturación (generar factura de venta): cubierto a nivel servicio; falta E2E desde comanda.
- Reservaciones (confirmación/consulta): cubierto a nivel servicio; falta E2E confirmación.
- Productos (consulta menú): cubierto a nivel servicio; E2E opcional smoke.
- Clientes (consulta básica): cubierto a nivel servicio.
- Tarjetas fidelización (uso/consulta): cubierto a nivel servicio; E2E opcional con factura.
- [ ] Documentar cobertura de flujos críticos (Auth, Preparaciones, Comandas, Mesas, Productos) y huecos restantes.

### Métricas objetivo
- Tiempo total: ≤ 5-8 min local, ≤ 12 min CI (indicativo).
- Flujos críticos e2e: ≥ 1 test por flujo principal (Auth, Preparaciones, Comandas, Mesas, Productos).
- Pruebas frágiles (flaky): 0.

### Riesgos y mitigaciones
- Cambios en contratos de servicios: centralizar helpers de construcción (factory) para `AuthService` y servicios dependientes.
- InMemory per-test: revisar aislamiento y, si procede, datos por clase con fixture compartido.

### Siguientes pasos
1) Crear `FakeNavigationService` y arreglar construcción de `AuthService` en todos los tests afectados.
2) Corregir namespaces/tipos en tests de Comandas.
3) Quitar mocks HTTP en pruebas de integración y usar `HttpClient` del fixture.
4) Ajustar aserciones a reglas xUnit.
5) Ejecutar suite y actualizar este documento con resultados.


