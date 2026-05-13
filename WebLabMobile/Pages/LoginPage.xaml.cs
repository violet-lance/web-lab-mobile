using WebLabMobile.Services;

namespace WebLabMobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;

    public LoginPage()
    {
        InitializeComponent();
        _auth = IPlatformApplication.Current!.Services.GetRequiredService<AuthService>();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Please fill in all fields.";
            ErrorLabel.IsVisible = true;
            return;
        }

        LoginButton.IsEnabled = false;
        ErrorLabel.IsVisible = false;

        try
        {
            await _auth.LoginAsync(email, password);
            await Shell.Current.GoToAsync("//home");
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("register");
    }
}
