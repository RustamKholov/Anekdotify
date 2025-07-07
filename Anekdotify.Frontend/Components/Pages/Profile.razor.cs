using Anekdotify.Models.DTOs.Accounts;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.DTOs.Jokes;
using Microsoft.AspNetCore.Components;

namespace Anekdotify.Frontend.Components.Pages;

public partial class Profile : ComponentBase
{
    private UserDto? _userProfile;
    private UserStatisticsDto? _statistics;
    private List<SuggestedJokeDto>? _recentJokes;

    protected override async Task OnInitializedAsync()
    {
        await LoadUserProfile();
        await LoadUserData();

    }

    private async Task LoadUserProfile()
    {
        try
        {
            var userRes = await ApiClient.GetAsync<UserDto>("api/account/profile");
            if (userRes?.Data != null) _userProfile = userRes?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user profile: {ex.Message}");
        }
    }

    private async Task LoadUserData()
    {
        try
        {

            var userJokesRes = await ApiClient.GetAsync<List<JokePreviewDto>>("api/joke/suggested-by-me");

            if (userJokesRes?.Data != null)
            {
                var userJokes = userJokesRes.Data;

                _recentJokes = new List<SuggestedJokeDto>();

                foreach (var userJoke in userJokes)
                {
                    var suggested = new SuggestedJokeDto
                    {
                        ClassificationName = userJoke.ClassificationName,
                        IsApproved = userJoke.IsApproved,
                        JokeId = userJoke.JokeId,
                        Text = userJoke.Text
                    };

                    _recentJokes.Add(suggested);
                }

                var userCommentsRes = await ApiClient.GetAsync<List<CommentDto>>("api/comments/created-by-me");
                var comments = userCommentsRes?.Data;

                _statistics = new UserStatisticsDto
                {
                    TotalJokes = userJokes.Count,
                    TotalLikes = userJokesRes.Data.Sum(c => c.LikeCount),
                    TotalComments = comments?.Count ?? 0
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user data: {ex.Message}");
        }
    }


    private string GetUserInitials(string? username)
    {
        if (string.IsNullOrEmpty(username))
            return "U";

        var parts = username.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();

        return username.Length >= 2 ? username.Substring(0, 2).ToUpper() : username.ToUpper();
    }

    private string GetUserRank()
    {
        if (_statistics == null) return "Newbie";

        var totalActivity = _statistics.TotalJokes + (_statistics.TotalLikes / 5) + (_statistics.TotalComments / 3);

        return totalActivity switch
        {
            >= 100 => "Comedy Legend",
            >= 50 => "Joke Master",
            >= 25 => "Humor Expert",
            >= 10 => "Rising Comedian",
            >= 5 => "Joke Teller",
            _ => "Newbie"
        };
    }

}