using AutoMapper;
using RestaurantePro.Application.Features.Usuarios.Commands.RegistrarUsuario;
using RestaurantePro.Application.Features.Usuarios.Dtos;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Application.Features.Inventario.Dtos;

namespace RestaurantePro.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeos de Usuario
            CreateMap<RegistrarUsuarioCommand, Usuario>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.UltimaModificacion, opt => opt.Ignore())
                .ForMember(dest => dest.Activo, opt => opt.Ignore());

            CreateMap<Usuario, UsuarioAutenticadoDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore());
                
            CreateMap<Usuario, UsuarioPerfilDto>();

            // Inventario mappings
            CreateMap<Domain.Entities.Inventario, InventarioDto>()
                .ForMember(d => d.NombreIngrediente, opt => opt.MapFrom(s => s.Ingrediente.Nombre))
                .ForMember(d => d.Categoria, opt => opt.MapFrom(s => s.Ingrediente.Categoria))
                .ForMember(d => d.UltimoInventarioPor, opt => opt.MapFrom(s => s.UltimoInventarioPor));

            CreateMap<Domain.Entities.MovimientoInventario, MovimientoDto>()
                .ForMember(d => d.Ingrediente, opt => opt.MapFrom(s => s.Ingrediente.Nombre))
                .ForMember(d => d.Usuario, opt => opt.MapFrom(s => s.UsuarioId));

            CreateMap<Domain.Entities.Proveedor, ProveedorDto>();

            CreateMap<Domain.Entities.OrdenCompra, OrdenCompraDto>()
                .ForMember(d => d.NombreProveedor, opt => opt.MapFrom(s => s.Proveedor.Nombre))
                .ForMember(d => d.NombreUsuario, opt => opt.MapFrom(s => s.UsuarioId));

            CreateMap<Domain.Entities.DetalleOrdenCompra, DetalleOrdenCompraDto>()
                .ForMember(d => d.NombreIngrediente, opt => opt.MapFrom(s => s.Ingrediente.Nombre));
        }
    }
} 