using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace frontend.Heplers;

public class ProtectedBrowserStorageAccessor
{
    private ProtectedLocalStorage _protectedLocalStorage;
    private ProtectedSessionStorage _protectedSessionStorage; // If you also use session storage
    private bool _isClientSide;

    public ProtectedBrowserStorageAccessor(ProtectedLocalStorage protectedLocalStorage, ProtectedSessionStorage protectedSessionStorage)
    {
        _protectedLocalStorage = protectedLocalStorage;
        _protectedSessionStorage = protectedSessionStorage;


        _isClientSide = OperatingSystem.IsBrowser();
    }

    public async Task<T> GetAsync<T>(ProtectedLocalStorage storage, string key)
    {
        if (_isClientSide)
        {
            var result = await storage.GetAsync<T>(key);
            return result.Success ? result.Value : default(T);
        }
        return default(T); // Return default if not client-side (during prerendering)
    }

    public async Task SetAsync<T>(ProtectedLocalStorage storage, string key, T value)
    {
        if (_isClientSide)
        {
            await storage.SetAsync(key, value);
        }
        // No action if not client-side, data will be set once client loads
    }

    public async Task DeleteAsync(ProtectedLocalStorage storage, string key)
    {
        if (_isClientSide)
        {
            await storage.DeleteAsync(key);
        }
    }


    public Task<T> GetLocalStorageAsync<T>(string key) => GetAsync<T>(_protectedLocalStorage, key);
    public Task SetLocalStorageAsync<T>(string key, T value) => SetAsync(_protectedLocalStorage, key, value);
    public Task DeleteLocalStorageAsync(string key) => DeleteAsync(_protectedLocalStorage, key);

}
