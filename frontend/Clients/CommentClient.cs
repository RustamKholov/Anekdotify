using System;
using System.Threading.Tasks;
using frontend.Comments.DTOs;
using frontend.DTOs;
using frontend.Mappers;
using frontend.Models;

namespace frontend.Clients;

public class CommentClient(HttpClient httpClient)
{
    
    public async Task<Comment[]> GetCommentsAsync()
    => await httpClient.GetFromJsonAsync<Comment[]>("api/comments") ?? [];

    public async Task AddCommentAsync(CommentCreateDTO commentCreateDTO)
        => await httpClient.PostAsJsonAsync($"api/comments/{commentCreateDTO.JokeId}", commentCreateDTO);

    public async Task UpdateCommentAsync(CommentEditDTO commentEditDTO, int id)
        => await httpClient.PutAsJsonAsync($"api/comments/{id}", commentEditDTO);

    public async Task<Comment> GetCommentAsync(int id)
        => await httpClient.GetFromJsonAsync<Comment>($"api/comments/{id}") ?? throw new Exception("comment not found");

    public async Task DeleteCommentAsync(int id)
        => await httpClient.DeleteAsync($"api/comments/{id}");
}
