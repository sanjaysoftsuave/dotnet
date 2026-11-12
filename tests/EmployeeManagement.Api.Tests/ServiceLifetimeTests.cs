using EmployeeManagement.Api.Configuration;
using EmployeeManagement.Api.Services;
using EmployeeManagement.Domain.Repositories;
using EmployeeManagement.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.Api.Tests;

// Uses the real registrations from DependencyInjection.AddEmployeeManagement.
public sealed class ServiceLifetimeTests : IDisposable
{
    private readonly ServiceProvider _provider;

    public ServiceLifetimeTests()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Hiring:MaxDaysInFuture"] = "45" })
            .Build();

        _provider = new ServiceCollection()
            .AddEmployeeManagement(configuration)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    [Fact]
    public void Singleton_SameInstanceAcrossScopes()
    {
        using var first = _provider.CreateScope();
        using var second = _provider.CreateScope();

        Assert.Same(
            first.ServiceProvider.GetRequiredService<IEmployeeRepository>(),
            second.ServiceProvider.GetRequiredService<IEmployeeRepository>());
    }

    [Fact]
    public void Scoped_SameInstanceWithinScope_DifferentAcrossScopes()
    {
        using var first = _provider.CreateScope();
        using var second = _provider.CreateScope();

        var a = first.ServiceProvider.GetRequiredService<OrganizationService>();
        var b = first.ServiceProvider.GetRequiredService<OrganizationService>();
        var c = second.ServiceProvider.GetRequiredService<OrganizationService>();

        Assert.Same(a, b);
        Assert.NotSame(a, c);
    }

    [Fact]
    public void Transient_NewInstanceEveryTime()
    {
        using var scope = _provider.CreateScope();

        Assert.NotSame(
            scope.ServiceProvider.GetRequiredService<IEmployeeFactory>(),
            scope.ServiceProvider.GetRequiredService<IEmployeeFactory>());
    }

    [Fact]
    public void ScopedService_ResolvedFromRoot_IsRejected()
    {
        // ValidateScopes catches the bug where a scoped service would silently live as long as the app.
        Assert.Throws<InvalidOperationException>(() => _provider.GetRequiredService<ICorrelationContext>());
    }

    [Fact]
    public void Options_AreBoundFromConfiguration()
    {
        Assert.Equal(45, _provider.GetRequiredService<IOptions<HiringOptions>>().Value.MaxDaysInFuture);
    }

    public void Dispose() => _provider.Dispose();
}
