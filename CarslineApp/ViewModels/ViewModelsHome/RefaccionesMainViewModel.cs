using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views;
using CarslineApp.Views.ViewHome;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels.ViewModelsHome
{
    public class RefaccionesMainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private int _tipoCitaSeleccionado = 1;
        private bool _isLoading;
        private string _nombreUsuarioActual = string.Empty;
        private DateTime _fechaSeleccionada = DateTime.Today.AddDays(1);
        // ✅ LISTA ÚNICA AGRUPADA
        private ObservableCollection<GrupoCitas> _todasLasCitasAgrupadas = new();

        public RefaccionesMainViewModel()
        {
            _apiService = new ApiService();

            // Comandos de navegación
            VerServicioCommand = new Command(() => CambiarTipoCita(1));
            VerDiagnosticoCommand = new Command(() => CambiarTipoCita(2));
            VerReparacionCommand = new Command(() => CambiarTipoCita(3));
            VerGarantiaCommand = new Command(() => CambiarTipoCita(4));

            // Comandos de acciones
            RefreshCommand = new Command(async () => await CargarCitas());
            LogoutCommand = new Command(async () => await OnLogout());

            NombreUsuarioActual = Preferences.Get("user_name", "Encargado Refacciones");
        }

        public async Task InicializarAsync()
        {
            await CargarCitas();
        }

        #region Propiedades

        public string NombreUsuarioActual
        {
            get => _nombreUsuarioActual;
            set { _nombreUsuarioActual = value; OnPropertyChanged(); }
        }

        public int TipoCitaSeleccionado
        {
            get => _tipoCitaSeleccionado;
            set
            {
                _tipoCitaSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TituloSeccion));
                OnPropertyChanged(nameof(EsServicio));
                OnPropertyChanged(nameof(EsDiagnostico));
                OnPropertyChanged(nameof(EsReparacion));
                OnPropertyChanged(nameof(EsGarantia));
            }
        }
        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                if (_fechaSeleccionada != value)
                {
                    _fechaSeleccionada = value;
                    OnPropertyChanged();
                     _ = CargarCitas();
                }
            }
        }

        public string TituloSeccion => TipoCitaSeleccionado switch
        {
            1 => "SERVICIOS",
            2 => "DIAGNÓSTICOS",
            3 => "REPARACIÓNES",
            4 => "GARANTÍAS",
            _ => "ÓRDENES"
        };

        public bool EsServicio => TipoCitaSeleccionado == 1;
        public bool EsDiagnostico => TipoCitaSeleccionado == 2;
        public bool EsReparacion => TipoCitaSeleccionado == 3;
        public bool EsGarantia => TipoCitaSeleccionado == 4;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ✅ LISTA ÚNICA OBSERVABLE
        public ObservableCollection<GrupoCitas> TodasLasCitasAgrupadas
        {
            get => _todasLasCitasAgrupadas;
            set
            {
                _todasLasCitasAgrupadas = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Comandos

        public ICommand VerServicioCommand { get; }
        public ICommand VerDiagnosticoCommand { get; }
        public ICommand VerReparacionCommand { get; }
        public ICommand VerGarantiaCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }

        #endregion

        #region Métodos

        private async void CambiarTipoCita(int tipoCita)
        {
            TipoCitaSeleccionado = tipoCita;
            await CargarCitas();
        }

        private async Task CargarCitas()
        {
            IsLoading = true;

            try
            {
                var citasList = await _apiService.ObtenerTrabajosCitasPorFechaAsync(TipoCitaSeleccionado, FechaSeleccionada);
                System.Diagnostics.Debug.WriteLine($"📦 Órdenes recibidas: {citasList?.Count ?? 0}");
                if (citasList == null || !citasList.Any())
                {
                    TodasLasCitasAgrupadas = new ObservableCollection<GrupoCitas>();
                    return;
                }
                // Separar oredenes cuyas refaccioens de trabajos ya estan listas
                var citasListas = new List<CitaConTrabajosDto>();
                var citasPendientes = new List<CitaConTrabajosDto>();


                foreach (var cita in citasList)
                {
                    if (cita != null)
                    {
                        if (cita.Trabajos.All(t => t.RefaccionesListas))
                        {
                            citasListas.Add(cita);
                            cita.RefaccionesListas = true;
                        }
                        else
                        {
                            citasPendientes.Add(cita);
                            cita.RefaccionesListas = false;
                        }
                        EnriquecerCita(cita);
                    }
                }

                var grupos = new ObservableCollection<GrupoCitas>();

                if (citasPendientes.Any())
                {
                    grupos.Add(new GrupoCitas(
                        "📋 Refacciones Pendientes",
                        "#FFE5E5",// (string titulo, string backgroundColor, string borderColor, string textColor
                        "#B00000",//
                        "Black",//
                        citasPendientes
                    ));
                }


                if (citasListas.Any())
                {
                    grupos.Add(new GrupoCitas(
                        "✅ Refacciones compradas",
                        "#F1F8F4",
                        "#4CAF50",
                        "#404040",
                        citasListas
                    ));
                }
                TodasLasCitasAgrupadas = grupos;                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al cargar citas: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// Enriquece la cita con propiedades calculadas para la UI
        /// </summary>
        private void EnriquecerCita(CitaConTrabajosDto cita)
        {
            if (cita.RefaccionesListas)
            {
                cita.BackgroundCita = "#F1F8F4";
                cita.BorderCita = "#4CAF50";
                cita.TextCita = "#1A1A1A";
                cita.BadgeColor = "#4CAF50";
            }
            else
            {
                cita.BackgroundCita = "#FFE5E5";
                cita.BorderCita = "#B00000";
                cita.TextCita = "#1A1A1A";
                cita.BadgeColor = "#B00000";
            }

            if (cita.Trabajos != null)
            {
                foreach (var trabajo in cita.Trabajos)
                {
                    //EnriquecerTrabajo(trabajo);
                }
            }
        }

        private async Task OnLogout()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Cerrar Sesión",
                "¿Estás seguro que deseas cerrar sesión?",
                "Sí",
                "No");

            if (confirm)
            {
                Preferences.Clear();
                Application.Current.MainPage = new NavigationPage(new LoginPage())
                {
                    BarBackgroundColor = Color.FromArgb("#B00000"),
                    BarTextColor = Colors.White
                };
            }
        }

        #endregion
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    // ✅ Clase para Agrupar Citas 
    public class GrupoCitas : ObservableCollection<CitaConTrabajosDto>
    {

        public string Titulo { get; set; }
        public string BackgroundColor { get; set; }
        public string BorderColor { get; set; }
        public string TextColor { get; set; }

        public GrupoCitas(string titulo, string backgroundColor, string borderColor, string textColor, List<CitaConTrabajosDto> citas)
            : base(citas)
        {
            Titulo = titulo;
            BackgroundColor = backgroundColor;
            BorderColor = borderColor;
            TextColor = textColor;
        }
    }
}