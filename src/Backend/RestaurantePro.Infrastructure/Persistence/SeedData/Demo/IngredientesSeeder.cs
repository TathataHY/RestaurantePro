using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Demo
{
    /// <summary>
    /// Seeder que crea ingredientes de demostración con stocks y costos realistas
    /// Usa enums reales del sistema y datos típicos de inventario de restaurante
    /// </summary>
    public class IngredientesSeeder : ISeedData
    {
        public string Name => "Ingredientes Demo";
        public int Order => 190;
        public bool IsDevOnly => false;
        public bool IsCritical => false;

        public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("🥕 Iniciando seed de ingredientes demo...");

            var ingredientes = new List<(string nombre, string codigo, string descripcion, UnidadMedida unidad, decimal stockMinimo, decimal stock, decimal costo, RotacionIngrediente rotacion, TemporadaIngrediente temporada)>
            {
                // CARNES Y PROTEÍNAS (Rotación Alta - Productos perecederos)
                ("Carne de Res Premium", "CAR-001", "Cortes de res premium para parrilla", UnidadMedida.Kilogramo, 5.0m, 12.5m, 280.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Pollo Entero Fresco", "POL-001", "Pollo entero fresco de granja", UnidadMedida.Kilogramo, 8.0m, 15.0m, 85.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Pechuga de Pollo", "PEC-001", "Pechuga de pollo sin hueso", UnidadMedida.Kilogramo, 6.0m, 10.5m, 120.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Salmón Fresco", "SAL-001", "Filete de salmón del Atlántico", UnidadMedida.Kilogramo, 3.0m, 8.0m, 450.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Camarones Grandes", "CAM-001", "Camarones U-12 frescos", UnidadMedida.Kilogramo, 2.0m, 5.5m, 380.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Tocino Ahumado", "TOC-001", "Tocino ahumado en rebanadas", UnidadMedida.Kilogramo, 2.0m, 4.0m, 195.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),

                // VERDURAS Y HORTALIZAS (Rotación Alta - Frescas)
                ("Tomate Rojo", "TOM-001", "Tomate rojo maduro para ensaladas", UnidadMedida.Kilogramo, 10.0m, 25.0m, 35.00m, RotacionIngrediente.Alta, TemporadaIngrediente.Verano),
                ("Lechuga Romana", "LEC-001", "Lechuga romana fresca", UnidadMedida.Piezas, 20.0m, 35.0m, 12.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Cebolla Blanca", "CEB-001", "Cebolla blanca mediana", UnidadMedida.Kilogramo, 8.0m, 15.0m, 28.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Aguacate Hass", "AGU-001", "Aguacate Hass maduro", UnidadMedida.Piezas, 50.0m, 80.0m, 18.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Pimiento Rojo", "PIM-001", "Pimiento rojo dulce", UnidadMedida.Kilogramo, 5.0m, 12.0m, 65.00m, RotacionIngrediente.Media, TemporadaIngrediente.Verano),
                ("Zanahoria", "ZAN-001", "Zanahoria fresca", UnidadMedida.Kilogramo, 8.0m, 18.0m, 22.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Apio", "APE-001", "Apio fresco en ramas", UnidadMedida.Piezas, 15.0m, 25.0m, 8.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Espinaca Baby", "ESP-001", "Espinaca baby orgánica", UnidadMedida.Kilogramo, 3.0m, 8.0m, 95.00m, RotacionIngrediente.Alta, TemporadaIngrediente.Invierno),

                // LÁCTEOS Y HUEVOS (Rotación Alta - Perecederos)
                ("Queso Mozzarella", "MOZ-001", "Queso mozzarella fresco", UnidadMedida.Kilogramo, 4.0m, 8.5m, 180.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Queso Parmesano", "PAR-001", "Queso parmesano rallado", UnidadMedida.Kilogramo, 2.0m, 3.5m, 420.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Huevos Frescos", "HUE-001", "Huevos frescos de granja", UnidadMedida.Piezas, 100.0m, 180.0m, 4.50m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Crema para Batir", "CRE-001", "Crema para batir 35% grasa", UnidadMedida.Litro, 3.0m, 6.0m, 85.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Leche Entera", "LEH-001", "Leche entera pasteurizada", UnidadMedida.Litro, 8.0m, 15.0m, 25.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Mantequilla", "MAN-001", "Mantequilla sin sal", UnidadMedida.Kilogramo, 2.0m, 4.0m, 165.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),

                // GRANOS Y CEREALES (Rotación Media - Duraderos)
                ("Arroz Blanco", "ARR-001", "Arroz blanco grano largo", UnidadMedida.Kilogramo, 15.0m, 35.0m, 32.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Pasta Spaghetti", "SPA-001", "Pasta spaghetti italiana", UnidadMedida.Kilogramo, 8.0m, 20.0m, 45.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Harina de Trigo", "HAR-001", "Harina de trigo todo uso", UnidadMedida.Kilogramo, 10.0m, 25.0m, 28.00m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño),
                ("Frijoles Negros", "FRI-001", "Frijoles negros secos", UnidadMedida.Kilogramo, 5.0m, 12.0m, 38.00m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño),
                ("Quinoa", "QUI-001", "Quinoa orgánica", UnidadMedida.Kilogramo, 3.0m, 8.0m, 125.00m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño),

                // ACEITES Y CONDIMENTOS (Rotación Media - Duraderos)
                ("Aceite de Oliva Extra Virgen", "AOL-001", "Aceite de oliva extra virgen", UnidadMedida.Litro, 3.0m, 8.0m, 185.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Aceite Vegetal", "AVE-001", "Aceite vegetal para freír", UnidadMedida.Litro, 5.0m, 12.0m, 45.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Sal de Mar", "SAL-002", "Sal de mar fina", UnidadMedida.Kilogramo, 2.0m, 8.0m, 15.00m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño),
                ("Pimienta Negra", "PIM-002", "Pimienta negra molida", UnidadMedida.Kilogramo, 1.0m, 2.5m, 280.00m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño),
                ("Ajo Fresco", "AJO-001", "Ajo fresco en cabezas", UnidadMedida.Kilogramo, 3.0m, 8.0m, 65.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),

                // HIERBAS Y ESPECIAS (Rotación Baja - Duraderas)
                ("Albahaca Fresca", "ALB-001", "Albahaca fresca en maceta", UnidadMedida.Piezas, 10.0m, 15.0m, 25.00m, RotacionIngrediente.Alta, TemporadaIngrediente.Verano),
                ("Orégano Seco", "ORE-001", "Orégano seco molido", UnidadMedida.Kilogramo, 0.5m, 1.5m, 350.00m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño),
                ("Tomillo Fresco", "TOM-002", "Tomillo fresco", UnidadMedida.Paquete, 5.0m, 12.0m, 15.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Perejil Fresco", "PER-001", "Perejil fresco en manojo", UnidadMedida.Piezas, 20.0m, 35.0m, 8.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Cilantro Fresco", "CIL-001", "Cilantro fresco en manojo", UnidadMedida.Piezas, 15.0m, 25.0m, 8.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),

                // BEBIDAS Y LÍQUIDOS (Rotación Media)
                ("Vino Blanco para Cocinar", "VIN-001", "Vino blanco seco para cocina", UnidadMedida.Litro, 2.0m, 5.0m, 95.00m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño),
                ("Caldo de Pollo", "CAL-001", "Caldo de pollo concentrado", UnidadMedida.Litro, 5.0m, 12.0m, 35.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Agua Mineral", "AGM-001", "Agua mineral natural", UnidadMedida.Litro, 20.0m, 50.0m, 12.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),

                // PRODUCTOS CONGELADOS (Rotación Media)
                ("Papas Fritas Congeladas", "PAP-001", "Papas fritas precocidas congeladas", UnidadMedida.Kilogramo, 10.0m, 25.0m, 65.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                ("Verduras Mixtas Congeladas", "VER-001", "Mezcla de verduras congeladas", UnidadMedida.Kilogramo, 8.0m, 18.0m, 45.00m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),

                // PANADERÍA Y POSTRES (Rotación Alta)
                ("Pan para Hamburguesa", "PAN-001", "Pan para hamburguesa artesanal", UnidadMedida.Piezas, 50.0m, 100.0m, 8.50m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Tortillas de Harina", "TOR-001", "Tortillas de harina frescas", UnidadMedida.Paquete, 20.0m, 40.0m, 15.00m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño),
                ("Chocolate Amargo", "CHO-001", "Chocolate amargo 70% cacao", UnidadMedida.Kilogramo, 2.0m, 5.0m, 285.00m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño),

                // PRODUCTOS ESPECIALES (Rotación Baja)
                ("Trufa Negra", "TRU-001", "Trufa negra premium", UnidadMedida.Gramo, 50.0m, 100.0m, 15.00m, RotacionIngrediente.Baja, TemporadaIngrediente.Invierno),
                ("Azafrán", "AZA-001", "Azafrán en hebras", UnidadMedida.Gramo, 10.0m, 25.0m, 25.00m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño),
                ("Aceite de Trufa", "ATR-001", "Aceite de oliva con trufa", UnidadMedida.Mililitro, 250.0m, 500.0m, 2.50m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño)
            };

            var ingredientesAgregados = 0;
            var ingredientesOmitidos = 0;

            foreach (var (nombre, codigo, descripcion, unidad, stockMinimo, stock, costo, rotacion, temporada) in ingredientes)
            {
                // Verificar si ya existe por código
                var existeIngrediente = await context.Ingredientes
                    .AnyAsync(i => i.Codigo == codigo, cancellationToken);

                if (!existeIngrediente)
                {
                    try
                    {
                        // Crear el ingrediente usando el método estático
                        var ingrediente = Ingrediente.Crear(
                            nombre,
                            codigo,
                            descripcion,
                            unidad,
                            stockMinimo,
                            stock,
                            rotacion,
                            temporada
                        );

                        // Asignar ID determinístico
                        var guidBytes = System.Text.Encoding.UTF8.GetBytes($"Ingrediente_{codigo}");
                        var hash = System.Security.Cryptography.MD5.HashData(guidBytes);
                        ingrediente.GetType().GetProperty("Id")?.SetValue(ingrediente, new Guid(hash));

                        // Asignar costo promedio usando reflexión
                        ingrediente.GetType().GetProperty("CostoPromedio")?.SetValue(ingrediente, costo);

                        context.Ingredientes.Add(ingrediente);
                        ingredientesAgregados++;

                        logger.LogInformation("  ✅ Agregado: {Nombre} ({Codigo}) - {Stock} {Unidad} - ${Costo} - {Rotacion}/{Temporada}", 
                            nombre, codigo, stock, unidad, costo, rotacion, temporada);
                    }
                    catch (Exception ex)
                    {
                        ingredientesOmitidos++;
                        logger.LogWarning("  ⚠️ Error al crear ingrediente {Nombre} ({Codigo}): {Error}", 
                            nombre, codigo, ex.Message);
                    }
                }
                else
                {
                    ingredientesOmitidos++;
                    logger.LogDebug("  ⏭️ Omitido ingrediente existente: {Nombre} ({Codigo})", nombre, codigo);
                }
            }

            if (ingredientesAgregados > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("💾 Guardados {Count} ingredientes en BD", ingredientesAgregados);
            }

            logger.LogInformation("🎯 Seed de ingredientes completado - ✅ {Agregados} agregados, ⏭️ {Omitidos} omitidos", 
                ingredientesAgregados, ingredientesOmitidos);
        }

        public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken)
        {
            // Considerar que existe si tenemos al menos 40 ingredientes
            var ingredienteCount = await context.Ingredientes.CountAsync(cancellationToken);
            return ingredienteCount >= 40;
        }
    }
} 