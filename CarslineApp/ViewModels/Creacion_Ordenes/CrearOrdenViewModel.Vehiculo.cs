using CarslineApp.Models;
using CarslineApp.Services;
using System.Collections.ObjectModel;

namespace CarslineApp.ViewModels
{

    public partial class CrearOrdenViewModel
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

        // Estado del decodificador VIN
        private bool _vinDecodificando;
        private bool _vinDecodificadoExito;
        private string _vinMensajeDecodificacion = string.Empty;

        // Autocompletado
        private string _busquedaMarca = string.Empty;
        private ObservableCollection<string> _marcasFiltradas = new();
        private bool _mostrarSugerenciasMarca;
        private string _busquedaModelo = string.Empty;
        private ObservableCollection<string> _modelosFiltrados = new();
        private bool _mostrarSugerenciasModelo;
        private string _busquedaAnio = string.Empty;
        private ObservableCollection<string> _aniosFiltrados = new();
        private bool _mostrarSugerenciasAnio;

        #endregion

        #region Catálogo Marcas / Modelos

        private static readonly Dictionary<string, List<string>> _catalogoMarcasModelos =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Chevrolet"] = new() { "Aveo", "Beat", "Blazer", "Captiva", "Cavalier", "Colorado", "Cruze", "Equinox", "Malibu", "Onix", "Silverado", "Sonic", "Spark", "Tahoe", "Trailblazer", "Trax" },
                ["Ford"] = new() { "Bronco", "EcoSport", "Edge", "Escape", "Explorer", "F-150", "Fiesta", "Focus", "Fusion", "Mustang", "Ranger", "Territory" },
                ["Nissan"] = new() { "Altima", "Frontier", "Kicks", "Leaf", "March", "Maxima", "Murano", "NP300", "Pathfinder", "Qashqai", "Sentra", "Tiida", "Versa", "X-Trail" },
                ["Toyota"] = new() { "4Runner", "Camry", "Corolla", "Fortuner", "Hilux", "Land Cruiser", "Prius", "RAV4", "Sequoia", "Sienna", "Tacoma", "Yaris" },
                ["Volkswagen"] = new() { "Bora", "Crossfox", "Gol", "Golf", "Jetta", "Passat", "Polo", "Saveiro", "Taos", "Tiguan", "Touareg", "Vento" },
                ["Honda"] = new() { "Accord", "BR-V", "City", "Civic", "CR-V", "Fit", "HR-V", "Odyssey", "Pilot", "Ridgeline" },
                ["Hyundai"] = new() { "Accent", "Creta", "Elantra", "Ioniq", "Kona", "Santa Fe", "Sonata", "Tucson", "Venue" },
                ["Kia"] = new() { "Carnival", "EV6", "K3", "K5", "Niro", "Rio", "Seltos", "Sorento", "Soul", "Sportage", "Stinger" },
                ["Mazda"] = new() { "CX-3", "CX-30", "CX-5", "CX-9", "Mazda2", "Mazda3", "Mazda6", "MX-5" },
                ["Dodge"] = new() { "Challenger", "Charger", "Durango", "Journey", "Ram 1500", "Ram 2500" },
                ["Jeep"] = new() { "Cherokee", "Compass", "Gladiator", "Grand Cherokee", "Renegade", "Wrangler" },
                ["RAM"] = new() { "1500", "2500", "ProMaster" },
                ["Audi"] = new() { "A1", "A3", "A4", "A5", "A6", "Q2", "Q3", "Q5", "Q7", "Q8", "TT" },
                ["BMW"] = new() { "116i", "118i", "218i", "320i", "330i", "420i", "520i", "530i", "X1", "X2", "X3", "X4", "X5", "X6" },
                ["Mercedes-Benz"] = new() { "A 200", "B 200", "C 180", "C 200", "CLA", "E 200", "GLA", "GLB", "GLC", "GLE", "GLS" },
                ["Peugeot"] = new() { "208", "2008", "3008", "308", "408", "5008" },
                ["Renault"] = new() { "Captur", "Duster", "Kangoo", "Koleos", "Logan", "Oroch", "Sandero", "Stepway" },
                ["SEAT"] = new() { "Arona", "Ateca", "Ibiza", "Leon", "Tarraco" },
                ["Subaru"] = new() { "BRZ", "Crosstrek", "Forester", "Impreza", "Legacy", "Outback", "WRX" },
                ["Mitsubishi"] = new() { "Eclipse Cross", "L200", "Mirage", "Montero", "Outlander" },
                ["Fiat"] = new() { "Argo", "Cronos", "Fastback", "Pulse", "Toro" },
                ["Suzuki"] = new() { "Baleno", "Ignis", "Jimny", "Swift", "Vitara" },
                ["Volvo"] = new() { "S60", "S90", "V60", "XC40", "XC60", "XC90" },
                ["GMC"] = new() { "Acadia", "Canyon", "Sierra", "Terrain", "Yukon" },
                ["Buick"] = new() { "Enclave", "Encore", "Envision" },
                ["Chrysler"] = new() { "300", "Pacifica" },
                ["Mini"] = new() { "Clubman", "Cooper", "Countryman", "Paceman" },
                ["Porsche"] = new() { "718", "911", "Cayenne", "Macan", "Panamera", "Taycan" },
                ["Land Rover"] = new() { "Defender", "Discovery", "Discovery Sport", "Freelander", "Range Rover", "Range Rover Evoque", "Range Rover Sport" },
                ["Tesla"] = new() { "Model 3", "Model S", "Model X", "Model Y" },
            };

        private static readonly List<string> _todasLasMarcas =
            _catalogoMarcasModelos.Keys.OrderBy(m => m).ToList();

        private static readonly List<string> _todosLosAnios =
            Enumerable.Range(2015, DateTime.Now.Year - 2015 + 2)
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
            }
        }

        /// <summary>
        /// Al llegar a 17 caracteres dispara automáticamente la decodificación VIN.
        /// </summary>
        public string VIN
        {
            get => _vin;
            set
            {
                var nuevo = value?.ToUpper() ?? string.Empty;
                _vin = nuevo;
                OnPropertyChanged();
                ErrorMessage = string.Empty;

                if (nuevo.Length == 17 && VehiculoId == 0)
                    _ = DecodificarVinAsync(nuevo);   // disparo async sin await (fire-and-forget)
                else if (nuevo.Length < 17)
                {
                    VinDecodificadoExito = false;
                    VinMensajeDecodificacion = string.Empty;
                }
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
                OnPropertyChanged(nameof(TextoBotonVehiculo));
                OnPropertyChanged(nameof(ColorBotonVehiculo));
                OnPropertyChanged(nameof(CampoPlacasBloqueado));
            }
        }

        public bool CampoPlacasBloqueado => VehiculoId > 0 && !ModoEdicionVehiculo;
        public bool CamposVehiculoBloqueados => VehiculoId > 0;
        public string TextoBotonVehiculo => ModoEdicionVehiculo ? "💾 Guardar Placas" : "✏️ Editar Placas";
        public string ColorBotonVehiculo => ModoEdicionVehiculo ? "#4CAF50" : "#FF9800";

        #endregion

        #region Propiedades del Decodificador VIN

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

        #region Decodificación VIN (NHTSA)

        /// <summary>
        /// Consulta la API gratuita de NHTSA y autocompleta Marca, Modelo y Año.
        /// Se invoca automáticamente al escribir el VIN completo.
        /// </summary>
        private async Task DecodificarVinAsync(string vin)
        {
            VinDecodificando = true;
            VinDecodificadoExito = false;
            VinMensajeDecodificacion = "🔍 Consultando datos del vehículo...";

            try
            {
                var resultado = await _vinDecoder.DecodificarVinAsync(vin);

                if (resultado == null)
                {
                    VinMensajeDecodificacion = "⚠️ Sin conexión. Ingresa los datos manualmente.";
                    return;
                }

                if (string.IsNullOrEmpty(resultado.Marca) || resultado.Anio < 2000)
                {
                    VinMensajeDecodificacion = "⚠️ VIN no reconocido. Ingresa los datos manualmente.";
                    return;
                }

                // ── Rellenar campos ────────────────────────────────────────────
                Marca = NormalizarMarca(resultado.Marca);
                Modelo = NormalizarModelo(resultado.Modelo, resultado.Marca);
                Anio = resultado.Anio;

                // Sincronizar buscadores de autocompletado
                BusquedaMarca = Marca;
                BusquedaModelo = Modelo;
                BusquedaAnio = Anio.ToString();
                MostrarSugerenciasMarca = false;
                MostrarSugerenciasModelo = false;
                MostrarSugerenciasAnio = false;

                // Construir resumen con datos del motor
                var extras = new List<string>();
                if (!string.IsNullOrEmpty(resultado.NumCilindros))
                    extras.Add($"{resultado.NumCilindros} cil.");
                if (!string.IsNullOrEmpty(resultado.Displacement) &&
                    float.TryParse(resultado.Displacement, out float lt) && lt > 0)
                    extras.Add($"{lt:F1}L");
                if (!string.IsNullOrEmpty(resultado.TipoCombust))
                    extras.Add(resultado.TipoCombust);

                var extraTexto = extras.Any() ? $"  ·  {string.Join(", ", extras)}" : string.Empty;

                VinDecodificadoExito = true;
                VinMensajeDecodificacion = $"✅ {resultado.Anio} {Marca} {Modelo}{extraTexto}";
            }
            catch (Exception ex)
            {
                VinMensajeDecodificacion = "⚠️ Error al decodificar. Ingresa los datos manualmente.";
                System.Diagnostics.Debug.WriteLine($"[VIN] {ex.Message}");
            }
            finally
            {
                VinDecodificando = false;
            }
        }

        private static string NormalizarMarca(string marcaNhtsa)
        {
            // Coincidencia exacta en el catálogo
            var enc = _catalogoMarcasModelos.Keys.FirstOrDefault(
                m => m.Equals(marcaNhtsa, StringComparison.OrdinalIgnoreCase));
            if (enc != null) return enc;

            // Coincidencia parcial
            enc = _catalogoMarcasModelos.Keys.FirstOrDefault(m =>
                marcaNhtsa.Contains(m, StringComparison.OrdinalIgnoreCase) ||
                m.Contains(marcaNhtsa, StringComparison.OrdinalIgnoreCase));
            if (enc != null) return enc;

            // Title Case si no está en catálogo
            return System.Globalization.CultureInfo.InvariantCulture
                         .TextInfo.ToTitleCase(marcaNhtsa.ToLower());
        }

        private static string NormalizarModelo(string modeloNhtsa, string marcaNhtsa)
        {
            if (string.IsNullOrEmpty(modeloNhtsa)) return string.Empty;

            var marcaNorm = _catalogoMarcasModelos.Keys.FirstOrDefault(
                m => m.Equals(marcaNhtsa, StringComparison.OrdinalIgnoreCase));

            if (marcaNorm != null &&
                _catalogoMarcasModelos.TryGetValue(marcaNorm, out var modelos))
            {
                var enc = modelos.FirstOrDefault(
                    m => m.Equals(modeloNhtsa, StringComparison.OrdinalIgnoreCase));
                if (enc != null) return enc;
            }

            return System.Globalization.CultureInfo.InvariantCulture
                         .TextInfo.ToTitleCase(modeloNhtsa.ToLower());
        }

        // Comando para decodificar manualmente si el usuario pegó el VIN
        private System.Windows.Input.ICommand? _decodificarVinManualCommand;
        public System.Windows.Input.ICommand DecodificarVinManualCommand =>
            _decodificarVinManualCommand ??= new Command(
                async () => await DecodificarVinAsync(VIN),
                () => VIN.Length == 17 && VehiculoId == 0);

        #endregion

        #region Autocompletado — Marca

        public string BusquedaMarca
        {
            get => _busquedaMarca;
            set { _busquedaMarca = value; OnPropertyChanged(); FiltrarMarcas(); }
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
            if (string.IsNullOrWhiteSpace(BusquedaMarca))
            {
                MarcasFiltradas = new ObservableCollection<string>(_todasLasMarcas);
            }
            else
            {
                var txt = BusquedaMarca.Trim();
                MarcasFiltradas = new ObservableCollection<string>(
                    _todasLasMarcas.Where(m => m.StartsWith(txt, StringComparison.OrdinalIgnoreCase))
                    .Concat(_todasLasMarcas.Where(m =>
                        !m.StartsWith(txt, StringComparison.OrdinalIgnoreCase) &&
                         m.Contains(txt, StringComparison.OrdinalIgnoreCase))));
            }
            MostrarSugerenciasMarca = MarcasFiltradas.Count > 0;
        }

        public void SeleccionarMarca(string marca)
        {
            Marca = marca; BusquedaMarca = marca;
            MostrarSugerenciasMarca = false;
            Modelo = string.Empty; BusquedaModelo = string.Empty;
            FiltrarModelos();
        }

        private System.Windows.Input.ICommand? _seleccionarMarcaCommand;
        public System.Windows.Input.ICommand SeleccionarMarcaCommand =>
            _seleccionarMarcaCommand ??= new Command<string>(SeleccionarMarca);

        private System.Windows.Input.ICommand? _abrirSugerenciasMarcaCommand;
        public System.Windows.Input.ICommand AbrirSugerenciasMarcaCommand =>
            _abrirSugerenciasMarcaCommand ??= new Command(() =>
            { FiltrarMarcas(); MostrarSugerenciasMarca = true; });

        #endregion

        #region Autocompletado — Modelo

        public string BusquedaModelo
        {
            get => _busquedaModelo;
            set { _busquedaModelo = value; OnPropertyChanged(); FiltrarModelos(); }
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
            if (!string.IsNullOrWhiteSpace(Marca) &&
                _catalogoMarcasModelos.TryGetValue(Marca, out var modelos))
            {
                if (string.IsNullOrWhiteSpace(BusquedaModelo))
                    ModelosFiltrados = new ObservableCollection<string>(modelos);
                else
                {
                    var txt = BusquedaModelo.Trim();
                    ModelosFiltrados = new ObservableCollection<string>(
                        modelos.Where(m => m.StartsWith(txt, StringComparison.OrdinalIgnoreCase))
                        .Concat(modelos.Where(m =>
                            !m.StartsWith(txt, StringComparison.OrdinalIgnoreCase) &&
                             m.Contains(txt, StringComparison.OrdinalIgnoreCase))));
                }
                MostrarSugerenciasModelo = ModelosFiltrados.Count > 0;
            }
            else
            {
                ModelosFiltrados = new ObservableCollection<string>();
                MostrarSugerenciasModelo = false;
            }
        }

        public void SeleccionarModelo(string modelo)
        {
            Modelo = modelo; BusquedaModelo = modelo;
            MostrarSugerenciasModelo = false;
        }

        private System.Windows.Input.ICommand? _seleccionarModeloCommand;
        public System.Windows.Input.ICommand SeleccionarModeloCommand =>
            _seleccionarModeloCommand ??= new Command<string>(SeleccionarModelo);

        private System.Windows.Input.ICommand? _abrirSugerenciasModeloCommand;
        public System.Windows.Input.ICommand AbrirSugerenciasModeloCommand =>
            _abrirSugerenciasModeloCommand ??= new Command(() =>
            { FiltrarModelos(); MostrarSugerenciasModelo = true; });

        #endregion

        #region Autocompletado — Año

        public string BusquedaAnio
        {
            get => _busquedaAnio;
            set { _busquedaAnio = value; OnPropertyChanged(); FiltrarAnios(); }
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
            AniosFiltrados = string.IsNullOrWhiteSpace(BusquedaAnio)
                ? new ObservableCollection<string>(_todosLosAnios)
                : new ObservableCollection<string>(
                    _todosLosAnios.Where(a => a.StartsWith(BusquedaAnio.Trim())));
            MostrarSugerenciasAnio = AniosFiltrados.Count > 0;
        }

        public void SeleccionarAnio(string anio)
        {
            if (int.TryParse(anio, out int anioInt)) Anio = anioInt;
            BusquedaAnio = anio;
            MostrarSugerenciasAnio = false;
        }

        private System.Windows.Input.ICommand? _seleccionarAnioCommand;
        public System.Windows.Input.ICommand SeleccionarAnioCommand =>
            _seleccionarAnioCommand ??= new Command<string>(SeleccionarAnio);

        private System.Windows.Input.ICommand? _abrirSugerenciasAnioCommand;
        public System.Windows.Input.ICommand AbrirSugerenciasAnioCommand =>
            _abrirSugerenciasAnioCommand ??= new Command(() =>
            { FiltrarAnios(); MostrarSugerenciasAnio = true; });

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
                    ErrorMessage = $"Se encontraron {VehiculosEncontrados.Count} vehículos. Selecciona uno:";
                }
                else
                {
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

            IsLoading = true;
            ErrorMessage = string.Empty;
            MostrarListaVehiculos = false;
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
                    BusquedaMarca = Marca; BusquedaModelo = Modelo; BusquedaAnio = Anio.ToString();
                    MostrarListaVehiculos = false;
                    VinDecodificadoExito = false;
                    VinMensajeDecodificacion = string.Empty;
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
                if (response.Success) { ModoEdicionVehiculo = false; await Application.Current.MainPage.DisplayAlert("✅ Éxito", "Las placas han sido actualizadas correctamente", "OK"); }
                else { ErrorMessage = response.Message; await Application.Current.MainPage.DisplayAlert("❌ Error", response.Message, "OK"); }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("❌ Error", $"Error al actualizar placas: {ex.Message}", "OK");
            }
            finally { IsLoading = false; }
        }

        private bool ValidarVehiculo()
        {
            if (string.IsNullOrWhiteSpace(VIN) || VIN.Length != 17) { ErrorMessage = "El VIN debe tener 17 caracteres"; return false; }
            if (string.IsNullOrWhiteSpace(Marca)) { ErrorMessage = "La marca es requerida"; return false; }
            if (string.IsNullOrWhiteSpace(Modelo)) { ErrorMessage = "El modelo es requerido"; return false; }
            if (string.IsNullOrWhiteSpace(Version)) { ErrorMessage = "La versión es requerida"; return false; }
            if (Anio < 2000 || Anio > DateTime.Now.Year + 1) { ErrorMessage = "El año ingresado del vehículo no es válido"; return false; }
            if (KilometrajeInicial <= 0) { ErrorMessage = "Ingresa el kilometraje inicial"; return false; }
            return true;
        }

        #endregion
    }
}