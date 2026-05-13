using WebLabMobile.Models;

namespace WebLabMobile.Services;

public class CommentService(ApiClient api)
{
    public Task<List<Comment>> GetAllAsync(int articleId) =>
        api.GetAsync<List<Comment>>($"/api/articles/{articleId}/comments");

    public Task<Comment> CreateAsync(int articleId, string content) =>
        api.PostAsync<Comment>($"/api/articles/{articleId}/comments", new { content });

    public Task<Comment> UpdateAsync(int articleId, int commentId, string content) =>
        api.PutAsync<Comment>($"/api/articles/{articleId}/comments/{commentId}", new { content });

    public Task DeleteAsync(int articleId, int commentId) =>
        api.DeleteAsync($"/api/articles/{articleId}/comments/{commentId}");
}
