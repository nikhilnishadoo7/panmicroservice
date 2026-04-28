// Infrastructure/Providers/BaseProvider.cs
using System.Text;
using Newtonsoft.Json;
using PAN.API.Infrastructure.Logging;

namespace PAN.API.Infrastructure.Providers;

public abstract class BaseProvider
{
    protected readonly HttpClient _client;

    protected BaseProvider(IHttpClientFactory factory, string clientName)
    {
        _client = factory.CreateClient(clientName);
    }

    // ✅ One shared method — both providers call this
    protected async Task<string> PostAsync(
        string baseUrl,
        string endpoint,
        string apiKey,
        object payload,
        string correlationId)
    {
        var url = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        SafeLogger.App($"[HTTP POST] URL: {url} | CorrelationId: {correlationId}");

        var request = new HttpRequestMessage(HttpMethod.Post, url);

        if (!string.IsNullOrEmpty(apiKey))
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

        request.Headers.Add("X-Request-Id", correlationId);

        var json = JsonConvert.SerializeObject(payload);
        SafeLogger.App($"[HTTP POST] Payload: {json}");

        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var res = await _client.SendAsync(request);

        SafeLogger.App($"[HTTP POST] Status: {res.StatusCode}");

        var responseJson = await res.Content.ReadAsStringAsync();

        SafeLogger.App($"[HTTP POST] Raw Response: {responseJson}");

        if (!res.IsSuccessStatusCode)
            throw new Exception($"Provider call failed [{res.StatusCode}]: {responseJson}");

        return responseJson;
    }
}