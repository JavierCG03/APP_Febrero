using CarslineApp.Models;
using CarslineApp.Views.Citas;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CarslineApp.ViewModels.Creacion_Citas
{
    /// <summary>
    /// Partial class para gestión de órdenes y servicios
    /// </summary>
    public partial class CrearCitaViewModel
    {
        #region Campos Privados Orden
        private string _nombreTrabajoPersonalizado = string.Empty;
        private string _descripcionTrabajoPersonalizado = string.Empty;
        private int _kilometrajeActual;
        private DateTime _fechaHoraPromesa = RedondearHora();
        private string _observaciones = string.Empty;
        private TipoServicioDto _tipoServicioSeleccionado;
        private ObservableCollection<TipoServicioDto> _tiposServicio = new();
        private ObservableCollection<ServicioExtraDto> _serviciosExtra = new();
        private decimal _costoTotal;


        #endregion

        #region Propiedades Orden

        public string NombreTrabajoPersonalizado
        {
            get => _nombreTrabajoPersonalizado;
            set { _nombreTrabajoPersonalizado = value; OnPropertyChanged(); }
        }
        public string DescripcionTrabajoPersonalizado
        {
            get => _descripcionTrabajoPersonalizado;
            set { _descripcionTrabajoPersonalizado = value; OnPropertyChanged(); }
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


        public TipoServicioDto TipoServicioSeleccionado
        {
            get => _tipoServicioSeleccionado;
            set
            {
                _tipoServicioSeleccionado = value;
                OnPropertyChanged();
                CalcularCostoTotal();
            }
        }

        public ObservableCollection<TipoServicioDto> TiposServicio
        {
            get => _tiposServicio;
            set { _tiposServicio = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ServicioExtraDto> ServiciosExtra
        {
            get => _serviciosExtra;
            set { _serviciosExtra = value; OnPropertyChanged(); }
        }

        public decimal CostoTotal
        {
            get => _costoTotal;
            set { _costoTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(CostoTotalFormateado)); }
        }

        public string CostoTotalFormateado => $"${CostoTotal:N2}";

        #endregion

        #region Propiedades de Historial

        private ObservableCollection<HistorialServicioDto> _historialServicios = new();
        private bool _isLoadingHistorial;
        private bool _tieneHistorial;
        private string _resumenHistorial = string.Empty;

        /// <summary>
        /// Historial de servicios del vehículo
        /// </summary>
        public ObservableCollection<HistorialServicioDto> HistorialServicios
        {
            get => _historialServicios;
            set { _historialServicios = value; OnPropertyChanged(); }
        }
        public ObservableCollection<TrabajoCrearDto> TrabajosPersonalizados { get; set; } = new ObservableCollection<TrabajoCrearDto>();

        /// <summary>
        /// Indicador de carga del historial
        /// </summary>
        public bool IsLoadingHistorial
        {
            get => _isLoadingHistorial;
            set { _isLoadingHistorial = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Indica si el vehículo tiene historial
        /// </summary>
        public bool TieneHistorial
        {
            get => _tieneHistorial;
            set { _tieneHistorial = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Resumen del historial (ej: "3 servicios - Promedio: $1,200")
        /// </summary>
        public string ResumenHistorial
        {
            get => _resumenHistorial;
            set { _resumenHistorial = value; OnPropertyChanged(); }
        }

        #endregion

        #region Propiedades de Servicio Subsecuente

        private string _servicioSugerido = string.Empty;
        private string _mensajeServicioSugerido = string.Empty;
        private string _colorServicioSugerido = "#4CAF50";

        /// <summary>
        /// Tipo de servicio sugerido (1er, 2do, 3er o Externo)
        /// </summary>
        public string ServicioSugerido
        {
            get => _servicioSugerido;
            set { _servicioSugerido = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Mensaje explicando por qué se sugiere ese servicio
        /// </summary>
        public string MensajeServicioSugerido
        {
            get => _mensajeServicioSugerido;
            set { _mensajeServicioSugerido = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Color del indicador de servicio sugerido
        /// </summary>
        public string ColorServicioSugerido
        {
            get => _colorServicioSugerido;
            set { _colorServicioSugerido = value; OnPropertyChanged(); }
        }

        #endregion

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



        #region Métodos de Historial

        /// <summary>
        /// Cargar historial de servicios del vehículo
        /// </summary>
        private async Task CargarHistorialVehiculo()
        {
            if (VehiculoId <= 0) return;

            IsLoadingHistorial = true;
            TieneHistorial = false;
            HistorialServicios.Clear();

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Cargando historial para vehículo ID: {VehiculoId}");

                var response = await _apiService.ObtenerHistorialVehiculoAsync(VehiculoId);

                if (response.Success && response.Historial != null && response.Historial.Any())
                {
                    foreach (var servicio in response.Historial)
                    {
                        HistorialServicios.Add(servicio);
                    }
                    TieneHistorial = true;

                    // Actualizar resumen con el último servicio
                    var ultimoServicio = response.Historial.First();
                    ResumenHistorial = $"Último servicio: {ultimoServicio.TipoServicio} / ({ultimoServicio.KilometrajeRegistrado:N0} km - {ultimoServicio.FechaFormateada}) ";

                    if (KilometrajeActual > 0 && _tipoOrdenId == 1)
                    {
                        CalcularServicioSubsecuente();
                    }
                    else if (KilometrajeActual > 0 && _tipoOrdenId == 4)
                    {
                        ValidacionGarantia();
                    }
                }
                else
                {
                    TieneHistorial = false;
                    ResumenHistorial = "Sin historial de servicios";

                    if (_tipoOrdenId == 1)
                    {
                        // Sin historial = Servicio Externo
                        ServicioSugerido = "Servicio Externo";
                        MensajeServicioSugerido = "Sin historial de servicios previos";
                        ColorServicioSugerido = "#FF9800";
                    }
                    else if (_tipoOrdenId == 4)
                    {
                        ServicioSugerido = "Sin Garantia";
                        MensajeServicioSugerido = "No tiene historial de servicios previos";
                        ColorServicioSugerido = "#F44336";
                    }
                    PreseleccionarServicioSugerido();


                    System.Diagnostics.Debug.WriteLine("ℹ️ No se encontró historial");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar historial: {ex.Message}");
                TieneHistorial = false;
                ResumenHistorial = "Error al cargar historial";


                if (_tipoOrdenId == 1)
                {
                    // Sin historial = Servicio Externo
                    ServicioSugerido = "Servicio Externo";
                    MensajeServicioSugerido = "No se pudo cargar el historial";
                    ColorServicioSugerido = "#FF9800";
                }
                else if (_tipoOrdenId == 4)
                {
                    ServicioSugerido = "Sin Garantia";
                    MensajeServicioSugerido = "No se pudo cargar el historial";
                    ColorServicioSugerido = "#F44336";
                }

            }
            finally
            {
                IsLoadingHistorial = false;
            }
        }

        #endregion

        #region Métodos de Cálculo de Servicio Subsecuente

        /// <summary>
        /// Calcular el servicio subsecuente basado en historial y kilometraje actual
        /// </summary>
        private void CalcularServicioSubsecuente()
        {
            int kmRecorridos;
            // Si no hay historial, es servicio externo
            if (!TieneHistorial || !HistorialServicios.Any())
            {
                kmRecorridos = KilometrajeActual - KilometrajeInicial;

                if (kmRecorridos < 6000 && kmRecorridos != 0)
                {

                    //ServicioSugerido = $"PRIMER SERVICIO";
                    ServicioSugerido = $"1er Servicio";
                    ColorServicioSugerido = "#4CAF50";
                    MensajeServicioSugerido = $"✅ En tiempo y forma ({kmRecorridos:N0} km recorridos)";

                    return;
                }
                else if (kmRecorridos > 6000)
                {
                    //ServicioSugerido = "🔧 SERVICIO EXTERNO";
                    ServicioSugerido = "Servicio Externo";
                    MensajeServicioSugerido = $"⚠️ Excedió los 6,000 km ({kmRecorridos:N0} km recorridos)";
                    ColorServicioSugerido = "#FF9800";
                    return;
                }
                else
                {
                    //ServicioSugerido = "🔧 SERVICIO EXTERNO";
                    ServicioSugerido = "Servicio Externo";
                    MensajeServicioSugerido = $"Sin historial de servicios previos {kmRecorridos:N0} Km ";
                    ColorServicioSugerido = "#FF9800";
                    return;

                }
            }

            // Obtener el último servicio
            var ultimoServicio = HistorialServicios.First();

            // Calcular diferencias
            kmRecorridos = KilometrajeActual - ultimoServicio.KilometrajeRegistrado;
            DateTime fechaActual = DateTime.Now;
            TimeSpan tiempoTranscurrido = fechaActual - ultimoServicio.FechaServicio;
            int mesesTranscurridos = (int)(tiempoTranscurrido.TotalDays / 30);

            // Determinar el tipo de servicio anterior
            string tipoServicioAnterior = ultimoServicio.TipoServicio.ToUpper();

            // Validar si se excedieron los límites (6000 km o 5 meses)
            bool excedioKilometraje = kmRecorridos > 6000;
            bool excedioTiempo = mesesTranscurridos > 5;
            bool VioloOdometro = kmRecorridos < 0;

            if (excedioKilometraje || excedioTiempo)
            {
                // Se excedieron los límites, servicio externo
                ServicioSugerido = "Servicio Externo";
                ColorServicioSugerido = "#FF9800";

                List<string> razones = new List<string>();
                if (excedioKilometraje)
                    razones.Add($"Excedió los 6,000 km ({kmRecorridos:N0} km recorridos)");
                if (excedioTiempo)
                    razones.Add($"Excedió los 5 meses ({mesesTranscurridos} meses transcurridos)");

                MensajeServicioSugerido = $"⚠️ {string.Join(" y ", razones)}";

            }
            else if (VioloOdometro)
            {
                ServicioSugerido = "Servicio Externo";
                ColorServicioSugerido = "#FF9800";
                MensajeServicioSugerido = $"Posible violación de odómetro: el kilometraje del último servicio fue de {ultimoServicio.KilometrajeRegistrado:N0} km y el kilometraje actual es de {KilometrajeActual:N0} km.";

            }
            else
            {
                // Dentro de los límites, determinar servicio subsecuente
                string servicioSubsecuente = DeterminarServicioSubsecuente(tipoServicioAnterior);

                if (servicioSubsecuente == "EXTERNO")
                {
                    ServicioSugerido = "Servicio Externo";
                    ColorServicioSugerido = "#FF9800";
                    MensajeServicioSugerido = "✅ Ya completó los 3 servicios programados";
                }
                else
                {
                    ServicioSugerido = $"✨ {servicioSubsecuente}";
                    ColorServicioSugerido = "#4CAF50";
                    MensajeServicioSugerido = $"✅ En tiempo y forma ({kmRecorridos:N0} km / {mesesTranscurridos} meses desde último servicio)";
                }

                System.Diagnostics.Debug.WriteLine($"   ✅ Servicio sugerido: {ServicioSugerido}");
            }
            // Preseleccionar automáticamente en la lista de cards
            PreseleccionarServicioSugerido();
        }

        /// <summary>
        /// Determinar el servicio subsecuente basado en el servicio anterior
        /// </summary>
        private string DeterminarServicioSubsecuente(string servicioAnterior)
        {
            // Normalizar el nombre del servicio
            if (servicioAnterior.Contains("1") || servicioAnterior.Contains("PRIMER"))
                //return "2DO SERVICIO";
                return "2do Servicio";

            if (servicioAnterior.Contains("2") || servicioAnterior.Contains("SEGUNDO"))
                //return "3ER SERVICIO";
                return "'3er Servicio'";

            if (servicioAnterior.Contains("3") || servicioAnterior.Contains("TERCER"))
                //return "EXTERNO";
                return "Servicio Externo";

            // Si no es ninguno de los anteriores, es externo
            return "Servicio Externo";
        }

        #endregion

        #region Validacion Garantia
        private void ValidacionGarantia()
        {
            int kmRecorridos;
            // Si no hay historial, es servicio externo
            if (!TieneHistorial || !HistorialServicios.Any())
            {
                kmRecorridos = KilometrajeActual - KilometrajeInicial;

                if (kmRecorridos < 5000 && kmRecorridos != 0)
                {

                    ServicioSugerido = $"Garantia Valida";
                    ColorServicioSugerido = "#4CAF50";
                    MensajeServicioSugerido = $"✅ Servicios cumplidos en tiempo y forma ({kmRecorridos:N0} km recorridos)";

                    return;
                }
                else if (kmRecorridos > 5000)
                {
                    ServicioSugerido = "Sin Garantia";
                    MensajeServicioSugerido = $"⚠️ Excedió los 5,000 km desde su ultimo servicio ({kmRecorridos:N0} km recorridos)";
                    ColorServicioSugerido = "#F44336";

                    return;
                }
                else
                {
                    ServicioSugerido = "Sin Garantia";
                    MensajeServicioSugerido = "No tiene historial de servicios previos";
                    ColorServicioSugerido = "#F44336";

                    return;

                }
            }

            // Obtener el último servicio
            var ultimoServicio = HistorialServicios.First();

            // Calcular diferencias
            kmRecorridos = KilometrajeActual - ultimoServicio.KilometrajeRegistrado;
            DateTime fechaActual = DateTime.Now;
            TimeSpan tiempoTranscurrido = fechaActual - ultimoServicio.FechaServicio;
            int mesesTranscurridos = (int)(tiempoTranscurrido.TotalDays / 30);

            // Determinar el tipo de servicio anterior
            string tipoServicioAnterior = ultimoServicio.TipoServicio.ToUpper();

            // Validar si se excedieron los límites (5000 km o 4 meses)
            bool excedioKilometraje = kmRecorridos > 5000;
            bool excedioTiempo = mesesTranscurridos > 4;
            bool VioloOdometro = kmRecorridos < 0;

            if (excedioKilometraje || excedioTiempo)
            {
                // Se excedieron los límites, servicio externo
                ServicioSugerido = "SIN GARANTIA";
                ColorServicioSugerido = "#F44336";


                List<string> razones = new List<string>();
                if (excedioKilometraje)
                    razones.Add($"Excedió los 5,000 km desde su ultimo Servicio({kmRecorridos:N0} km recorridos)");
                if (excedioTiempo)
                    razones.Add($"Excedió los 4 meses desde su ultimo Servicio({mesesTranscurridos} meses transcurridos)");

                MensajeServicioSugerido = $"⚠️ {string.Join(" y ", razones)}";

            }
            else if (VioloOdometro)
            {
                ServicioSugerido = "SIN GARANTIA";
                ColorServicioSugerido = "#F44336";
                MensajeServicioSugerido = $"Posible violación de odómetro: el kilometraje del último servicio fue de {ultimoServicio.KilometrajeRegistrado:N0} km y el kilometraje actual es de {KilometrajeActual:N0} km.";

            }
            else
            {
                // Dentro de los límites, determinar servicio subsecuente
                string servicioSubsecuente = DeterminarGarantia(tipoServicioAnterior);

                if (servicioSubsecuente == "EXTERNO")
                {
                    ServicioSugerido = "Sin Garantia";
                    ColorServicioSugerido = "#F44336";
                    MensajeServicioSugerido = "Se ha terminado el Periodo de Garantia";
                }
                else
                {
                    ServicioSugerido = $"✨ {servicioSubsecuente}";
                    ColorServicioSugerido = "#4CAF50";
                    MensajeServicioSugerido = $"Servicios cumplidos en tiempo y forma ({kmRecorridos:N0} km / {mesesTranscurridos} meses desde último servicio)";
                }
            }
        }

        /// <summary>
        /// Determinar el servicio subsecuente basado en el servicio anterior
        /// </summary>
        private string DeterminarGarantia(string servicioAnterior)
        {
            // Normalizar el nombre del servicio
            if (servicioAnterior.Contains("1") || servicioAnterior.Contains("PRIMER"))
                return "Garantia Valida";

            if (servicioAnterior.Contains("2") || servicioAnterior.Contains("SEGUNDO"))
                return "Garantia Valida";

            if (servicioAnterior.Contains("3") || servicioAnterior.Contains("TERCER"))
                return "EXTERNO";

            // Si no es ninguno de los anteriores, es externo
            return "EXTERNO";
        }
        #endregion 

        #region Trabajos Personalizados

        private void AgregarTrabajoPersonalizado()
        {
            if (string.IsNullOrWhiteSpace(NombreTrabajoPersonalizado))
            {
                ErrorMessage = "Ingresa el nombre del trabajo";
                return;
            }

            var trabajoNuevo = new TrabajoCrearDto
            {
                Trabajo = NombreTrabajoPersonalizado,
                Indicaciones = DescripcionTrabajoPersonalizado,

            };

            TrabajosPersonalizados.Add(trabajoNuevo);

            // Limpiar campos
            NombreTrabajoPersonalizado = string.Empty;
            DescripcionTrabajoPersonalizado = string.Empty;
            ErrorMessage = string.Empty;
        }

        private void EliminarTrabajoPersonalizado(TrabajoCrearDto trabajo)
        {
            TrabajosPersonalizados.Remove(trabajo);
        }

        #endregion

        #region Métodos de Creación de Orden - MODIFICADOS

        /// <summary>
        /// Navegar al resumen de orden de garantía
        /// </summary>
        private async Task CrearOrdenGarantia()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var trabajos = new List<TrabajoCrearDto>();

                // Agregar trabajos personalizados
                foreach (var trabajo in TrabajosPersonalizados)
                {
                    trabajos.Add(new TrabajoCrearDto
                    {
                        Trabajo = trabajo.Trabajo,
                        Indicaciones = string.IsNullOrWhiteSpace(trabajo.Indicaciones)
                            ? null
                            : trabajo.Indicaciones
                    });
                }

                // Validar que haya al menos un trabajo
                if (trabajos.Count == 0)
                {
                    ErrorMessage = "Debe agregar al menos una reparación o trabajo";
                    return;
                }

                // Navegar a página de resumen
                var resumenPage = new ResumenCrearCitaPage(
                    tipoOrdenId: _tipoOrdenId,
                    clienteId: ClienteId,
                    vehiculoId: VehiculoId,
                    tipoServicioId: 5, // Sin servicio
                    fechaHoraCita: _entradafechahoracita,
                    observaciones: Observaciones,
                    trabajos: trabajos
                );

                await Application.Current.MainPage.Navigation.PushAsync(resumenPage);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al preparar resumen: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Navegar al resumen de orden de diagnóstico
        /// </summary>
        private async Task CrearOrdenDiagnostico()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var trabajos = new List<TrabajoCrearDto>();

                // Agregar trabajos personalizados
                foreach (var trabajo in TrabajosPersonalizados)
                {
                    trabajos.Add(new TrabajoCrearDto
                    {
                        Trabajo = trabajo.Trabajo,
                        Indicaciones = string.IsNullOrWhiteSpace(trabajo.Indicaciones)
                            ? null
                            : trabajo.Indicaciones
                    });
                }

                // Validar que haya al menos un trabajo
                if (trabajos.Count == 0)
                {
                    ErrorMessage = "Debe agregar al menos un diagnóstico";
                    return;
                }

                // Navegar a página de resumen
                var resumenPage = new ResumenCrearCitaPage(
                    tipoOrdenId: _tipoOrdenId,
                    clienteId: ClienteId,
                    vehiculoId: VehiculoId,
                    tipoServicioId: 5, // Sin servicio
                    fechaHoraCita: _entradafechahoracita,
                    observaciones: Observaciones,
                    trabajos: trabajos
                );

                await Application.Current.MainPage.Navigation.PushAsync(resumenPage);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al preparar resumen: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Navegar al resumen de orden de reparación
        /// </summary>
        private async Task CrearOrdenReparacion()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var trabajos = new List<TrabajoCrearDto>();

                // 1. Agregar reparaciones frecuentes seleccionadas
                var serviciosSeleccionados = ServiciosExtra.Where(s => s.Seleccionado).ToList();
                foreach (var servicio in serviciosSeleccionados)
                {
                    trabajos.Add(new TrabajoCrearDto
                    {
                        Trabajo = servicio.Nombre,
                        Indicaciones = servicio.IndicacionesPersonalizadas
                    });
                }

                // 2. Agregar trabajos personalizados
                foreach (var trabajo in TrabajosPersonalizados)
                {
                    trabajos.Add(new TrabajoCrearDto
                    {
                        Trabajo = trabajo.Trabajo,
                        Indicaciones = string.IsNullOrWhiteSpace(trabajo.Indicaciones)
                            ? null
                            : trabajo.Indicaciones
                    });
                }

                // Validar que haya al menos un trabajo
                if (trabajos.Count == 0)
                {
                    ErrorMessage = "Debe agregar al menos una reparación o trabajo";
                    return;
                }

                // Navegar a página de resumen
                var resumenPage = new ResumenCrearCitaPage(
                    tipoOrdenId: _tipoOrdenId,
                    clienteId: ClienteId,
                    vehiculoId: VehiculoId,
                    tipoServicioId: 5, // Sin servicio
                    fechaHoraCita: _entradafechahoracita,
                    observaciones: Observaciones,
                    trabajos: trabajos
                );

                await Application.Current.MainPage.Navigation.PushAsync(resumenPage);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al preparar resumen: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Navegar al resumen de orden de servicio
        /// </summary>
        private async Task CrearOrdenServicio()
        {
            if (TipoServicioSeleccionado == null)
            {
                ErrorMessage = "Selecciona un tipo de servicio";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var trabajos = new List<TrabajoCrearDto>();

                // 1. Agregar el servicio principal
                trabajos.Add(new TrabajoCrearDto
                {
                    Trabajo = TipoServicioSeleccionado.Nombre,
                    Indicaciones = null
                });

                // 2. Agregar servicios extra seleccionados
                var serviciosSeleccionados = ServiciosExtra.Where(s => s.Seleccionado).ToList();
                foreach (var servicio in serviciosSeleccionados)
                {
                    trabajos.Add(new TrabajoCrearDto
                    {
                        Trabajo = servicio.Nombre,
                        Indicaciones = servicio.IndicacionesPersonalizadas
                    });
                }

                // Navegar a página de resumen
                var resumenPage = new ResumenCrearCitaPage(
                    tipoOrdenId: _tipoOrdenId,
                    clienteId: ClienteId,
                    vehiculoId: VehiculoId,
                    tipoServicioId: TipoServicioSeleccionado.Id,
                    fechaHoraCita: _entradafechahoracita,
                    observaciones: Observaciones,
                    trabajos: trabajos
                );

                await Application.Current.MainPage.Navigation.PushAsync(resumenPage);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al preparar resumen: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }


        public void CalcularCostoTotal()
        {
            decimal total = 0;

            if (TipoServicioSeleccionado != null)
            {
                total += TipoServicioSeleccionado.Precio;
            }

            total += ServiciosExtra
                .Where(s => s.Seleccionado)
                .Sum(s => s.Precio);

            CostoTotal = total;
        }
        /// <summary>
        /// Busca en TiposServicio el ítem cuyo Nombre coincida con el texto del servicio sugerido
        /// y lo asigna como TipoServicioSeleccionado. Se llama automáticamente al calcular la sugerencia.
        /// </summary>
        public void PreseleccionarServicioSugerido()
        {
            if (!TiposServicio.Any() || string.IsNullOrEmpty(ServicioSugerido)) return;

            foreach (var t in TiposServicio)
            {
                t.EsSugerido = false;
                t.EstaSeleccionado = false;
            }

            var sugeridoLimpio = ServicioSugerido
                .Replace("✨", "").Replace("🔧", "").Replace("✅", "").Replace("⚠️", "")
                .Trim().ToUpper();

            var coincidencia =
                TiposServicio.FirstOrDefault(t => t.Nombre.ToUpper() == sugeridoLimpio)
                ?? TiposServicio.FirstOrDefault(t =>
                    sugeridoLimpio.Contains(t.Nombre.ToUpper()) ||
                    t.Nombre.ToUpper().Contains(sugeridoLimpio));

            if (coincidencia != null)
            {
                coincidencia.EsSugerido = true;
                coincidencia.EstaSeleccionado = true;
                TipoServicioSeleccionado = coincidencia;
            }
        }

        #endregion
    }
}