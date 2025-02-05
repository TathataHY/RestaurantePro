using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Core.Interfaces.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Core.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IRestauranteContext _context;

        public TransactionBehavior(IRestauranteContext context)
        {
            _context = context;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                await _context.BeginTransactionAsync();
                var response = await next();
                await _context.CommitTransactionAsync();
                return response;
            }
            catch (Exception)
            {
                await _context.RollbackTransactionAsync();
                throw;
            }
        }
    }
}