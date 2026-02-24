using CarslineApp.ViewModels;

namespace CarslineApp.Views.Citas;
public partial class AgendaCitas : ContentPage
{
    private readonly AgendaCitasViewModel _viewModel;

    public AgendaCitas(int CitaId, int ClienteId, int VehiculoId)
    {
        InitializeComponent();
        _viewModel = new AgendaCitasViewModel(CitaId, ClienteId, VehiculoId);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InicializarAsync();
    }
}

