using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows.Input;

namespace CarslineApp.ViewModels
{
    public class AgendaCitasViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private DateTime _fechaSeleccionada;
        private TipoVistaAgenda _vistaActual;
        private bool _isLoading;
        public int CitaId { get; set; }
        public int ClienteId { get; set; }
        public int VehiculoId { get; set; }
        private ObservableCollection<SlotHorario> _slotsHorarios;
        private ObservableCollection<DiaCalendario> _diasSemana;
        private ObservableCollection<DiaCalendario> _diasMes;
        private DateTime _fechaUsuario;
        private DateTime _fechaMinimaDia;
        private DateTime _fechaMinimaSemana;
        private DateTime _fechaMinimaMes;


        // Horarios disponibles (8:30 AM - 1:00 PM)
        private readonly List<TimeSpan> _horariosDisponibles = new()
        {
            new TimeSpan(8, 30, 0),   // 8:30 AM
            new TimeSpan(9, 0, 0),    // 9:00 AM
            new TimeSpan(9, 30, 0),   // 9:30 AM
            new TimeSpan(10, 0, 0),   // 10:00 AM
            new TimeSpan(10, 30, 0),  // 10:30 AM
            new TimeSpan(11, 0, 0),   // 11:00 AM
            new TimeSpan(11, 30, 0),  // 11:30 AM
            new TimeSpan(12, 0, 0),   // 12:00 PM
            new TimeSpan(12, 30, 0),  // 12:30 PM
            new TimeSpan(13, 0, 0)    // 1:00 PM
        };

        public AgendaCitasViewModel(int citaId, int clienteId, int vehiculoId)
        {
            _apiService = new ApiService();
            _fechaSeleccionada = DateTime.Today;
            _fechaUsuario = DateTime.Today;
            _vistaActual = TipoVistaAgenda.Dia;
            _slotsHorarios = new ObservableCollection<SlotHorario>();
            _diasSemana = new ObservableCollection<DiaCalendario>();
            _diasMes = new ObservableCollection<DiaCalendario>();
            _fechaMinimaDia = DateTime.Today;
            var diasDesdeLunes = ((int)DateTime.Today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            _fechaMinimaSemana = DateTime.Today.AddDays(-diasDesdeLunes);
            _fechaMinimaMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            CitaId = citaId;
            ClienteId = clienteId;
            VehiculoId = vehiculoId;


            // Comandos
            SiguienteCommand = new Command(async () => await CambiarSiguiente());
            AnteriorCommand = new Command(async () => await CambiarAnterior());
            CambiarVistaDiaCommand = new Command(async () => await CambiarVista(TipoVistaAgenda.Dia));
            CambiarVistaSemanaActualCommand = new Command(async () => await CambiarVista(TipoVistaAgenda.SemanaActual));
            CambiarVistaMesCommand = new Command(async () => await CambiarVista(TipoVistaAgenda.Mes));
            DisponibleCommand = new Command<SlotHorario>(async (slot) => await DesicionDisponible(slot));
            HorarioSemanaCommand = new Command<SlotHorario>(async (slot) => await ManejarSlotSemana(slot));
            VerDetalleCitaCommand = new Command<CitaDto>(async (cita) => await VerDetalleCita(cita));
            SeleccionarDiaCommand = new Command<DiaCalendario>(async (dia) => await SeleccionarDia(dia));

        }
        #region Propiedades

        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                _fechaSeleccionada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FechaSeleccionadaTexto));
                OnPropertyChanged(nameof(TituloVista));
                OnPropertyChanged(nameof(PuedeRegresar));
                
            }
        }

        public string FechaSeleccionadaTexto => FechaSeleccionada.ToString("dddd, dd 'de' MMMM yyyy");

        public bool PuedeRegresar
        {
            get
            {
                return VistaActual switch
                {
                    TipoVistaAgenda.Dia =>
                        FechaSeleccionada.Date > _fechaMinimaDia.Date,
                    TipoVistaAgenda.SemanaActual =>
                        ObtenerLunes(FechaSeleccionada) > _fechaMinimaSemana,
                    TipoVistaAgenda.Mes =>
                        FechaSeleccionada.Year > _fechaMinimaMes.Year ||
                        (FechaSeleccionada.Year == _fechaMinimaMes.Year &&
                         FechaSeleccionada.Month > _fechaMinimaMes.Month),

                    _ => false
                };
            }
        }


        public TipoVistaAgenda VistaActual
        {
            get => _vistaActual;
            set
            {
                _vistaActual = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EsVistaDia));
                OnPropertyChanged(nameof(EsVistaSemana));
                OnPropertyChanged(nameof(EsVistaMes));
                OnPropertyChanged(nameof(TituloVista));
                OnPropertyChanged(nameof(PuedeRegresar));

            }
        }

        public string TituloVista => VistaActual switch
        {
            TipoVistaAgenda.Dia => FechaSeleccionada.ToString("dddd dd MMM"),
            TipoVistaAgenda.SemanaActual => ObtenerTituloSemana(),
            TipoVistaAgenda.Mes => FechaSeleccionada.ToString("MMMM yyyy"),
            _ => ""
        };
        public bool EsCrearNueva => (CitaId == 0 && ClienteId == 0 && VehiculoId == 0);
        public bool EsReagendar => (CitaId > 0 && ClienteId == 0 && VehiculoId == 0);
        public bool EsContinuarCita => (CitaId == 0 && VehiculoId > 0 && ClienteId > 0);
        public bool EsVistaDia => VistaActual == TipoVistaAgenda.Dia;
        public bool EsVistaSemana => VistaActual == TipoVistaAgenda.SemanaActual;
        public bool EsVistaMes => VistaActual == TipoVistaAgenda.Mes;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SlotHorario> SlotsHorarios
        {
            get => _slotsHorarios;
            set { _slotsHorarios = value; OnPropertyChanged(); }
        }

        public ObservableCollection<DiaCalendario> DiasSemana
        {
            get => _diasSemana;
            set { _diasSemana = value; OnPropertyChanged(); }
        }

        public ObservableCollection<DiaCalendario> DiasMes
        {
            get => _diasMes;
            set { _diasMes = value; OnPropertyChanged(); }
        }

        #endregion

        #region Comandos

        public ICommand CambiarVistaDiaCommand { get; }
        public ICommand CambiarVistaSemanaActualCommand { get; }
        public ICommand CambiarVistaMesCommand { get; }
        public ICommand CrearCitaCommand { get; }
        public ICommand VerDetalleCitaCommand { get; }
        public ICommand SeleccionarDiaCommand { get; }
        public ICommand AnteriorCommand { get; }
        public ICommand SiguienteCommand { get; }
        public ICommand DisponibleCommand { get; }
        public ICommand HorarioSemanaCommand { get; }

        #endregion

        #region Métodos Públicos

        public async Task InicializarAsync()
        {
            await CargarVista();
        }

        #endregion

        #region Métodos Privados
        private string ObtenerTituloSemana()
        {
            var lunesSeleccionado = ObtenerLunes(FechaSeleccionada);
            var lunesActual = ObtenerLunes(DateTime.Today);
            var lunesSiguiente = lunesActual.AddDays(7);

            if (lunesSeleccionado == lunesActual)
                return "Semana actual";

            if (lunesSeleccionado == lunesSiguiente)
                return "Semana siguiente";

            var sabado = lunesSeleccionado.AddDays(5);

            return $"Semana del {lunesSeleccionado:dd} – {sabado:dd MMM}";
        }

        private DateTime ObtenerLunes(DateTime fecha)
        {
            int diff = (7 + (fecha.DayOfWeek - DayOfWeek.Monday)) % 7;
            return fecha.AddDays(-diff).Date;
        }


        private async Task CambiarVista(TipoVistaAgenda nuevaVista)
        {
            VistaActual = nuevaVista;

            if (nuevaVista == TipoVistaAgenda.Dia)
            {
                // ✅ Al regresar a DÍA, restaurar la fecha del usuario
                FechaSeleccionada = _fechaUsuario;
            }

            if (nuevaVista == TipoVistaAgenda.SemanaActual)
            {
                FechaSeleccionada = ObtenerLunes(DateTime.Today);

            }

            await CargarVista();
        }

        private async Task CargarVista()
        {
            IsLoading = true;

            try
            {
                switch (VistaActual)
                {
                    case TipoVistaAgenda.Dia:
                        await CargarVistaDia();
                        break;
                    case TipoVistaAgenda.SemanaActual:
                        await CargarVistaSemana();
                        break;
                    case TipoVistaAgenda.Mes:
                        await CargarVistaMes();
                        break;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CargarVistaDia()
        {
            System.Diagnostics.Debug.WriteLine($"📅 Cargando citas para: {FechaSeleccionada:dd/MMM/yyyy}");

            // Obtener citas del día
            var response = await _apiService.ObtenerCitasPorFechaAsync(FechaSeleccionada);

            System.Diagnostics.Debug.WriteLine($"✅ Se obtuvieron {response.Citas?.Count ?? 0} cita(s)");

            SlotsHorarios.Clear();

            foreach (var horario in _horariosDisponibles)
            {
                var fechaHoraCita = FechaSeleccionada.Date + horario;
                var horaFin = horario.Add(TimeSpan.FromMinutes(30));

                // ✅ CORRECCIÓN: Buscar citas que caen dentro de este slot (horario a horario + 30min)
                var citaEnHorario = response.Citas?.FirstOrDefault(c =>
                {
                    var horaCita = c.FechaCita.TimeOfDay;
                    // Una cita pertenece a este slot si su hora está entre horario y horario+30min
                    return horaCita >= horario && horaCita < horaFin;
                });

                bool esPasado = fechaHoraCita < DateTime.Now;
                bool tieneCita = (citaEnHorario != null);

                var slot = new SlotHorario
                {
                    FechaHora = fechaHoraCita,
                    HoraTexto = horario.ToString(@"hh\:mm"),
                    Cita = citaEnHorario,
                    EsPasado = esPasado && !tieneCita,
                    TieneCita = tieneCita
                };

                SlotsHorarios.Add(slot);
            }
        }

        private async Task CargarVistaSemana()
        {
            DiasSemana.Clear();

            var lunesSemana = ObtenerLunes(FechaSeleccionada);
            var lunesActual = ObtenerLunes(DateTime.Today);

            DateTime diaInicio;

            if (lunesSemana == lunesActual)
            {
                diaInicio = DateTime.Today;
            }
            else
            {
                diaInicio = lunesSemana;
            }

            // 🔹 Mostrar hasta sábado
            var sabado = lunesSemana.AddDays(5);

            for (var dia = diaInicio; dia <= sabado; dia = dia.AddDays(1))
            {
                var response = await _apiService.ObtenerCitasPorFechaAsync(dia);

                var diaCalendario = new DiaCalendario
                {
                    Fecha = dia,
                    NumeroDia = dia.Day,
                    NombreDia = dia.ToString("dddd"),
                    EsPasado = dia.Date < DateTime.Today,
                    EsHoy = dia.Date == DateTime.Today,
                    TieneCitas = response.TieneCitas,
                    Citas = new ObservableCollection<CitaDto>(response.Citas ?? new List<CitaDto>())
                };

                // Crear slots
                diaCalendario.Slots = new ObservableCollection<SlotHorario>();
                foreach (var horario in _horariosDisponibles)
                {
                    var fechaHoraCita = dia.Date + horario;
                    var horaFin = horario.Add(TimeSpan.FromMinutes(30));

                    var citaEnHorario = response.Citas?.FirstOrDefault(c =>
                    {
                        var horaCita = c.FechaCita.TimeOfDay;
                        return horaCita >= horario && horaCita < horaFin;
                    });

                    bool esPasado = fechaHoraCita < DateTime.Now;
                    bool tieneCita = citaEnHorario != null;

                    diaCalendario.Slots.Add(new SlotHorario
                    {
                        FechaHora = fechaHoraCita,
                        HoraTexto = horario.ToString(@"hh\:mm"),
                        Cita = citaEnHorario,
                        EsPasado = esPasado && !tieneCita,
                        TieneCita = tieneCita
                    });
                }

                DiasSemana.Add(diaCalendario);
            }
        }

        private async Task CambiarSiguiente()
        {
            if (EsVistaDia)
            {
                FechaSeleccionada = FechaSeleccionada.AddDays(1);
                await CargarVista();

            }
            else if (EsVistaSemana)
            {
                FechaSeleccionada = ObtenerLunes(FechaSeleccionada.AddDays(7));
                await CargarVistaSemana();
            }
            else
            {
                FechaSeleccionada = FechaSeleccionada.AddMonths(1);
                await CargarVistaMes();
            }
        }
        private async Task CambiarAnterior()
        {
            if (!PuedeRegresar)
                return;

            if (EsVistaDia)
                FechaSeleccionada = FechaSeleccionada.AddDays(-1);
            else if (EsVistaSemana)
                FechaSeleccionada = ObtenerLunes(FechaSeleccionada.AddDays(-7));
            else
                FechaSeleccionada = FechaSeleccionada.AddMonths(-1);

            await CargarVista();
        }

        private async Task CargarVistaMes()
        {
            DiasMes.Clear();

            var primerDiaMes = new DateTime(FechaSeleccionada.Year, FechaSeleccionada.Month, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            // Agregar días vacíos al inicio para alinear
            var primerDiaSemana = (int)primerDiaMes.DayOfWeek;
            var diasVaciosInicio = primerDiaSemana == 0 ? 6 : primerDiaSemana - 1;

            for (int i = 0; i < diasVaciosInicio; i++)
            {
                DiasMes.Add(new DiaCalendario 
                { 
                    EsVacio = true,
                    EsPasado = true
                });
            }
            // Agregar todos los días del mes
            for (var dia = primerDiaMes; dia <= ultimoDiaMes; dia = dia.AddDays(1))
            {

                DiasMes.Add(new DiaCalendario
                {
                    Fecha = dia,
                    NumeroDia = dia.Day,
                    EsPasado = dia.Date < DateTime.Today,
                    EsHoy = dia.Date == DateTime.Today,
                });
            }
        }
        private async Task DesicionDisponible(SlotHorario slot)
        {
            if (slot.EsPasado)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Horario pasado",
                    "No puedes crear citas en horarios que ya pasaron",
                    "OK");
                return;
            }

            if (slot.TieneCita)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Horario ocupado",
                    $"Este horario ya está ocupado por {slot.Cita.ClienteNombre}",
                    "OK");
                return;
            }

            if (EsCrearNueva)
            {
                await CrearNuevaCita(slot);
            }
            if (EsReagendar)
            {
                await ReagendarCita(slot);
            }
            if (EsContinuarCita)
            {
                await ContinuarCita(slot);
            }

        }
        private async Task ManejarSlotSemana(SlotHorario slot)
        {
            if (slot == null) return;

            if (slot.EsPasado)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Horario Pasado",
                    "No se pueden realizar acciones en horarios que ya pasaron",
                    "OK");
                return;
            }
            if (slot.TieneCita)
            {
                await ManejarSlotConCita(slot);
            }
            else
            {
                await ManejarSlotDisponible(slot);
            }
        }
        /// <summary>
        /// Maneja el toque en un slot que tiene una cita
        /// </summary>
        private async Task ManejarSlotConCita(SlotHorario slot)
        {
            if (slot.Cita == null) return;

            // ✅ SI ESTAMOS EN MODO REAGENDAR: Mostrar que no puede seleccionar cita ocupada
            if (EsReagendar)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Horario Ocupado",
                    $"Este horario ya está ocupado por {slot.Cita.ClienteNombre}.\n\nSeleccione un horario disponible para reagendar.",
                    "OK");
                return;
            }

            // ✅ SI NO ESTAMOS REAGENDANDO: Mostrar opciones de la cita
            var accion = await Application.Current.MainPage.DisplayActionSheet(
                $"Cita: {slot.Cita.ClienteNombre}",
                "Cancelar",
                null,
                "👁️ Ver Detalles",
                "🗑️ Cancelar Cita"
            );

            switch (accion)
            {
                case "👁️ Ver Detalles":
                    await VerDetalleCita(slot.Cita);
                    break;

                case "🗑️ Cancelar Cita":
                    await CancelarCitaConfirmacion(slot.Cita);
                    break;
            }
        }

        /// <summary>
        /// Maneja el toque en un slot disponible
        /// </summary>
        private async Task ManejarSlotDisponible(SlotHorario slot)
        {
            // ✅ MODO 1: CREAR NUEVA CITA
            if (EsCrearNueva)
            {
                await CrearNuevaCita(slot);
            }

            // ✅ MODO 2: REAGENDAR CITA EXISTENTE
            else if (EsReagendar)
            {
                await ReagendarCita(slot);
            }

            // ✅ MODO 3: CONTINUAR CREACIÓN DE CITA (con cliente/vehículo)
            else if (EsContinuarCita)
            {
                await ContinuarCita(slot);
            }

            // ✅ MODO DEFAULT: Preguntar qué hacer
            else
            {
                await CrearNuevaCita(slot);
            }
        }

        private async Task CrearNuevaCita(SlotHorario slot)
        {
            try
            {
                var accion = await Application.Current.MainPage.DisplayActionSheet(
                    "Selecciona tu tipo de Cita",
                    "Cancelar",
                    null,
                    "🔧 Servicio",
                    "🔍 Diagnóstico",
                    "🛠️ Reparación",
                    "✅ Garantía"
                );

                if (accion == "Cancelar" || string.IsNullOrEmpty(accion))
                    return;

                // Determinar el tipo de orden según la selección
                int tipoOrden = accion switch
                {
                    "🔧 Servicio" => 1,
                    "🔍 Diagnóstico" => 2,
                    "🛠️ Reparación" => 3,
                    "✅ Garantía" => 4,
                    _ => 1
                };

                // Navegar a la página de crear orden
                var crearOrdenPage = new CrearCitaPage(tipoOrden, 0, 0, slot.FechaHora);
                await Application.Current.MainPage.Navigation.PushAsync(crearOrdenPage);

                // Recargar cuando regrese
                crearOrdenPage.Disappearing += async (s, e) =>
                {
                    FechaSeleccionada = slot.FechaHora;
                    await CargarVistaDia();
                };
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al abrir selector: {ex.Message}",
                    "OK");
            }
        }
        private async Task ReagendarCita(SlotHorario slot)
        {
            try
            {
                IsLoading = true;

                var response = await _apiService.ReagendarCitaAsync(CitaId, slot.FechaHora);

                if (response != null && response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Cita reagendada",
                        "La cita fue reagendada exitosamente.",
                        "OK");

                    CitaId = 0;
                    ClienteId = 0;
                    VehiculoId = 0;

                    OnPropertyChanged(nameof(EsCrearNueva));
                    OnPropertyChanged(nameof(EsReagendar));
                    OnPropertyChanged(nameof(EsContinuarCita));

                    FechaSeleccionada = slot.FechaHora.Date;
                    await CargarVistaDia();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ Error",
                        response?.Message ?? "No se pudo reagendar la cita.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al reagendar: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ContinuarCita(SlotHorario slot)
        {


        }

        /// <summary>
        /// Cancela una cita con confirmación
        /// </summary>
        private async Task CancelarCitaConfirmacion(CitaDto cita)
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "🗑️ Cancelar Cita",
                $"¿Está seguro que desea cancelar la cita de {cita.ClienteNombre}?\n\nEsta acción no se puede deshacer.",
                "Sí, cancelar",
                "No");

            if (!confirmar) return;

            try
            {
                IsLoading = true;

                var response = await _apiService.CancelarCitaAsync(cita.Id);

                if (response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Cita Cancelada",
                        "La cita ha sido cancelada exitosamente",
                        "OK");

                    // Recargar la vista
                    await CargarVista();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        response.Message ?? "No se pudo cancelar la cita",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cancelar cita: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    "Ocurrió un error al cancelar la cita",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task VerDetalleCita(CitaDto cita)
        {
            if (EsReagendar  )
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Horario ocupado",
                    $"Elige un horario disponible para reagendar",
                    "OK");
                return;
            }
            else if(EsContinuarCita)
            {
                await Application.Current.MainPage.DisplayAlert(
                "⚠️ Horario ocupado",
                $"Elige un horario disponible para tu nueva cita",
                "OK");
                return;

            }
            else
            {
                if (cita == null) return;

                // Navegar a detalle de cita
                var detallePage = new ResumenCitaPage(cita.Id);
                await Application.Current.MainPage.Navigation.PushAsync(detallePage);

            }
        }

        private async Task SeleccionarDia(DiaCalendario dia)
        {
            if (dia.EsVacio || dia.EsPasado) return;

            FechaSeleccionada = dia.Fecha;
            VistaActual = TipoVistaAgenda.Dia;
            await CargarVista();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #region Enums y Clases Auxiliares

    public enum TipoVistaAgenda
    {
        Dia,
        SemanaActual,
        Mes
    }

    public class SlotHorario : INotifyPropertyChanged
    {
        private bool _tieneCita;
        private CitaDto _cita;

        public DateTime FechaHora { get; set; }
        public string HoraTexto { get; set; }
        public bool EsPasado { get; set; }

        public bool TieneCita
        {
            get => _tieneCita;
            set { _tieneCita = value; OnPropertyChanged(); OnPropertyChanged(nameof(Disponible)); }
        }

        public CitaDto Cita
        {
            get => _cita;
            set { _cita = value; OnPropertyChanged(); }
        }

        public bool Disponible => !TieneCita && !EsPasado;

        public string InfoCita => !EsPasado && TieneCita && Cita != null
            ? $"{Cita.ClienteNombre}\n{Cita.TipoOrden}"
    :       string.Empty;
        public string ColorFondo =>EsPasado? "#F5F5F5": (TieneCita ? "#FFEBEE" : "White"); // fondo rojo suave

        public string ColorBorde => EsPasado? "#E0E0E0": (TieneCita ? "#B00000" : "#BDBDBD"); // rojo fuerte

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DiaCalendario : INotifyPropertyChanged
    {
        public DateTime Fecha { get; set; }
        public int NumeroDia { get; set; }
        public string NombreDia { get; set; }
        public bool EsPasado { get; set; }
        public bool EsHoy { get; set; }
        public bool EsVacio { get; set; }
        public bool TieneCitas { get; set; }
        public int CantidadCitas { get; set; }
        public ObservableCollection<CitaDto> Citas { get; set; }
        public ObservableCollection<SlotHorario> Slots { get; set; }

        public string ColorFondo => EsPasado ? "#F5F5F5" : (EsHoy ? "#FFEBEE" : "White");
        public string ColorTexto => EsPasado ? "#BDBDBD" : (EsHoy ? "#B00000" : "Black");
        public bool MostrarTachado => EsPasado && !EsVacio;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion
}