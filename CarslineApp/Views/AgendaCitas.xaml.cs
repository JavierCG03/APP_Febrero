
using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class AgendaCitas : ContentPage
    {
        private readonly AgendaCitasViewModel _viewModel;

        public AgendaCitas()
        {
            InitializeComponent();
            _viewModel = new AgendaCitasViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InicializarAsync();
        }
    }
}