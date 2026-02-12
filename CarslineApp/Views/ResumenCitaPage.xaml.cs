using CarslineApp.Models;
using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class ResumenCitaPage : ContentPage
    {
        public ResumenCitaPage(int CitaId)
        {
            InitializeComponent();

            var viewModel = new ResumenCitaViewModel(CitaId);

            BindingContext = viewModel;
        }
    }
}