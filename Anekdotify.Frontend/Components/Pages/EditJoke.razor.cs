using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Anekdotify.Frontend.Clients;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.Accounts;
using Anekdotify.Models.DTOs.Classification;
using Anekdotify.Models.DTOs.Source;
using Blazored.Toast.Services;

namespace Anekdotify.Frontend.Components.Pages;

public partial class EditJoke
{
    [Parameter] public int JokeId { get; set; }
    
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] private ApiClient ApiClient { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    
    private JokeDto? _joke;
    private JokeUpdateDto _editModel = new();
    private UserDto? _currentUser;
    private string? _errorMessage;
    private bool _isSubmitting = false;
    
    private List<ClassificationDetailedDto> _classifications = new();
    private List<SourceDto> _sources = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadCurrentUser();
        await LoadJoke();
        await LoadClassifications();
        
        if (IsAdminOrModerator())
        {
            //await LoadSources();
        }
    }

    private async Task LoadCurrentUser()
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                var response = await ApiClient.GetAsync<UserDto>("/api/account/profile");
                if (response.IsSuccess)
                {
                    _currentUser = response.Data;
                }
            }
        }
        catch (Exception ex)
        {
            _errorMessage = "Failed to load user information." + ex.Message;
        }
    }

    private async Task LoadJoke()
    {
        try
        {
            var response = await ApiClient.GetAsync<JokeDto>($"/api/joke/{JokeId}");
            if (response.IsSuccess)
            {
                _joke = response.Data;
                
                if (_joke != null)
                {
                    if (!(await CanEditJoke()))
                    {
                        _errorMessage = "You don't have permission to edit this joke.";
                        return;
                    }
                    
                    _editModel = new JokeUpdateDto
                    {
                        Text = _joke.Text,
                        ClassificationId = _joke.ClassificationId,

                    };
                }
            }
            else
            {
                _errorMessage = "Joke not found.";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = "Failed to load joke." + ex.Message;
        }
    }

    private async Task LoadClassifications()
    {
        try
        {
            var response = await ApiClient.GetAsync<List<ClassificationDetailedDto>>("/api/classification");
            if (response.IsSuccess)
            {
                _classifications = response.Data ?? new();
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
        }
    }

    private async Task LoadSources()
    {
        try
        {
            var response = await ApiClient.GetAsync<List<SourceDto>>("/api/sources");
            if (response.IsSuccess)
            {
                _sources = response.Data ?? new();
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
        }
    }

    private async Task<bool> CanEditJoke()
    {
        if (_currentUser == null || _joke == null) return false;
        
        if (IsAdminOrModerator()) return true;
        
        var ownerRes = await ApiClient.GetAsync<bool>($"/api/joke/{JokeId}/created-by-me");
        return ownerRes is { IsSuccess: true, Data: true };
    }

    private bool IsAdminOrModerator()
    {
        return _currentUser?.Role is "Admin" or "Moderator";
    }

    private async Task HandleValidSubmit()
    {
        if (_joke == null) return;
        
        _isSubmitting = true;
        
        try
        {
            var updateRequest = new JokeUpdateDto()
            {
                Text = _editModel.Text,
                ClassificationId = _editModel.ClassificationId
            };
            
            if (IsAdminOrModerator())
            {
                //TODO include additional fields for Admin/Moderator
            }
            
            var response = await ApiClient.PutAsync<JokeDto, JokeUpdateDto>($"/api/jokes/{JokeId}", updateRequest);
            
            if (response.IsSuccess)
            {
                Navigation.NavigateTo($"/joke/{JokeId}");
            }
            else
            {
                _errorMessage = "Failed to update joke. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = "An error occurred while updating the joke." + ex.Message;
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo($"/joke/{JokeId}");
    }
    
}