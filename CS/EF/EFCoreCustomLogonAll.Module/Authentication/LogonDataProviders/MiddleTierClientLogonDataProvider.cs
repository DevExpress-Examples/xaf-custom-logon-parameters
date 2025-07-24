#nullable enable
using DevExpress.ExpressApp.Security.ClientServer;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace EFCoreCustomLogonAll.Module.Authentication;

public class MiddleTierClientLogonDataProvider : ILogonDataProvider {
    readonly WebApiSecuredDataServerClientBase dataServerClient;
    public MiddleTierClientLogonDataProvider(WebApiSecuredDataServerClientBase dataServerClient) {
        this.dataServerClient = dataServerClient;
        var t = dataServerClient.HttpClient;
    }
    public IList<CompanyDTO> GetCompanies() {
        var companies = GetAllAsync<CompanyDTO>(dataServerClient.HttpClient, "GetCompanies").GetAwaiter().GetResult();
        foreach(var company in companies) {
            company.LogonDataProvider = this;
        }
        return new ReadOnlyCollection<CompanyDTO>(companies);
    }

    public IList<ApplicationUserDTO> GetCompanyUsers(Guid companyID) {
        var result = GetAllAsync<ApplicationUserDTO>(dataServerClient.HttpClient, $"GetApplicationUsers({companyID})").GetAwaiter().GetResult();
        return new ReadOnlyCollection<ApplicationUserDTO>(result);
    }

    async Task<T[]> GetAllAsync<T>(HttpClient httpClient, string requestUri) {
        var json = await RequestAsync(httpClient, new HttpRequestMessage(HttpMethod.Get, requestUri));

        return json.GetProperty("$values").Deserialize<T[]>()
            ?? throw new NullReferenceException();
    }
    async Task<JsonElement> RequestAsync(HttpClient httpClient, HttpRequestMessage request, bool preventAuthorization = false) {
        request.Headers.Add("Accept", "application/json");
        using var httpResponse = Send(httpClient, request, preventAuthorization);
        if(httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound) {
            throw new HttpRequestException($"{request.Method} request has no JSON! Code {(int)httpResponse.StatusCode}, '{httpResponse.ReasonPhrase}'");
        }
        if(httpResponse.StatusCode == System.Net.HttpStatusCode.NoContent) {
            return new JsonElement();
        } else {
            return await httpResponse.Content.ReadFromJsonAsync<JsonElement>();
        }
    }
    HttpResponseMessage Send(HttpClient httpClient, HttpRequestMessage request, bool preventAuthorization = false) {
        var httpResponse = httpClient.SendAsync(request).GetAwaiter().GetResult();
        if(!httpResponse.IsSuccessStatusCode) {
            using(var stream = httpResponse.Content.ReadAsStream()) {
                using(StreamReader reader = new StreamReader(stream)) {
                    throw new HttpRequestException(httpResponse.ReasonPhrase + Environment.NewLine + reader.ReadToEnd(), null, httpResponse.StatusCode);
                }
            }
        }
        return httpResponse;
    }
}

#nullable restore