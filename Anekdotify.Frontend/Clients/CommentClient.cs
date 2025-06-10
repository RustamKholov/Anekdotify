using Anekdotify.Models.DTOs.Comments;
using Anekdotify.Models.Entities;

namespace Anekdotify.Frontend.Clients;

public class CommentClient(HttpClient httpClient)
{
    
    public async Task<Comment[]> GetCommentsAsync()
    => await httpClient.GetFromJsonAsync<Comment[]>("api/comments") ?? [];

    public async Task AddCommentAsync(CommentCreateDTO commentCreateDTO, int commentId)
        => await httpClient.PostAsJsonAsync($"api/comments/{commentId}", commentCreateDTO);

    public async Task UpdateCommentAsync(string commentText, int id)
        => await httpClient.PutAsJsonAsync($"api/comments/{id}", commentText);

    public async Task<Comment> GetCommentAsync(int id)
        => await httpClient.GetFromJsonAsync<Comment>($"api/comments/{id}") ?? throw new Exception("comment not found");

    public async Task DeleteCommentAsync(int id)
        => await httpClient.DeleteAsync($"api/comments/{id}");
}
