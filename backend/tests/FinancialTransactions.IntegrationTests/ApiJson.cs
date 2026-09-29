using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.IntegrationTests;

internal static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) }
    };

    public static Task<T?> ReadAsync<T>(HttpResponseMessage response) =>
        response.Content.ReadFromJsonAsync<T>(Options);

    public static async Task<AccountDto?> GetAccountAsync(HttpClient client, Guid accountId)
    {
        var accounts = await client.GetFromJsonAsync<List<AccountDto>>("/api/accounts", Options);

        return accounts!.Single(account => account.Id == accountId);
    }
}
