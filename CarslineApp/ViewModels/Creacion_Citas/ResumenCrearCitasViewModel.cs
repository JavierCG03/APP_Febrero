using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Views;
using CarslineApp.Services;

namespace CarslineApp.ViewModels.Creacion_Citas
{
    public class ResumenCrearCitasViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        // Datos pasados desde CrearOrdenViewModel
        public int TipoOrdenId { get; set; }
        public int ClienteId { get; set; }
        public int VehiculoId { get; set; }
        public int TipoServicioId { get; set; }
        public DateTime FechaHoraCita { get; set; }

        public List<TrabajoCrearDto> Trabajos { get; set; }

        // Datos para mostrar en UI
        private string _nombreCliente = string.Empty;
        private string _direccionCliente = string.Empty;
        private string _rfcCliente = string.Empty;
        private string _telefonoCliente = string.Empty;
        private string _vehiculoCompleto = string.Empty;
        private string _vinVehiculo = string.Empty;
        private string _placasVehiculo = string.Empty;
        private string _tipoOrdenNombre = string.Empty;
        private string _tipoServicioNombre = string.Empty;
        private decimal _costoTotal;

        public ResumenCrearCitasViewModel(
            int tipoOrdenId,
            int clienteId,
            int vehiculoId,
            int tipoServicioId,
            DateTime fechaHoraCita,
            string observaciones,
            List<TrabajoCrearDto> trabajos)
        {
            _apiService = new ApiService();

            TipoOrdenId = tipoOrdenId;
            ClienteId = clienteId;
            VehiculoId = vehiculoId;
            TipoServicioId = tipoServicioId;
            FechaHoraCita = fechaHoraCita;
            Trabajos = trabajos ?? new List<TrabajoCrearDto>();
            // Comandos
            ConfirmarCitaCommand = new Command(async () => await ConfirmarCita(), () => !IsLoading);
            EditarCitaCommand = new Command(async () => await EditarCita());

            // Cargar datos para el resumen
            CargarDatosResumen();
        }

        #region Propiedades

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)ConfirmarCitaCommand).ChangeCanExecute();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
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

        public decimal CostoTotal
        {
            get => _costoTotal;
            set { _costoTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(CostoTotalFormateado)); }
        }

        public string CostoTotalFormateado => $"${CostoTotal:N2}";

        public string FechaCitaFormateada => FechaHoraCita.ToString("dd/MMM/yyyy");
        public string HoraCitaFormateada => FechaHoraCita.ToString("hh:mm tt");
        public int CantidadTrabajos => Trabajos?.Count ?? 0;


        #endregion

        #region Comandos

        public ICommand ConfirmarCitaCommand { get; }
        public ICommand EditarCitaCommand { get; }

        #endregion

        #region Métodos

        private async void CargarDatosResumen()
        {
            IsLoading = true;

            try
            {
                // Cargar datos del cliente
                var clienteResponse = await _apiService.ObtenerClientePorIdAsync(ClienteId);
                if (clienteResponse.Success && clienteResponse.Cliente != null)
                {
                    NombreCliente = clienteResponse.Cliente.NombreCompleto;
                    DireccionCliente = $"{clienteResponse.Cliente.Colonia}, Mpio.{clienteResponse.Cliente.Municipio}, Edo.{clienteResponse.Cliente.Estado}";
                    RfcCliente = clienteResponse.Cliente.RFC;
                    TelefonoCliente = clienteResponse.Cliente.TelefonoMovil;
                }

                // Cargar datos del vehículo
                var vehiculoResponse = await _apiService.ObtenerVehiculoPorIdAsync(VehiculoId);
                if (vehiculoResponse.Success && vehiculoResponse.Vehiculo != null)
                {
                    VehiculoCompleto = vehiculoResponse.Vehiculo.VehiculoCompleto;
                    VinVehiculo = vehiculoResponse.Vehiculo.VIN;
                    PlacasVehiculo = vehiculoResponse.Vehiculo.Placas;
                }

                // Determinar tipo de orden
                TipoOrdenNombre = TipoOrdenId switch
                {
                    1 => "SERVICIO",
                    2 => "DIAGNÓSTICO",
                    3 => "REPARACIÓN",
                    4 => "GARANTÍA",
                    _ => "ORDEN"
                };

                // Cargar tipo de servicio (si aplica)
                if (TipoServicioId > 0 && TipoOrdenId == 1)
                {
                    var tiposServicio = await _apiService.ObtenerTiposServicioAsync();
                    var tipoServicio = tiposServicio.FirstOrDefault(t => t.Id == TipoServicioId);
                    if (tipoServicio != null)
                    {
                        TipoServicioNombre = tipoServicio.Nombre;
                        CostoTotal = tipoServicio.Precio;
                    }
                }

                // Calcular costo total para servicios
                if (TipoOrdenId == 1)
                {
                    var serviciosExtra = await _apiService.ObtenerServiciosFrecuentesAsync();
                    foreach (var trabajo in Trabajos.Where(t => t.Trabajo != TipoServicioNombre))
                    {
                        var servicio = serviciosExtra.FirstOrDefault(s => s.Nombre == trabajo.Trabajo);
                        if (servicio != null)
                        {
                            CostoTotal += servicio.Precio;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar datos: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ConfirmarCita()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var request = new CrearCitaConTrabajosRequest
                {
                    TipoOrdenId = TipoOrdenId,
                    ClienteId = ClienteId,
                    VehiculoId = VehiculoId,
                    TipoServicioId = TipoServicioId,
                    FechaCita = FechaHoraCita,
                    Trabajos = Trabajos
                };

                int encargadoCitasId = Preferences.Get("user_id", 0);
                var response = await _apiService.CrearCitaConTrabajosAsync(request, encargadoCitasId);
                if (response.Success)
                {
                    // Mensaje de éxito
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ ¡Éxito!",
                        $"Cita creada exitosamente",
                        "OK");
                    await Application.Current.MainPage.Navigation.PushAsync(new AgendaCitas(0, 0, 0));

                }

            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al crear Cita: {ex.Message}",
                    "OK");

                Application.Current.MainPage = new NavigationPage(new AgendaCitas(0, 0, 0));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditarCita()
        {

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