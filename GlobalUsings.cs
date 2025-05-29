#nullable enable
global using System;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;
global using System.Linq;
global using System.Reflection;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Text.RegularExpressions;
global using System.Globalization;

// Microsoft Extensions
global using Microsoft.Extensions.Logging;

// Core
global using RestaurantePro.Domain.Core.Base;
global using RestaurantePro.Domain.Core.Base.Interfaces;
global using RestaurantePro.Domain.Core.Base.Events;
global using RestaurantePro.Domain.Core.SharedKernel.Interfaces;

// Domain core imports - SharedKernel Results
global using RestaurantePro.Domain.Core.SharedKernel.Results;

// Domain core imports - SharedKernel Validation
global using RestaurantePro.Domain.Core.SharedKernel.Validation;

// Domain Operaciones - Comandas
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.Enums;
global using RestaurantePro.Domain.Operaciones.Comandas.Builders;

// Domain Operaciones - Reservaciones
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Enums; 