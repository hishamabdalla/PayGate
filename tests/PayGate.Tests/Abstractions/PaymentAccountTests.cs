namespace PayGate.Tests.Abstractions;

public sealed class PaymentAccountTests
{
    [Fact]
    public void ProviderId_is_normalized_to_lowercase()
    {
        new PaymentProviderId(" Kashier ").ShouldBe(PaymentProviderId.Kashier);
        PaymentProviderId.Kashier.Value.ShouldBe("kashier");
    }

    [Theory]
    [InlineData("")]
    [InlineData("kash ier")]
    [InlineData("kashier:x")]
    [InlineData("kash_ier")]
    [InlineData("كاشير")]
    public void ProviderId_rejects_invalid_characters(string value)
    {
        Should.Throw<ArgumentException>(() => new PaymentProviderId(value));
    }

    [Fact]
    public void Default_account_formats_as_the_provider()
    {
        var account = new PaymentAccount(PaymentProviderId.Kashier);

        account.IsDefault.ShouldBeTrue();
        account.Name.ShouldBe(PaymentAccount.DefaultName);
        account.ToString().ShouldBe("kashier");
    }

    [Fact]
    public void Named_account_formats_as_provider_colon_name()
    {
        new PaymentAccount(PaymentProviderId.Kashier, "Secondary").ToString().ShouldBe("kashier:secondary");
    }

    [Theory]
    [InlineData("kashier")]
    [InlineData("kashier:secondary")]
    [InlineData("stripe:tenant_42")]
    [InlineData("my-provider:eu-1")]
    public void Parse_round_trips(string value)
    {
        PaymentAccount.Parse(value).ToString().ShouldBe(value);
    }

    [Fact]
    public void Parse_treats_an_explicit_default_name_as_the_default_account()
    {
        var account = PaymentAccount.Parse("Kashier:Default");

        account.ShouldBe(new PaymentAccount(PaymentProviderId.Kashier));
        account.ToString().ShouldBe("kashier");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(":secondary")]
    [InlineData("kashier:")]
    [InlineData("kashier:a:b")]
    [InlineData("ka shier")]
    public void TryParse_rejects_invalid_values(string? value)
    {
        PaymentAccount.TryParse(value, out var account).ShouldBeFalse();
        account.ShouldBe(default);
    }

    [Fact]
    public void Parse_throws_a_format_exception_for_invalid_values()
    {
        Should.Throw<FormatException>(() => PaymentAccount.Parse("kashier:"));
    }

    [Fact]
    public void Account_requires_a_provider()
    {
        Should.Throw<ArgumentException>(() => new PaymentAccount(default));
    }
}
