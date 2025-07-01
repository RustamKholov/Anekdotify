using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Anekdotify.Frontend.Services;

public interface INavigationHistoryService
{
    event Action<bool> OnNavigationChanged;
    bool CanGoBack { get; }
    void NavigateTo(string uri, bool forceLoad = false);
    void GoBack();
    void Reset();
}

public class NavigationHistoryService : INavigationHistoryService
{
    private readonly NavigationManager _navigationManager;
    private readonly Stack<string> _navigationHistory = new();
    private string _currentUri;

    public event Action<bool>? OnNavigationChanged;
    public bool CanGoBack => _navigationHistory.Count > 0;

    public NavigationHistoryService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _navigationManager.LocationChanged += OnLocationChanged;
        _currentUri = _navigationManager.Uri;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (!_navigationHistory.Contains(e.Location))
        {
            if (!string.IsNullOrEmpty(_currentUri) && _currentUri != e.Location)
            {
                _navigationHistory.Push(_currentUri);
            }
        }
        
        _currentUri = e.Location;
        OnNavigationChanged?.Invoke(CanGoBack);
    }

    public void NavigateTo(string uri, bool forceLoad = false)
    {
        _navigationManager.NavigateTo(uri, forceLoad);
    }

    public void GoBack()
    {
        if (CanGoBack)
        {
            var previousUri = _navigationHistory.Pop();
            _navigationManager.NavigateTo(previousUri);
            OnNavigationChanged?.Invoke(CanGoBack);
        }
    }

    public void Reset()
    {
        _navigationHistory.Clear();
        OnNavigationChanged?.Invoke(CanGoBack);
    }
}
