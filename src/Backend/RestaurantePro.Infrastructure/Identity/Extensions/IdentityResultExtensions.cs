using Microsoft.AspNetCore.Identity;
using RestaurantePro.Domain.Core.SharedKernel;
using System.Linq;

namespace RestaurantePro.Infrastructure.Identity.Extensions
{
    /// <summary>
    /// Extensiones para IdentityResult para facilitar la conversión a Result
    /// </summary>
    public static class IdentityResultExtensions
    {
        /// <summary>
        /// Convierte un IdentityResult a un Result
        /// </summary>
        public static Result ToResult(this IdentityResult identityResult)
        {
            if (identityResult == null)
                return Result.Failure("Resultado de identidad nulo");

            return identityResult.Succeeded
                ? Result.Success()
                : Result.Failure(identityResult.Errors.Select(e => e.Description).FirstOrDefault() ?? "Error de identidad");
        }

        /// <summary>
        /// Convierte un IdentityResult a un Result con mensaje personalizado en caso de éxito
        /// </summary>
        public static Result ToResult(this IdentityResult identityResult, string successMessage)
        {
            if (identityResult == null)
                return Result.Failure("Resultado de identidad nulo");

            return identityResult.Succeeded
                ? Result.Success(successMessage)
                : Result.Failure(identityResult.Errors.Select(e => e.Description).FirstOrDefault() ?? "Error de identidad");
        }

        /// <summary>
        /// Convierte un IdentityResult a un Result con valor de retorno
        /// </summary>
        public static Result<T> ToResult<T>(this IdentityResult identityResult, T value)
        {
            if (identityResult == null)
                return Result.Failure<T>("Resultado de identidad nulo");

            return identityResult.Succeeded
                ? Result.Success(value)
                : Result.Failure<T>(identityResult.Errors.Select(e => e.Description).FirstOrDefault() ?? "Error de identidad");
        }

        /// <summary>
        /// Obtiene todos los errores de un IdentityResult como una lista de strings
        /// </summary>
        public static string[] GetErrors(this IdentityResult identityResult)
        {
            if (identityResult == null || identityResult.Succeeded)
                return new string[0];

            return identityResult.Errors.Select(e => e.Description).ToArray();
        }

        /// <summary>
        /// Obtiene todos los errores de un IdentityResult como un string único
        /// </summary>
        public static string GetErrorsAsString(this IdentityResult identityResult, string separator = ", ")
        {
            if (identityResult == null || identityResult.Succeeded)
                return string.Empty;

            return string.Join(separator, identityResult.Errors.Select(e => e.Description));
        }
    }
} 