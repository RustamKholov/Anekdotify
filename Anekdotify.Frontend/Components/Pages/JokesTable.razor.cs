using System.Net;
using Anekdotify.Frontend.Authentication;
using Anekdotify.Frontend.Clients;
using Anekdotify.Models.DTOs.Jokes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Anekdotify.Frontend.Components.Pages
{
    public partial class JokesTable
    {
        private List<JokePreviewDTO> jokes = new List<JokePreviewDTO>();
        private bool isLoading = true;
        private string errorMessage = null;

        [Inject]
        public ApiClient ApiClient { get; set; }
        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadJokes();
        }
        private async Task LoadJokes()
        {
            isLoading = true;
            errorMessage = null;
            jokes = new List<JokePreviewDTO>();
            StateHasChanged();

            try
            {
                var authState = await ((CustomAuthStateProvider)AuthenticationStateProvider).GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true)
                {
                    jokes = (await ApiClient.GetAsync<List<JokePreviewDTO>>("api/saved-jokes")).Data;
                }
                else
                {
                    errorMessage = "You need to be logged in to view jokes.";
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
            {
                errorMessage = "Access denied. Please log in.";
                NavigationManager.NavigateTo("/login", forceLoad: true);
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load jokes: {ex.Message}";
                Console.WriteLine($"Error loading jokes: {ex}");
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}
