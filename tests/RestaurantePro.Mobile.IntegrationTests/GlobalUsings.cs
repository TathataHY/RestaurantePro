// Global usings para todas las pruebas de integración del proyecto móvil
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using System.Net.Http;
global using System.Text.Json;
global using Xunit;
global using Moq;
global using FluentAssertions;
global using AutoFixture;
global using AutoFixture.Xunit2;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
global using Microsoft.AspNetCore.Mvc.Testing;

// Namespaces de la biblioteca compartida móvil
global using RestaurantePro.Mobile.Core.Models.DTOs;
global using RestaurantePro.Mobile.Core.Models.ViewModels;
global using RestaurantePro.Mobile.Core.Services.Api;
global using RestaurantePro.Mobile.Core.Services.Authentication;
global using RestaurantePro.Mobile.Core.Services.Navigation;
global using RestaurantePro.Mobile.Core.Services.Dialog;
global using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;

// Namespaces del backend (para pruebas de integración)
global using RestaurantePro.Api; 