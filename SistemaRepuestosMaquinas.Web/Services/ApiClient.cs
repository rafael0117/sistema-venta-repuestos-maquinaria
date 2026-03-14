using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SistemaRepuestosMaquinas.Web.Services;

public class ApiClient(HttpClient httpClient)
{
    public HttpClient HttpClient => httpClient;

    public void AttachJwt(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
            return;
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<T>(url, cancellationToken);

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T body, CancellationToken cancellationToken = default)
        => await httpClient.PostAsJsonAsync(url, body, cancellationToken);

    public async Task<HttpResponseMessage> PutAsync<T>(string url, T body, CancellationToken cancellationToken = default)
        => await httpClient.PutAsJsonAsync(url, body, cancellationToken);

    public async Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken cancellationToken = default)
        => await httpClient.DeleteAsync(url, cancellationToken);
}
