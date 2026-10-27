namespace PayGate.Tests.Abstractions;

public sealed class PaymentResultTests
{
    [Fact]
    public void Success_carries_the_value()
    {
        var result = PaymentResult.Success("session");

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe("session");
        result.Error.ShouldBeNull();
        result.ToString().ShouldBe("Success(session)");
    }

    [Fact]
    public void Failure_carries_the_error_and_reading_the_value_throws()
    {
        var result = PaymentResult.Failure<string>(PaymentError.Validation("Customer phone is required."));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(PaymentErrorCode.Validation);
        Should.Throw<InvalidOperationException>(() => result.Value).Message.ShouldContain("Validation");
        result.ToString().ShouldBe("Failure(Validation: Customer phone is required.)");
    }

    [Fact]
    public void Converts_implicitly_from_a_value_and_from_an_error()
    {
        PaymentResult<string> success = "session";
        PaymentResult<string> failure = PaymentError.NotSupported("Lookup by merchant reference.");

        success.IsSuccess.ShouldBeTrue();
        failure.Error!.Code.ShouldBe(PaymentErrorCode.NotSupported);
    }

    [Fact]
    public void Match_calls_the_matching_branch()
    {
        PaymentResult<int> success = 42;
        PaymentResult<int> failure = PaymentError.OutcomeUnknown("Create timed out.");

        success.Match(v => $"ok {v}", e => $"error {e.Code}").ShouldBe("ok 42");
        failure.Match(v => $"ok {v}", e => $"error {e.Code}").ShouldBe("error OutcomeUnknown");
    }

    [Fact]
    public void Success_rejects_null()
    {
        Should.Throw<ArgumentNullException>(() => PaymentResult.Success<string>(null!));
    }

    [Fact]
    public void Error_factories_set_the_code_and_are_not_retryable()
    {
        PaymentError.OutcomeUnknown("x").ShouldSatisfyAllConditions(
            e => e.Code.ShouldBe(PaymentErrorCode.OutcomeUnknown),
            e => e.IsRetryable.ShouldBeFalse());
        PaymentError.DuplicateReference("x", "ERR_ORD_02").ProviderCode.ShouldBe("ERR_ORD_02");
    }
}
