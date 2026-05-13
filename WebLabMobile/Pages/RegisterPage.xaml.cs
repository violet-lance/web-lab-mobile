using WebLabMobile.Services;

namespace WebLabMobile.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _auth;

    public RegisterPage()
    {
        InitializeComponent();
        _auth = IPlatformApplication.Current!.Services.GetRequiredService<AuthService>();
        DobPicker.MaximumDate = DateTime.Today;
        DobPicker.Date = DateTime.Today.AddYears(-18);
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        var gender = GenderPicker.SelectedItem as string;
        var d = DobPicker.Date ?? DateTime.Today.AddYears(-18);
        var dob = $"{d.Year:D4}-{d.Month:D2}-{d.Day:D2}";

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || gender is null)
        {
            ErrorLabel.Text = "Please fill in all fields.";
            ErrorLabel.IsVisible = true;
            return;
        }

        RegisterButton.IsEnabled = false;
        ErrorLabel.IsVisible = false;

        try
        {
            await _auth.RegisterAsync(email, password, name, dob, gender);
            await Shell.Current.GoToAsync("//home");
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            RegisterButton.IsEnabled = true;
        }
    }

    private async void OnLoginTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
