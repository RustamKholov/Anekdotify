using Anekdotify.Frontend.Components.BaseComponents;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.DTOs.JokeRating;
using Anekdotify.Models.DTOs.Jokes;
using Microsoft.AspNetCore.Components;

namespace Anekdotify.Frontend.Components.Pages
{
    public partial class JokeCard
    {
        [Parameter] public required JokeDTO Joke { get; set; }
        [Parameter] public EventCallback OnFirstFlip { get; set; }
        [Parameter] public bool IsFlipped { get; set; } = false;
        private AppModal? Modal { get; set; }

        public int? SelectedJokeId;

        private int DeleteId { get; set; }
        private bool? _isLiked;
        private bool? _isSaved;

        private async Task HandleFlip()
        {
            if (!IsFlipped && OnFirstFlip.HasDelegate)
            {
                await OnFirstFlip.InvokeAsync();
            }
            IsFlipped = !IsFlipped;
        }

        private static string JokeUrl(int id) => $"/editJoke/{id}";

        protected override async Task OnInitializedAsync()
        {
            var resRate = await ApiClient.GetAsync<RatingDTO>($"api/joke/{Joke.JokeId}/rating");
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
            if (_isLiked == newValue)
            {
                var res = await ApiClient.DeleteAsync($"api/joke/{Joke.JokeId}/rating/delete");
                if (res.IsSuccess)
                {
                    if (newValue) Joke.TotalLikes--;
                    else Joke.TotalDislikes--;

                    _isLiked = null;
                    StateHasChanged();
                }
                else
                {
                    ToastService.ShowError("Failed to remove rating");
                }
                return;
            }

            var updateRes = await ApiClient.PutAsync<RatingDTO, bool>($"api/joke/{Joke.JokeId}/rating", newValue);
            if (updateRes.IsSuccess)
            {
                if (newValue)
                {
                    if (_isLiked == false)
                    {
                        Joke.TotalDislikes--;
                        Joke.TotalLikes++;
                    }
                    else if (_isLiked == null) Joke.TotalLikes++;
                }
                else
                {
                    if (_isLiked == true)
                    {
                        Joke.TotalLikes--;
                        Joke.TotalDislikes++;
                    }
                    else if (_isLiked == null) Joke.TotalDislikes++;
                }

                _isLiked = newValue;
                StateHasChanged();
            }
            else
            {
                ToastService.ShowError("Failed to rate joke");
            }
        }

        private async Task HandleDelete()
        {
            var res = await ApiClient.DeleteAsync($"api/joke/{DeleteId}");
            if (res.IsSuccess)
            {
                ToastService.ShowSuccess("Joke deleted");
                Modal?.Close();
                NavigationManager.NavigateTo("/", true);
            }
            else ToastService.ShowError("Failed to delete");
        }

        private int CommentsCount()
        {
            return Joke?.Comments == null ? 0 : CountCommentsRecursive(Joke.Comments);
        }

        private static int CountCommentsRecursive(List<CommentDTO> comments)
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

    }
}
