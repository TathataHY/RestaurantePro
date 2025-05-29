namespace RestaurantePro.Application.Core.Productos.Commands.EliminarProducto;

public class EliminarProductoCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }

    public EliminarProductoCommand(Guid id)
    {
        Id = id;
    }

    public EliminarProductoCommand() { }
} 