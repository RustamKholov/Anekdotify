using System.Net;
using Anekdotify.Frontend.Authentication;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.DTOs.Classification;
using Anekdotify.Models.DTOs.Source;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace Anekdotify.Frontend.Components.Pages;

public partial class JokeDisplayPage : IDisposable
{
    [Inject] IToastService ToastService { get; set; } = null!;

    private bool IsFlipped { get; set; }
    private bool _isJokeAvailable;
    private bool _previousJokeAvailable;
    private bool _showingPreviousJoke;
    private bool _isLoading = true;
    private bool _isCompletelyRandom = true;
    private bool _isLoadingJoke = false;

    private JokeDto? _currentJoke;
    private string _timeUntilNextJoke = "";
    private Timer? _timer;

    private List<ClassificationDetailedDto> _classifications = new();
    private List<SourceDto> _sources = new();
    private readonly List<int> _selectedClassifications = new();
    private readonly List<int> _selectedSources = new();

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        try
        {
            await CheckAuthenticationAsync();
            await LoadFiltersDataAsync();
            UpdateTimeUntilNextJoke();
            StartCountdownTimer();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
        {
            await HandleUnauthorizedAsync();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task CheckAuthenticationAsync()
    {
        if (AuthenticationStateProvider is CustomAuthStateProvider customAuthStateProvider)
        {
            var authState = await customAuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var newRandomAvailableRes = await ApiClient.GetAsync<IsActiveRandomResponse>("api/joke/random/isActive");
                _isJokeAvailable = newRandomAvailableRes.Data?.IsActive ?? false;

                var isPreviousActualResult = await ApiClient.GetAsync<bool>("api/joke/last-viewed/isActual");
                _previousJokeAvailable = isPreviousActualResult.Data;
            }
        }
    }

    private async Task LoadFiltersDataAsync()
    {
        var classificationsTask = ApiClient.GetAsync<List<ClassificationDetailedDto>>("api/classification");
        var sourcesTask = ApiClient.GetAsync<List<SourceDto>>("api/source");

        await Task.WhenAll(classificationsTask, sourcesTask);

        var classificationsResult = await classificationsTask;
        var sourcesResult = await sourcesTask;

        if (classificationsResult.IsSuccess && classificationsResult.Data != null)
        {
            _classifications = classificationsResult.Data.Where(c => c.ClassificationId > 0).ToList();
        }

        if (sourcesResult.IsSuccess && sourcesResult.Data != null)
        {
            _sources = sourcesResult.Data.Where(s => s.Id > 0).ToList();
        }
    }

    private void StartCountdownTimer()
    {
        _timer = new Timer(1000); // Update every second
        _timer.Elapsed += (_, _) => InvokeAsync(() =>
        {
            UpdateTimeUntilNextJoke();
            StateHasChanged();
        });
        _timer.Start();
    }

    private void UpdateTimeUntilNextJoke()
    {
        var now = DateTime.Now;
        var tomorrow = DateTime.Today.AddDays(1);
        var timeSpan = tomorrow - now;
        _timeUntilNextJoke = $"{timeSpan.Hours}h {timeSpan.Minutes}m";
    }

    private void OnRandomModeChanged()
    {
        _currentJoke = null;
        IsFlipped = false;
        if (_isCompletelyRandom)
        {
            _selectedClassifications.Clear();
            _selectedSources.Clear();
        }
        StateHasChanged();
    }

    private void ToggleClassification(int classificationId)
    {
        if (_selectedClassifications.Contains(classificationId))
        {
            _selectedClassifications.Remove(classificationId);
        }
        else
        {
            _selectedClassifications.Add(classificationId);
        }
        _currentJoke = null;
        IsFlipped = false;
        StateHasChanged();
    }

    private void ToggleSource(int sourceId)
    {
        if (_selectedSources.Contains(sourceId))
        {
            _selectedSources.Remove(sourceId);
        }
        else
        {
            _selectedSources.Add(sourceId);
        }
        _currentJoke = null;
        IsFlipped = false;
        StateHasChanged();
    }

    private bool IsClassificationSelected(int classificationId)
    {
        return _selectedClassifications.Contains(classificationId);
    }

    private bool IsSourceSelected(int sourceId)
    {
        return _selectedSources.Contains(sourceId);
    }

    private async Task OnFlipAsync()
    {
        if (_isLoadingJoke)
            return;

        if (!IsFlipped)
        {
            // First flip - load joke if not already loaded
            if (_currentJoke == null)
            {
                _isLoadingJoke = true;
                StateHasChanged();

                await LoadJokeAsync();

                _isLoadingJoke = false;
            }

            // Only flip if we successfully loaded a joke
            if (_currentJoke != null)
            {
                IsFlipped = true;
                StateHasChanged();
            }
        }
        else
        {
            // Second flip - flip back to front
            IsFlipped = false;
            StateHasChanged();
        }
    }

    private Task OnJokeFlip()
    {
        return Task.CompletedTask;
    }

    private async Task LoadJokeAsync()
    {
        try
        {
            string endpoint;
            if (_isCompletelyRandom || (!_selectedClassifications.Any() && !_selectedSources.Any()))
            {
                endpoint = "api/joke/random";
            }
            else
            {
                var queryParams = new List<string>();
                if (_selectedClassifications.Any())
                {
                    foreach (var id in _selectedClassifications)
                        queryParams.Add($"ClassificationIds={id}");
                }
                if (_selectedSources.Any())
                {
                    foreach (var id in _selectedSources)
                        queryParams.Add($"SourceIds={id}");
                }

                var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                endpoint = $"api/joke/random{query}";
            }

            var result = await ApiClient.GetAsync<JokeDto>(endpoint);
            if (result.IsSuccess && result.Data != null)
            {
                _currentJoke = result.Data;
            }
            else
            {
                ToastService.ShowError("Failed to load joke. Please try again.");
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError("Failed to load joke. Please try again.");
            Console.WriteLine($"Error loading joke: {ex.Message}");
        }
    }

    private async Task ShowPreviousJoke()
    {
        var result = await ApiClient.GetAsync<JokeDto>("api/joke/last-viewed");
        if (result is { IsSuccess: true, Data: not null })
        {
            _currentJoke = result.Data;
            _showingPreviousJoke = true;
            IsFlipped = false;
            StateHasChanged();
        }
    }

    private void UpdateComments(List<CommentDto> comments)
    {
        if (_currentJoke != null)
        {
            _currentJoke.Comments = comments;
            StateHasChanged();
        }
    }

    private async Task HandleUnauthorizedAsync()
    {
        if (AuthenticationStateProvider is CustomAuthStateProvider customAuthStateProvider)
        {
            await customAuthStateProvider.MarkUserAsLoggedOut();
        }
        NavigationManager?.NavigateTo("/login", forceLoad: true);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public class IsActiveRandomResponse
    {
        public bool IsActive { get; set; }
    }
}