using System.Net;
using Anekdotify.Frontend.Authentication;
using Anekdotify.Frontend.Clients;
using Anekdotify.Models.DTOs.Jokes;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Anekdotify.Frontend.Components.Pages
{
    public partial class JokesTable
    {
        private List<JokeDto> _jokes = new List<JokeDto>();
        private bool _isLoading = true;
        private string? _errorMessage;

        [Inject] public ApiClient? ApiClient { get; set; }
        [Inject] public AuthenticationStateProvider? AuthenticationStateProvider { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadJokes();
        }
        private async Task LoadJokes()
        {
            _isLoading = true;
            _errorMessage = null;
            _jokes = new List<JokeDto>();
            StateHasChanged();

            try
            {
                if (AuthenticationStateProvider is CustomAuthStateProvider customAuthStateProvider)
                {
                    var authState = await customAuthStateProvider.GetAuthenticationStateAsync();
                    var user = authState.User;

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        if (ApiClient != null)
                        {
                            var response = await ApiClient.GetAsync<List<JokeDto>>("api/saved-jokes");
                            _jokes = response.Data ?? new List<JokeDto>();
                        }
                        else
                        {
                            _errorMessage = "API client is not available.";
                        }
                    }
                    else
                    {
                        _errorMessage = "You need to be logged in to view jokes.";
                    }
                }
                else
                {
                    _errorMessage = "Authentication provider is not available.";
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
            {
                _errorMessage = "Access denied. Please log in.";
                if (NavigationManager != null)
                {
                    NavigationManager.NavigateTo("/login", forceLoad: true);
                }
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to load jokes: {ex.Message}";
                Console.WriteLine($"Error loading jokes: {ex}");
            }
            finally
            {
                _isLoading = false;
                if (_errorMessage != null)
                {
                    ToastService?.ShowError(_errorMessage);
                }

            }
        }
    }
    
}
