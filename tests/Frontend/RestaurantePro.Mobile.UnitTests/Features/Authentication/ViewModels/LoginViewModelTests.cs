using System.ComponentModel;
using FluentAssertions;
using Moq;
using RestaurantePro.Mobile.Core.Features.Authentication.ViewModels;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Navigation;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Features.Authentication.ViewModels;

public class LoginViewModelTests
{
    private readonly Mock<IAuthService> _mockAuth = new();
    private readonly Mock<INavigationService> _mockNav = new();

    private LoginViewModel CreateVm() => new LoginViewModel(_mockAuth.Object, _mockNav.Object);

    [Fact]
    public async Task LoginAsync_WithEmptyEmail_ShouldShowError()
    {
        var vm = CreateVm();
        vm.Email = "";
        vm.Password = "secret";

        await vm.LoginCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("email");
        _mockAuth.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ShouldShowError()
    {
        var vm = CreateVm();
        vm.Email = "user@mail.com";
        vm.Password = "";

        await vm.LoginCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("contraseña");
        _mockAuth.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_Success_ShouldNavigateToDashboard()
    {
        _mockAuth
            .Setup(a => a.LoginAsync("user@mail.com", "secret", true))
            .ReturnsAsync(ApiResponse<AuthResponse>.SuccessResponse(new AuthResponse { Token = "t" }));

        var vm = CreateVm();
        vm.Email = "user@mail.com";
        vm.Password = "secret";
        vm.Recordarme = true;

        await vm.LoginCommand.ExecuteAsync(null);

        vm.HasError.Should().BeFalse();
        _mockNav.Verify(n => n.NavigateToAsync("//main/dashboard"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenBusy_ShouldNotInvoke()
    {
        var vm = CreateVm();
        vm.IsLoading = true;
        await vm.LoginCommand.ExecuteAsync(null);
        _mockAuth.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void PropertyChanged_Email_Password_Recordarme_ShouldRaise()
    {
        var vm = CreateVm();
        var changes = new List<string>();
        ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) => changes.Add(e.PropertyName!);

        vm.Email = "a@b.com";
        vm.Password = "x";
        vm.Recordarme = true;

        changes.Should().Contain(nameof(LoginViewModel.Email));
        changes.Should().Contain(nameof(LoginViewModel.Password));
        changes.Should().Contain(nameof(LoginViewModel.Recordarme));
    }

    [Fact]
    public void ClearFields_ShouldResetInputsAndError()
    {
        var vm = CreateVm();
        vm.Email = "a@b.com";
        vm.Password = "x";
        vm.Recordarme = true;
        vm.SetError("e");

        vm.ClearFieldsCommand.Execute(null);

        vm.Email.Should().BeEmpty();
        vm.Password.Should().BeEmpty();
        vm.Recordarme.Should().BeFalse();
        vm.HasError.Should().BeFalse();
        vm.ErrorMessage.Should().BeEmpty();
    }
} 