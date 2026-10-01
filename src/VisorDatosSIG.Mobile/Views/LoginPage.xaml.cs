namespace VisorDatosSIG.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // 1. Mostrar estado de carga
        LoginButton.IsEnabled = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        string usuario = UserEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Por favor ingresa usuario y contraseña.", "OK");
            ResetState();
            return;
        }

        // Conectar a la API real
        var apiService = new VisorDatosSIG.Mobile.Services.ApiService();
        var token = await apiService.LoginAsync(usuario, password);

        if (!string.IsNullOrEmpty(token))
        {
            // Guardar Token de forma segura
            Preferences.Default.Set("jwt_token", token);
            
            // Navegar al Panel Principal (Raíz del Flyout) de forma directa, sin mensajes
            await Shell.Current.GoToAsync("//DashboardPage");
        }
        else
        {
            await DisplayAlert("Error", "Credenciales incorrectas o servidor no disponible.", "OK");
        }

        ResetState();
    }

    private void ResetState()
    {
        LoginButton.IsEnabled = true;
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
    }
}
