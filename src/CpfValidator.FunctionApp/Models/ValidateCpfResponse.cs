namespace CpfValidator.FunctionApp.Models;

public sealed record ValidateCpfResponse(
    string? Input,
    string? NormalizedCpf,
    bool IsValid,
    string? ReasonCode,
    string? Reason,
    string RequestId,
    DateTimeOffset TimestampUtc
);
