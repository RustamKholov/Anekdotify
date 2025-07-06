
using Anekdotify.Frontend.Components.BaseComponents;
using Anekdotify.Models.DTOs.Classification;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.DTOs.JokeRating;
using Anekdotify.Models.DTOs.Jokes;
using Microsoft.AspNetCore.Components;

namespace Anekdotify.Frontend.Components.Pages
{
    public partial class JokeCard
    {
        [Parameter] public required JokeDto Joke { get; set; }
        [Parameter] public EventCallback<bool> OnFlip { get; set; }
        [Parameter] public bool IsFlipped { get; set; }
        [Parameter] public bool ShowTextOnFront { get; set; } = false;
        private AppModal? DeleteModal { get; set; }

        private EditJokeModal EditModal = new();

        public int? SelectedJokeId;

        private int DeleteId { get; set; }
        private bool? _isLiked;
        private bool? _isSaved;
        private bool _isRatingBusy = false;
        private bool areCommentsOpen = false;
        private async Task HandleFlip()
        {
            CloseCommentsIfOpen();
            if (OnFlip.HasDelegate)
            {
                await OnFlip.InvokeAsync(areCommentsOpen);
            }
            IsFlipped = !IsFlipped;
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            var resRate = await ApiClient.GetAsync<RatingDto>($"api/joke/{Joke.JokeId}/rating");
            if (resRate is { IsSuccess: true, Data.IsLike: not null })
            {
                _isLiked = resRate.Data.IsLike;
            }
            var resSave = await ApiClient.GetAsync<bool>($"api/joke/{Joke.JokeId}/is-saved");
            if (resSave.IsSuccess)
            {
                _isSaved = resSave.Data;
            }
            else
            {
                ToastService.ShowError("Failed to load joke rating");
            }

            await base.OnInitializedAsync();
        }

        private async Task OnSaveClick()
        {
            if (_isSaved == true)
            {
                var res = await ApiClient.DeleteAsync($"api/saved-jokes/{Joke.JokeId}");
                if (res.IsSuccess)
                {
                    _isSaved = false;
                    StateHasChanged();
                }
                else
                {
                    ToastService.ShowError("Failed to remove save");
                }
            }
            else
            {
                var res = await ApiClient.PostAsync<bool, int>($"api/saved-jokes/{Joke.JokeId}", Joke.JokeId);
                if (res.IsSuccess)
                {
                    _isSaved = true;
                    StateHasChanged();
                }
                else
                {
                    ToastService.ShowError("Failed to save joke");
                }
            }
        }

        private async Task OnRateClick(bool newValue)
        {
            if (_isRatingBusy) return;
            _isRatingBusy = true;
            try
            {
                if (_isLiked == newValue)
                {
                    var res = await ApiClient.PutAsync<RatingDto, bool?>($"api/joke/{Joke.JokeId}/rating", null);
                    if (res.IsSuccess)
                    {
                        if (newValue) Joke.TotalLikes--;
                        else Joke.TotalDislikes--;
                        _isLiked = null;
                    }
                    else
                    {
                        ToastService.ShowError("Failed to remove rating");
                    }
                    StateHasChanged();
                    return;
                }

                var updateRes = await ApiClient.PutAsync<RatingDto, bool>($"api/joke/{Joke.JokeId}/rating", newValue);
                if (updateRes.IsSuccess)
                {
                    if (newValue)
                    {
                        if (_isLiked == false) Joke.TotalDislikes--;
                        if (_isLiked != true) Joke.TotalLikes++;
                    }
                    else
                    {
                        if (_isLiked == true) Joke.TotalLikes--;
                        if (_isLiked != false) Joke.TotalDislikes++;
                    }
                    _isLiked = newValue;
                }
                else
                {
                    ToastService.ShowError("Failed to rate joke");
                }
                StateHasChanged();
            }
            finally
            {
                _isRatingBusy = false;
            }
        }
        private void ToggleComments()
        {
            areCommentsOpen = !areCommentsOpen;
        }
        private void CloseCommentsIfOpen()
        {
            if (areCommentsOpen)
                areCommentsOpen = false;
        }

        private async Task HandleDelete()
        {
            var res = await ApiClient.DeleteAsync($"api/joke/{DeleteId}");
            if (res.IsSuccess)
            {
                ToastService.ShowSuccess("Joke deleted");
                DeleteModal?.Close();
                NavigationManager.NavigateTo("/", true);
            }
            else ToastService.ShowError("Failed to delete");
        }
        private int CommentsCount()
        {
            return CountCommentsRecursive(Joke.Comments);
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
        private async Task OnEditSave()
        {
            var jokeRes = await ApiClient.GetAsync<JokeDto>($"api/joke/{Joke.JokeId}");
            if (jokeRes.IsSuccess && jokeRes.Data != null)
            {
                Joke = jokeRes.Data;
                StateHasChanged();
            }
            else
            {
                ToastService.ShowError("Fail to load joke");
            }
        }
    }
}