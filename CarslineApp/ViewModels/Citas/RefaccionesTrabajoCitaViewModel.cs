using CarslineApp.Services;
using CarslineApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels
{
    public class RefaccionesTrabajoCitaViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly int _trabajoCitaId;

        private ObservableCollection<RefaccionCitaViewModel> _refacciones;
        private ObservableCollection<RefaccionPredeterminadaViewModel> _refaccionesPredeterminadas;
        private bool _estaCargando;
        private decimal _totalCosto;
        private decimal? _totalVenta;

        private string _nuevaRefaccion = string.Empty;
        private string _nuevaCantidad = string.Empty;
        private string _nuevoPrecio = string.Empty;
        private string _nuevoPrecioVenta = string.Empty;

        private bool _formularioExpandido = false;
        private bool _predeterminadasExpandido = true;
        private bool _infoTrabajoVisible = true;

        private string _nombreTrabajo = string.Empty;
        private string _vehiculoCompleto = string.Empty;
        private string _vin = string.Empty;

        public RefaccionesTrabajoCitaViewModel(int trabajoCitaId, string trabajo, string vehiculo, string vin)
        {
            _apiService = new ApiService();
            _trabajoCitaId = trabajoCitaId;
            _nombreTrabajo = trabajo;
            _vehiculoCompleto = vehiculo;
            _vin = vin;

            _refacciones = new ObservableCollection<RefaccionCitaViewModel>();
            _refaccionesPredeterminadas = new ObservableCollection<RefaccionPredeterminadaViewModel>();

            AgregarRefaccionCommand = new Command(async () => await AgregarRefaccion(), () => !EstaCargando);
            EliminarRefaccionCommand = new Command<RefaccionCitaViewModel>(async (r) => await EliminarRefaccion(r));
            EditarPrecioVentaCommand = new Command<RefaccionCitaViewModel>(async (r) => await EditarPrecioVenta(r));
            ToggleFormularioCommand = new Command(() => FormularioExpandido = !FormularioExpandido);
            ToggleInfoCommand = new Command(() => InfoTrabajoVisible = !InfoTrabajoVisible);
            TogglePredeterminadasCommand = new Command(() => PredeterminadasExpandido = !PredeterminadasExpandido);
            AgregarPredeterminadaCommand = new Command<RefaccionPredeterminadaViewModel>(
                async (r) => await AgregarRefaccionPredeterminada(r));
        }

        #region Propiedades

        public ObservableCollection<RefaccionCitaViewModel> Refacciones
        {
            get => _refacciones;
            set { _refacciones = value; OnPropertyChanged(); }
        }

        public ObservableCollection<RefaccionPredeterminadaViewModel> RefaccionesPredeterminadas
        {
            get => _refaccionesPredeterminadas;
            set { _refaccionesPredeterminadas = value; OnPropertyChanged(); }
        }

        public bool PredeterminadasExpandido
        {
            get => _predeterminadasExpandido;
            set { _predeterminadasExpandido = value; OnPropertyChanged(); OnPropertyChanged(nameof(IconoPredeterminadas)); }
        }

        public string IconoPredeterminadas => PredeterminadasExpandido ? "▲" : "▼";
        public bool HayRefaccionesPredeterminadas => RefaccionesPredeterminadas?.Count > 0;

        public decimal TotalCosto
        {
            get => _totalCosto;
            set
            {
                _totalCosto = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalCostoFormateado));
            }
        }

        public decimal? TotalVenta
        {
            get => _totalVenta;
            set
            {
                _totalVenta = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalVentaFormateado));
                OnPropertyChanged(nameof(TieneVenta));
            }
        }

        public string TotalCostoFormateado => $"${TotalCosto:N2}";
        public string TotalVentaFormateado => TotalVenta.HasValue ? $"${TotalVenta.Value:N2}" : "-";
        public bool TieneVenta => TotalVenta.HasValue;

        public bool FormularioExpandido
        {
            get => _formularioExpandido;
            set { _formularioExpandido = value; OnPropertyChanged(); OnPropertyChanged(nameof(IconoFormulario)); }
        }

        public bool InfoTrabajoVisible
        {
            get => _infoTrabajoVisible;
            set { _infoTrabajoVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IconoInfo)); }
        }

        public string IconoFormulario => FormularioExpandido ? "▲" : "▼";
        public string IconoInfo => InfoTrabajoVisible ? "▲" : "▼";

        public string NombreTrabajo
        {
            get => _nombreTrabajo;
            set { _nombreTrabajo = value; OnPropertyChanged(); }
        }

        public string VehiculoCompleto
        {
            get => _vehiculoCompleto;
            set { _vehiculoCompleto = value; OnPropertyChanged(); }
        }

        public string VIN
        {
            get => _vin;
            set { _vin = value; OnPropertyChanged(); }
        }

        public bool EstaCargando
        {
            get => _estaCargando;
            set { _estaCargando = value; OnPropertyChanged(); ((Command)AgregarRefaccionCommand).ChangeCanExecute(); }
        }

        public string NuevaRefaccion
        {
            get => _nuevaRefaccion;
            set { _nuevaRefaccion = value; OnPropertyChanged(); }
        }

        public string NuevaCantidad
        {
            get => _nuevaCantidad;
            set { _nuevaCantidad = value; OnPropertyChanged(); }
        }

        public string NuevoPrecio
        {
            get => _nuevoPrecio;
            set { _nuevoPrecio = value; OnPropertyChanged(); }
        }

        public string NuevoPrecioVenta
        {
            get => _nuevoPrecioVenta;
            set { _nuevoPrecioVenta = value; OnPropertyChanged(); }
        }

        #endregion

        #region Comandos

        public ICommand AgregarRefaccionCommand { get; }
        public ICommand EliminarRefaccionCommand { get; }
        public ICommand EditarPrecioVentaCommand { get; }
        public ICommand ToggleFormularioCommand { get; }
        public ICommand ToggleInfoCommand { get; }
        public ICommand TogglePredeterminadasCommand { get; }
        public ICommand AgregarPredeterminadaCommand { get; }

        #endregion

        #region Métodos Públicos

        public async Task InicializarAsync()
        {
            CargarRefaccionesPredeterminadas(_nombreTrabajo);
            await CargarRefacciones();
        }

        #endregion

        #region Refacciones Predeterminadas

        private void CargarRefaccionesPredeterminadas(string nombreTrabajo)
        {
            var catalogo = ObtenerCatalogoPorTrabajo(nombreTrabajo);
            RefaccionesPredeterminadas.Clear();
            foreach (var item in catalogo)
            {
                bool yaAgregada = Refacciones.Any(r =>
                    r.Nombre.Equals(item.Nombre, StringComparison.OrdinalIgnoreCase));
                if (!yaAgregada)
                    RefaccionesPredeterminadas.Add(item);
            }
            OnPropertyChanged(nameof(HayRefaccionesPredeterminadas));
        }

        private List<RefaccionPredeterminadaViewModel> ObtenerCatalogoPorTrabajo(string nombreTrabajo)
        {
            if (string.IsNullOrWhiteSpace(nombreTrabajo))
                return new List<RefaccionPredeterminadaViewModel>();

            string nombre = nombreTrabajo.ToLowerInvariant();

            if (nombre.Contains("1er") || nombre.Contains("3er"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Aceite de motor",         Cantidad = null },
                    new() { Nombre = "Filtro de aceite",        Cantidad = 1 },
                    new() { Nombre = "Filtro de Aire de Motor", Cantidad = 1 },
                };

            if (nombre.Contains("2do") || nombre.Contains("externo"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Aceite de motor",           Cantidad = null },
                    new() { Nombre = "Filtro de aceite",          Cantidad = 1 },
                    new() { Nombre = "Filtro de Aire de Motor",   Cantidad = 1 },
                    new() { Nombre = "Filtro de Aire de Polen",   Cantidad = 1 },
                };

            if (nombre.Contains("balatas delanteras"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Pastillas de freno delanteras", Cantidad = 1 },
                };

            if (nombre.Contains("balatas traseras"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Pastillas de freno traseras", Cantidad = 1 },
                };

            if (nombre.Contains("suspension"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Amortiguador delantero", Cantidad = 2 },
                    new() { Nombre = "Amortiguador trasero",   Cantidad = 2 },
                    new() { Nombre = "Resorte",                Cantidad = 4 },
                    new() { Nombre = "Bujes de suspensión",    Cantidad = 4 },
                };

            if (nombre.Contains("embrague") || nombre.Contains("clutch"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Disco de embrague",    Cantidad = 1 },
                    new() { Nombre = "Collarín de embrague", Cantidad = 1 },
                };

            if (nombre.Contains("batería") || nombre.Contains("bateria"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Batería",           Cantidad = 1 },
                    new() { Nombre = "Bornes de batería", Cantidad = 2 },
                };

            if (nombre.Contains("bujía") || nombre.Contains("bujia") || nombre.Contains("encendido"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Bujías",              Cantidad = null },
                    new() { Nombre = "Cables de bujías",    Cantidad = 1 },
                    new() { Nombre = "Bobina de encendido", Cantidad = 1 },
                };

            return new List<RefaccionPredeterminadaViewModel>();
        }

        private async Task AgregarRefaccionPredeterminada(RefaccionPredeterminadaViewModel predeterminada)
        {
            if (predeterminada == null) return;

            int? cantidadEfectiva = predeterminada.CantidadEfectiva;
            if (cantidadEfectiva == null)
            {
                await MostrarAlerta("Cantidad requerida", $"Ingresa la cantidad para:\n{predeterminada.Nombre}");
                return;
            }

            if (!decimal.TryParse(predeterminada.PrecioTexto, out decimal precio) || precio <= 0)
            {
                await MostrarAlerta("Precio requerido", $"Ingresa el precio de costo para:\n{predeterminada.Nombre}");
                return;
            }
            
            // Precio de venta es opcional para predeterminadas
            decimal? precioVenta = null;
            if (!string.IsNullOrWhiteSpace(predeterminada.PrecioVentaTexto) &&
                decimal.TryParse(predeterminada.PrecioVentaTexto, out decimal pv) && pv > 0)
                precioVenta = pv;
            
            EstaCargando = true;
            try
            {
                var request = new AgregarRefaccionesCitaRequest
                {
                    TrabajoCitaId = _trabajoCitaId,
                    Refacciones = new List<AgregarRefaccionCitaDto>
                    {
                        new AgregarRefaccionCitaDto
                        {
                            Refaccion  = predeterminada.Nombre,
                            Cantidad   = cantidadEfectiva.Value,
                            Precio     = precio,
                            PrecioVenta = precioVenta
                        }
                    }
                };

                var response = await _apiService.AgregarRefaccionesCitaAsync(request);
                if (response.Success)
                {
                    RefaccionesPredeterminadas.Remove(predeterminada);
                    OnPropertyChanged(nameof(HayRefaccionesPredeterminadas));
                    await CargarRefacciones();
                }
                else
                {
                    await MostrarAlerta("Error", response.Message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {ex.Message}");
                await MostrarAlerta("Error", "No se pudo agregar la refacción");
            }
            finally { EstaCargando = false; }
        }

        #endregion

        #region Métodos Privados

        private async Task CargarRefacciones()
        {
            EstaCargando = true;
            try
            {
                var response = await _apiService.ObtenerRefaccionesPorTrabajoCitaAsync(_trabajoCitaId);
                if (response.Success)
                {
                    Refacciones.Clear();
                    foreach (var r in response.Refacciones)
                        Refacciones.Add(new RefaccionCitaViewModel(r));

                    TotalCosto = response.TotalCosto;
                    TotalVenta = response.TotalVenta;

                    // Quitar predeterminadas ya agregadas
                    var yaAgregadas = Refacciones
                        .Select(r => r.Nombre.ToLowerInvariant())
                        .ToHashSet();
                    foreach (var item in RefaccionesPredeterminadas
                        .Where(p => yaAgregadas.Contains(p.Nombre.ToLowerInvariant()))
                        .ToList())
                        RefaccionesPredeterminadas.Remove(item);

                    OnPropertyChanged(nameof(HayRefaccionesPredeterminadas));
                }
                else
                {
                    await MostrarAlerta("Error", response.Message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {ex.Message}");
                await MostrarAlerta("Error", "No se pudieron cargar las refacciones");
            }
            finally { EstaCargando = false; }
        }

        private async Task AgregarRefaccion()
        {
            if (string.IsNullOrWhiteSpace(NuevaRefaccion))
            { await MostrarAlerta("Campo requerido", "Ingresa el nombre de la refacción"); return; }

            if (string.IsNullOrWhiteSpace(NuevaCantidad) ||
                !int.TryParse(NuevaCantidad, out int cantidad) || cantidad <= 0)
            { await MostrarAlerta("Campo inválido", "Ingresa una cantidad válida"); return; }

            if (string.IsNullOrWhiteSpace(NuevoPrecio) ||
                !decimal.TryParse(NuevoPrecio, out decimal precio) || precio <= 0)
            { await MostrarAlerta("Campo inválido", "Ingresa un precio de costo válido"); return; }

            // Precio de venta es opcional
            decimal? precioVenta = null;
            if (!string.IsNullOrWhiteSpace(NuevoPrecioVenta) &&
                decimal.TryParse(NuevoPrecioVenta, out decimal pv) && pv > 0)
                precioVenta = pv;

            EstaCargando = true;
            try
            {
                var request = new AgregarRefaccionesCitaRequest
                {
                    TrabajoCitaId = _trabajoCitaId,
                    Refacciones = new List<AgregarRefaccionCitaDto>
                    {
                        new() {
                            Refaccion   = NuevaRefaccion.Trim(),
                            Cantidad    = cantidad,
                            Precio      = precio,
                            PrecioVenta = precioVenta
                        }
                    }
                };

                var response = await _apiService.AgregarRefaccionesCitaAsync(request);
                if (response.Success)
                {
                    NuevaRefaccion = string.Empty;
                    NuevaCantidad = string.Empty;
                    NuevoPrecio = string.Empty;
                    NuevoPrecioVenta = string.Empty;
                    await CargarRefacciones();
                }
                else { await MostrarAlerta("Error", response.Message); }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {ex.Message}");
                await MostrarAlerta("Error", "No se pudo agregar la refacción");
            }
            finally { EstaCargando = false; }
        }

        private async Task EliminarRefaccion(RefaccionCitaViewModel refaccion)
        {
            if (refaccion == null) return;

            if (refaccion.Transferida)
            {
                await MostrarAlerta("No permitido",
                    "Esta refacción ya fue transferida a una orden de trabajo y no puede eliminarse.");
                return;
            }

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminación",
                $"¿Eliminar la refacción:\n{refaccion.Nombre}?",
                "Sí, eliminar", "Cancelar");
            if (!confirmar) return;

            EstaCargando = true;
            try
            {
                var response = await _apiService.EliminarRefaccionCitaAsync(refaccion.Id);
                if (response.Success)
                {
                    await CargarRefacciones();
                    CargarRefaccionesPredeterminadas(NombreTrabajo);
                }
                else { await MostrarAlerta("Error", response.Message); }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {ex.Message}");
                await MostrarAlerta("Error", "No se pudo eliminar la refacción");
            }
            finally { EstaCargando = false; }
        }

        private async Task EditarPrecioVenta(RefaccionCitaViewModel refaccion)
        {
            if (refaccion == null) return;

            if (refaccion.Transferida)
            {
                await MostrarAlerta("No permitido",
                    "Esta refacción ya fue transferida y no puede modificarse.");
                return;
            }

            string resultado = await Application.Current.MainPage.DisplayPromptAsync(
                "Precio de Venta",
                $"Ingresa el precio de venta para:\n{refaccion.Nombre}",
                initialValue: refaccion.PrecioVentaRaw?.ToString("F2") ?? string.Empty,
                keyboard: Keyboard.Numeric,
                placeholder: "$0.00");

            if (resultado == null) return; // canceló

            if (!decimal.TryParse(resultado, out decimal nuevoPrecio) || nuevoPrecio <= 0)
            {
                await MostrarAlerta("Precio inválido", "Ingresa un precio de venta mayor a 0");
                return;
            }

            EstaCargando = true;
            try
            {
                var response = await _apiService.ActualizarPrecioVentaRefaccionCitaAsync(refaccion.Id, nuevoPrecio);
                if (response.Success)
                    await CargarRefacciones();
                else
                    await MostrarAlerta("Error", response.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {ex.Message}");
                await MostrarAlerta("Error", "No se pudo actualizar el precio de venta");
            }
            finally { EstaCargando = false; }
        }

        private async Task MostrarAlerta(string titulo, string mensaje)
        {
            try
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "OK");
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"❌ {ex.Message}"); }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }

    // ─────────────────────────────────────────────────────
    // ViewModel de fila para la lista de refacciones de cita
    // ─────────────────────────────────────────────────────
    public class RefaccionCitaViewModel : INotifyPropertyChanged
    {
        private readonly RefaccionCitaDto _dto;

        public RefaccionCitaViewModel(RefaccionCitaDto dto)
        {
            _dto = dto;
        }

        public int Id => _dto.Id;
        public string Nombre => _dto.Refaccion;
        public string CantidadTexto => _dto.CantidadTexto;
        public string PrecioFormateado => _dto.PrecioFormateado;
        public string PrecioVentaFormateado => _dto.PrecioVentaFormateado;
        public string TotalCostoFormateado => _dto.TotalCostoFormateado;
        public string TotalVentaFormateado => _dto.TotalVentaFormateado;
        public bool Transferida => _dto.Transferida;
        public bool PuedeEditar => !_dto.Transferida;
        public decimal? PrecioVentaRaw => _dto.PrecioVenta;
        public bool TienePrecioVenta => _dto.PrecioVenta.HasValue;

        // Color visual según si ya fue transferida
        public string ColorEstado => _dto.Transferida ? "#43A047" : "#FB8C00";
        public string TextoEstado => _dto.Transferida ? "✔ Transferida" : "⏳ Pendiente";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}