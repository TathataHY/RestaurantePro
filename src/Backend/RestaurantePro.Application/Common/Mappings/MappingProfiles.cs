using AutoMapper;
using RestaurantePro.Application.Features.Categorias.Commands.CrearCategoria;
using RestaurantePro.Application.Features.Categorias.Dtos;
using RestaurantePro.Application.Features.Comandas.Dtos;
using RestaurantePro.Application.Features.Mesas.Commands.CrearMesa;
using RestaurantePro.Application.Features.Mesas.Dtos;
using RestaurantePro.Application.Features.Pagos.Dtos;
using RestaurantePro.Application.Features.Productos.Commands.ActualizarProducto;
using RestaurantePro.Application.Features.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Features.Productos.Dtos;
using RestaurantePro.Application.Features.Usuarios.Commands.RegistrarUsuario;
using RestaurantePro.Application.Features.Usuarios.Dtos;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using RestaurantePro.Infrastructure.Identity;
using System.Linq;

namespace RestaurantePro.Application.Common.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Usuarios
            CreateMap<RegistrarUsuarioCommand, Usuario>()
                .ForMember(dest => dest.FechaRegistro, opt => opt.Ignore())
                .ForMember(dest => dest.UltimoLogin, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Usuario, UsuarioAutenticadoDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore());

            CreateMap<Usuario, UsuarioPerfilDto>();

            // Identity
            CreateMap<RegistrarUsuarioCommand, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore());

            CreateMap<ApplicationUser, UsuarioAutenticadoDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore());

            CreateMap<ApplicationUser, UsuarioPerfilDto>();

            // Productos
            CreateMap<Producto, ProductoDto>()
                .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria.Nombre))
                .ForMember(dest => dest.Ingredientes, opt => opt.MapFrom(src => src.Ingredientes));

            CreateMap<IngredienteProducto, IngredienteProductoDto>()
                .ForMember(dest => dest.NombreIngrediente, opt => opt.MapFrom(src => src.Ingrediente.Nombre));

            CreateMap<CrearProductoCommand, Producto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.UltimaModificacion, opt => opt.Ignore())
                .ForMember(dest => dest.Categoria, opt => opt.Ignore())
                .ForMember(dest => dest.Ingredientes, opt => opt.Ignore());

            CreateMap<IngredienteProductoInfo, IngredienteProducto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductoId, opt => opt.Ignore())
                .ForMember(dest => dest.Producto, opt => opt.Ignore())
                .ForMember(dest => dest.Ingrediente, opt => opt.Ignore());

            CreateMap<ActualizarProductoCommand, Producto>()
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.UltimaModificacion, opt => opt.Ignore())
                .ForMember(dest => dest.Categoria, opt => opt.Ignore())
                .ForMember(dest => dest.Ingredientes, opt => opt.Ignore());

            // Categorías
            CreateMap<Categoria, CategoriaDto>()
                .ForMember(dest => dest.CantidadProductos, opt => opt.Ignore());

            CreateMap<CrearCategoriaCommand, Categoria>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.UltimaModificacion, opt => opt.Ignore())
                .ForMember(dest => dest.Productos, opt => opt.Ignore());

            // Mesas
            CreateMap<Mesa, MesaDto>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()));

            CreateMap<CrearMesaCommand, Mesa>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.UltimaModificacion, opt => opt.Ignore())
                .ForMember(dest => dest.QrCode, opt => opt.Ignore())
                .ForMember(dest => dest.Comandas, opt => opt.Ignore())
                .ForMember(dest => dest.Reservaciones, opt => opt.Ignore());

            // Comandas
            CreateMap<Comanda, ComandaDto>()
                .ForMember(dest => dest.NumeroMesa, opt => opt.MapFrom(src => src.Mesa.Numero.ToString()))
                .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario.Nombre))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => src.UltimaModificacion))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.ComandaDetalles));

            CreateMap<ComandaDetalle, ComandaDetalleDto>()
                .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto.Nombre))
                .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.NotasEspeciales))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.Personalizaciones, opt => opt.MapFrom(src => src.Personalizaciones));

            CreateMap<ComandaDetallePersonalizacion, ComandaDetallePersonalizacionDto>()
                .ForMember(dest => dest.NombreIngrediente, opt => opt.MapFrom(src => src.Ingrediente.Nombre))
                .ForMember(dest => dest.Agregar, opt => opt.MapFrom(src => src.Accion == AccionPersonalizacion.Agregar))
                .ForMember(dest => dest.Quitar, opt => opt.MapFrom(src => src.Accion == AccionPersonalizacion.Quitar))
                .ForMember(dest => dest.PrecioExtra, opt => opt.MapFrom(src => CalcularPrecioExtra(src)));

            // Pagos
            CreateMap<Pago, PagoDto>()
                .ForMember(dest => dest.NumeroComanda, opt => opt.MapFrom(src => src.Comanda.NumeroComanda))
                .ForMember(dest => dest.FormaPago, opt => opt.MapFrom(src => src.MetodoPago.ToString()))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.ReferenciaPago, opt => opt.MapFrom(src => src.Referencia))
                .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario.Nombre));
        }

        private decimal CalcularPrecioExtra(ComandaDetallePersonalizacion personalizacion)
        {
            // En un sistema real, aquí se calcularía el precio extra basado en el ingrediente y la acción
            // Por ahora, devolvemos un valor fijo solo para demostración
            if (personalizacion.Accion == AccionPersonalizacion.Agregar)
                return personalizacion.Cantidad * 5; // Precio adicional por agregar ingrediente
            else
                return 0;
        }
    }
} 