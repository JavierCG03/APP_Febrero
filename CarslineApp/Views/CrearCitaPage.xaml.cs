
using CarslineApp.ViewModels.Creacion_Citas;

namespace CarslineApp.Views
{
    public partial class CrearCitaPage : ContentPage
    {
        private readonly CrearCitaViewModel _viewModel;
        public CrearCitaPage(int tipoOrdenId, int VehiculoId, int ClienteId, DateTime FechahoraCita)
        {
            InitializeComponent();
            _viewModel = new CrearCitaViewModel(tipoOrdenId, VehiculoId, ClienteId, FechahoraCita);
            BindingContext = _viewModel;
        }
        // Evento para recalcular costo cuando se selecciona un servicio extra
        private void OnServicioExtraChanged(object sender, CheckedChangedEventArgs e)
        {
            _viewModel.CalcularCostoTotal();
        }
    }
}