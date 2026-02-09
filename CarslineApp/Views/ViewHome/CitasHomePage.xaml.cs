using CarslineApp.ViewModels.ViewModelsHome;
using CarslineApp.Models;

namespace CarslineApp.Views
{
    public partial class CitasHomePage : FlyoutPage
    {
        private readonly CitasMainViewModel _viewModel;
        private bool _isInitialized = false;

        public CitasHomePage()
        {
            InitializeComponent();
            _viewModel = new CitasMainViewModel();

            // Pasar la función de navegación al ViewModel
            _viewModel.SetNavigationAction(NavegarADetalle);

            BindingContext = _viewModel;

            // Configurar el comportamiento del flyout según la plataforma
            ConfigurarFlyout();
        }

        private void ConfigurarFlyout()
        {
#if WINDOWS
            // En Windows, el menú inicia cerrado
            IsPresented = false;
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
#elif ANDROID
            // En Android, el menú se puede deslizar
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
#else
            // Otros dispositivos
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
#endif
        }

        // Manejador del evento del botón hamburguesa
        private void OnMenuButtonClicked(object sender, EventArgs e)
        {
            // Alternar la visibilidad del menú
            IsPresented = !IsPresented;
        }

        // Inicializar datos SOLO UNA VEZ
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Solo inicializar la primera vez que aparece la página
            if (!_isInitialized)
            {
                _isInitialized = true;

                // Cargar datos iniciales
                await _viewModel.InicializarAsync();

                // Cerrar el menú cuando se ejecute algún comando de navegación
                _viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(_viewModel.TituloSeccion))
                    {
                        IsPresented = false;
                    }
                };
            }
        }

        // Limpiar cuando se sale de la página
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Aquí podrías limpiar recursos si es necesario
        }

        // Método helper para navegación desde el ViewModel
        private async Task NavegarADetalle(RecordatorioServicioSimpleDto recordatorio, int tipoRecordatorio)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? [CODE-BEHIND] Navegando al detalle ID: {recordatorio.Id}");

                var paginaDetalle = new RecordatorioDetallePage(recordatorio.Id, tipoRecordatorio);

                // Acceder directamente al NavigationPage del Detail
                if (this.Detail is NavigationPage navPage)
                {
                    System.Diagnostics.Debug.WriteLine("? [CODE-BEHIND] NavigationPage encontrado, navegando...");
                    await navPage.PushAsync(paginaDetalle);
                    System.Diagnostics.Debug.WriteLine("? [CODE-BEHIND] Navegación completada");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("? [CODE-BEHIND] Detail no es NavigationPage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? [CODE-BEHIND] Error en navegación: {ex.Message}");
                throw;
            }
        }
    }
}