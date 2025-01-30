using RestaurantePro.App.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RestaurantePro.App.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private static bool _initialized = false;

        public DatabaseService()
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "RestaurantePro.db3");
            _database = new SQLiteAsyncConnection(databasePath);
            InitializeDatabaseAsync().ConfigureAwait(false);
        }

        private async Task InitializeDatabaseAsync()
        {
            if (_initialized)
                return;

            _initialized = true;

            try
            {
                await _database.CreateTableAsync<Mesa>();
                await _database.CreateTableAsync<Plato>();
                await _database.CreateTableAsync<Comanda>();
                await _database.CreateTableAsync<ComandaDetalle>();
                await _database.CreateTableAsync<Usuario>();

                // Crear usuarios por defecto si no existen
                await CreateDefaultUsers();

                // Inicializar platos peruanos
                await InitializePlatosAsync();

                // Inicializar mesas
                await InitializeMesasAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.InnerException?.Message}");
                throw;
            }
        }

        private async Task CreateDefaultUsers()
        {
            var adminUser = await _database.Table<Usuario>().FirstOrDefaultAsync(u => u.NombreUsuario == "admin");
            if (adminUser == null)
            {
                var defaultAdmin = new Usuario
                {
                    Nombre = "Administrador",
                    NombreUsuario = "admin",
                    Contraseña = "admin123",
                    Rol = RolUsuario.Administrador,
                    Activo = true
                };
                await SaveUsuarioAsync(defaultAdmin);
            }

            var meseroUser = await _database.Table<Usuario>().FirstOrDefaultAsync(u => u.NombreUsuario == "mesero");
            if (meseroUser == null)
            {
                var defaultMesero = new Usuario
                {
                    Nombre = "Mesero",
                    NombreUsuario = "mesero",
                    Contraseña = "mesero123",
                    Rol = RolUsuario.Mesero,
                    Activo = true
                };
                await _database.InsertAsync(defaultMesero);
            }

            var cocineroUser = await _database.Table<Usuario>().FirstOrDefaultAsync(u => u.NombreUsuario == "cocinero");
            if (cocineroUser == null)
            {
                var defaultCocinero = new Usuario
                {
                    Nombre = "Cocinero",
                    NombreUsuario = "cocinero",
                    Contraseña = "cocinero123",
                    Rol = RolUsuario.Cocinero,
                    Activo = true
                };
                await _database.InsertAsync(defaultCocinero);
            }
        }

        public async Task InitializePlatosAsync()
        {
            var existingPlatos = await _database.Table<Plato>().CountAsync();
            if (existingPlatos == 0)
            {
                var platos = new List<Plato>
                {
                    new Plato { Nombre = "Ceviche", Descripcion = "Plato de pescado marinado en jugo de limón", Precio = 25.0m, Disponible = true, Categoria = CategoriaPlato.Entrada },
                    new Plato { Nombre = "Lomo Saltado", Descripcion = "Plato de carne salteada con cebolla y tomate", Precio = 30.0m, Disponible = true, Categoria = CategoriaPlato.Principal },
                    new Plato { Nombre = "Ají de Gallina", Descripcion = "Plato de pollo desmenuzado en salsa de ají amarillo", Precio = 20.0m, Disponible = true, Categoria = CategoriaPlato.Principal },
                    new Plato { Nombre = "Anticuchos", Descripcion = "Brochetas de corazón de res", Precio = 15.0m, Disponible = true, Categoria = CategoriaPlato.Entrada },
                    new Plato { Nombre = "Papa a la Huancaína", Descripcion = "Papas con salsa de ají amarillo y queso", Precio = 10.0m, Disponible = true, Categoria = CategoriaPlato.Entrada },
                    new Plato { Nombre = "Rocoto Relleno", Descripcion = "Pimiento relleno de carne y vegetales", Precio = 18.0m, Disponible = true, Categoria = CategoriaPlato.Entrada },
                    new Plato { Nombre = "Pollo a la Brasa", Descripcion = "Pollo asado con especias", Precio = 35.0m, Disponible = true, Categoria = CategoriaPlato.Principal },
                    new Plato { Nombre = "Tacu Tacu", Descripcion = "Plato de arroz y frijoles fritos", Precio = 22.0m, Disponible = true, Categoria = CategoriaPlato.Principal },
                    new Plato { Nombre = "Seco de Cordero", Descripcion = "Plato de cordero guisado con cilantro", Precio = 28.0m, Disponible = true, Categoria = CategoriaPlato.Principal },
                    new Plato { Nombre = "Arroz con Pollo", Descripcion = "Plato de arroz verde con pollo", Precio = 20.0m, Disponible = true, Categoria = CategoriaPlato.Principal }
                };

                foreach (var plato in platos)
                {
                    await SavePlatoAsync(plato);
                }
            }
        }

        private async Task InitializeMesasAsync()
        {
            var existingMesas = await _database.Table<Mesa>().CountAsync();
            if (existingMesas == 0)
            {
                var mesas = new List<Mesa>
                {
                    new Mesa { Numero = "Mesa 1", Estado = EstadoMesa.Disponible },
                    new Mesa { Numero = "Mesa 2", Estado = EstadoMesa.Ocupada },
                    new Mesa { Numero = "Mesa 3", Estado = EstadoMesa.Reservada },
                    new Mesa { Numero = "Mesa 4", Estado = EstadoMesa.Disponible },
                    new Mesa { Numero = "Mesa 5", Estado = EstadoMesa.Ocupada }
                };

                foreach (var mesa in mesas)
                {
                    await SaveMesaAsync(mesa);
                }
            }
        }

        public Task<List<Plato>> GetPlatosAsync()
        {
            return _database.Table<Plato>().ToListAsync();
        }

        public Task<List<Plato>> GetPlatosDisponiblesAsync()
        {
            return _database.Table<Plato>().Where(p => p.Disponible).ToListAsync();
        }

        public Task<int> SavePlatoAsync(Plato plato)
        {
            if (plato.Id != 0)
            {
                return _database.UpdateAsync(plato);
            }
            else
            {
                return _database.InsertAsync(plato);
            }
        }

        public Task<int> DeletePlatoAsync(Plato plato)
        {
            return _database.DeleteAsync(plato);
        }

        public Task<Plato> GetPlatoByIdAsync(int id)
        {
            return _database.Table<Plato>().FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<List<Comanda>> GetComandasAsync()
        {
            return _database.Table<Comanda>().ToListAsync();
        }

        public Task<int> SaveComandaAsync(Comanda comanda)
        {
            if (comanda.Id != 0)
            {
                return _database.UpdateAsync(comanda);
            }
            else
            {
                return _database.InsertAsync(comanda);
            }
        }

        public Task<int> DeleteComandaAsync(Comanda comanda)
        {
            return _database.DeleteAsync(comanda);
        }

        public Task<Comanda> GetComandaByIdAsync(int id)
        {
            return _database.Table<Comanda>().FirstOrDefaultAsync(c => c.Id == id);
        }

        public Task<List<Mesa>> GetMesasAsync()
        {
            return _database.Table<Mesa>().ToListAsync();
        }

        public Task<List<Mesa>> GetMesasDisponiblesAsync()
        {
            return _database.Table<Mesa>().Where(m => m.Estado == EstadoMesa.Disponible).ToListAsync();
        }

        public Task<int> SaveMesaAsync(Mesa mesa)
        {
            if (mesa.Id != 0)
            {
                return _database.UpdateAsync(mesa);
            }
            else
            {
                return _database.InsertAsync(mesa);
            }
        }

        public Task<int> DeleteMesaAsync(Mesa mesa)
        {
            return _database.DeleteAsync(mesa);
        }

        public Task<Mesa> GetMesaByIdAsync(int id)
        {
            return _database.Table<Mesa>().FirstOrDefaultAsync(m => m.Id == id);
        }

        public Task<Usuario> GetUsuarioAsync(string nombreUsuario, string contraseña)
        {
            return _database.Table<Usuario>().FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Contraseña == contraseña);
        }

        public Task<Usuario> GetUsuarioByIdAsync(int id)
        {
            return _database.Table<Usuario>().FirstOrDefaultAsync(u => u.Id == id);
        }

        public Task<List<Usuario>> GetUsuariosAsync()
        {
            return _database.Table<Usuario>().ToListAsync();
        }

        public Task<int> SaveUsuarioAsync(Usuario usuario)
        {
            if (usuario.Id != 0)
            {
                return _database.UpdateAsync(usuario);
            }
            else
            {
                return _database.InsertAsync(usuario);
            }
        }

        public Task<int> DeleteUsuarioAsync(Usuario usuario)
        {
            return _database.DeleteAsync(usuario);
        }

        public async Task<List<ReporteVenta>> GetReporteVentasAsync(DateTime startDate, DateTime endDate)
        {
            var comandas = await _database.Table<Comanda>()
                .Where(c => c.FechaHora >= startDate && c.FechaHora <= endDate)
                .ToListAsync();

            var reporteVentas = comandas
                .GroupBy(c => c.FechaHora.Date)
                .Select(g => new ReporteVenta
                {
                    Fecha = g.Key,
                    TotalVentas = g.Sum(c => c.Total),
                    TotalPlatosVendidos = g.Sum(c => c.Detalles.Count)
                })
                .ToList();

            return reporteVentas;
        }

        public async Task<List<ComandaDetalle>> GetComandaDetallesAsync(int comandaId)
        {
            var detalles = await _database.Table<ComandaDetalle>().Where(d => d.ComandaId == comandaId).ToListAsync();
            foreach (var detalle in detalles)
            {
                var plato = await _database.Table<Plato>().FirstOrDefaultAsync(p => p.Id == detalle.PlatoId);
                if (plato != null)
                {
                    detalle.PlatoNombre = plato.Nombre;
                }
            }
            return detalles;
        }

        public Task<int> SaveComandaDetalleAsync(ComandaDetalle comandaDetalle)
        {
            if (comandaDetalle.Id != 0)
            {
                return _database.UpdateAsync(comandaDetalle);
            }
            else
            {
                return _database.InsertAsync(comandaDetalle);
            }
        }
    }
}