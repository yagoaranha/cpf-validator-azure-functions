using Xunit;


using CpfValidator.FunctionApp.Services;

namespace CpfValidator.Tests;

public sealed class CpfValidatorServiceTests
{
    private readonly ICpfValidator _validator = new CpfValidatorService();

    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData("11144477735")]
    [InlineData("12345678909")]
    [InlineData("93541134780")]
    [InlineData("39053344705")]
    [InlineData("01234567890")]
    public void ShouldValidateKnownValidCpfs(string cpf)
    {
        var r = _validator.Validate(cpf);
        Assert.True(r.IsValid);
        Assert.NotNull(r.NormalizedCpf);
        Assert.Equal(11, r.NormalizedCpf!.Length);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ShouldRejectEmptyCpf(string? cpf)
    {
        var r = _validator.Validate(cpf);
        Assert.False(r.IsValid);
        Assert.Equal("CPF_EMPTY", r.ReasonCode);
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    public void ShouldRejectRepeatedDigits(string cpf)
    {
        var r = _validator.Validate(cpf);
        Assert.False(r.IsValid);
        Assert.Equal("CPF_REPEATED", r.ReasonCode);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    public void ShouldRejectWrongLength(string cpf)
    {
        var r = _validator.Validate(cpf);
        Assert.False(r.IsValid);
        Assert.Equal("CPF_LENGTH", r.ReasonCode);
    }

    [Fact]
    public void ShouldRejectInvalidCheckDigits()
    {
        var r = _validator.Validate("52998224726"); // last digit changed
        Assert.False(r.IsValid);
        Assert.Contains("CPF_CHECKDIGIT", r.ReasonCode);
    }
}
