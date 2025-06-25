using Anekdotify.Frontend.Components.BaseComponents;
using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.DTOs.JokeRating;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;
using Microsoft.AspNetCore.Components;

namespace Anekdotify.Frontend.Components.Pages
{
    public partial class JokeCard
    {
        [Parameter] public JokeDTO? Joke { get; set; }
        public AppModal? Modal { get; set; }

        public int? SelectedJokeId = null;

        private List<CommentDTO> comments = new List<CommentDTO>();
        public int DeleteId { get; set; }
        private bool? isLiked = null;
        private bool? isSaved = null;

        private static string JokeUrl(int id) => $"/editJoke/{id}";

        protected override async Task OnInitializedAsync()
        {

            if (Joke == null)
            {
                ToastService.ShowError("Joke not found");
                return;
            }

            var resRate = await ApiClient.GetAsync<RatingDTO>($"api/joke/{Joke.JokeId}/rating");
            if (resRate.IsSuccess && resRate.Data != null)
            {
                if (resRate.Data.IsLike != null)
                {
                    isLiked = resRate.Data.IsLike;
                }

            }
            var resSave = await ApiClient.GetAsync<bool>($"api/joke/{Joke.JokeId}/is-saved");
            if (resSave.IsSuccess)
            {
                isSaved = resSave.Data;
            }
            else
            {
                ToastService.ShowError("Failed to load joke rating");
            }

            await base.OnInitializedAsync();
        }
        private async Task OnSaveClick()
        {
            if (Joke == null)
            {
                ToastService.ShowError("Joke not found");
                return;
            }

            if (isSaved == true)
            {
                var res = await ApiClient.DeleteAsync($"api/saved-jokes/{Joke.JokeId}");
                if (res.IsSuccess)
                {
                    isSaved = false;
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
                    isSaved = true;
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
            if (isLiked == newValue)
            {
                if (Joke == null)
                {
                    ToastService.ShowError("Joke not found");
                    return;
                }
                var res = await ApiClient.DeleteAsync($"api/joke/{Joke.JokeId}/rating/delete");
                if (res.IsSuccess)
                {
                    if (newValue) Joke.TotalLikes--;
                    else Joke.TotalDislikes--;

                    isLiked = null;
                    StateHasChanged();
                }
                else
                {
                    ToastService.ShowError("Failed to remove rating");
                }
                return;
            }

            if (Joke == null)
            {
                ToastService.ShowError("Joke not found");
                return;
            }
            var updateRes = await ApiClient.PutAsync<RatingDTO, bool>($"api/joke/{Joke.JokeId}/rating", newValue);
            if (updateRes.IsSuccess)
            {
                if (newValue)
                {
                    if (isLiked == false)
                    {
                        Joke.TotalDislikes--;
                        Joke.TotalLikes++;
                    }
                    else if (isLiked == null) Joke.TotalLikes++;
                }
                else
                {
                    if (isLiked == true)
                    {
                        Joke.TotalLikes--;
                        Joke.TotalDislikes++;
                    }
                    else if (isLiked == null) Joke.TotalDislikes++;
                }

                isLiked = newValue;
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

        private void CloseModal()
        {
            SelectedJokeId = null;
        }
    }
}
