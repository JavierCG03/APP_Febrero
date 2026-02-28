using CarslineApp.Models;
using CarslineApp.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels.Ordenes
{
    public class RefaccionesTrabajoViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly int _trabajoId;

        private ObservableCollection<RefaccionTrabajoViewModel> _refacciones;
        private ObservableCollection<RefaccionCitaViewModel> _refaccionesCompradas;
        private ObservableCollection<RefaccionPredeterminadaViewModel> _refaccionesPredeterminadas;
        private bool _estaCargando;
        private decimal _totalRefacciones;
        private decimal _precioManoObra;
        private decimal _manoObraOriginal;
        private string _nuevaRefaccion = string.Empty;
        private string _nuevaCantidad = string.Empty;
        private string _nuevoPrecioUnitario = string.Empty;
        private string _precioManoObraTexto = string.Empty;
        private bool _formularioExpandido = false;
        private bool _manoObraExpandido = false;
        private bool _predeterminadasExpandido = true;

        private string _costoTotalTrabajoTexto = string.Empty;
        private decimal _manoObraCalculada = 0;
        private bool _hayManoObraCalculada = false;

        private string _nombreTrabajo = string.Empty;
        private string _vehiculoCompleto = string.Empty;
        private string _vin = string.Empty;
        private bool _infoTrabajoVisible = true;

        public RefaccionesTrabajoViewModel(int trabajoId)
        {
            _apiService = new ApiService();
            _trabajoId = trabajoId;
            _refaccionesCompradas = new ObservableCollection<RefaccionCitaViewModel>();
            _refacciones = new ObservableCollection<RefaccionTrabajoViewModel>();
            _refaccionesPredeterminadas = new ObservableCollection<RefaccionPredeterminadaViewModel>();

            AgregarRefaccionCommand = new Command(async () => await AgregarRefaccion(), () => !EstaCargando);
            EliminarRefaccionCommand = new Command<RefaccionTrabajoViewModel>(async (r) => await EliminarRefaccion(r));
            EditarManoObraCommand = new Command(async () => await GuardarManoObra());
            ToggleFormularioCommand = new Command(() => FormularioExpandido = !FormularioExpandido);
            ToggleManoObraCommand = new Command(() => ManoObraExpandido = !ManoObraExpandido);
            ToggleInfoCommand = new Command(() => InfoTrabajoVisible = !InfoTrabajoVisible);

            TogglePredeterminadasCommand = new Command(() => PredeterminadasExpandido = !PredeterminadasExpandido);
            AgregarPredeterminadaCommand = new Command<RefaccionPredeterminadaViewModel>(async (r) => await AgregarRefaccionPredeterminada(r));
            CalcularManoObraCommand = new Command(CalcularManoObraDesdeTotal);
            BackCommand = new Command(async () => await RegresarAtras());
            AplicarManoObraCalculadaCommand = new Command(async () => await AplicarManoObraCalculada(),
                () => HayManoObraCalculada);
        }

        #region Propiedades
        public ObservableCollection<RefaccionCitaViewModel> RefaccionesCompradas
        {
            get => _refaccionesCompradas;
            set { _refaccionesCompradas = value; OnPropertyChanged(); }
        }
        public ObservableCollection<RefaccionTrabajoViewModel> Refacciones
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

        public string CostoTotalTrabajoTexto
        {
            get => _costoTotalTrabajoTexto;
            set { _costoTotalTrabajoTexto = value; OnPropertyChanged(); }
        }

        public decimal ManoObraCalculada
        {
            get => _manoObraCalculada;
            set { _manoObraCalculada = value; OnPropertyChanged(); OnPropertyChanged(nameof(ManoObraCalculadaFormateada)); }
        }

        public bool HayManoObraCalculada
        {
            get => _hayManoObraCalculada;
            set { _hayManoObraCalculada = value; OnPropertyChanged(); ((Command)AplicarManoObraCalculadaCommand).ChangeCanExecute(); }
        }

        public string ManoObraCalculadaFormateada => $"${ManoObraCalculada:N2}";

        public decimal TotalRefacciones
        {
            get => _totalRefacciones;
            set
            {
                _totalRefacciones = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalRefaccionesFormateado));
                CalcularTotales();
                if (!string.IsNullOrWhiteSpace(CostoTotalTrabajoTexto))
                    CalcularManoObraDesdeTotal();
            }
        }

        public decimal PrecioManoObra
        {
            get => _precioManoObra;
            set { _precioManoObra = value; OnPropertyChanged(); OnPropertyChanged(nameof(ManoObraFormateado)); CalcularTotales(); }
        }

        public string PrecioManoObraTexto
        {
            get => _precioManoObraTexto;
            set { _precioManoObraTexto = value; OnPropertyChanged(); }
        }

        public bool FormularioExpandido
        {
            get => _formularioExpandido;
            set { _formularioExpandido = value; OnPropertyChanged(); OnPropertyChanged(nameof(IconoFormulario)); }
        }

        public bool ManoObraExpandido
        {
            get => _manoObraExpandido;
            set { _manoObraExpandido = value; OnPropertyChanged(); OnPropertyChanged(nameof(IconoManoObra)); }
        }

        public bool InfoTrabajoVisible
        {
            get => _infoTrabajoVisible;
            set { _infoTrabajoVisible = value; OnPropertyChanged(); OnPropertyChanged(nameof(IconoInfo)); }
        }

        public string IconoFormulario => FormularioExpandido ? "▲" : "▼";
        public string IconoManoObra => ManoObraExpandido ? "▲" : "▼";
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

        public decimal Subtotal => TotalRefacciones + PrecioManoObra;
        public decimal Iva => Subtotal * 0.16m;
        public decimal TotalGeneral => Subtotal + Iva;

        public string TotalRefaccionesFormateado => $"${TotalRefacciones:N2}";
        public string ManoObraFormateado => $"${PrecioManoObra:N2}";
        public string SubtotalFormateado => $"${Subtotal:N2}";
        public string IvaFormateado => $"${Iva:N2}";
        public string TotalGeneralFormateado => $"${TotalGeneral:N2}";

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

        public string NuevoPrecioUnitario
        {
            get => _nuevoPrecioUnitario;
            set { _nuevoPrecioUnitario = value; OnPropertyChanged(); }
        }

        #endregion

        #region Comandos

        public ICommand AgregarRefaccionCommand { get; }
        public ICommand EliminarRefaccionCommand { get; }
        public ICommand EditarManoObraCommand { get; }
        public ICommand ToggleFormularioCommand { get; }
        public ICommand ToggleManoObraCommand { get; }
        public ICommand ToggleInfoCommand { get; }
        public ICommand TogglePredeterminadasCommand { get; }
        public ICommand AgregarPredeterminadaCommand { get; }
        public ICommand CalcularManoObraCommand { get; }
        public ICommand AplicarManoObraCalculadaCommand { get; }
        public ICommand BackCommand { get; }

        #endregion

        #region Métodos Públicos

        public async Task InicializarAsync()
        {
            await CargarInformacionTrabajo();
            await CargarRefacciones();
            await CargarManoObra();
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

        /// <summary>
        /// Catálogo de refacciones predeterminadas.
        ///
        /// Para definir cantidad FIJA:    new() { Nombre = "Filtro de aceite", Cantidad = 1 }
        /// Para definir cantidad VARIABLE: new() { Nombre = "Aceite de motor", Cantidad = null }
        ///   → en pantalla aparecerá un campo para que el usuario ingrese la cantidad.
        /// </summary>
        private List<RefaccionPredeterminadaViewModel> ObtenerCatalogoPorTrabajo(string nombreTrabajo)
        {
            if (string.IsNullOrWhiteSpace(nombreTrabajo))
                return new List<RefaccionPredeterminadaViewModel>();

            string nombre = nombreTrabajo.ToLowerInvariant();

            // --- 1er y 3er Servicio ---
            if (nombre.Contains("1er") || nombre.Contains("3er"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Aceite de motor",         Cantidad = null }, // variable: 4 o 5 lt según auto
                    new() { Nombre = "Filtro de aceite",        Cantidad = 1 },
                    new() { Nombre = "Filtro de Aire de Motor", Cantidad = 1 },
                };

            // --- 2do Servicio y Externos ---
            if (nombre.Contains("2do") || nombre.Contains("externo"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Aceite de motor",           Cantidad = null }, // variable
                    new() { Nombre = "Filtro de aceite",          Cantidad = 1 },
                    new() { Nombre = "Filtro de Aire de Motor",   Cantidad = 1 },
                    new() { Nombre = "Filtro de Aire de Polen",   Cantidad = 1 },
                };

            // --- Balatas delanteras ---
            if (nombre.Contains("balatas delanteras"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Pastillas de freno delanteras", Cantidad = 1 },
                };

            // --- Balatas traseras ---
            if (nombre.Contains("balatas traseras"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Pastillas de freno traseras", Cantidad = 1 },
                };

            // --- Suspensión ---
            if (nombre.Contains("suspension"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Amortiguador delantero", Cantidad = 2 },
                    new() { Nombre = "Amortiguador trasero",   Cantidad = 2 },
                    new() { Nombre = "Resorte",                Cantidad = 4 },
                    new() { Nombre = "Bujes de suspensión",    Cantidad = 4 },
                };

            // --- Embrague / Clutch ---
            if (nombre.Contains("embrague") || nombre.Contains("clutch"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Disco de embrague",    Cantidad = 1 },
                    new() { Nombre = "Collarín de embrague", Cantidad = 1 },
                };

            // --- Batería ---
            if (nombre.Contains("batería") || nombre.Contains("bateria"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Batería",           Cantidad = 1 },
                    new() { Nombre = "Bornes de batería", Cantidad = 2 },
                };

            // --- Bujías ---
            if (nombre.Contains("bujía") || nombre.Contains("bujia") || nombre.Contains("encendido"))
                return new List<RefaccionPredeterminadaViewModel>
                {
                    new() { Nombre = "Bujías",              Cantidad = null }, 
                    new() { Nombre = "Cables de bujías",    Cantidad = 1 },
                    new() { Nombre = "Bobina de encendido", Cantidad = 1 },
                };

            return new List<RefaccionPredeterminadaViewModel>();
        }

        private async Task RegresarAtras()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al regresar: {ex.Message}");
            }
        }

        private async Task AgregarRefaccionPredeterminada(RefaccionPredeterminadaViewModel predeterminada)
        {
            if (predeterminada == null) return;

            // Validar cantidad (solo si es variable)
            int? cantidadEfectiva = predeterminada.CantidadEfectiva;
            if (cantidadEfectiva == null)
            {
                await MostrarAlerta("Cantidad requerida",
                    $"Ingresa la cantidad para:\n{predeterminada.Nombre}");
                return;
            }

            // Validar precio
            if (!decimal.TryParse(predeterminada.PrecioTexto, out decimal precio) || precio <= 0)
            {
                await MostrarAlerta("Precio requerido",
                    $"Ingresa el precio unitario para:\n{predeterminada.Nombre}");
                return;
            }

            EstaCargando = true;
            try
            {
                var request = new AgregarRefaccionesTrabajoRequest
                {
                    TrabajoId = _trabajoId,
                    Refacciones = new List<AgregarRefaccionDto>
                    {
                        new AgregarRefaccionDto
                        {
                            Refaccion      = predeterminada.Nombre,
                            Cantidad       = cantidadEfectiva.Value,
                            PrecioUnitario = precio
                        }
                    }
                };

                var response = await _apiService.AgregarRefaccionesTrabajo(request);
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

        #region Cálculo Automático de Mano de Obra

        private void CalcularManoObraDesdeTotal()
        {
            if (!decimal.TryParse(CostoTotalTrabajoTexto, out decimal costoTotal) || costoTotal <= 0)
            {
                HayManoObraCalculada = false; ManoObraCalculada = 0; return;
            }

            decimal manoObra = (costoTotal / 1.16m) - TotalRefacciones;

            if (manoObra < 0)
            {
                HayManoObraCalculada = false; ManoObraCalculada = 0; return;
            }

            ManoObraCalculada = manoObra;
            HayManoObraCalculada = true;
        }

        private async Task AplicarManoObraCalculada()
        {
            if (!HayManoObraCalculada) return;
            EstaCargando = true;
            try
            {
                var response = await _apiService.FijarCostoManoObraAsync(_trabajoId, ManoObraCalculada);
                if (response.Success)
                {
                    PrecioManoObra = ManoObraCalculada;
                    _manoObraOriginal = ManoObraCalculada;
                    PrecioManoObraTexto = ManoObraCalculada.ToString("F2");
                    await MostrarAlerta("✅ Éxito", "Mano de obra calculada y guardada correctamente");
                }
                else { await MostrarAlerta("Error", response.Message); }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {ex.Message}");
                await MostrarAlerta("Error", "No se pudo guardar la mano de obra");
            }
            finally { EstaCargando = false; }
        }

        #endregion

        #region Métodos Privados

        private async Task CargarInformacionTrabajo()
        {
            EstaCargando = true;
            try
            {
                var response = await _apiService.ObtenerInfoTrabajo(_trabajoId);
                if (response.Success)
                {
                    NombreTrabajo = response.Trabajo;
                    VehiculoCompleto = response.VehiculoCompleto;
                    VIN = response.VIN;
                    InfoTrabajoVisible = true;
                    CargarRefaccionesPredeterminadas(NombreTrabajo);
                }
                else { InfoTrabajoVisible = false; }
            }
            catch { InfoTrabajoVisible = false; }
            finally { EstaCargando = false; }
        }

        private async Task CargarRefacciones()
        {
            EstaCargando = true;
            try
            {
                var response = await _apiService.ObtenerRefaccionesPorTrabajo(_trabajoId);
                if (response.Success)
                {
                    Refacciones.Clear();
                    foreach (var r in response.Refacciones)
                        Refacciones.Add(new RefaccionTrabajoViewModel(r));

                    TotalRefacciones = response.TotalRefacciones;

                    var yaAgregadas = Refacciones.Select(r => r.Nombre.ToLowerInvariant()).ToHashSet();
                    foreach (var item in RefaccionesPredeterminadas
                        .Where(p => yaAgregadas.Contains(p.Nombre.ToLowerInvariant())).ToList())
                        RefaccionesPredeterminadas.Remove(item);

                    OnPropertyChanged(nameof(HayRefaccionesPredeterminadas));
                }
                else { await MostrarAlerta("Error", response.Message); }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {ex.Message}");
                await MostrarAlerta("Error", "No se pudieron cargar las refacciones");
            }
            await CargarRefaccionesCompradas();
            //finally { EstaCargando = false; }
        }

        private async Task CargarRefaccionesCompradas()
        {
            EstaCargando = true;
            try
            {
                var response = await _apiService.ObtenerRefaccionesPorTrabajoCitaAsync(_trabajoId, true);
                if (response.Success)
                {
                    RefaccionesCompradas.Clear();
                    foreach (var r in response.Refacciones)
                        RefaccionesCompradas.Add(new RefaccionCitaViewModel(r));

                    // Quitar predeterminadas ya agregadas
                    var yaAgregadas = RefaccionesCompradas
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


        private async Task CargarManoObra()
        {
            EstaCargando = true;
            try
            {
                var response = await _apiService.ObtenerCostoManoObraAsync(_trabajoId);
                if (response.Success)
                {
                    PrecioManoObra = response.CostoManoObra;
                    _manoObraOriginal = response.CostoManoObra;
                    PrecioManoObraTexto = PrecioManoObra.ToString("F2");
                }
                else { PrecioManoObra = 0; _manoObraOriginal = 0; PrecioManoObraTexto = "0.00"; }
            }
            catch { PrecioManoObra = 0; _manoObraOriginal = 0; PrecioManoObraTexto = "0.00"; }
            finally { EstaCargando = false; }
        }

        private async Task GuardarManoObra()
        {
            EstaCargando = true;
            try
            {
                if (!decimal.TryParse(PrecioManoObraTexto, out decimal nuevoPrecio) || nuevoPrecio < 0)
                {
                    await MostrarAlerta("Campo inválido", "Ingresa un precio válido"); return;
                }
                if (nuevoPrecio != _manoObraOriginal)
                {
                    var response = await _apiService.FijarCostoManoObraAsync(_trabajoId, nuevoPrecio);
                    if (response.Success)
                    {
                        PrecioManoObra = nuevoPrecio; _manoObraOriginal = nuevoPrecio;
                        PrecioManoObraTexto = nuevoPrecio.ToString("F2");
                        await MostrarAlerta("✅ Éxito", "Mano de obra actualizada correctamente");
                    }
                    else
                    {
                        await MostrarAlerta("Error", response.Message);
                        PrecioManoObraTexto = _manoObraOriginal.ToString("F2");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {ex.Message}");
                await MostrarAlerta("Error", "No se pudo actualizar la mano de obra");
                PrecioManoObraTexto = _manoObraOriginal.ToString("F2");
            }
            finally { EstaCargando = false; }
        }

        private async Task AgregarRefaccion()
        {
            if (string.IsNullOrWhiteSpace(NuevaRefaccion))
            { await MostrarAlerta("Campo requerido", "Ingresa el nombre de la refacción"); return; }
            if (string.IsNullOrWhiteSpace(NuevaCantidad) || !int.TryParse(NuevaCantidad, out int cantidad) || cantidad <= 0)
            { await MostrarAlerta("Campo inválido", "Ingresa una cantidad válida"); return; }
            if (string.IsNullOrWhiteSpace(NuevoPrecioUnitario) || !decimal.TryParse(NuevoPrecioUnitario, out decimal precio) || precio <= 0)
            { await MostrarAlerta("Campo inválido", "Ingresa un precio unitario válido"); return; }

            EstaCargando = true;
            try
            {
                var request = new AgregarRefaccionesTrabajoRequest
                {
                    TrabajoId = _trabajoId,
                    Refacciones = new List<AgregarRefaccionDto>
                    { new() { Refaccion = NuevaRefaccion.Trim(), Cantidad = cantidad, PrecioUnitario = precio } }
                };
                var response = await _apiService.AgregarRefaccionesTrabajo(request);
                if (response.Success)
                {
                    NuevaRefaccion = string.Empty; NuevaCantidad = string.Empty; NuevoPrecioUnitario = string.Empty;
                    await CargarRefacciones();
                    await MostrarAlerta("✅ Éxito", "Refacción agregada correctamente");
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

        private async Task EliminarRefaccion(RefaccionTrabajoViewModel refaccion)
        {
            if (refaccion == null) return;
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminación", $"¿Eliminar la refacción:\n{refaccion.Nombre}?",
                "Sí, eliminar", "Cancelar");
            if (!confirmar) return;

            EstaCargando = true;
            try
            {
                var response = await _apiService.EliminarRefaccionTrabajo(refaccion.Id);
                if (response.Success)
                {
                    await CargarRefacciones();
                    CargarRefaccionesPredeterminadas(NombreTrabajo);
                    await MostrarAlerta("✅ Éxito", "Refacción eliminada correctamente");
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

        private void CalcularTotales()
        {
            OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(Iva));
            OnPropertyChanged(nameof(TotalGeneral)); OnPropertyChanged(nameof(SubtotalFormateado));
            OnPropertyChanged(nameof(IvaFormateado)); OnPropertyChanged(nameof(TotalGeneralFormateado));
        }

        private async Task MostrarAlerta(string titulo, string mensaje)
        {
            try { if (Application.Current?.MainPage != null) await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "OK"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"❌ {ex.Message}"); }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}