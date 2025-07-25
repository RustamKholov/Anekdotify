﻿using System.Net;
using Anekdotify.Frontend.Authentication;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.DTOs.Classification;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;
using Anekdotify.Models.Models;

namespace Anekdotify.Frontend.Components.Pages;

public partial class JokeDisplayPage : IDisposable
{
    [Inject] IToastService ToastService { get; set; } = null!;
    private bool _isJokeAvailable;
    private bool _previousJokeAvailable;
    private bool _showingPreviousJoke;
    private bool _isLoading = true;
    private bool _isCompletelyRandom = true;
    private bool _isLoadingJoke = false;
    private JokeDto? _currentJoke;
    private string _timeUntilNextJoke = "";
    private Timer? _timer;
    private bool _showJokeCard = false;

    private List<ClassificationDetailedDto> _classifications = new();
    private readonly List<int> _selectedClassifications = new();

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

        var classificationsResult = await classificationsTask;

        if (classificationsResult.IsSuccess && classificationsResult.Data != null)
        {
            _classifications = classificationsResult.Data.ToList();
        }
    }

    private void StartCountdownTimer()
    {
        _timer = new Timer(1000); // Update every second
        _timer.Elapsed += (_, _) => InvokeAsync(() =>
        {
            UpdateTimeUntilNextJoke();
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

        if (_isCompletelyRandom)
        {
            _selectedClassifications.Clear();
        }
        else
        {
            foreach (var classification in _classifications)
            {
                _selectedClassifications.Add(classification.ClassificationId);
            }
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
        StateHasChanged();
    }

    private bool IsClassificationSelected(int classificationId)
    {
        return _selectedClassifications.Contains(classificationId);
    }
    private void ReturnToHome()
    {
        _showingPreviousJoke = false;
        _currentJoke = null;
        StateHasChanged();
    }

    private async Task OnFetch()
    {
        if (_isLoadingJoke)
            return;

        _isLoadingJoke = true;
        _showJokeCard = true;

        StateHasChanged();

        await LoadJokeAsync();

        _isLoadingJoke = false;
        StateHasChanged();
    }

    private async Task LoadJokeAsync()
    {
        try
        {
            string endpoint;
            if (_isCompletelyRandom || !_selectedClassifications.Any())
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
}