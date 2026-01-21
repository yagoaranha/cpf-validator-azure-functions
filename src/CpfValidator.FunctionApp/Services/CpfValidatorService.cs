using System.Text.RegularExpressions;

namespace CpfValidator.FunctionApp.Services;

/// <summary>
/// CPF validation based on the official check-digit algorithm.
/// - Strips any non-digit characters.
/// - Rejects CPFs with all digits equal.
/// - Validates both check digits.
/// </summary>
public sealed class CpfValidatorService : ICpfValidator
{
    private static readonly Regex NonDigits = new("\\D+", RegexOptions.Compiled);

    public CpfValidationResult Validate(string? cpfInput)
    {
        if (string.IsNullOrWhiteSpace(cpfInput))
        {
            return new CpfValidationResult(
                Input: cpfInput,
                NormalizedCpf: null,
                IsValid: false,
                ReasonCode: "CPF_EMPTY",
                Reason: "CPF não informado."
            );
        }

        var normalized = NonDigits.Replace(cpfInput.Trim(), string.Empty);

        if (normalized.Length != 11)
        {
            return new CpfValidationResult(
                Input: cpfInput,
                NormalizedCpf: normalized,
                IsValid: false,
                ReasonCode: "CPF_LENGTH",
                Reason: "CPF deve conter 11 dígitos (após normalização)."
            );
        }

        // Reject repeated digits (e.g., 00000000000, 11111111111)
        if (normalized.All(c => c == normalized[0]))
        {
            return new CpfValidationResult(
                Input: cpfInput,
                NormalizedCpf: normalized,
                IsValid: false,
                ReasonCode: "CPF_REPEATED",
                Reason: "CPF inválido (dígitos repetidos)."
            );
        }

        if (!normalized.All(char.IsDigit))
        {
            return new CpfValidationResult(
                Input: cpfInput,
                NormalizedCpf: normalized,
                IsValid: false,
                ReasonCode: "CPF_FORMAT",
                Reason: "CPF contém caracteres inválidos."
            );
        }

        var digits = normalized.Select(c => c - '0').ToArray();

        var d1 = CalculateCheckDigit(digits, length: 9);
        if (d1 != digits[9])
        {
            return new CpfValidationResult(
                Input: cpfInput,
                NormalizedCpf: normalized,
                IsValid: false,
                ReasonCode: "CPF_CHECKDIGIT_1",
                Reason: "Primeiro dígito verificador inválido."
            );
        }

        var d2 = CalculateCheckDigit(digits, length: 10);
        if (d2 != digits[10])
        {
            return new CpfValidationResult(
                Input: cpfInput,
                NormalizedCpf: normalized,
                IsValid: false,
                ReasonCode: "CPF_CHECKDIGIT_2",
                Reason: "Segundo dígito verificador inválido."
            );
        }

        return new CpfValidationResult(
            Input: cpfInput,
            NormalizedCpf: normalized,
            IsValid: true,
            ReasonCode: null,
            Reason: null
        );
    }

    // Standard CPF algorithm: weights (length+1) down to 2.
    // digit = 0 if (sum % 11) < 2 else 11 - (sum % 11)
    private static int CalculateCheckDigit(int[] digits, int length)
    {
        var sum = 0;
        var weight = length + 1;

        for (var i = 0; i < length; i++)
        {
            sum += digits[i] * (weight - i);
        }

        var mod = sum % 11;
        return mod < 2 ? 0 : 11 - mod;
    }
}
