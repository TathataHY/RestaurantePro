namespace RestaurantePro.Application.Common.Interfaces
{
    public interface IUsuarioActualService
    {
        int GetUsuarioId();
        string GetUsuarioEmail();
        string GetUsuarioRol();
        bool EsUsuarioAutenticado();
    }
} 