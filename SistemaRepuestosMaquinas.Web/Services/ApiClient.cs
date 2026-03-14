namespace SistemaRepuestosMaquinas.Web.Services;

public class ApiClient(HttpClient httpClient)
{
    public HttpClient HttpClient => httpClient;

    public void AttachJwt(string token)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
