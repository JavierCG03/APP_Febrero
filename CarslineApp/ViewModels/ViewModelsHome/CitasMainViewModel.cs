using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views.ViewHome;
using CarslineApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels.ViewModelsHome
{
    public class CitasMainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private bool _isRefreshing;
        private string _tituloSeccion = "Recordatorios";
        private string _mensajeNoRecordatorios = "No hay recordatorios pendientes";
        private int _tipoRecordatorioActual = 1;
        private Func<RecordatorioServicioSimpleDto, int, Task> _navigationAction;

        public CitasMainViewModel()
        {
            _apiService = new ApiService();
            Recordatorios = new ObservableCollection<RecordatorioServicioSimpleDto>();

            // Comandos de navegación entre secciones
            PrimerRecordatorioCommand = new Command(async () => await CargarRecordatoriosPorTipo(1));
            SegundoRecordatorioCommand = new Command(async () => await CargarRecordatoriosPorTipo(2));
            TercerRecordatorioCommand = new Command(async () => await CargarRecordatoriosPorTipo(3));

            // Comando para ver detalle
            VerDetalleRecordatorioCommand = new Command<RecordatorioServicioSimpleDto>(async (recordatorio) => await VerDetalleRecordatorio(recordatorio));

            // Otros comandos
            RefreshCommand = new Command(async () => await CargarRecordatoriosPorTipo(_tipoRecordatorioActual));
            LogoutCommand = new Command(async () => await CerrarSesion());
            VerAgendaCommand = new Command(async () => await VerAgenda(), () => !IsLoading);
     
        }

        /// <summary>
        /// Método para inyectar la acción de navegación desde el code-behind
        /// </summary>
        public void SetNavigationAction(Func<RecordatorioServicioSimpleDto, int, Task> navigationAction)
        {
            _navigationAction = navigationAction;
            Debug.WriteLine("✅ Acción de navegación configurada en el ViewModel");
        }

        #region Propiedades

        public ObservableCollection<RecordatorioServicioSimpleDto> Recordatorios { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(); }
        }

        public string TituloSeccion
        {
            get => _tituloSeccion;
            set
            {
                _tituloSeccion = value;
                OnPropertyChanged();
            }
        }

        public string MensajeNoRecordatorios
        {
            get => _mensajeNoRecordatorios;
            set
            {
                _mensajeNoRecordatorios = value;
                OnPropertyChanged();
            }
        }

        public bool TieneRecordatorios => Recordatorios?.Count > 0;

        public bool EsPrimerRecordatorio => _tipoRecordatorioActual == 1;
        public bool EsSegundoRecordatorio => _tipoRecordatorioActual == 2;
        public bool EsTercerRecordatorio => _tipoRecordatorioActual == 3;

        public string NombreUsuarioActual { get; set; } = "Usuario";

        #endregion

        #region Comandos

        public ICommand PrimerRecordatorioCommand { get; }
        public ICommand SegundoRecordatorioCommand { get; }
        public ICommand TercerRecordatorioCommand { get; }
        public ICommand VerDetalleRecordatorioCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand VerAgendaCommand { get; }
        

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Inicializar el ViewModel - Se llama UNA SOLA VEZ desde OnAppearing
        /// </summary>
        public async Task InicializarAsync()
        {
            Debug.WriteLine("🚀 Inicializando CitasMainViewModel...");

            // Cargar usuario actual
            CargarUsuarioActual();

            // Cargar primer recordatorio por defecto
            await CargarRecordatoriosPorTipo(1);
        }

        #endregion

        #region Métodos Privados

        private async Task VerAgenda ()
        {
            try
            {
                IsLoading = true;
                await Application.Current.MainPage.Navigation.PushAsync(new AgendaCitas(0,0,0));
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

        /// <summary>
        /// Cargar recordatorios por tipo (1, 2 o 3)
        /// </summary>
        private async Task CargarRecordatoriosPorTipo(int tipo)
        {

            if (IsLoading)
            {
                Debug.WriteLine("⚠️ Ya hay una carga en progreso, ignorando...");
                return;
            }

            try
            {
                IsLoading = true;
                IsRefreshing = true;
                _tipoRecordatorioActual = tipo;

                Debug.WriteLine($"📥 Cargando recordatorios tipo {tipo}...");

                // Actualizar título y propiedades de selección
                TituloSeccion = tipo switch
                {
                    1 => "Primer Recordatorio",
                    2 => "Segundo Recordatorio",
                    3 => "Tercer Recordatorio",
                    _ => "Recordatorios"
                };

                OnPropertyChanged(nameof(EsPrimerRecordatorio));
                OnPropertyChanged(nameof(EsSegundoRecordatorio));
                OnPropertyChanged(nameof(EsTercerRecordatorio));

                // Llamar al servicio
                var response = await _apiService.ObtenerRecordatoriosPorTipoAsync(tipo);

                if (response.Success)
                {
                    Recordatorios.Clear();

                    if (response.Recordatorios != null && response.Recordatorios.Count > 0)
                    {
                        foreach (var recordatorio in response.Recordatorios)
                        {
                            Recordatorios.Add(recordatorio);
                        }

                        MensajeNoRecordatorios = $"No hay recordatorios pendientes en esta categoría";
                        Debug.WriteLine($"✅ {Recordatorios.Count} recordatorios cargados");
                    }
                    else
                    {
                        MensajeNoRecordatorios = $"No hay {TituloSeccion.ToLower()}s pendientes";
                        Debug.WriteLine("ℹ️ No hay recordatorios para este tipo");
                    }
                }
                else
                {
                    await MostrarError("Error al cargar recordatorios", response.Message);
                }

                OnPropertyChanged(nameof(TieneRecordatorios));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en CargarRecordatoriosPorTipo: {ex.Message}");
                await MostrarError("Error", $"Error al cargar recordatorios: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        /// <summary>
        /// Navegar a la vista de detalle del recordatorio
        /// </summary>
        private async Task VerDetalleRecordatorio(RecordatorioServicioSimpleDto recordatorioSimple)
        {
            if (recordatorioSimple == null)
            {
                Debug.WriteLine("⚠️ recordatorioSimple es null");
                return;
            }

            try
            {
                Debug.WriteLine($"🔍 [VIEWMODEL] Intentando navegar al detalle ID: {recordatorioSimple.Id}");

                // Si tenemos la acción de navegación inyectada, usarla
                if (_navigationAction != null)
                {
                    Debug.WriteLine("✅ [VIEWMODEL] Usando acción de navegación inyectada");
                    await _navigationAction(recordatorioSimple, _tipoRecordatorioActual);
                    Debug.WriteLine("✅ [VIEWMODEL] Navegación delegada completada");
                }
                else
                {
                    Debug.WriteLine("⚠️ [VIEWMODEL] No hay acción de navegación inyectada, usando método alternativo");

                    // Fallback: Intentar navegación directa
                    var paginaDetalle = new RecordatorioDetallePage(recordatorioSimple.Id, _tipoRecordatorioActual);

                    if (Application.Current?.MainPage is FlyoutPage flyoutPage &&
                        flyoutPage.Detail is NavigationPage navigationPage)
                    {
                        Debug.WriteLine("✅ [VIEWMODEL] Navegando vía FlyoutPage.Detail.NavigationPage");
                        await navigationPage.PushAsync(paginaDetalle);
                    }
                    else
                    {
                        Debug.WriteLine("❌ [VIEWMODEL] No se pudo determinar la estructura de navegación");
                        await MostrarError("Error", "No se pudo abrir el detalle del recordatorio");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [VIEWMODEL] Error al navegar: {ex.Message}");
                Debug.WriteLine($"❌ [VIEWMODEL] StackTrace: {ex.StackTrace}");
                await MostrarError("Error", $"No se pudo abrir el detalle: {ex.Message}");
            }
        }

        /// <summary>
        /// Cargar información del usuario actual
        /// </summary>
        private void CargarUsuarioActual()
        {

            NombreUsuarioActual = Preferences.Get("user_name", "Citas");
            OnPropertyChanged(nameof(NombreUsuarioActual));
        }

        /// <summary>
        /// Cerrar sesión
        /// </summary>
        private async Task CerrarSesion()
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Cerrar Sesión",
                "¿Estás seguro que deseas cerrar sesión?",
                "Sí",
                "No");

            if (confirmar)
            {
                // Limpiar preferencias
                Preferences.Clear();

                // Navegar a login
                Application.Current.MainPage = new LoginPage(); // Ajusta según tu página de login
            }
        }

        /// <summary>
        /// Mostrar mensaje de error
        /// </summary>
        private async Task MostrarError(string titulo, string mensaje)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "OK");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}