using WebLabMobile.Models;

namespace WebLabMobile.Services;

public class AuthService(ApiClient api)
{
    public User? CurrentUser { get; private set; }

    public async Task<bool> TryRestoreSessionAsync()
    {
        if (await SecureStorage.GetAsync("token") is null) return false;
        try
        {
            CurrentUser = await api.GetAsync<User>("/api/profile/me");
            return true;
        }
        catch
        {
            SecureStorage.Remove("token");
            CurrentUser = null;
            return false;
        }
    }

    public async Task LoginAsync(string email, string password)
    {
        var res = await api.PostAsync<AuthResponse>("/api/auth/login", new { email, password });
        await SecureStorage.SetAsync("token", res.Token);
        CurrentUser = await api.GetAsync<User>("/api/profile/me");
    }

    public async Task RegisterAsync(string email, string password, string name, string dateOfBirth, string gender)
    {
        var res = await api.PostAsync<AuthResponse>("/api/auth/register",
            new { email, password, name, dateOfBirth, gender });
        await SecureStorage.SetAsync("token", res.Token);
        CurrentUser = await api.GetAsync<User>("/api/profile/me");
    }

    public void Logout()
    {
        SecureStorage.Remove("token");
        CurrentUser = null;
    }
}
