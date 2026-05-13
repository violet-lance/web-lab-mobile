using WebLabMobile.Pages;
using WebLabMobile.Services;

namespace WebLabMobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("articledetail", typeof(ArticleDetailPage));

        Dispatcher.DispatchAsync(async () =>
        {
            var auth = IPlatformApplication.Current!.Services.GetRequiredService<AuthService>();
            var isAuthenticated = await auth.TryRestoreSessionAsync();
            await GoToAsync(isAuthenticated ? "//home" : "//login");
        });
    }
}
