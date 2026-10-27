namespace PayGate.Tests.Abstractions;

using PayGate.Webhooks;

public sealed class ModelTests
{
    [Fact]
    public void Lookups_carry_their_kind_and_value()
    {
        PaymentLookup.ByProviderPaymentId("sess_1").ShouldSatisfyAllConditions(
            l => l.Kind.ShouldBe(PaymentLookupKind.ProviderPaymentId),
            l => l.Value.ShouldBe("sess_1"));
        PaymentLookup.ByMerchantReference("order-1-R2").ShouldSatisfyAllConditions(
            l => l.Kind.ShouldBe(PaymentLookupKind.MerchantReference),
            l => l.Value.ShouldBe("order-1-R2"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Lookups_reject_blank_ids(string value)
    {
        Should.Throw<ArgumentException>(() => PaymentLookup.ByProviderPaymentId(value));
        Should.Throw<ArgumentException>(() => PaymentLookup.ByMerchantReference(value));
    }

    [Fact]
    public void Next_action_kinds_match_their_types()
    {
        new RedirectAction { Url = new Uri("https://pay.example/s/1") }.Kind.ShouldBe(NextActionKind.Redirect);
        new ClientSdkAction { ClientSecret = "secret" }.Kind.ShouldBe(NextActionKind.ClientSdk);
        new DisplayReferenceAction { Code = "123456" }.Kind.ShouldBe(NextActionKind.DisplayReference);
        NoAction.Instance.Kind.ShouldBe(NextActionKind.None);
    }

    [Fact]
    public void GetProviderData_returns_the_typed_details_or_null()
    {
        var details = new PaymentDetails
        {
            ProviderPaymentId = "sess_1",
            Status = PaymentStatus.Succeeded,
            RawStatus = "SUCCESS",
            Amount = new Money(100m, Currency.Egp),
            RefundedAmount = Money.Zero(Currency.Egp),
            ProviderData = new SampleProviderData("settled"),
        };

        details.GetProviderData<SampleProviderData>()!.Note.ShouldBe("settled");
        details.GetProviderData<Uri>().ShouldBeNull();
    }

    [Fact]
    public void Capabilities_default_to_nothing_supported()
    {
        var capabilities = new PaymentCapabilities();

        capabilities.SupportedNextActions.ShouldBeEmpty();
        capabilities.SupportedPaymentMethods.ShouldBeEmpty();
        capabilities.LookupByMerchantReference.ShouldBeFalse();
        capabilities.IdempotentWrites.ShouldBeFalse();
        capabilities.PartialRefund.ShouldBeFalse();
        capabilities.MultipleRefunds.ShouldBeFalse();
        capabilities.PaymentExpiry.ShouldBeFalse();
        capabilities.PerRequestNotificationUrl.ShouldBeFalse();
        capabilities.MetadataPassthrough.ShouldBeFalse();
    }

    [Fact]
    public void Webhook_headers_and_query_are_read_case_insensitively()
    {
        var request = new WebhookRequest
        {
            Account = new PaymentAccount(PaymentProviderId.Paymob),
            Method = "POST",
            ReceivedAt = DateTimeOffset.UnixEpoch,
            Headers = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
            {
                ["X-Kashier-Signature"] = ["abc"],
                ["X-Empty"] = [],
            },
            Query = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { ["hmac"] = ["def", "ignored"] },
        };

        request.GetHeader("x-kashier-signature").ShouldBe("abc");
        request.GetHeader("X-Kashier-Signature").ShouldBe("abc");
        request.GetHeader("x-empty").ShouldBeNull();
        request.GetHeader("missing").ShouldBeNull();
        request.GetQuery("HMAC").ShouldBe("def");
    }

    [Fact]
    public void Return_callback_query_is_read_case_insensitively()
    {
        var request = new ReturnCallbackRequest
        {
            Account = new PaymentAccount(PaymentProviderId.Kashier),
            Query = new Dictionary<string, IReadOnlyList<string>> { ["merchantOrderId"] = ["order-1"] },
        };

        request.GetQuery("MERCHANTORDERID").ShouldBe("order-1");
    }

    [Fact]
    public void Parse_results_carry_events_only_when_verified()
    {
        var paymentEvent = new PaymentEvent
        {
            EventId = "evt_1",
            Account = new PaymentAccount(PaymentProviderId.Stripe),
            Type = PaymentEventType.PaymentSucceeded,
            Status = PaymentStatus.Succeeded,
            OccurredAt = DateTimeOffset.UnixEpoch,
        };

        WebhookParseResult.Verified([paymentEvent]).ShouldSatisfyAllConditions(
            r => r.Kind.ShouldBe(WebhookParseKind.Verified),
            r => r.Events.ShouldHaveSingleItem());
        WebhookParseResult.InvalidSignature("Signature mismatch.").ShouldSatisfyAllConditions(
            r => r.Kind.ShouldBe(WebhookParseKind.InvalidSignature),
            r => r.Events.ShouldBeEmpty(),
            r => r.Reason.ShouldBe("Signature mismatch."));
        WebhookParseResult.Malformed("Not JSON.").Kind.ShouldBe(WebhookParseKind.Malformed);
        WebhookParseResult.Ignored("customer.created").Kind.ShouldBe(WebhookParseKind.Ignored);
    }

    private sealed record SampleProviderData(string Note);
}
