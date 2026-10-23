namespace PayGate.SandboxTests;

/// <summary>Placeholder for the Kashier sandbox suite (Phase 3). Skips unless sandbox credentials are present.</summary>
public sealed class KashierSandboxTests
{
    [Fact]
    [Trait("Category", "Sandbox")]
    public void Sandbox_credentials_are_available()
    {
        const string merchantIdVariable = "KASHIER_MERCHANT_ID";
        string? merchantId = Environment.GetEnvironmentVariable(merchantIdVariable);

        Assert.SkipWhen(string.IsNullOrWhiteSpace(merchantId), $"{merchantIdVariable} is not set; sandbox tests are skipped.");

        merchantId.ShouldNotBeNullOrWhiteSpace();
    }
}
