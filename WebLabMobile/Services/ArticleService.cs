using WebLabMobile.Models;

namespace WebLabMobile.Services;

public class ArticleService(ApiClient api)
{
    public Task<List<Article>> GetAllAsync() =>
        api.GetAsync<List<Article>>("/api/articles");

    public Task<Article> GetByIdAsync(int id) =>
        api.GetAsync<Article>($"/api/articles/{id}");

    public Task<Article> CreateAsync(string title, string content) =>
        api.PostAsync<Article>("/api/articles", new { title, content });

    public Task<Article> UpdateAsync(int id, string title, string content) =>
        api.PutAsync<Article>($"/api/articles/{id}", new { title, content });

    public Task DeleteAsync(int id) =>
        api.DeleteAsync($"/api/articles/{id}");
}
