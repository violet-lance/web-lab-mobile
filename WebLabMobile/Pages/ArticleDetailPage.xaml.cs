using WebLabMobile.Models;
using WebLabMobile.Services;

namespace WebLabMobile.Pages;

[QueryProperty(nameof(ArticleId), "id")]
public partial class ArticleDetailPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ArticleService _articles;
    private readonly CommentService _comments;

    private int _articleId;
    private Article? _article;

    public int ArticleId
    {
        get => _articleId;
        set
        {
            _articleId = value;
            Title = "Article";
        }
    }

    public ArticleDetailPage()
    {
        InitializeComponent();
        var svc = IPlatformApplication.Current!.Services;
        _auth = svc.GetRequiredService<AuthService>();
        _articles = svc.GetRequiredService<ArticleService>();
        _comments = svc.GetRequiredService<CommentService>();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await LoadData();
    }

    private async Task LoadData()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            _article = await _articles.GetByIdAsync(_articleId);
            var uid = _auth.CurrentUser?.Id;

            Title = _article.Title.Length > 30
                ? _article.Title[..30] + "…"
                : _article.Title;

            ArticleTitleLabel.Text = _article.Title;
            ArticleMetaLabel.Text = $"{_article.AuthorName} · {FormatDate(_article.CreatedAt)}";
            ArticleContentLabel.Text = _article.Content;

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            await LoadComments(uid);
        }
        catch (Exception ex)
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task LoadComments(int? uid = null)
    {
        uid ??= _auth.CurrentUser?.Id;
        try
        {
            var comments = await _comments.GetAllAsync(_articleId);
            CommentsView.ItemsSource = comments
                .Select(c => new CommentItem(c, uid.HasValue && c.AuthorId == uid.Value))
                .ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnPostCommentClicked(object sender, EventArgs e)
    {
        var content = CommentEditor.Text?.Trim();
        if (string.IsNullOrEmpty(content)) return;

        PostButton.IsEnabled = false;
        try
        {
            await _comments.CreateAsync(_articleId, content);
            CommentEditor.Text = string.Empty;
            await LoadComments();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            PostButton.IsEnabled = true;
        }
    }

    private async void OnEditCommentTapped(object sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not CommentItem item) return;

        var result = await DisplayPromptAsync("Edit Comment", null,
            initialValue: item.Comment.Content,
            maxLength: 2000,
            keyboard: Keyboard.Text);

        if (result is null || result.Trim() == item.Comment.Content) return;

        var trimmed = result.Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        try
        {
            await _comments.UpdateAsync(_articleId, item.Comment.Id, trimmed);
            await LoadComments();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnDeleteCommentTapped(object sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not CommentItem item) return;

        bool confirmed = await DisplayAlert("Delete Comment",
            "Are you sure you want to delete this comment?", "Delete", "Cancel");
        if (!confirmed) return;

        try
        {
            await _comments.DeleteAsync(_articleId, item.Comment.Id);
            await LoadComments();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private static string FormatDate(string s) =>
        DateTime.TryParse(s, out var dt) ? dt.ToLocalTime().ToShortDateString() : s;

    private sealed class CommentItem(Comment comment, bool isOwner)
    {
        public Comment Comment { get; } = comment;
        public bool IsOwner { get; } = isOwner;
        public string Content => Comment.Content;
        public string AuthorAndDate =>
            $"{Comment.AuthorName} · {FormatDate(Comment.CreatedAt)}";
    }
}
