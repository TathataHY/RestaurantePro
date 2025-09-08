# Plan de Pruebas de Integración - Web Pública

Estado: [x] Proyecto listo · [ ] Cobertura en progreso · [ ] Completo

## Objetivo
Validar flujos completos de la Web Pública (páginas + servicios + navegación + SEO) usando pruebas de integración con bUnit, asegurando que la UI reaccione correctamente ante respuestas reales simuladas (mock) de la API.

## Alcance
- Páginas: `Home`, `Menu`, `Promociones`, `Registro`, `Reviews`, `Contact`, `Reservas`, `About`, `Políticas`.
- Servicios: `MenuApiService`, `PromocionesApiService`, `ClientesPublicApiService`, `ReviewsApiService`, `ContactApiService`.
- Navegación/SEO: `Router`, `HeadOutlet`, canonical, Open Graph, querystrings.

## Convenciones
- Framework: bUnit + xUnit + FluentAssertions.
- Networking: `RichardSzalay.MockHttp` como stub de API (sin red real).
- Test naming: `ComponenteFlujoTests` / `PaginaFlujoTests` / `ServicioFlujoTests`.
- Data: usar fixtures simples y deterministas (listas pequeñas, casos de error 4xx/5xx, timeouts).

## Setup recomendado
- Registrar `HeadOutlet` y `Router` cuando aplique.
- Registrar `HttpClient` con `MockHttpMessageHandler` y `BaseAddress = http://localhost/`.
- Registrar servicios de la página bajo prueba (ej. `PromocionesApiService`).
- Verificar DOM final (markup) y side-effects (cambios de estado, deshabilitado de botones, etc.).

---

## Checklist por Páginas (Integración)

- Home
  - [ ] HeadContent: og:title/description/image y twitter:image con URLs https absolutas
  - [ ] No duplica metas en re-render; PageTitle coincide con encabezado

- Menú
  - [ ] Carga inicial: GET categorías → selecciona primera → GET productos; UI renderizada
  - [ ] Paginación: Next/Prev actualiza querystring y conserva filtros/categoría; evita doble click
  - [ ] Búsqueda: trimming y case-insensitive ("pizza", "  PIZZA  ") → misma llamada
  - [ ] Robustez: lista grande (200 ítems) sin bloquear; errores 500/timeout → UI estable

- Promociones
  - [ ] Loading con delay: “Cargando…” visible y desaparece tras data/error
  - [ ] Parámetros: combinar `ordenarPor`/`dirección` (FechaFin asc, Nombre desc) mapeados a la URL
  - [ ] Contenido: fechas locales correctas; HTML en descripción escapado

- Registro
  - [ ] POST 200 (Success=true) → confirmación y reset de formulario (defaults)
  - [ ] Validaciones: inválido bloquea envío; botón deshabilitado durante envío; timeout/500 → error sin reset
  - [ ] Body: serializa campos y fecha correctamente (UTC)

- Reviews
  - [ ] GET inicial lista reseñas; POST 201 inserta primero
  - [ ] 4xx/5xx → servicio retorna false y botón re-habilitado; botón deshabilitado durante envío
  - [ ] XSS: contenido se muestra escapado en DOM final

- Contacto
  - [ ] GET lista; POST ok → “Gracias…”; 4xx/timeout → mensaje de error
  - [ ] Doble submit: botón deshabilitado durante envío; trimming de campos
  - [ ] Validación: email inválido muestra mensaje del framework

- Reservas / Info Operativa
  - [ ] WhatsApp: `text=` codificado; enlaces externos con `target="_blank"` y `rel="noopener"`
  - [ ] `tel:`/`mailto:` válidos; mapa con `loading=lazy` y `referrerpolicy`

- Políticas / About
  - [ ] PageTitle coincide con encabezado; HeadContent no se duplica
  - [ ] About: navegación modal envuelve inicio/fin

---

## Pruebas de Servicios (Integración ligera)
- `MenuApiService`
  - [ ] Defaults sensatos (SoloActivos, PageSize) y caracteres especiales en filtros
  - [ ] Manejo de `TaskCanceledException`/timeout → lista vacía y UI estable
- `PromocionesApiService`
  - [ ] Encoding de `ordenarPor`/`dirección` con espacios/acentos
  - [ ] Data null → lista vacía sin romper UI
- `ClientesPublicApiService`
  - [ ] Content-Type application/json; body con fechas correcto; timeout retorna false
- `ReviewsApiService`
  - [ ] GET error → lista vacía; POST éxito/fallo según respuesta
- `ContactApiService`
  - [ ] Content-Type application/json en POST; GET error → lista vacía

---

## Navegación/SEO
- [ ] Router: NotFound muestra contenido esperado
- [ ] Canonical correcto por ruta; og:* no se duplica en re-render
- [ ] PageTitle consistente con encabezado visible

## Métricas
- % páginas cubiertas: TBA
- % servicios cubiertos: TBA
- Tiempo medio ejecución integración: < 8s

## Priorización
1. Menú (flujo base + paginación + búsqueda)
2. Registro (POST/validaciones/body)
3. Promociones (loading/ordenar por/dirección)
4. Reviews/Contacto (envío/errores)
5. Home/SEO/Router (consistencia general)

---

## Fase 8 – Integración (Plan de ejecución)

- Lote 1 (Alta)
  - [ ] Menú: flujo base (categorías→productos)
  - [ ] Menú: paginación con filtros + anti doble click
  - [ ] Registro: POST éxito/validaciones/timeout

- Lote 2 (Media)
  - [ ] Promociones: loading con delay + parámetros URL (FechaFin asc, Nombre desc)
  - [ ] Reviews: POST manejo de error y estado de botón; XSS integración
  - [ ] Contacto: POST/errores/doble submit

- Lote 3 (Media)
  - [ ] Home/SEO: og:image/twitter:image https; PageTitle y no duplicado
  - [ ] Router/NotFound: render mensaje/CTA
  - [ ] Reservas/Info: WhatsApp/maps/tel/mailto

---

## Próximos pasos
1. Crear/ajustar tests en `tests/Frontend/RestaurantePro.Web.Public.IntegrationTests` por lote.
2. Ejecutar con filtros por clase/nombre mientras se implementa cada lote.
3. Añadir reporte de cobertura (coverlet) y fijar umbral mínimo al cierre de la fase.
