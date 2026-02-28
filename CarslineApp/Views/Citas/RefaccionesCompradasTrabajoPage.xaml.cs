using CarslineApp.ViewModels.Citas;

namespace CarslineApp.Views.Citas;

public partial class RefaccionesCompradasTrabajoPage : ContentPage
{
    private readonly RefaccionesCompradasTrabajoViewModel _viewModel;

    public RefaccionesCompradasTrabajoPage(int trabajoId, string trabajo, string vehiculo, string vin, bool orden)
    {
        InitializeComponent();

        _viewModel = new RefaccionesCompradasTrabajoViewModel(trabajoId,trabajo,vehiculo,vin,orden);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InicializarAsync();
    }
}