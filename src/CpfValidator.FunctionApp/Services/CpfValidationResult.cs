namespace CpfValidator.FunctionApp.Services;

public sealed record CpfValidationResult(
    string? Input,
    string? NormalizedCpf,
    bool IsValid,
    string? ReasonCode,
    string? Reason
);
