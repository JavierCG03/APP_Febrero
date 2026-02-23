using System.Collections.ObjectModel;
using CarslineApp.Services;
using CarslineApp.Models;

namespace CarslineApp.ViewModels.Creacion_Citas
{
    /// <summary>
    /// Partial class para gestión de datos del vehículo
    /// </summary>
    public partial class CrearCitaViewModel
    {
        #region Campos Privados Vehículo

        private readonly VinDecoderService _vinDecoder = new();

        private ObservableCollection<VehiculoDto> _vehiculosEncontrados = new();
        private bool _mostrarListaVehiculos;
        private int _vehiculoId;
        private string _ultimos4VIN = string.Empty;
        private string _vin = string.Empty;
        private string _marca = string.Empty;
        private string _modelo = string.Empty;
        private string _version = string.Empty;
        private int _anio = DateTime.Now.Year;
        private string _color = string.Empty;
        private string _placas = string.Empty;
        private int _kilometrajeInicial;
        private bool _modoEdicionVehiculo;
        private bool _vinDecodificando;
        private bool _vinDecodificadoExito;
        private string _vinMensajeDecodificacion = string.Empty;
        private string _busquedaMarca = string.Empty;
        private ObservableCollection<string> _marcasFiltradas = new();
        private bool _mostrarSugerenciasMarca;
        private string _busquedaModelo = string.Empty;
        private ObservableCollection<string> _modelosFiltrados = new();
        private bool _mostrarSugerenciasModelo;
        private string _busquedaVersion = string.Empty;
        private ObservableCollection<string> _versionesFiltradas = new();
        private bool _mostrarSugerenciasVersion;
        private string _busquedaAnio = string.Empty;
        private ObservableCollection<string> _aniosFiltrados = new();
        private bool _mostrarSugerenciasAnio;
        private bool _mostrarCamposVehiculo;


        #endregion

        #region Listas del catálogo (desde VehicleCatalogService)

        private static readonly List<string> _todasLasMarcas =
            VehiculosCatalogo.ObtenerMarcas();

        private static readonly List<string> _todosLosAnios =
            Enumerable.Range(2015, DateTime.Now.Year - 2015 + 1)
                      .Reverse().Select(y => y.ToString()).ToList();

        #endregion

        #region Propiedades Vehículo

        public ObservableCollection<VehiculoDto> VehiculosEncontrados
        {
            get => _vehiculosEncontrados;
            set { _vehiculosEncontrados = value; OnPropertyChanged(); }
        }
        public bool MostrarListaVehiculos
        {
            get => _mostrarListaVehiculos;
            set { _mostrarListaVehiculos = value; OnPropertyChanged(); }
        }
        public string Ultimos4VIN
        {
            get => _ultimos4VIN;
            set { _ultimos4VIN = value.ToUpper(); OnPropertyChanged(); ErrorMessage = string.Empty; }
        }
        public int VehiculoId
        {
            get => _vehiculoId;
            set
            {
                _vehiculoId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarBotonEditarVehiculo));
                OnPropertyChanged(nameof(CampoPlacasBloqueado));
                OnPropertyChanged(nameof(CamposVehiculoBloqueados));
                OnPropertyChanged(nameof(MostrarBotonLimpiarVehiculo));
                OnPropertyChanged(nameof(TextoToggleCamposVehiculo));
            }
        }

        public string VIN
        {
            get => _vin;
            set
            {
                var nuevo = value?.ToUpper() ?? string.Empty;
                _vin = nuevo;
                OnPropertyChanged();
                ErrorMessage = string.Empty;
            }
        }

        public string Marca
        {
            get => _marca;
            set { _marca = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }
        public string Modelo
        {
            get => _modelo;
            set { _modelo = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }
        public string Version
        {
            get => _version;
            set { _version = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }
        public int Anio
        {
            get => _anio;
            set { _anio = value; OnPropertyChanged(); }
        }
        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }
        public string Placas
        {
            get => _placas;
            set { _placas = value.ToUpper(); OnPropertyChanged(); }
        }
        public int KilometrajeInicial
        {
            get => _kilometrajeInicial;
            set { _kilometrajeInicial = value; OnPropertyChanged(); }
        }
        public bool ModoEdicionVehiculo
        {
            get => _modoEdicionVehiculo;
            set
            {
                _modoEdicionVehiculo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImagenBotonVehiculo));
                OnPropertyChanged(nameof(CampoPlacasBloqueado));
            }
        }
        public string ImagenBotonVehiculo => ModoEdicionVehiculo ? "guardar.png" : "editar.png";
        public bool CampoPlacasBloqueado => VehiculoId > 0 && !ModoEdicionVehiculo;
        public bool CamposVehiculoBloqueados => VehiculoId > 0;
        public bool MostrarCamposVehiculo
        {
            get => _mostrarCamposVehiculo;
            set
            {
                _mostrarCamposVehiculo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IconoToggleCamposVehiculo));
                OnPropertyChanged(nameof(TextoToggleCamposVehiculo));
            }
        }
        public string IconoToggleCamposVehiculo => MostrarCamposVehiculo ? "▲" : "▼";

        public string TextoToggleCamposVehiculo =>
            VehiculoId > 0
                ? (MostrarCamposVehiculo ? "Ocultar datos del vehículo" : "Ver datos del vehículo seleccionado")
                : (MostrarCamposVehiculo ? "Ocultar formulario" : "Registrar nuevo vehículo");

        public bool MostrarBotonLimpiarVehiculo => VehiculoId > 0;


        #endregion

        #region Panel de estado VIN

        public bool VinDecodificando
        {
            get => _vinDecodificando;
            set { _vinDecodificando = value; OnPropertyChanged(); OnPropertyChanged(nameof(MostrarEstadoVin)); }
        }
        public bool VinDecodificadoExito
        {
            get => _vinDecodificadoExito;
            set
            {
                _vinDecodificadoExito = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarEstadoVin));
                OnPropertyChanged(nameof(ColorEstadoVin));
                OnPropertyChanged(nameof(ColorBordeEstadoVin));
                OnPropertyChanged(nameof(ColorTextoEstadoVin));
            }
        }
        public string VinMensajeDecodificacion
        {
            get => _vinMensajeDecodificacion;
            set { _vinMensajeDecodificacion = value; OnPropertyChanged(); OnPropertyChanged(nameof(MostrarEstadoVin)); }
        }

        public bool MostrarEstadoVin => VinDecodificando || !string.IsNullOrEmpty(VinMensajeDecodificacion);
        public string ColorEstadoVin => VinDecodificadoExito ? "#E8F5E9" : "#FFF8E1";
        public string ColorBordeEstadoVin => VinDecodificadoExito ? "#4CAF50" : "#FF9800";
        public string ColorTextoEstadoVin => VinDecodificadoExito ? "#2E7D32" : "#E65100";

        #endregion

        #region Decodificación VIN

        private async Task DecodificarVinAsync(string vin)
        {
            VinDecodificando = true;
            VinDecodificadoExito = false;
            VinMensajeDecodificacion = "🔍 Consultando datos del vehículo...";
            if (vin.Length == 17)
            {
                try
                {
                    var resultado = await _vinDecoder.DecodificarVinAsync(vin);

                    if (resultado == null || !resultado.MarcaReconocida)
                    {
                        VinMensajeDecodificacion = "⚠️ VIN no reconocido. Ingresa los datos manualmente.";
                        return;
                    }

                    // ── Marca ──────────────────────────────────────────────────
                    var marcaNorm = NormalizarConCatalogo(_todasLasMarcas, resultado.Marca);
                    Marca = marcaNorm;
                    BusquedaMarca = marcaNorm;
                    MostrarSugerenciasMarca = false;

                    // ── Año ────────────────────────────────────────────────────
                    if (resultado.AnioReconocido)
                    {
                        Anio = resultado.Anio;
                        BusquedaAnio = resultado.Anio.ToString();
                    }
                    MostrarSugerenciasAnio = false;

                    // ── Modelo ─────────────────────────────────────────────────
                    var modelosDisponibles = VehiculosCatalogo.ObtenerModelos(Marca);
                    var modeloNorm = NormalizarConCatalogo(modelosDisponibles, resultado.Modelo);

                    if (!string.IsNullOrEmpty(modeloNorm))
                    {
                        Modelo = modeloNorm; BusquedaModelo = modeloNorm;
                        MostrarSugerenciasModelo = false;

                        // Modelo conocido → abrir lista de versiones automáticamente
                        Version = string.Empty; BusquedaVersion = string.Empty;
                        FiltrarVersiones();
                        MostrarSugerenciasVersion = VersionesFiltradas.Count > 0;
                    }
                    else
                    {
                        // Sin modelo → abrir lista de modelos
                        Modelo = string.Empty; BusquedaModelo = string.Empty;
                        FiltrarModelos();
                        MostrarSugerenciasModelo = ModelosFiltrados.Count > 0;
                        Version = string.Empty; BusquedaVersion = string.Empty;
                        MostrarSugerenciasVersion = false;
                    }

                    // ── Mensaje de estado ──────────────────────────────────────
                    var extras = new List<string>();
                    if (!string.IsNullOrEmpty(resultado.NumCilindros))
                        extras.Add($"{resultado.NumCilindros} cil.");
                    if (!string.IsNullOrEmpty(resultado.Displacement) &&
                        float.TryParse(resultado.Displacement, out float lt) && lt > 0)
                        extras.Add($"{lt:F1}L");
                    if (!string.IsNullOrEmpty(resultado.TipoCombust))
                        extras.Add(resultado.TipoCombust);

                    var extraTexto = extras.Any() ? $"  ·  {string.Join(", ", extras)}" : string.Empty;
                    var pendiente = string.IsNullOrEmpty(modeloNorm)
                                        ? "  —  Selecciona el modelo 👇"
                                        : "  —  Selecciona la versión 👇";

                    VinDecodificadoExito = true;
                    VinMensajeDecodificacion = $"✅ {resultado.Anio} {Marca}{extraTexto}{pendiente}";
                }
                catch (Exception ex)
                {
                    VinMensajeDecodificacion = "⚠️ Error al decodificar. Ingresa los datos manualmente.";
                    System.Diagnostics.Debug.WriteLine($"[VIN] {ex.Message}");
                }
                finally { VinDecodificando = false; }
            }
            else
            {
                var faltantes = 17 - vin.Length;
                VinMensajeDecodificacion = $"⚠️ Error al decodificar, faltan {faltantes} Caracteres";
                VinDecodificadoExito = true;
            }

        }

        /// <summary>
        /// Busca el texto en una lista del catálogo (exacto → parcial → Title Case).
        /// </summary>
        private static string NormalizarConCatalogo(List<string> lista, string? texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            var exacto = lista.FirstOrDefault(x => x.Equals(texto, StringComparison.OrdinalIgnoreCase));
            if (exacto != null) return exacto;
            var parcial = lista.FirstOrDefault(x =>
                texto.Contains(x, StringComparison.OrdinalIgnoreCase) ||
                x.Contains(texto, StringComparison.OrdinalIgnoreCase));
            if (parcial != null) return parcial;
            return System.Globalization.CultureInfo.InvariantCulture
                         .TextInfo.ToTitleCase(texto.ToLower());
        }

        #endregion

        #region Autocompletado — Marca

        public string BusquedaMarca
        {
            get => _busquedaMarca;
            set
            {
                _busquedaMarca = value;
                OnPropertyChanged();
                Marca = value?.Trim() ?? string.Empty;
                if (!CamposVehiculoBloqueados) FiltrarMarcas();
            }
        }

        public ObservableCollection<string> MarcasFiltradas
        {
            get => _marcasFiltradas;
            set { _marcasFiltradas = value; OnPropertyChanged(); }
        }
        public bool MostrarSugerenciasMarca
        {
            get => _mostrarSugerenciasMarca;
            set { _mostrarSugerenciasMarca = value; OnPropertyChanged(); }
        }

        private void FiltrarMarcas()
        {
            var txt = BusquedaMarca?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(txt))
            {
                MarcasFiltradas = new ObservableCollection<string>();
                MostrarSugerenciasMarca = false;
                return;
            }

            var lista = _todasLasMarcas
                .Where(m => m.StartsWith(txt, StringComparison.OrdinalIgnoreCase))
                .Concat(_todasLasMarcas.Where(m =>
                    !m.StartsWith(txt, StringComparison.OrdinalIgnoreCase) &&
                     m.Contains(txt, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            MarcasFiltradas = new ObservableCollection<string>(lista);
            MostrarSugerenciasMarca = MarcasFiltradas.Count > 0;
        }


        /// <summary>Marca seleccionada → limpia modelo y versión → abre lista de modelos.</summary>
        public void SeleccionarMarca(string marca)
        {
            Marca = marca; BusquedaMarca = marca;
            MostrarSugerenciasMarca = false;

            Modelo = string.Empty; BusquedaModelo = string.Empty;
            FiltrarModelos();
            MostrarSugerenciasModelo = ModelosFiltrados.Count > 0;

            Version = string.Empty; BusquedaVersion = string.Empty;
            MostrarSugerenciasVersion = false;
        }

        #endregion

        #region Autocompletado — Modelo

        public string BusquedaModelo
        {
            get => _busquedaModelo;
            set
            {
                _busquedaModelo = value;
                OnPropertyChanged();
                Modelo = value?.Trim() ?? string.Empty;
                if (!CamposVehiculoBloqueados) FiltrarModelos();
            }
        }
        public ObservableCollection<string> ModelosFiltrados
        {
            get => _modelosFiltrados;
            set { _modelosFiltrados = value; OnPropertyChanged(); }
        }
        public bool MostrarSugerenciasModelo
        {
            get => _mostrarSugerenciasModelo;
            set { _mostrarSugerenciasModelo = value; OnPropertyChanged(); }
        }

        private void FiltrarModelos()
        {
            var modelos = VehiculosCatalogo.ObtenerModelos(Marca);
            if (!modelos.Any())
            {
                ModelosFiltrados = new ObservableCollection<string>();
                MostrarSugerenciasModelo = false;
                return;
            }
            var txt = BusquedaModelo.Trim();
            var lista = string.IsNullOrWhiteSpace(txt)
                ? modelos
                : modelos
                    .Where(m => m.StartsWith(txt, StringComparison.OrdinalIgnoreCase))
                    .Concat(modelos.Where(m =>
                        !m.StartsWith(txt, StringComparison.OrdinalIgnoreCase) &&
                         m.Contains(txt, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            ModelosFiltrados = new ObservableCollection<string>(lista);
            MostrarSugerenciasModelo = ModelosFiltrados.Count > 0;
        }

        /// <summary>Modelo seleccionado → limpia versión → abre lista de versiones.</summary>
        public void SeleccionarModelo(string modelo)
        {
            Modelo = modelo; BusquedaModelo = modelo;
            MostrarSugerenciasModelo = false;

            Version = string.Empty; BusquedaVersion = string.Empty;
            FiltrarVersiones();
            MostrarSugerenciasVersion = VersionesFiltradas.Count > 0;
        }

        #endregion

        #region Autocompletado — Versión

        public string BusquedaVersion
        {
            get => _busquedaVersion;
            set
            {
                _busquedaVersion = value;
                OnPropertyChanged();
                Version = value?.Trim() ?? string.Empty; // ← AGREGAR ESTA LÍNEA
                if (!CamposVehiculoBloqueados) FiltrarVersiones();
            }
        }
        public ObservableCollection<string> VersionesFiltradas
        {
            get => _versionesFiltradas;
            set { _versionesFiltradas = value; OnPropertyChanged(); }
        }
        public bool MostrarSugerenciasVersion
        {
            get => _mostrarSugerenciasVersion;
            set { _mostrarSugerenciasVersion = value; OnPropertyChanged(); }
        }

        private void FiltrarVersiones()
        {
            var versiones = VehiculosCatalogo.ObtenerVersiones(Marca, Modelo);
            if (!versiones.Any())
            {
                VersionesFiltradas = new ObservableCollection<string>();
                MostrarSugerenciasVersion = false;
                return;
            }
            var txt = BusquedaVersion.Trim();
            var lista = string.IsNullOrWhiteSpace(txt)
                ? versiones
                : versiones
                    .Where(v => v.StartsWith(txt, StringComparison.OrdinalIgnoreCase))
                    .Concat(versiones.Where(v =>
                        !v.StartsWith(txt, StringComparison.OrdinalIgnoreCase) &&
                         v.Contains(txt, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            VersionesFiltradas = new ObservableCollection<string>(lista);
            MostrarSugerenciasVersion = VersionesFiltradas.Count > 0;
        }

        public void SeleccionarVersion(string version)
        {
            Version = version; BusquedaVersion = version;
            MostrarSugerenciasVersion = false;
            BusquedaAnio = string.Empty;
            FiltrarAnios();
            MostrarSugerenciasAnio = AniosFiltrados.Count > 0;
        }

        #endregion

        #region Autocompletado — Año

        public string BusquedaAnio
        {
            get => _busquedaAnio;
            set { _busquedaAnio = value; OnPropertyChanged(); if (!CamposVehiculoBloqueados) FiltrarAnios(); }
        }
        public ObservableCollection<string> AniosFiltrados
        {
            get => _aniosFiltrados;
            set { _aniosFiltrados = value; OnPropertyChanged(); }
        }
        public bool MostrarSugerenciasAnio
        {
            get => _mostrarSugerenciasAnio;
            set { _mostrarSugerenciasAnio = value; OnPropertyChanged(); }
        }

        private void FiltrarAnios()
        {
            if (string.IsNullOrWhiteSpace(Version))
            {
                AniosFiltrados = new ObservableCollection<string>();
                MostrarSugerenciasAnio = false;
                return;
            }

            var txt = BusquedaAnio?.Trim() ?? string.Empty;

            AniosFiltrados = string.IsNullOrWhiteSpace(txt)
                ? new ObservableCollection<string>(_todosLosAnios)
                : new ObservableCollection<string>(
                    _todosLosAnios.Where(a => a.StartsWith(txt)));

            MostrarSugerenciasAnio = AniosFiltrados.Count > 0;
        }


        public void SeleccionarAnio(string anio)
        {
            if (int.TryParse(anio, out int anioInt)) Anio = anioInt;
            BusquedaAnio = anio; MostrarSugerenciasAnio = false;
        }

        #endregion

        #region Búsqueda y Selección de Vehículo

        private async Task BuscarVehiculoCliente(int clienteId)
        {
            try
            {
                var response = await _apiService.BuscarVehiculosPorClienteIdAsync(clienteId);
                if (response.Success && response.Vehiculos?.Any() == true)
                {
                    VehiculosEncontrados.Clear();
                    foreach (var v in response.Vehiculos) VehiculosEncontrados.Add(v);
                    MostrarListaVehiculos = true;
                    // Si hay vehículos en lista, los campos se ocultan hasta seleccionar uno
                    MostrarCamposVehiculo = false;
                    ErrorMessage = $"Se encontraron {VehiculosEncontrados.Count} vehículos. Selecciona uno:";
                }
                else
                {
                    MostrarListaVehiculos = false;
                    MostrarCamposVehiculo = true;
                    ErrorMessage = response.Message ?? "No hay vehículos. Puedes registrar uno nuevo.";
                    MostrarListaVehiculos = false;
                }
            }
            catch (Exception ex) { ErrorMessage = $"Error: {ex.Message}"; MostrarListaVehiculos = false; }
            finally { IsLoading = false; }
        }

        private async Task BuscarVehiculo()
        {
            if (ModoEdicionVehiculo) { await GuardarCambiosVehiculo(); return; }
            if (string.IsNullOrWhiteSpace(Ultimos4VIN) || Ultimos4VIN.Length != 4)
            { ErrorMessage = "Ingresa exactamente 4 caracteres del VIN"; return; }

            IsLoading = true; ErrorMessage = string.Empty; MostrarListaVehiculos = false;
            try
            {
                var response = await _apiService.BuscarVehiculosPorUltimos4VINAsync(Ultimos4VIN);
                if (response.Success && response.Vehiculos?.Any() == true)
                {
                    VehiculosEncontrados.Clear();
                    foreach (var v in response.Vehiculos) VehiculosEncontrados.Add(v);
                    if (VehiculosEncontrados.Count == 1) await SeleccionarVehiculo(VehiculosEncontrados[0]);
                    else { MostrarListaVehiculos = true; ErrorMessage = $"Se encontraron {VehiculosEncontrados.Count} vehículos. Selecciona uno:"; }
                }
                else { ErrorMessage = response.Message ?? "Vehículo no encontrado. Puedes registrar uno nuevo."; }
            }
            catch (Exception ex) { ErrorMessage = $"Error: {ex.Message}"; }
            finally { IsLoading = false; }
        }

        private async Task SeleccionarVehiculo(VehiculoDto vehiculoSeleccionado)
        {
            if (vehiculoSeleccionado == null) return;
            IsLoading = true;
            try
            {
                var response = await _apiService.ObtenerVehiculoPorIdAsync(vehiculoSeleccionado.Id);
                if (response.Success && response.Vehiculo != null)
                {
                    VehiculoId = response.Vehiculo.Id;
                    VIN = response.Vehiculo.VIN;
                    Marca = response.Vehiculo.Marca;
                    Modelo = response.Vehiculo.Modelo;
                    Version = response.Vehiculo.Version;
                    Anio = response.Vehiculo.Anio;
                    Color = response.Vehiculo.Color;
                    Placas = response.Vehiculo.Placas;
                    KilometrajeInicial = response.Vehiculo.KilometrajeInicial;

                    BusquedaMarca = Marca;
                    BusquedaModelo = Modelo;
                    BusquedaVersion = Version;
                    BusquedaAnio = Anio.ToString();

                    MostrarListaVehiculos = false;
                    MostrarSugerenciasMarca = false;
                    MostrarSugerenciasModelo = false;
                    MostrarSugerenciasVersion = false;
                    MostrarSugerenciasAnio = false;
                    VinDecodificadoExito = false;
                    VinMensajeDecodificacion = string.Empty;
                    // Expandir automáticamente los campos al seleccionar un vehículo
                    MostrarCamposVehiculo = true;
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Vehículo Seleccionado",
                        $"Se ha cargado: {response.Vehiculo.VehiculoCompleto}\nCliente: {response.Vehiculo.NombreCliente}", "OK");
                }
                else { ErrorMessage = response.Message; }
            }
            catch (Exception ex) { ErrorMessage = $"Error: {ex.Message}"; }
            finally { IsLoading = false; }
        }

        private async Task EditarGuardarVehiculo()
        {
            if (!ModoEdicionVehiculo) { ModoEdicionVehiculo = true; return; }
            MostrarCamposVehiculo = true;
            await GuardarCambiosVehiculo();
        }

        private async Task GuardarCambiosVehiculo()
        {
            if (string.IsNullOrWhiteSpace(Placas))
            {
                ErrorMessage = "Las placas son requeridas";
                await Application.Current.MainPage.DisplayAlert("⚠️ Advertencia", "Debes ingresar las placas del vehículo", "OK");
                return;
            }
            IsLoading = true;
            try
            {
                var response = await _apiService.ActualizarPlacasVehiculoAsync(VehiculoId, Placas);
                if (response.Success)
                {
                    ModoEdicionVehiculo = false;
                    await Application.Current.MainPage.DisplayAlert("✅ Éxito", "Las placas han sido actualizadas correctamente", "OK");
                }
                else
                {
                    ErrorMessage = response.Message;
                    await Application.Current.MainPage.DisplayAlert("❌ Error", response.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("❌ Error", $"Error al actualizar placas: {ex.Message}", "OK");
            }
            finally { IsLoading = false; }
        }
        private void LimpiarDatosVehiculo()
        {
            VehiculoId = 0;
            VIN = string.Empty;
            Marca = string.Empty;
            Modelo = string.Empty;
            Version = string.Empty;
            Anio = DateTime.Now.Year;
            Color = string.Empty;
            Placas = string.Empty;
            KilometrajeInicial = 0;
            Ultimos4VIN = string.Empty;

            BusquedaMarca = string.Empty;
            BusquedaModelo = string.Empty;
            BusquedaVersion = string.Empty;
            BusquedaAnio = string.Empty;

            MarcasFiltradas = new ObservableCollection<string>();
            ModelosFiltrados = new ObservableCollection<string>();
            VersionesFiltradas = new ObservableCollection<string>();
            AniosFiltrados = new ObservableCollection<string>();

            MostrarSugerenciasMarca = false;
            MostrarSugerenciasModelo = false;
            MostrarSugerenciasVersion = false;
            MostrarSugerenciasAnio = false;

            VinDecodificadoExito = false;
            VinMensajeDecodificacion = string.Empty;
            ModoEdicionVehiculo = false;

            // Mostrar formulario vacío listo para nuevo vehículo
            MostrarCamposVehiculo = true;
            MostrarListaVehiculos = true; // Re-mostrar la lista para que pueda volver a seleccionar

            ErrorMessage = string.Empty;
        }
        private bool ValidarVehiculo()
        {
            if (string.IsNullOrWhiteSpace(VIN) || VIN.Length != 17)
            {
                ErrorMessage = "El VIN debe tener 17 caracteres";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Marca))
            {
                ErrorMessage = "La marca es requerida";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Modelo))
            {
                ErrorMessage = "El modelo es requerido";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Version))
            {
                ErrorMessage = "La Version es requerida";
                return false;
            }

            if (Anio < 2000 || Anio > DateTime.Now.Year + 1)
            {
                ErrorMessage = "El año ingresado del vehiculo no es válido";
                return false;
            }

            if (KilometrajeInicial <= 0)
            {
                ErrorMessage = "Ingresa el kilometraje inicial";
                return false;
            }
            return true;
        }

        #endregion
    }
}