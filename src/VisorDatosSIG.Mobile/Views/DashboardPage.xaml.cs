namespace VisorDatosSIG.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    private async void OnOpenMapClicked(object sender, EventArgs e)
    {
        // Navegar a la página del mapa dentro del Shell
        await Shell.Current.GoToAsync("//MapPage");
    }
}
