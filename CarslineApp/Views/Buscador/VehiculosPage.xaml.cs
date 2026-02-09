using CarslineApp.ViewModels.ViewModelBuscador;
namespace CarslineApp.Views.Buscador;

public partial class VehiculosPage : ContentPage
{

    public VehiculosPage(int vehiculoId)
    {
        {
            InitializeComponent();
            BindingContext = new VehiculoDetalleViewModel(vehiculoId);
        }
        ConfigurarBarraNavegacion();
    }
    private void ConfigurarBarraNavegacion()
    {

        Shell.SetBackgroundColor(this, Color.FromArgb("#B00000"));
        Shell.SetForegroundColor(this, Colors.White);


        if (Application.Current?.MainPage is NavigationPage navigationPage)
        {
            navigationPage.BarBackgroundColor = Color.FromArgb("#B00000");
            navigationPage.BarTextColor = Colors.White;
        }
    }
}

