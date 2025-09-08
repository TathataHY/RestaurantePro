# Plan de Pruebas Unitarias - Web Pública

Estado: [x] Proyectos creados · [ ] Cobertura en progreso · [x] Completo

## Objetivo
Definir y rastrear la cobertura de pruebas unitarias (xUnit + bUnit) para `RestaurantePro.Web.Public`, verificando comportamiento aislado de componentes y servicios sin depender de red ni navegador real.

## Alcance
- Componentes/Páginas `.razor`
- Servicios `.cs`
- Layout/Shared

## Convenciones
- Framework: xUnit + bUnit + FluentAssertions
- Aislamiento: sin llamadas reales a red; usar dobles/mocks.
- Nomenclatura: `NombreComponenteTests` / `NombreServicioTests`.

---

## Checklist por Páginas
- [x] Home.razor
  - [x] Renderiza hero y CTAs (Menú, Reseñas)
- [x] Menu.razor
  - [x] Muestra skeleton en carga
  - [x] Estado vacío sin productos
  - [x] Render de productos con data
  - [x] Paginación/filtros invocan servicio con parámetros esperados
  - [x] Muestra error en fallo del servicio
- [x] Promociones.razor (integración ligera inicial)
  - [x] Lista ítems con nombre y código
  - [x] Estado vacío
  - [x] Manejo de error
- [x] Registro.razor (integración ligera inicial)
  - [x] Envío exitoso muestra confirmación
  - [x] Error muestra mensaje
  - [x] Botón deshabilitado durante envío (ajustado a estado estable)
- [x] Reviews.razor
  - [x] Lista reseñas iniciales
  - [x] Crear reseña resetea formulario tras éxito
  - [x] Manejo de error en post
- [x] Contact.razor
  - [x] Validaciones de campos requeridos
  - [x] Envío exitoso
  - [x] Manejo de error
- [x] Reservas.razor
  - [x] Renderiza información y enlaces
- [x] Politicas.razor
  - [x] Renderiza contenido base
- [x] About.razor
  - [x] Renderiza contenido base

## Checklist Layout/Shared
- [x] NavMenu.razor (unit)
  - [x] Enlaces presentes (Inicio, Menú, Reseñas, Registro, Promociones)
  - [x] Toggle colapsa/expande (clase CSS)
  - [x] Inicio activo en raíz
- [x] MainLayout.razor
  - [x] Renderiza Footer y Body
- [x] App.razor
  - [x] NotFound personalizado
- [x] Footer.razor
  - [x] Renderiza horarios
  - [x] Enlaces sociales con href correctos
  - [x] Muestra año actual

## Checklist Servicios
- [x] PromocionesApiService
  - [x] Construye URL con soloVigentes/ordenarPor/direccion
  - [x] Devuelve lista vacía si ApiResponse es null
  - [x] Manejo de error de red
- [x] ClientesPublicApiService
  - [x] True si 2xx y Success=true
  - [x] False si 4xx/5xx o cuerpo null/Success=false
- [x] ReviewsApiService
  - [x] GET parsea correctamente
  - [x] POST éxito/fallo según respuesta
- [x] MenuApiService
  - [x] Mapea filtros/paginación a querystring
  - [x] Devuelve colecciones por defecto en null
- [x] ContactApiService
  - [x] POST éxito/fallo según Success

---

## Métricas
- % componentes cubiertos: TBA
- % servicios cubiertos: TBA
- Tiempo medio ejecución unit: < 5s

## Próximos pasos
1. Añadir unit tests para `Menu.razor` (skeleton, vacío, render).
2. Añadir unit tests para `PromocionesApiService` y `ClientesPublicApiService`.
3. Completar cobertura básica de `Reviews` y `Contact`.

## Nota
Las unit tests validan comportamientos pequeños y deterministas y actúan como documentación viva; evitan dependencias externas para detectar regresiones temprano.

---

## Pruebas adicionales sugeridas (fase 2)

- Menú
  - [x] Ordenar por precio asc/desc y popularidad reflejado en la UI
  - [x] Formato de precio con símbolo y miles
  - [x] Búsqueda de categorías: resultados y estado vacío

- Promociones
  - [ ] Meta SEO en HeadContent (title/description presentes)
  - [x] Verificación básica de contenido en UI

- Registro
  - [x] Reseteo de formulario tras éxito (valores default)

- Reseñas
  - [x] Nueva reseña aparece primera en la lista

- Contacto
  - [x] GET mensajes error → lista vacía y sin romper UI

- Servicios (extras)
  - [x] MenuApiService.ObtenerProductosPorCategoriaAsync con soloActivos=false incluye inactivos
  - [x] MenuApiService.BuscarCategoriasAsync codifica querystring
  - [x] ReviewsApiService GET error → lista vacía
  - [x] ContactApiService GET error → lista vacía

---

## Pruebas SEO y UI adicionales (fase 3)

- SEO HeadContent
  - [x] Home: canonical, og:title, og:description
  - [x] About: canonical, og:title, og:description
  - [x] Reservas: canonical, og:title, og:description
  - [x] Menu: canonical, og:title, og:description
  - [x] Registro: canonical, og:title, og:description

- Menú (UI/estado)
  - [x] Paginación: botones deshabilitados en primera/última página
  - [x] Paginación: texto "Página X de Y" correcto
  - [x] Productos inactivos muestran "Sin stock" y opacidad

- Contacto
  - [x] Render de lista con nombre/email/fecha formateada

- NavMenu
  - [x] Link activo en /menu
  - [x] Link activo en /resenas
  - [x] Link activo en /registro
  - [x] Link activo en /promociones
