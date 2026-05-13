using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace WebLabMobile.Services;

public class ApiClient
{
#if ANDROID
    public const string BaseUrl = "http://10.0.2.2:5013";
#else
    public const string BaseUrl = "http://localhost:5013";
#endif

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _http = new();

    private async Task<HttpRequestMessage> CreateRequest(HttpMethod method, string path, object? body = null)
    {
        var req = new HttpRequestMessage(method, $"{BaseUrl}{path}");
        var token = await SecureStorage.GetAsync("token");
        if (token is not null)
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        return req;
    }

    public async Task<T> GetAsync<T>(string path)
    {
        var req = await CreateRequest(HttpMethod.Get, path);
        var res = await _http.SendAsync(req);
        await EnsureSuccess(res);
        return await res.Content.ReadFromJsonAsync<T>(JsonOptions)
            ?? throw new InvalidOperationException("Empty response");
    }

    public async Task<T> PostAsync<T>(string path, object body)
    {
        var req = await CreateRequest(HttpMethod.Post, path, body);
        var res = await _http.SendAsync(req);
        await EnsureSuccess(res);
        return await res.Content.ReadFromJsonAsync<T>(JsonOptions)
            ?? throw new InvalidOperationException("Empty response");
    }

    public async Task<T> PutAsync<T>(string path, object body)
    {
        var req = await CreateRequest(HttpMethod.Put, path, body);
        var res = await _http.SendAsync(req);
        await EnsureSuccess(res);
        return await res.Content.ReadFromJsonAsync<T>(JsonOptions)
            ?? throw new InvalidOperationException("Empty response");
    }

    public async Task DeleteAsync(string path)
    {
        var req = await CreateRequest(HttpMethod.Delete, path);
        var res = await _http.SendAsync(req);
        await EnsureSuccess(res);
    }

    private static async Task EnsureSuccess(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode) return;
        string? message = null;
        try
        {
            var body = await res.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
            if (body?.TryGetValue("message", out var msg) == true)
                message = msg.GetString();
        }
        catch { }
        throw new Exception(message ?? $"HTTP {(int)res.StatusCode}");
    }
}
