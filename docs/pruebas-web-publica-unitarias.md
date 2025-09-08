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
- [ ] Home.razor
  - [x] Renderiza hero y CTAs (Menú, Reseñas)
- [ ] Menu.razor
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
- [ ] Reviews.razor
  - [x] Lista reseñas iniciales
  - [x] Crear reseña resetea formulario tras éxito
  - [x] Manejo de error en post
- [ ] Contact.razor
  - [x] Validaciones de campos requeridos
  - [x] Envío exitoso
  - [x] Manejo de error
- [ ] Reservas.razor
  - [x] Renderiza información y enlaces
- [ ] Politicas.razor
  - [x] Renderiza contenido base
- [ ] About.razor
  - [x] Renderiza contenido base

## Checklist Layout/Shared
- [x] NavMenu.razor (unit)
  - [x] Enlaces presentes (Inicio, Menú, Reseñas, Registro, Promociones)
  - [x] Toggle colapsa/expande (clase CSS)
- [ ] MainLayout.razor
  - [x] Renderiza Footer y Body
- [ ] App.razor
  - [x] NotFound personalizado
- [ ] Footer.razor
  - [x] Renderiza horarios
  - [x] Enlaces sociales con href correctos

## Checklist Servicios
- [ ] PromocionesApiService
  - [x] Construye URL con soloVigentes/ordenarPor/direccion
  - [x] Devuelve lista vacía si ApiResponse es null
  - [x] Manejo de error de red
- [ ] ClientesPublicApiService
  - [x] True si 2xx y Success=true
  - [x] False si 4xx/5xx o cuerpo null/Success=false
- [ ] ReviewsApiService
  - [x] GET parsea correctamente
  - [x] POST éxito/fallo según respuesta
- [ ] MenuApiService
  - [x] Mapea filtros/paginación a querystring
  - [x] Devuelve colecciones por defecto en null
- [ ] ContactApiService
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
