using WebLabMobile.Services;

namespace WebLabMobile.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly AuthService _auth;

    public ProfilePage()
    {
        InitializeComponent();
        _auth = IPlatformApplication.Current!.Services.GetRequiredService<AuthService>();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        var user = _auth.CurrentUser;
        if (user is null) return;

        NameLabel.Text = user.Name;
        EmailLabel.Text = user.Email;
        GenderLabel.Text = string.IsNullOrEmpty(user.Gender) ? "—" : user.Gender;
        DobLabel.Text = FormatDate(user.DateOfBirth);
        MemberSinceLabel.Text = FormatDate(user.CreatedAt);
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        _auth.Logout();
        await Shell.Current.GoToAsync("//login");
    }

    private static string FormatDate(string s) =>
        DateTime.TryParse(s, out var dt) ? dt.ToLocalTime().ToShortDateString() : s;
}
