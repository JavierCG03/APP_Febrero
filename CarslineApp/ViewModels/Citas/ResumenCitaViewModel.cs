using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Views.Ordenes;
using CarslineApp.Views.Citas;
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
        public int VehiculoId { get; set; }
        public int ClienteId { get; set; }
        public int TipoOrdenId { get; set; }
        public int TipoServicioId { get; set; }

        // Datos para mostrar en UI
        private bool _modoRecepcion;
        private DateTime _fechaHoraPromesa = RedondearHora();
        private int _kilometrajeActual;
        private string _observaciones = string.Empty;
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
            RecepcionarCitaCommand = new Command(async () => await RecepcionarCita());
            CrearOrdenCommand = new Command(async () => await CrearOrden(), () => !IsLoading);
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
                ((Command)CrearOrdenCommand).ChangeCanExecute();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }
        public int KilometrajeActual
        {
            get => _kilometrajeActual;
            set
            {
                _kilometrajeActual = value;
                OnPropertyChanged();


            }
        }
        public bool ModoRecepcion
        {
            get => _modoRecepcion;
            set
            {
                _modoRecepcion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarBotonesDetalle));
                OnPropertyChanged(nameof(MostrarBotonCrearOrden));
            }
        }

        public bool MostrarBotonesDetalle => !ModoRecepcion;

        public bool MostrarBotonCrearOrden => ModoRecepcion;
        public DateTime FechaHoraPromesa
        {
            get => _fechaHoraPromesa;
            set
            {
                if (_fechaHoraPromesa == value) return;

                _fechaHoraPromesa = value;
                OnPropertyChanged();

                // sincroniza fecha y hora
                OnPropertyChanged(nameof(Fecha));
                OnPropertyChanged(nameof(Hora));
            }
        }
        public DateTime Fecha
        {
            get => FechaHoraPromesa.Date;
            set
            {
                var nuevaFechaHora = value.Date.Add(Hora);
                FechaHoraPromesa = nuevaFechaHora;
            }
        }


        public TimeSpan Hora
        {
            get => FechaHoraPromesa.TimeOfDay;
            set
            {
                var nuevaFechaHora = Fecha.Date.Add(value);
                FechaHoraPromesa = nuevaFechaHora;
            }
        }
        public string Observaciones
        {
            get => _observaciones;
            set { _observaciones = value; OnPropertyChanged(); }
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

        public ICommand RecepcionarCitaCommand { get; }
        public ICommand CrearOrdenCommand { get; }
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
                    ClienteId = cita.ClienteId;
                    VehiculoId = cita.VehiculoId;
                    TipoOrdenId = cita.TipoOrdenId; 
                    TipoServicioId = cita.TipoServicioId;
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
        private async Task RecepcionarCita()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Recepcionar Unidad",
                "¿Deseas recepcionar la unidad y crear orden?",
                "Sí",
                "No");

            if (!confirm) return;

            ModoRecepcion = true;
        }

        private async Task ReagendarCita()
        {
            try
            {
                IsLoading = true;
                await Application.Current.MainPage.DisplayAlert("Reagendar Cita", "Selecciona un Horario Disponible en la agenda", "OK");
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

        private static DateTime RedondearHora()
        {
            var fecha = DateTime.Now.AddHours(5);
            int minutos = fecha.Minute;
            int minutosRedondeados = (int)(Math.Round(minutos / 30.0) * 30);

            // Si se pasa de 60, avanzamos una hora
            if (minutosRedondeados == 60)
            {
                fecha = fecha.AddHours(1);
                minutosRedondeados = 0;
            }

            // Crear la fecha con minutos redondeados
            var fechaRedondeada = new DateTime(fecha.Year, fecha.Month, fecha.Day, fecha.Hour, minutosRedondeados, 0);

            // Limitar a las 5:30 PM (17:30)
            var horaLimite = new DateTime(fechaRedondeada.Year, fechaRedondeada.Month, fechaRedondeada.Day, 17, 30, 0);

            if (fechaRedondeada > horaLimite)
            {
                return horaLimite;
            }

            return fechaRedondeada;
        }
        private async Task CrearOrden()
        {
            if (KilometrajeActual == 0) 
            {
                ErrorMessage = "Ingresa el Kilometraje Actual de la Unidad";
                return;

            }
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var trabajosCrear = Trabajos
                    .Select(t => new TrabajoCrearDto
                    {
                        Trabajo = t.Trabajo,
                        Indicaciones = t.IndicacionesTrabajo
                    })
                    .ToList();
                var request = new CrearOrdenConTrabajosRequest
                {
                    TipoOrdenId = TipoOrdenId,
                    ClienteId = ClienteId,
                    VehiculoId = VehiculoId,
                    TipoServicioId = TipoServicioId,
                    KilometrajeActual = KilometrajeActual,
                    FechaHoraPromesaEntrega = FechaHoraPromesa,
                    ObservacionesAsesor = Observaciones,
                    Trabajos = trabajosCrear
                };

                int asesorId = Preferences.Get("user_id", 0);
                var response = await _apiService.CrearOrdenConTrabajosAsync(request, asesorId);

                if (response.Success)
                {
                    var cancelarcita = await _apiService.CancelarCitaAsync(CitaId);

                    if (cancelarcita.Success)
                    {
                        // Mensaje de éxito
                        await Application.Current.MainPage.DisplayAlert(
                            "✅ ¡Éxito!",
                            $"Orden {response.NumeroOrden} creada exitosamente",
                            "OK");

                    }

                    // Preguntar si desea tomar evidencias
                    bool tomarEvidencias = await Application.Current.MainPage.DisplayAlert(
                        "📸 Evidencias de recepción",
                        "¿Deseas tomar evidencias de la unidad ahora?",
                        "Sí",
                        "No");

                    if (tomarEvidencias)
                    {
                        // 🔹 Limpiar stack y luego navegar a evidencias
                        await Application.Current.MainPage.Navigation.PopToRootAsync();

                        var evidenciasPage = new EvidenciasOrdenPage(response.OrdenId);
                        await Application.Current.MainPage.Navigation.PushAsync(evidenciasPage);
                    }
                    else
                    {
                        // 🔹 Solo regresar al inicio
                        await Application.Current.MainPage.Navigation.PopToRootAsync();
                    }

                    // Notificar a otras páginas
                    MessagingCenter.Send(this, "OrdenCreada");
                }

            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al crear orden: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
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