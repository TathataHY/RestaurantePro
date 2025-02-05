using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantePro.Core.Commands;
using RestaurantePro.Core.Common;
using RestaurantePro.Core.DTOs;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Services;
using RestaurantePro.Core.Specifications;
using RestaurantePro.Core.Validators;

namespace RestaurantePro.Core.Behaviors
{
    public class ComandaBehavior : IPipelineBehavior<CreateComandaCommand, Result<ComandaDto>>
    {
        private readonly IComandaCalculationService _calculationService;
        private readonly IValidator<Comanda> _validator;

        public ComandaBehavior(IComandaCalculationService calculationService, IValidator<Comanda> validator)
        {
            _calculationService = calculationService;
            _validator = validator;
        }

        public async Task<Result<ComandaDto>> Handle(
            CreateComandaCommand request,
            RequestHandlerDelegate<Result<ComandaDto>> next,
            CancellationToken cancellationToken)
        {
            var comanda = request.ToEntity();
            var validationResult = await _validator.ValidateAsync(comanda);
            if (!validationResult.IsValid)
                return Result<ComandaDto>.Failure(validationResult.Errors.First().ErrorMessage);

            foreach (var detalle in comanda.Detalles)
            {
                if (!await _calculationService.ValidarStockDisponible(detalle.PlatoId, detalle.Cantidad))
                    return Result<ComandaDto>.Failure($"Stock insuficiente para el plato {detalle.PlatoId}");
            }

            return await next();
        }
    }
} 