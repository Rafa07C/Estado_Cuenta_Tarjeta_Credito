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
        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/transactions/purchase", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
        var message = result.GetProperty("message").GetString() ?? string.Empty;
        return (response.IsSuccessStatusCode, message);
    }

    public async Task<(bool Success, string Message)> AddPaymentAsync(AddPaymentDto dto)
    {
        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/transactions/payment", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
        var message = result.GetProperty("message").GetString() ?? string.Empty;
        return (response.IsSuccessStatusCode, message);
    }
}