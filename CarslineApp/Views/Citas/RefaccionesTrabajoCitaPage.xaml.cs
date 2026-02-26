using CarslineApp.ViewModels.Ordenes;

namespace CarslineApp.Views.Citas;

public partial class RefaccionesTrabajoCitaPage : ContentPage
{
    private readonly RefaccionesTrabajoCitaViewModel _viewModel;

    public RefaccionesTrabajoCitaPage(int trabajoCitaId, string trabajo, string vehiculo, string vin)
    {
        InitializeComponent();

        _viewModel = new RefaccionesTrabajoCitaViewModel(trabajoCitaId,trabajo,vehiculo,vin);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InicializarAsync();
    }
}