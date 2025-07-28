// Global usings para todas las pruebas unitarias del proyecto móvil
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using Xunit;
global using Moq;
global using FluentAssertions;
global using AutoFixture;
global using AutoFixture.Xunit2;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Http;
global using Microsoft.Extensions.Logging;
global using System.Text.Json;
global using System.Net.Http;

// Namespaces de la biblioteca compartida
global using RestaurantePro.Mobile.Core.Models.DTOs;
global using RestaurantePro.Mobile.Core.Models.ViewModels;
global using RestaurantePro.Mobile.Core.Services.Api;
global using RestaurantePro.Mobile.Core.Services.Authentication;
global using RestaurantePro.Mobile.Core.Services.Navigation;
global using RestaurantePro.Mobile.Core.Services.Dialog;
global using RestaurantePro.Mobile.Core.Services.Platform;
global using RestaurantePro.Mobile.Core.Models.Common;
global using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels; 