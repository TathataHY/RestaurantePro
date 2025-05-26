namespace RestaurantePro.Domain.Core.SharedKernel.Results
{
    /// <summary>
    /// Extensiones asíncronas para trabajar con Result
    /// </summary>
    public static class AsyncResultExtensions
    {
        /// <summary>
        /// Convierte una tarea de Result a Result de manera asíncrona
        /// </summary>
        public static async Task<Result<T>> AsResultAsync<T>(this Task<T> task)
        {
            try
            {
                var result = await task;
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<T>(ex.Message);
            }
        }
        
        /// <summary>
        /// Ejecuta una función asíncrona y devuelve su resultado encapsulado en un Result
        /// </summary>
        public static async Task<Result> TryAsync(this Task task)
        {
            try
            {
                await task;
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta una función asíncrona con resultado y devuelve su resultado encapsulado en un Result
        /// </summary>
        public static async Task<Result<T>> TryAsync<T>(this Task<T> task)
        {
            try
            {
                var result = await task;
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<T>(ex.Message);
            }
        }
        
        /// <summary>
        /// Encadena múltiples operaciones asíncronas que devuelven Result
        /// </summary>
        public static async Task<Result> ThenAsync(this Task<Result> task, Func<Task<Result>> nextTask)
        {
            var result = await task;
            
            if (!result.Succeeded)
                return result;
                
            return await nextTask();
        }

        /// <summary>
        /// Encadena múltiples operaciones asíncronas que devuelven Result<T>
        /// </summary>
        public static async Task<Result<T>> ThenAsync<T>(this Task<Result> task, Func<Task<Result<T>>> nextTask)
        {
            var result = await task;
            
            if (!result.Succeeded)
                return Result.Failure<T>(result.Error ?? "Error desconocido");
                
            return await nextTask();
        }
        
        /// <summary>
        /// Combina múltiples resultados asíncronos en uno solo
        /// </summary>
        public static async Task<Result> CombineAsync(params Task<Result>[] tasks)
        {
            var results = await Task.WhenAll(tasks);
            return ResultExtensions.Combine(results);
        }
    }
} 