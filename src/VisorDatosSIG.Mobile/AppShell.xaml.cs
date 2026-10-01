namespace VisorDatosSIG.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("MapPage", typeof(Views.MapPage));
	}

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        Preferences.Default.Remove("jwt_token");
        await Current.GoToAsync("//LoginPage");
    }
}
