namespace PayGate.Tests.Abstractions;

using static PayGate.PaymentStatus;

public sealed class PaymentStatusTests
{
    private static readonly PaymentStatus[] _all = Enum.GetValues<PaymentStatus>();

    // The documented transition table, written out independently of the implementation.
    private static readonly Dictionary<PaymentStatus, PaymentStatus[]> _allowed = new()
    {
        [Unknown] = [Pending, RequiresAction, Processing, Authorized, Succeeded, Failed, Canceled, Expired, PartiallyRefunded, Refunded],
        [Pending] = [RequiresAction, Processing, Authorized, Succeeded, Failed, Canceled, Expired, PartiallyRefunded, Refunded],
        [RequiresAction] = [Pending, Processing, Authorized, Succeeded, Failed, Canceled, Expired, PartiallyRefunded, Refunded],
        [Processing] = [Pending, RequiresAction, Authorized, Succeeded, Failed, Canceled, Expired, PartiallyRefunded, Refunded],
        [Authorized] = [Succeeded, Failed, Canceled, Expired],
        [Succeeded] = [PartiallyRefunded, Refunded, Canceled],
        [PartiallyRefunded] = [Refunded],
        [Failed] = [Succeeded, Authorized],
        [Canceled] = [],
        [Expired] = [],
        [Refunded] = [],
    };

    public static TheoryData<PaymentStatus, PaymentStatus, bool> Transitions
    {
        get
        {
            var data = new TheoryData<PaymentStatus, PaymentStatus, bool>();
            foreach (var from in _all)
            {
                foreach (var to in _all)
                {
                    data.Add(from, to, _allowed[from].Contains(to));
                }
            }

            return data;
        }
    }

    [Fact]
    public void The_expected_table_covers_every_status()
    {
        _allowed.Keys.Order().ShouldBe(_all.Order());
    }

    [Theory]
    [MemberData(nameof(Transitions))]
    public void CanTransitionTo_follows_the_transition_table(PaymentStatus from, PaymentStatus to, bool expected)
    {
        from.CanTransitionTo(to).ShouldBe(expected);
    }

    [Theory]
    [InlineData(Failed, true)]
    [InlineData(Canceled, true)]
    [InlineData(Expired, true)]
    [InlineData(Refunded, true)]
    [InlineData(Unknown, false)]
    [InlineData(Pending, false)]
    [InlineData(RequiresAction, false)]
    [InlineData(Processing, false)]
    [InlineData(Authorized, false)]
    [InlineData(Succeeded, false)]
    [InlineData(PartiallyRefunded, false)]
    public void IsTerminal_matches_the_documented_rules(PaymentStatus status, bool expected)
    {
        status.IsTerminal().ShouldBe(expected);
    }

    [Fact]
    public void A_late_success_after_a_failure_is_applied()
    {
        Failed.CanTransitionTo(Succeeded).ShouldBeTrue();
    }

    [Fact]
    public void An_unknown_status_never_overwrites_a_known_one()
    {
        _all.ShouldAllBe(status => !status.CanTransitionTo(Unknown));
    }
}
