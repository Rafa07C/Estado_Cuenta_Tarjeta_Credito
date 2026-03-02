using CreditCardStatement.Core.DTOs;
using System.Text;
using System.Text.Json;

namespace CreditCardStatement.Mvc.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<StatementDto?> GetStatementAsync(int cardId, int year, int month)
    {
        var response = await _httpClient.GetAsync($"api/statement/{cardId}/{year}/{month}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<StatementDto>(json, _jsonOptions);
    }

    public async Task<IEnumerable<TransactionDto>> GetMonthTransactionsAsync(int cardId, int year, int month)
    {
        var response = await _httpClient.GetAsync($"api/transactions/{cardId}/{year}/{month}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<TransactionDto>>(json, _jsonOptions)
               ?? Enumerable.Empty<TransactionDto>();
    }

    public async Task<(bool Success, string Message)> AddPurchaseAsync(AddPurchaseDto dto)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/transactions/purchase", content);
            var json = await response.Content.ReadAsStringAsync();

            var message = ExtractMessage(json);

            if (string.IsNullOrWhiteSpace(message))
                message = response.IsSuccessStatusCode
                    ? "Compra registrada correctamente."
                    : "No se pudo registrar la compra.";

            return (response.IsSuccessStatusCode, message);
        }
        catch
        {
            return (false, "No se pudo conectar con la API. Intenta de nuevo.");
        }
    }

    public async Task<(bool Success, string Message)> AddPaymentAsync(AddPaymentDto dto)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/transactions/payment", content);
            var json = await response.Content.ReadAsStringAsync();

            var message = ExtractMessage(json);

            if (string.IsNullOrWhiteSpace(message))
                message = response.IsSuccessStatusCode
                    ? "Pago registrado correctamente."
                    : "No se pudo registrar el pago.";

            return (response.IsSuccessStatusCode, message);
        }
        catch
        {
            return (false, "No se pudo conectar con la API. Intenta de nuevo.");
        }
    }

    private static string ExtractMessage(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;

        try
        {
            // A veces el API puede devolver texto plano en vez de JSON
            // Si no es JSON válido, caerá en catch y devolvemos el texto.
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                // Caso: { "message": "..." }
                if (root.TryGetProperty("message", out var m1) && m1.ValueKind == JsonValueKind.String)
                    return m1.GetString() ?? string.Empty;

                // Caso: { "Message": "..." }
                if (root.TryGetProperty("Message", out var m2) && m2.ValueKind == JsonValueKind.String)
                    return m2.GetString() ?? string.Empty;

                // Por si en algún momento devuelves ProblemDetails: { "title": "..." }
                if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                    return t.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch
        {
            // Si no es JSON, devolvemos el texto como mensaje (corto)
            return json.Length > 200 ? json.Substring(0, 200) : json;
        }
    }
}