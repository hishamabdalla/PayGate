namespace PayGate.Tests.Abstractions;

using System.Text;
using PayGate.Webhooks;

public sealed class RedactionTests
{
    private static readonly Customer _customer = new()
    {
        Reference = "cust-1",
        Name = "Mona Ali",
        Email = "mona@example.com",
        Phone = "+201001234567",
    };

    [Fact]
    public void Customer_redacts_name_email_and_phone()
    {
        string text = _customer.ToString();

        text.ShouldContain("cust-1");
        text.ShouldNotContain("Mona");
        text.ShouldNotContain("mona@example.com");
        text.ShouldNotContain("+201001234567");
    }

    [Fact]
    public void A_request_with_a_customer_does_not_leak_it()
    {
        var request = new CreatePaymentRequest
        {
            MerchantReference = "order-1",
            Amount = new Money(100m, Currency.Egp),
            Customer = _customer,
        };

        request.ToString().ShouldNotContain("mona@example.com");
    }

    [Fact]
    public void ClientSdkAction_redacts_the_client_secret()
    {
        var action = new ClientSdkAction { ClientSecret = "pi_123_secret_456", PublicKey = "pk_test_1" };

        string text = action.ToString();

        text.ShouldNotContain("pi_123_secret_456");
        text.ShouldContain("pk_test_1");
        text.ShouldContain("ClientSdk");
    }

    [Fact]
    public void PaymentSession_leaves_out_the_raw_response()
    {
        var session = new PaymentSession
        {
            ProviderPaymentId = "sess_1",
            Status = PaymentStatus.Pending,
            NextAction = new ClientSdkAction { ClientSecret = "pi_123_secret_456" },
            RawResponse = "{\"client_secret\":\"pi_123_secret_456\"}",
        };

        string text = session.ToString();

        text.ShouldContain("sess_1");
        text.ShouldNotContain("pi_123_secret_456");
    }

    [Fact]
    public void PaymentEvent_leaves_out_metadata_and_the_raw_payload()
    {
        var paymentEvent = new PaymentEvent
        {
            EventId = "evt_1",
            Account = new PaymentAccount(PaymentProviderId.Kashier),
            Type = PaymentEventType.PaymentSucceeded,
            Status = PaymentStatus.Succeeded,
            OccurredAt = DateTimeOffset.UnixEpoch,
            Metadata = new Dictionary<string, string> { ["email"] = "mona@example.com" },
            RawPayload = "{\"maskedCard\":\"411111******1111\"}",
        };

        string text = paymentEvent.ToString();

        text.ShouldContain("evt_1");
        text.ShouldNotContain("mona@example.com");
        text.ShouldNotContain("411111");
    }

    [Fact]
    public void WebhookRequest_shows_only_the_body_length()
    {
        var request = new WebhookRequest
        {
            Account = new PaymentAccount(PaymentProviderId.Kashier),
            Method = "POST",
            Body = Encoding.UTF8.GetBytes("payload-secret"),
            ReceivedAt = DateTimeOffset.UnixEpoch,
        };

        string text = request.ToString();

        text.ShouldNotContain("payload-secret");
        text.ShouldContain("14 bytes");
    }
}
