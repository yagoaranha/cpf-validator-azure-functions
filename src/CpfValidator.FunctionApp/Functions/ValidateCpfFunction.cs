using System.Net;
using System.Text.Json;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using CpfValidator.FunctionApp.Models;
using CpfValidator.FunctionApp.Services;

namespace CpfValidator.FunctionApp.Functions;

public sealed class ValidateCpfFunction
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly ICpfValidator _cpfValidator;
    private readonly ILogger<ValidateCpfFunction> _logger;

    public ValidateCpfFunction(ICpfValidator cpfValidator, ILogger<ValidateCpfFunction> logger)
    {
        _cpfValidator = cpfValidator;
        _logger = logger;
    }

    /// <summary>
    /// Validates a CPF.
    ///
    /// GET  /api/cpf/validate/{cpf}
    /// GET  /api/cpf/validate?cpf=...
    /// POST /api/cpf/validate  { "cpf": "..." }
    /// </summary>
    [Function("ValidateCpf")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "cpf/validate/{cpf?}")] HttpRequestData req,
        string? cpf)
    {
        var requestId = req.Headers.TryGetValues("x-ms-client-request-id", out var ids)
            ? ids.FirstOrDefault() ?? Guid.NewGuid().ToString("N")
            : Guid.NewGuid().ToString("N");

        var input = cpf;

        // If CPF not present in route, attempt query string
        if (string.IsNullOrWhiteSpace(input))
        {
            input = GetQueryParam(req.Url, "cpf");
        }

        // If POST and still not provided, attempt body JSON
        if (string.IsNullOrWhiteSpace(input) && req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using var stream = req.Body;
                var body = await JsonSerializer.DeserializeAsync<ValidateCpfRequest>(stream, JsonOptions);
                input = body?.Cpf;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse request body as JSON. requestId={RequestId}", requestId);
                return await BadRequest(req, requestId, "BODY_INVALID_JSON", "O corpo da requisição não é um JSON válido.");
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return await BadRequest(req, requestId, "CPF_MISSING", "Informe o CPF via rota, querystring ou body JSON.");
        }

        var result = _cpfValidator.Validate(input);

        // 200 OK for valid/invalid CPF outcomes (business result)
        // 400 BadRequest only when CPF is missing or cannot be normalized to 11 digits
        if (result.ReasonCode is "CPF_LENGTH" or "CPF_FORMAT" or "CPF_EMPTY")
        {
            return await BadRequest(req, requestId, result.ReasonCode!, result.Reason ?? "CPF inválido.", result);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");

        var payload = new ValidateCpfResponse(
            Input: result.Input,
            NormalizedCpf: result.NormalizedCpf,
            IsValid: result.IsValid,
            ReasonCode: result.ReasonCode,
            Reason: result.Reason,
            RequestId: requestId,
            TimestampUtc: DateTimeOffset.UtcNow
        );

        await response.WriteStringAsync(JsonSerializer.Serialize(payload, JsonOptions));

        _logger.LogInformation(
            "CPF validation completed. isValid={IsValid} normalized={NormalizedCpf} requestId={RequestId}",
            result.IsValid, result.NormalizedCpf, requestId);

        return response;
    }

    private static string? GetQueryParam(Uri url, string key)
    {
        // url.Query includes a leading '?'
        var q = url.Query;
        if (string.IsNullOrWhiteSpace(q)) return null;

        var parts = q.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var kv = part.Split('=', 2, StringSplitOptions.TrimEntries);
            if (kv.Length != 2) continue;

            var k = Uri.UnescapeDataString(kv[0]);
            if (!string.Equals(k, key, StringComparison.OrdinalIgnoreCase)) continue;

            // '+' is commonly used for space in query strings
            var v = kv[1].Replace('+', ' ');
            return Uri.UnescapeDataString(v);
        }

        return null;
    }

    private static async Task<HttpResponseData> BadRequest(
        HttpRequestData req,
        string requestId,
        string reasonCode,
        string reason,
        CpfValidationResult? result = null)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");

        var payload = new ValidateCpfResponse(
            Input: result?.Input,
            NormalizedCpf: result?.NormalizedCpf,
            IsValid: false,
            ReasonCode: reasonCode,
            Reason: reason,
            RequestId: requestId,
            TimestampUtc: DateTimeOffset.UtcNow
        );

        await response.WriteStringAsync(JsonSerializer.Serialize(payload, JsonOptions));
        return response;
    }
}
