using WebLabMobile.Models;
using WebLabMobile.Services;

namespace WebLabMobile.Pages;

public partial class HomePage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ArticleService _articles;
    private int? _editingArticleId;

    public HomePage()
    {
        InitializeComponent();
        var svc = IPlatformApplication.Current!.Services;
        _auth = svc.GetRequiredService<AuthService>();
        _articles = svc.GetRequiredService<ArticleService>();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        NewArticleButton.IsVisible = _auth.CurrentUser is not null;
        await LoadArticles();
    }

    private async Task LoadArticles()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        ErrorLabel.IsVisible = false;

        try
        {
            var articles = await _articles.GetAllAsync();
            var uid = _auth.CurrentUser?.Id;
            ArticlesView.ItemsSource = articles
                .Select(a => new ArticleItem(a, uid.HasValue && a.AuthorId == uid.Value))
                .ToList();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            Refresher.IsRefreshing = false;
        }
    }

    private async void OnRefreshing(object sender, EventArgs e) => await LoadArticles();

    private async void OnArticleTapped(object sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is ArticleItem item)
            await Shell.Current.GoToAsync($"articledetail?id={item.Id}");
    }

    private void OnNewArticleClicked(object sender, EventArgs e) => ShowModal(null);

    private async void OnArticleMenuTapped(object sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not ArticleItem item) return;

        var action = await DisplayActionSheet(item.Title, "Cancel", null, "Edit", "Delete");
        if (action == "Edit")
            ShowModal(item);
        else if (action == "Delete")
            await DeleteArticle(item);
    }

    private async Task DeleteArticle(ArticleItem item)
    {
        bool confirmed = await DisplayAlert("Delete Article",
            $"Delete \"{item.Title}\"?", "Delete", "Cancel");
        if (!confirmed) return;

        try
        {
            await _articles.DeleteAsync(item.Id);
            await LoadArticles();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void ShowModal(ArticleItem? editing)
    {
        _editingArticleId = editing?.Id;
        ModalTitle.Text = editing is null ? "New Article" : "Edit Article";
        TitleEntry.Text = editing?.Article.Title ?? string.Empty;
        ContentEditor.Text = editing?.Article.Content ?? string.Empty;
        SubmitButton.Text = editing is null ? "Create" : "Save";
        ModalError.IsVisible = false;
        ModalOverlay.IsVisible = true;
    }

    private void OnCancelClicked(object sender, EventArgs e) => ModalOverlay.IsVisible = false;

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim();
        var content = ContentEditor.Text?.Trim();

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
        {
            ModalError.Text = "Title and content are required.";
            ModalError.IsVisible = true;
            return;
        }

        SubmitButton.IsEnabled = false;
        ModalError.IsVisible = false;

        try
        {
            if (_editingArticleId is int id)
                await _articles.UpdateAsync(id, title, content);
            else
                await _articles.CreateAsync(title, content);

            ModalOverlay.IsVisible = false;
            await LoadArticles();
        }
        catch (Exception ex)
        {
            ModalError.Text = ex.Message;
            ModalError.IsVisible = true;
        }
        finally
        {
            SubmitButton.IsEnabled = true;
        }
    }

    private sealed class ArticleItem(Article article, bool isOwner)
    {
        public Article Article { get; } = article;
        public bool IsOwner { get; } = isOwner;
        public int Id => Article.Id;
        public string Title => Article.Title;
        public string Excerpt => Article.Content.Length > 120
            ? Article.Content[..120] + "…"
            : Article.Content;
        public string AuthorAndDate =>
            $"{Article.AuthorName} · {FormatDate(Article.CreatedAt)}";

        private static string FormatDate(string s) =>
            DateTime.TryParse(s, out var dt) ? dt.ToLocalTime().ToShortDateString() : s;
    }
}
