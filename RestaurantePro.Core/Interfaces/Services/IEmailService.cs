using System.Threading.Tasks;

namespace RestaurantePro.Core.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailWithTemplateAsync(string to, string templateName, object model);
    }
} 