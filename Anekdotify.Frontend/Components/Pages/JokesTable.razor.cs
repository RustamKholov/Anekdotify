using System.Net;
using Anekdotify.Frontend.Authentication;
using Anekdotify.Frontend.Clients;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.DTOs.Jokes;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Anekdotify.Frontend.Components.Pages
{
    public partial class JokesTable
    {
        private List<JokeDTO> jokes = new List<JokeDTO>();
        private bool isLoading = true;
        private string? errorMessage = null;

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
            isLoading = true;
            errorMessage = null;
            jokes = new List<JokeDTO>();
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
                            var response = await ApiClient.GetAsync<List<JokeDTO>>("api/saved-jokes");
                            jokes = response?.Data ?? new List<JokeDTO>();
                        }
                        else
                        {
                            errorMessage = "API client is not available.";
                        }
                    }
                    else
                    {
                        errorMessage = "You need to be logged in to view jokes.";
                    }
                }
                else
                {
                    errorMessage = "Authentication provider is not available.";
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
            {
                errorMessage = "Access denied. Please log in.";
                if (NavigationManager != null)
                {
                    NavigationManager.NavigateTo("/login", forceLoad: true);
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load jokes: {ex.Message}";
                Console.WriteLine($"Error loading jokes: {ex}");
            }
            finally
            {
                isLoading = false;
                if (errorMessage != null)
                {
                    ToastService?.ShowError(errorMessage);
                }

            }
        }
    }
    
}
