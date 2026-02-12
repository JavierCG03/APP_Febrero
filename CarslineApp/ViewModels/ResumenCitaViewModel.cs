using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Views;
using CarslineApp.Services;

namespace CarslineApp.ViewModels
{
    public class ResumenCitaViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        // ID de la cita
        public int CitaId { get; set; }

        // Datos para mostrar en UI
        private DateTime _fechaCita;
        private string _nombreCliente = string.Empty;
        private string _direccionCliente = string.Empty;
        private string _rfcCliente = string.Empty;
        private string _telefonoCliente = string.Empty;
        private string _vehiculoCompleto = string.Empty;
        private string _vinVehiculo = string.Empty;
        private string _placasVehiculo = string.Empty;
        private string _tipoOrdenNombre = string.Empty;
        private string _tipoServicioNombre = string.Empty;
        private ObservableCollection<TrabajoDetalleDto> _trabajos;

        public ResumenCitaViewModel(int citaId)
        {
            _apiService = new ApiService();
            CitaId = citaId;
            _trabajos = new ObservableCollection<TrabajoDetalleDto>();

            // Comandos
            EditarOrdenCommand = new Command(async () => await EditarOrden());
            ConfirmarOrdenCommand = new Command(async () => await ConfirmarOrden(), () => !IsLoading);
            CancelarCitaCommand = new Command(async () => await CancelarCita());
            ReagendarCommand = new Command(async () => await ReagendarCita());
            // Cargar datos para el resumen
            _ = CargarDatosResumenAsync();
        }

        #region Propiedades

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)ConfirmarOrdenCommand).ChangeCanExecute();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public DateTime FechaCita
        {
            get => _fechaCita;
            set
            {
                _fechaCita = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FechaPromesaFormateada));
                OnPropertyChanged(nameof(HoraPromesaFormateada));
            }
        }

        public string NombreCliente
        {
            get => _nombreCliente;
            set { _nombreCliente = value; OnPropertyChanged(); }
        }

        public string DireccionCliente
        {
            get => _direccionCliente;
            set { _direccionCliente = value; OnPropertyChanged(); }
        }

        public string RfcCliente
        {
            get => _rfcCliente;
            set { _rfcCliente = value; OnPropertyChanged(); }
        }

        public string TelefonoCliente
        {
            get => _telefonoCliente;
            set { _telefonoCliente = value; OnPropertyChanged(); }
        }

        public string VehiculoCompleto
        {
            get => _vehiculoCompleto;
            set { _vehiculoCompleto = value; OnPropertyChanged(); }
        }

        public string VinVehiculo
        {
            get => _vinVehiculo;
            set { _vinVehiculo = value; OnPropertyChanged(); }
        }

        public string PlacasVehiculo
        {
            get => _placasVehiculo;
            set { _placasVehiculo = value; OnPropertyChanged(); }
        }

        public string TipoOrdenNombre
        {
            get => _tipoOrdenNombre;
            set { _tipoOrdenNombre = value; OnPropertyChanged(); }
        }

        public string TipoServicioNombre
        {
            get => _tipoServicioNombre;
            set { _tipoServicioNombre = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TrabajoDetalleDto> Trabajos
        {
            get => _trabajos;
            set
            {
                _trabajos = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CantidadTrabajos));
            }
        }

        public string FechaPromesaFormateada => FechaCita.ToString("dd/MMM/yyyy");
        public string HoraPromesaFormateada => FechaCita.ToString("hh:mm tt");
        public int CantidadTrabajos => Trabajos?.Count ?? 0;

        #endregion

        #region Comandos

        public ICommand ConfirmarOrdenCommand { get; }
        public ICommand EditarOrdenCommand { get; }
        public ICommand CancelarCitaCommand { get; }
        public ICommand ReagendarCommand { get; }

        #endregion

        #region Métodos

        private async Task CargarDatosResumenAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine($"📋 Cargando resumen de cita {CitaId}");

                // ✅ Llamar al nuevo endpoint que trae TODA la información
                var response = await _apiService.ObtenerDetalleCitaAsync(CitaId);

                if (response.Success && response.Cita != null)
                {
                    var cita = response.Cita;

                    // ✅ Asignar todos los datos directamente desde la respuesta
                    FechaCita = cita.FechaCita;
                    NombreCliente = cita.NombreCliente;
                    TelefonoCliente = cita.TelefonoCliente;
                    DireccionCliente = cita.DireccionCliente;
                    RfcCliente = cita.RfcCliente;
                    VehiculoCompleto = cita.VehiculoCompleto;
                    VinVehiculo = cita.VinVehiculo;
                    PlacasVehiculo = cita.PlacasVehiculo;
                    TipoOrdenNombre = cita.TipoOrdenNombre;
                    TipoServicioNombre = cita.TipoServicioNombre;

                    // ✅ Asignar trabajos
                    if (cita.Trabajos != null && cita.Trabajos.Any())
                    {
                        Trabajos = new ObservableCollection<TrabajoDetalleDto>(cita.Trabajos);
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Resumen cargado: {NombreCliente} - {VehiculoCompleto}");
                    System.Diagnostics.Debug.WriteLine($"✅ Trabajos: {CantidadTrabajos}");
                }
                else
                {
                    ErrorMessage = response.Message ?? "No se pudo cargar la información de la cita";
                    System.Diagnostics.Debug.WriteLine($"❌ Error: {ErrorMessage}");

                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        ErrorMessage,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar datos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Excepción: {ex.Message}");

                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    "Ocurrió un error al cargar el resumen de la cita",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }


        private async Task CancelarCita()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Cancelar Cita",
                "¿Estás seguro que deseas cancelar esta Cita?\n\n" +
                " Esto cancelará todos los trabajos.",
                "Sí",
                "No");

            if (!confirm) return;

            IsLoading = true;

            try
            {
                var response = await _apiService.CancelarCitaAsync(CitaId);

                if (response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Éxito",
                        "Cita Cancelada",
                        "OK");

                    await Application.Current.MainPage.Navigation.PopToRootAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        response.Message,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al cancelar la cita: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ReagendarCita()
        {
            try
            {
                IsLoading = true;
                await Application.Current.MainPage.Navigation.PushAsync(new AgendaCitas(CitaId, 0, 0));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo abrir la agenda de citas: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }

        }




        private async Task ConfirmarOrden()
        {

            await Application.Current.MainPage.DisplayAlert(
                "✅ Confirmación",
                "Orden confirmada exitosamente",
                "OK");

            // Navegar de regreso a la página principal o agenda
            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }

        private async Task EditarOrden()
        {
            // Regresar a la página anterior (CrearOrden)
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}