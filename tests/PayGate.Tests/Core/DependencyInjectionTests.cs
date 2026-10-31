namespace PayGate.Tests.Core;

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using PayGate.Webhooks;

public sealed class DependencyInjectionTests
{
    private static readonly PaymentAccount _primary = new(PaymentProviderId.Kashier);
    private static readonly PaymentAccount _secondary = new(PaymentProviderId.Kashier, "secondary");

    [Fact]
    public void AddPayGate_registers_the_core_services_once()
    {
        var services = new ServiceCollection();

        services.AddPayGate();
        services.AddPayGate();

        services.Count(d => d.ServiceType == typeof(IPaymentGatewayResolver)).ShouldBe(1);
        services.Count(d => d.ServiceType == typeof(TimeProvider)).ShouldBe(1);
        services.Count(d => d.ServiceType == typeof(IPaymentCredentialsProvider<>)).ShouldBe(1);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<TimeProvider>().ShouldBeSameAs(TimeProvider.System);
    }

    [Fact]
    public void AddPayGate_keeps_a_clock_the_app_registered()
    {
        var services = new ServiceCollection();
        var clock = Substitute.For<TimeProvider>();
        services.AddSingleton(clock);

        services.AddPayGate();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<TimeProvider>().ShouldBeSameAs(clock);
    }

    [Fact]
    public void Gateways_resolve_by_account_and_as_keyed_services()
    {
        var services = new ServiceCollection();
        services.AddPayGate()
            .AddGateway(_primary, _ => Gateway(_primary))
            .AddGateway(_secondary, _ => Gateway(_secondary));

        using var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<IPaymentGatewayResolver>();

        resolver.GetGateway(_primary).Account.ShouldBe(_primary);
        resolver.GetGateway(_secondary).Account.ShouldBe(_secondary);
        provider.GetRequiredKeyedService<IPaymentGateway>("kashier").Account.ShouldBe(_primary);
        provider.GetRequiredKeyedService<IPaymentGateway>("kashier:secondary").Account.ShouldBe(_secondary);
    }

    [Fact]
    public void Gateways_are_created_on_each_resolution()
    {
        var services = new ServiceCollection();
        services.AddPayGate().AddGateway(_primary, _ => Gateway(_primary));

        using var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<IPaymentGatewayResolver>();

        resolver.GetGateway(_primary).ShouldNotBeSameAs(resolver.GetGateway(_primary));
    }

    [Fact]
    public void Registering_the_same_account_twice_throws()
    {
        var builder = new ServiceCollection().AddPayGate().AddGateway(_primary, _ => Gateway(_primary));

        Should.Throw<InvalidOperationException>(() => builder.AddGateway(_primary, _ => Gateway(_primary)))
            .Message.ShouldContain("'kashier'");
    }

    [Fact]
    public void AddGateway_requires_a_provider()
    {
        var builder = new ServiceCollection().AddPayGate();

        Should.Throw<ArgumentException>(() => builder.AddGateway(default, _ => Gateway(_primary)));
    }

    [Fact]
    public void A_missing_account_names_the_account_and_what_is_registered()
    {
        var services = new ServiceCollection();
        services.AddPayGate().AddGateway(_secondary, _ => Gateway(_secondary));

        using var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<IPaymentGatewayResolver>();

        resolver.TryGetGateway(_primary, out var gateway).ShouldBeFalse();
        gateway.ShouldBeNull();
        Should.Throw<InvalidOperationException>(() => resolver.GetGateway(_primary)).Message.ShouldSatisfyAllConditions(
            m => m.ShouldContain("'kashier'"),
            m => m.ShouldContain("Registered accounts: kashier:secondary"));
    }

    [Fact]
    public void A_missing_account_says_when_nothing_is_registered()
    {
        using var provider = new ServiceCollection().AddPayGate().Services.BuildServiceProvider();

        Should.Throw<InvalidOperationException>(() => provider.GetRequiredService<IPaymentGatewayResolver>().GetGateway(_primary))
            .Message.ShouldContain("Registered accounts: none");
    }

    [Fact]
    public void The_event_handler_is_scoped_and_a_second_registration_replaces_the_first()
    {
        var services = new ServiceCollection();
        services.AddPayGate()
            .AddEventHandler<FirstHandler>()
            .AddEventHandler<SecondHandler>();

        var descriptor = services.Where(d => d.ServiceType == typeof(IPaymentEventHandler)).ShouldHaveSingleItem();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IPaymentEventHandler>().ShouldBeOfType<SecondHandler>();
    }

    [Fact]
    public async Task Each_account_gets_its_own_named_options_through_the_default_credentials_provider_Async()
    {
        var services = new ServiceCollection();
        var builder = services.AddPayGate();
        builder.AddAccountOptions<TestAccountOptions>(_primary, o => o.MerchantId = "MID-1");
        builder.AddAccountOptions<TestAccountOptions>(_secondary, o => o.MerchantId = "MID-2");

        using var provider = services.BuildServiceProvider();
        var credentials = provider.GetRequiredService<IPaymentCredentialsProvider<TestAccountOptions>>();

        (await credentials.GetCredentialsAsync(_primary, TestContext.Current.CancellationToken)).MerchantId.ShouldBe("MID-1");
        (await credentials.GetCredentialsAsync(_secondary, TestContext.Current.CancellationToken)).MerchantId.ShouldBe("MID-2");
    }

    [Fact]
    public void Invalid_account_options_fail_at_startup()
    {
        var services = new ServiceCollection();
        services.AddPayGate()
            .AddAccountOptions<TestAccountOptions>(_primary, o => o.MerchantId = "MID-1")
            .Validate(o => !string.IsNullOrEmpty(o.ApiKey), "ApiKey is required.");

        using var provider = services.BuildServiceProvider();

        Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate())
            .Message.ShouldContain("ApiKey is required.");
    }

    [Fact]
    public async Task An_app_credentials_provider_replaces_the_default_Async()
    {
        var services = new ServiceCollection();
        services.AddPayGate();
        var custom = Substitute.For<IPaymentCredentialsProvider<TestAccountOptions>>();
        custom.GetCredentialsAsync(_primary, Arg.Any<CancellationToken>()).Returns(new TestAccountOptions { MerchantId = "from-db" });
        services.AddSingleton(custom);

        using var provider = services.BuildServiceProvider();
        var credentials = provider.GetRequiredService<IPaymentCredentialsProvider<TestAccountOptions>>();

        (await credentials.GetCredentialsAsync(_primary, TestContext.Current.CancellationToken)).MerchantId.ShouldBe("from-db");
    }

    [Fact]
    public void Account_options_redact_secrets_in_ToString()
    {
        var options = new TestAccountOptions
        {
            MerchantId = "MID-1",
            ApiKey = "sk_live_123",
            BaseUrlOverride = new Uri("http://localhost:9091"),
        };

        string text = options.ToString();

        text.ShouldBe("TestAccountOptions { Environment = Sandbox, BaseUrlOverride = http://localhost:9091/, MerchantId = MID-1, ApiKey = *** }");
        new TestAccountOptions().ToString().ShouldContain("ApiKey = (not set)");
    }

    private static IPaymentGateway Gateway(PaymentAccount account)
    {
        var gateway = Substitute.For<IPaymentGateway>();
        gateway.Account.Returns(account);
        return gateway;
    }

    public sealed class TestAccountOptions : PaymentAccountOptions
    {
        public string? MerchantId { get; set; }

        public string? ApiKey { get; set; }

        protected override void AppendMembers(StringBuilder builder) =>
            builder.Append(", MerchantId = ").Append(MerchantId).Append(", ApiKey = ").Append(Redact(ApiKey));
    }

    private sealed class FirstHandler : IPaymentEventHandler
    {
        public ValueTask<WebhookHandlingOutcome> HandleAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(WebhookHandlingOutcome.Processed);
    }

    private sealed class SecondHandler : IPaymentEventHandler
    {
        public ValueTask<WebhookHandlingOutcome> HandleAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(WebhookHandlingOutcome.Processed);
    }
}
