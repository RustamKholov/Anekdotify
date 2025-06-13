using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Anekdotify.Common;
using Anekdotify.Frontend.Authentication;
using Anekdotify.Frontend.Heplers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;

namespace Anekdotify.Frontend.Clients;

public class ApiClient(HttpClient httpClient, ProtectedLocalStorage storage)
{

    public async Task SetAuthorizeHeader()
    {
        var token = (await storage.GetAsync<string>( "authToken")).Value;
        if (token != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
    public async Task<ApiResult<TSuccessData>> GetAsync<TSuccessData>(string path)
    {
        await SetAuthorizeHeader();
        var response = await httpClient.GetAsync(path);
        if(response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<TSuccessData>();
            return ApiResult<TSuccessData>.Success(data, response.StatusCode);
        }
        else
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            return ApiResult<TSuccessData>.Failure(errorMessage, response.StatusCode);
        }
    }

    public async Task<ApiResult<TSuccessData>> PostAsync<TSuccessData, TRequest>(string path, TRequest postModel)
    {
        await SetAuthorizeHeader();
        var response = await httpClient.PostAsJsonAsync(path, postModel);
        if(response.IsSuccessStatusCode)
        {
            
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return ApiResult<TSuccessData>.Success(default, response.StatusCode);
            }

            var successData = await response.Content.ReadFromJsonAsync<TSuccessData>();
            if (successData == null && typeof(TSuccessData) != typeof(string))
            {
                return null;
            }
            return ApiResult<TSuccessData>.Success(successData, response.StatusCode);
        }
        else
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            return ApiResult<TSuccessData>.Failure(errorMessage, response.StatusCode);
        }
    }
    public async Task<ApiResult<TSuccessData>> PutAsync<TSuccessData, TRequestData>(string path, TRequestData postModel)
    {
        await SetAuthorizeHeader();
        var response = await httpClient.PutAsJsonAsync(path, postModel);
        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return ApiResult<TSuccessData>.Success(default, response.StatusCode);
            }
            var successData = await response.Content.ReadFromJsonAsync<TSuccessData>();
            if (successData == null && typeof(TSuccessData) != typeof(string))
            {
                return null;
            }
            return ApiResult<TSuccessData>.Success(successData, response.StatusCode);
        }
        else
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            return ApiResult<TSuccessData>.Failure(errorMessage, response.StatusCode);
        }
    }
    public async Task<ApiResult<bool>> DeleteAsync(string path)
    {
        await SetAuthorizeHeader();
        var response = await httpClient.DeleteAsync(path);
        if (response.IsSuccessStatusCode)
        {
            return ApiResult<bool>.Success(true, response.StatusCode);
        }
        else
        {
            string errorMessage = await response.Content.ReadAsStringAsync();
            return ApiResult<bool>.Failure(errorMessage, response.StatusCode);
        }
    }
}
