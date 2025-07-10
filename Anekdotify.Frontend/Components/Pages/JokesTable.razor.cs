using System.Net;
using Anekdotify.Frontend.Authentication;
using Anekdotify.Frontend.Clients;
using Anekdotify.Models.DTOs.Classification;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.DTOs.Jokes;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Anekdotify.Frontend.Components.Pages
{
    public partial class JokesTable
    {
        [Inject] public ApiClient? ApiClient { get; set; }
        [Inject] public AuthenticationStateProvider? AuthenticationStateProvider { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        private List<JokeDto> _jokes = new List<JokeDto>();
        private List<ClassificationDetailedDto> _classifications = new();
        private List<int> _selectedClassifications = new();
        private bool _isLoading = true;
        private string? _errorMessage;
        private string _sortBy = "comments";
        private bool _sortDesc = true;
        private int _visibleCount = 8;
        private const int PageSize = 8;
        private IEnumerable<JokeDto> FilteredJokes
        {
            get
            {
                var filtered = _jokes
                    .Where(j => !_selectedClassifications.Any() ||
                                (j.ClassificationId.HasValue && _selectedClassifications.Contains(j.ClassificationId.Value)));
                IEnumerable<JokeDto> sorted = _sortBy switch
                {
                    "likes" => _sortDesc ? filtered.OrderByDescending(j => j.TotalLikes) : filtered.OrderBy(j => j.TotalLikes),
                    "comments" => _sortDesc ? filtered.OrderByDescending(j => CountCommentsRecursive(j.Comments)) : filtered.OrderBy(j => CountCommentsRecursive(j.Comments)),
                    _ => _sortDesc ? filtered.OrderByDescending(j => j.SubmissionDate) : filtered.OrderBy(j => j.SubmissionDate)
                };
                return sorted.Take(_visibleCount);
            }
        }


        protected override async Task OnInitializedAsync()
        {
            await LoadJokes();
            await LoadClassifications();
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
        private async Task LoadClassifications()
        {
            if (ApiClient != null)
            {
                var result = await ApiClient.GetAsync<List<ClassificationDetailedDto>>("api/classification");
                if (result.IsSuccess && result.Data != null)
                    _classifications = result.Data;
            }
        }
        private void ToggleClassification(int classificationId)
        {
            if (_selectedClassifications.Contains(classificationId))
                _selectedClassifications.Remove(classificationId);
            else
                _selectedClassifications.Add(classificationId);
        }
        private void ShowMore()
        {
            _visibleCount += PageSize;
        }
        private static int CountCommentsRecursive(List<CommentDto> comments)
        {
            var count = 0;
            foreach (var comment in comments)
            {
                count++;
                bool? any = comment.Replies.Count != 0;

                if (any == true)
                {
                    count += CountCommentsRecursive(comment.Replies);
                }
            }
            return count;
        }
        private void SelectAllClassifications()
        {
            if (_selectedClassifications.Count == _classifications.Count)
            {
                _selectedClassifications.Clear();
            }
            else
            {
                _selectedClassifications = _classifications.Select(c => c.ClassificationId).ToList();
            }
        }
    }

}
