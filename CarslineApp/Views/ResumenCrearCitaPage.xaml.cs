using CarslineApp.Models;
using CarslineApp.ViewModels.Creacion_Citas;

namespace CarslineApp.Views
{
    public partial class ResumenCrearCitaPage : ContentPage
    {
        public ResumenCrearCitaPage(
            int tipoOrdenId,
            int clienteId,
            int vehiculoId,
            int tipoServicioId,
            DateTime fechaHoraCita,
            string observaciones,
            List<TrabajoCrearDto> trabajos)
        {
            InitializeComponent();

            var viewModel = new ResumenCitasViewModel(
                tipoOrdenId,
                clienteId,
                vehiculoId,
                tipoServicioId,
                fechaHoraCita,
                observaciones,
                trabajos
            );

            BindingContext = viewModel;
        }
    }
}