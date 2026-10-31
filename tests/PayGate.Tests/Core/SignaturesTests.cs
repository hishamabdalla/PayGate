namespace PayGate.Tests.Core;

using System.Text;
using PayGate.Providers;

public sealed class SignaturesTests
{
    // RFC 4231 test case 2.
    private const string _key = "Jefe";
    private const string _data = "what do ya want for nothing?";
    private const string _hmacSha256Hex = "5bdcc146bf60754e6a042426089575c75a003f089d2739839dec58b964ec3843";
    private const string _hmacSha512Hex =
        "164b7a7bfcf819e2e395fbe73b56e0a387bd64222e831fd610270cd7ea2505549758bf75c05a994a6d034f65f8f0e6fdcaeab1a34d4a6b4b636e070a38bce737";

    // FIPS 180-2 example: SHA-256("abc").
    private const string _sha256AbcHex = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

    [Fact]
    public void HmacSha256_matches_the_rfc_4231_vector()
    {
        Convert.ToHexStringLower(Signatures.HmacSha256(_key, _data)).ShouldBe(_hmacSha256Hex);
        Convert.ToHexStringLower(Signatures.HmacSha256(Encoding.UTF8.GetBytes(_key), Encoding.UTF8.GetBytes(_data))).ShouldBe(_hmacSha256Hex);
    }

    [Fact]
    public void HmacSha512_matches_the_rfc_4231_vector()
    {
        Convert.ToHexStringLower(Signatures.HmacSha512(_key, _data)).ShouldBe(_hmacSha512Hex);
        Convert.ToHexStringLower(Signatures.HmacSha512(Encoding.UTF8.GetBytes(_key), Encoding.UTF8.GetBytes(_data))).ShouldBe(_hmacSha512Hex);
    }

    [Fact]
    public void Sha256_matches_the_fips_vector()
    {
        Convert.ToHexStringLower(Signatures.Sha256("abc")).ShouldBe(_sha256AbcHex);
        Convert.ToHexStringLower(Signatures.Sha256("abc"u8)).ShouldBe(_sha256AbcHex);
    }

    [Fact]
    public void BodyHash_is_lowercase_hex_sha256()
    {
        Signatures.BodyHash("abc"u8).ShouldBe(_sha256AbcHex);
    }

    [Theory]
    [InlineData(_hmacSha256Hex)]
    [InlineData("5BDCC146BF60754E6A042426089575C75A003F089D2739839DEC58B964EC3843")]
    public void FixedTimeEqualsHex_accepts_a_matching_signature_in_either_case(string signature)
    {
        Signatures.FixedTimeEqualsHex(Signatures.HmacSha256(_key, _data), signature).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("5bdcc146")] // too short
    [InlineData("5bdcc146bf60754e6a042426089575c75a003f089d2739839dec58b964ec3844")] // last byte differs
    [InlineData("zzdcc146bf60754e6a042426089575c75a003f089d2739839dec58b964ec3843")] // not hex
    [InlineData("5bdcc146bf60754e6a042426089575c75a003f089d2739839dec58b964ec384")] // odd length
    [InlineData(" 5bdcc146bf60754e6a042426089575c75a003f089d2739839dec58b964ec3843")] // padded
    public void FixedTimeEqualsHex_rejects_anything_else_without_throwing(string? signature)
    {
        Signatures.FixedTimeEqualsHex(Signatures.HmacSha256(_key, _data), signature).ShouldBeFalse();
    }

    [Fact]
    public void FixedTimeEqualsHex_rejects_an_empty_expected_value()
    {
        Signatures.FixedTimeEqualsHex([], string.Empty).ShouldBeFalse();
    }

    [Fact]
    public void FixedTimeEqualsHex_handles_macs_larger_than_the_stack_buffer()
    {
        byte[] large = Enumerable.Range(0, 200).Select(i => (byte)i).ToArray();

        Signatures.FixedTimeEqualsHex(large, Convert.ToHexString(large)).ShouldBeTrue();
        Signatures.FixedTimeEqualsBase64(large, Convert.ToBase64String(large)).ShouldBeTrue();
    }

    [Fact]
    public void FixedTimeEqualsBase64_accepts_a_matching_signature()
    {
        byte[] mac = Signatures.HmacSha256(_key, _data);

        Signatures.FixedTimeEqualsBase64(mac, Convert.ToBase64String(mac)).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not base64!")]
    [InlineData("W9zBRr9gdU5qBCQmCJV1x1oAPwidJzmDnexYuWTsOEI=")] // last byte differs
    [InlineData("W9zBRr9gdU5qBCQm")] // too short
    [InlineData("W9zBRr9gdU5qBCQmCJV1x1oAPwidJzmDnexYuWTsOENBQkM=")] // too long
    public void FixedTimeEqualsBase64_rejects_anything_else_without_throwing(string? signature)
    {
        Signatures.FixedTimeEqualsBase64(Signatures.HmacSha256(_key, _data), signature).ShouldBeFalse();
    }
}
