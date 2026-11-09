using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Web.Services;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}

public class ApiClient
{
    private const string CookieKey = "ApiSessionCookie";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    public ApiClient(HttpClient http, IHttpContextAccessor accessor, IConfiguration config)
    {
        _http = http;
        _accessor = accessor;
        _http.BaseAddress = new Uri(config["Api:BaseUrl"]!);
    }

    private ISession Session => _accessor.HttpContext!.Session;

    public Task<ApiResult<T>> GetAsync<T>(string url) =>
        SendAsync<T>(HttpMethod.Get, url, null);

    public Task<ApiResult<T>> PostAsync<T>(string url, object? body = null) =>
        SendAsync<T>(HttpMethod.Post, url, body == null ? null : JsonContent.Create(body));

    public Task<ApiResult<T>> PutAsync<T>(string url, object body) =>
        SendAsync<T>(HttpMethod.Put, url, JsonContent.Create(body));

    public Task<ApiResult<bool>> DeleteAsync(string url) =>
        SendAsync<bool>(HttpMethod.Delete, url, null);

    /// <summary>Sends a file to the API as multipart/form-data (field name "file").</summary>
    public async Task<ApiResult<T>> UploadAsync<T>(string url, IFormFile file)
    {
        var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);
        return await SendAsync<T>(HttpMethod.Post, url, content);
    }

    /// <summary>Forgets the API session (used when logging out).</summary>
    public void ClearApiSession() => Session.Remove(CookieKey);

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, HttpContent? content)
    {
        try
        {
            var request = new HttpRequestMessage(method, url) { Content = content };

            var cookie = Session.GetString(CookieKey);
            if (!string.IsNullOrEmpty(cookie))
                request.Headers.Add("Cookie", cookie);

            var response = await _http.SendAsync(request);
            CaptureCookie(response);

            var text = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult<T>
                {
                    Success = false,
                    StatusCode = response.StatusCode,
                    Error = FriendlyError(response.StatusCode, text)
                };
            }

            T? data = default;
            if (!string.IsNullOrWhiteSpace(text) && typeof(T) != typeof(bool))
            {
                try { data = JsonSerializer.Deserialize<T>(text, JsonOptions); }
                catch (JsonException) { /* body was not JSON; ignore */ }
            }

            return new ApiResult<T> { Success = true, StatusCode = response.StatusCode, Data = data };
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T>
            {
                Success = false,
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Error = "The RaceDay service is not available right now. Please try again shortly."
            };
        }
        catch (TaskCanceledException)
        {
            return new ApiResult<T>
            {
                Success = false,
                StatusCode = HttpStatusCode.RequestTimeout,
                Error = "The request took too long. Please try again."
            };
        }
    }

    private void CaptureCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values)) return;

        foreach (var value in values)
        {
            if (value.StartsWith(".AspNetCore.Session=", StringComparison.OrdinalIgnoreCase))
            {
                // keep only "name=value", drop the attributes (path, httponly, ...)
                Session.SetString(CookieKey, value.Split(';')[0]);
            }
        }
    }

    private static string FriendlyError(HttpStatusCode status, string body)
    {
        // Validation errors come back as JSON with an "errors" section
        if (body.TrimStart().StartsWith("{"))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("errors", out var errors))
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString()))
                        .Where(m => !string.IsNullOrWhiteSpace(m));
                    var joined = string.Join(" ", messages);
                    if (joined.Length > 0) return joined;
                }
            }
            catch (JsonException) { }
        }

        // Plain text messages written by the API are safe to show
        if (!string.IsNullOrWhiteSpace(body) && !body.TrimStart().StartsWith("{") && body.Length < 200)
            return body.Trim('"');

        return status switch
        {
            HttpStatusCode.Unauthorized => "Please log in to continue.",
            HttpStatusCode.Forbidden => "You do not have permission to do that.",
            HttpStatusCode.NotFound => "That item could not be found.",
            HttpStatusCode.Conflict => "That action conflicts with existing data.",
            _ => "Something went wrong. Please try again."
        };
    }
}