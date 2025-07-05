-- Script para poblar datos demo de RestaurantePro
-- Ejecutar después de las migraciones en el entorno demo

USE [RestaurantePro_Demo]
GO

-- Limpiar datos existentes (excepto usuarios del sistema)
DELETE FROM [dbo].[Comandas] WHERE 1=1;
DELETE FROM [dbo].[Productos] WHERE 1=1;
DELETE FROM [dbo].[Categorias] WHERE 1=1;
DELETE FROM [dbo].[Mesas] WHERE 1=1;
DELETE FROM [dbo].[Ingredientes] WHERE 1=1;
DELETE FROM [dbo].[Proveedores] WHERE 1=1;

-- Insertar categorías de productos
INSERT INTO [dbo].[Categorias] ([Nombre], [Descripcion], [Activo], [FechaCreacion], [FechaModificacion])
VALUES 
('Entradas', 'Platos de entrada y aperitivos', 1, GETDATE(), GETDATE()),
('Platos Principales', 'Platos fuertes y especialidades', 1, GETDATE(), GETDATE()),
('Postres', 'Dulces y postres caseros', 1, GETDATE(), GETDATE()),
('Bebidas', 'Bebidas y refrescos', 1, GETDATE(), GETDATE()),
('Especialidades', 'Platos especiales del chef', 1, GETDATE(), GETDATE());

-- Insertar productos demo
INSERT INTO [dbo].[Productos] ([Nombre], [Descripcion], [Precio], [CategoriaId], [Activo], [FechaCreacion], [FechaModificacion])
VALUES 
-- Entradas
('Ensalada César', 'Lechuga romana, crutones, parmesano y aderezo César', 8500, 1, 1, GETDATE(), GETDATE()),
('Sopa del Día', 'Sopa casera preparada diariamente', 6500, 1, 1, GETDATE(), GETDATE()),
('Empanadas de Pino', 'Empanadas caseras con carne, cebolla y huevo', 12000, 1, 1, GETDATE(), GETDATE()),

-- Platos Principales
('Pasta Carbonara', 'Pasta con salsa cremosa, panceta y parmesano', 18500, 2, 1, GETDATE(), GETDATE()),
('Pescado del Día', 'Pescado fresco con vegetales de temporada', 22000, 2, 1, GETDATE(), GETDATE()),
('Bife a la Parrilla', 'Bife de res a la parrilla con papas fritas', 25000, 2, 1, GETDATE(), GETDATE()),
('Pollo al Horno', 'Pechuga de pollo al horno con hierbas', 18000, 2, 1, GETDATE(), GETDATE()),

-- Postres
('Tiramisú', 'Postre italiano con café y mascarpone', 8500, 3, 1, GETDATE(), GETDATE()),
('Cheesecake', 'Tarta de queso con frutos rojos', 7500, 3, 1, GETDATE(), GETDATE()),
('Flan Casero', 'Flan de caramelo casero', 6500, 3, 1, GETDATE(), GETDATE()),

-- Bebidas
('Limonada Natural', 'Limonada fresca con menta', 3500, 4, 1, GETDATE(), GETDATE()),
('Cerveza Nacional', 'Cerveza de barril', 4500, 4, 1, GETDATE(), GETDATE()),
('Vino Tinto', 'Vino tinto de la casa', 8500, 4, 1, GETDATE(), GETDATE()),

-- Especialidades
('Paella Valenciana', 'Paella tradicional con mariscos y pollo', 28000, 5, 1, GETDATE(), GETDATE()),
('Risotto de Hongos', 'Risotto cremoso con hongos silvestres', 19500, 5, 1, GETDATE(), GETDATE());

-- Insertar mesas
INSERT INTO [dbo].[Mesas] ([Numero], [Capacidad], [Estado], [Ubicacion], [Activo], [FechaCreacion], [FechaModificacion])
VALUES 
(1, 2, 'Disponible', 'Terraza', 1, GETDATE(), GETDATE()),
(2, 4, 'Disponible', 'Terraza', 1, GETDATE(), GETDATE()),
(3, 6, 'Disponible', 'Interior', 1, GETDATE(), GETDATE()),
(4, 2, 'Disponible', 'Interior', 1, GETDATE(), GETDATE()),
(5, 4, 'Disponible', 'Interior', 1, GETDATE(), GETDATE()),
(6, 8, 'Disponible', 'Sala Privada', 1, GETDATE(), GETDATE()),
(7, 2, 'Disponible', 'Barra', 1, GETDATE(), GETDATE()),
(8, 4, 'Disponible', 'Barra', 1, GETDATE(), GETDATE());

-- Insertar ingredientes demo
INSERT INTO [dbo].[Ingredientes] ([Nombre], [Descripcion], [StockActual], [StockMinimo], [UnidadMedida], [PrecioUnitario], [Activo], [FechaCreacion], [FechaModificacion])
VALUES 
('Harina', 'Harina de trigo para panadería', 50.0, 10.0, 'kg', 1200, 1, GETDATE(), GETDATE()),
('Carne de Res', 'Carne de res premium', 25.0, 5.0, 'kg', 8500, 1, GETDATE(), GETDATE()),
('Pollo', 'Pechugas de pollo frescas', 20.0, 3.0, 'kg', 4500, 1, GETDATE(), GETDATE()),
('Pescado', 'Pescado fresco del día', 15.0, 2.0, 'kg', 12000, 1, GETDATE(), GETDATE()),
('Lechuga', 'Lechuga romana fresca', 8.0, 1.0, 'kg', 2500, 1, GETDATE(), GETDATE()),
('Tomates', 'Tomates maduros', 12.0, 2.0, 'kg', 1800, 1, GETDATE(), GETDATE()),
('Cebollas', 'Cebollas blancas', 10.0, 1.5, 'kg', 1200, 1, GETDATE(), GETDATE()),
('Queso Parmesano', 'Queso parmesano rallado', 5.0, 0.5, 'kg', 15000, 1, GETDATE(), GETDATE()),
('Aceite de Oliva', 'Aceite de oliva extra virgen', 8.0, 1.0, 'L', 8500, 1, GETDATE(), GETDATE()),
('Vino Blanco', 'Vino blanco para cocina', 12.0, 2.0, 'L', 6500, 1, GETDATE(), GETDATE());

-- Insertar proveedores demo
INSERT INTO [dbo].[Proveedores] ([Nombre], [Rut], [Direccion], [Telefono], [Email], [Activo], [FechaCreacion], [FechaModificacion])
VALUES 
('Distribuidora Central', '12345678-9', 'Av. Providencia 1234, Santiago', '+56912345678', 'contacto@distcentral.cl', 1, GETDATE(), GETDATE()),
('Carnes Premium', '98765432-1', 'Av. Las Condes 567, Santiago', '+56987654321', 'ventas@carnespremium.cl', 1, GETDATE(), GETDATE()),
('Pescados Frescos', '45678912-3', 'Puerto de Valparaíso', '+56945678912', 'info@pescadosfrescos.cl', 1, GETDATE(), GETDATE()),
('Verduras Orgánicas', '78912345-6', 'Camino La Dehesa 890, Santiago', '+56978912345', 'pedidos@verdurasorganicas.cl', 1, GETDATE(), GETDATE());

-- Insertar algunas comandas de ejemplo (historial)
INSERT INTO [dbo].[Comandas] ([MesaId], [Estado], [Total], [Observaciones], [FechaCreacion], [FechaModificacion])
VALUES 
(1, 'Finalizada', 18500, 'Cliente satisfecho con el servicio', DATEADD(day, -1, GETDATE()), DATEADD(day, -1, GETDATE())),
(3, 'Finalizada', 45000, 'Celebración de cumpleaños', DATEADD(day, -2, GETDATE()), DATEADD(day, -2, GETDATE())),
(5, 'Finalizada', 32000, 'Cena de negocios', DATEADD(day, -3, GETDATE()), DATEADD(day, -3, GETDATE()));

PRINT 'Datos demo insertados correctamente en RestaurantePro_Demo'
GO 