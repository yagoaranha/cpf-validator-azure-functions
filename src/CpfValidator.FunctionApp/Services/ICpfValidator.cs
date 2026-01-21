namespace CpfValidator.FunctionApp.Services;

public interface ICpfValidator
{
    CpfValidationResult Validate(string? cpfInput);
}
