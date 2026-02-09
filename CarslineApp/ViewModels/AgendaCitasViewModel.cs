// ViewModels/AgendaCitasViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.ViewModels
{
    public class AgendaCitasViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private DateTime _fechaSeleccionada;
        private TipoVistaAgenda _vistaActual;
        private bool _isLoading;
        private ObservableCollection<SlotHorario> _slotsHorarios;
        private ObservableCollection<DiaCalendario> _diasSemana;
        private ObservableCollection<DiaCalendario> _diasMes;

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

        public AgendaCitasViewModel()
        {
            _apiService = new ApiService();
            _fechaSeleccionada = DateTime.Today;
            _vistaActual = TipoVistaAgenda.Dia;
            _slotsHorarios = new ObservableCollection<SlotHorario>();
            _diasSemana = new ObservableCollection<DiaCalendario>();
            _diasMes = new ObservableCollection<DiaCalendario>();

            // Comandos
            SiguienteCommand = new Command(async () => await CambiarSiguiente());
            AnteriorCommand = new Command(async () => await CambiarAnterior());
            CambiarVistaDiaCommand = new Command(async () => await CambiarVista(TipoVistaAgenda.Dia));
            CambiarVistaSemanaActualCommand = new Command(async () => await CambiarVista(TipoVistaAgenda.SemanaActual));
            CambiarVistaMesCommand = new Command(async () => await CambiarVista(TipoVistaAgenda.Mes));
            CrearCitaCommand = new Command<SlotHorario>(async (slot) => await CrearCita(slot));
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
            }
        }

        public string FechaSeleccionadaTexto => FechaSeleccionada.ToString("dddd, dd 'de' MMMM yyyy");

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
            }
        }

        public string TituloVista => VistaActual switch
        {
            TipoVistaAgenda.Dia => FechaSeleccionada.ToString("dddd dd MMM"),
            TipoVistaAgenda.SemanaActual => "Semana",
            TipoVistaAgenda.Mes => FechaSeleccionada.ToString("MMMM yyyy"),
            _ => ""
        };

        public bool EsVistaDia => VistaActual == TipoVistaAgenda.Dia;
        public bool EsVistaSemana => VistaActual == TipoVistaAgenda.SemanaActual || VistaActual == TipoVistaAgenda.SemanaSiguiente;
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


        #endregion

        #region Métodos Públicos

        public async Task InicializarAsync()
        {
            await CargarVista();
        }

        #endregion

        #region Métodos Privados

        private async Task CambiarVista(TipoVistaAgenda nuevaVista)
        {
            VistaActual = nuevaVista;

            if (nuevaVista == TipoVistaAgenda.SemanaSiguiente)
            {
                // Calcular lunes de la próxima semana
                var diasHastaLunes = ((int)DayOfWeek.Monday - (int)DateTime.Today.DayOfWeek + 7) % 7;
                if (diasHastaLunes == 0) diasHastaLunes = 7;
                FechaSeleccionada = DateTime.Today.AddDays(diasHastaLunes);
            }
            else if (nuevaVista == TipoVistaAgenda.SemanaActual)
            {
                // Calcular lunes de esta semana
                var diasDesdeeLunes = ((int)DateTime.Today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                FechaSeleccionada = DateTime.Today.AddDays(-diasDesdeeLunes);
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
                    case TipoVistaAgenda.SemanaSiguiente:
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

                var slot = new SlotHorario
                {
                    FechaHora = fechaHoraCita,
                    HoraTexto = horario.ToString(@"hh\:mm"),
                    TieneCita = citaEnHorario != null,
                    Cita = citaEnHorario,
                    EsPasado = fechaHoraCita < DateTime.Now
                };

                SlotsHorarios.Add(slot);

                // Debug para verificar
                if (citaEnHorario != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔴 Slot {slot.HoraTexto} OCUPADO por: {citaEnHorario.ClienteNombre} ({citaEnHorario.FechaCita:HH:mm})");
                }
            }

            System.Diagnostics.Debug.WriteLine($"✅ Slots creados: {SlotsHorarios.Count}, Ocupados: {SlotsHorarios.Count(s => s.TieneCita)}");
        }

        private async Task CargarVistaSemana()
        {
            DiasSemana.Clear();

            // Calcular lunes de la semana
            var diasDesdeLunes = ((int)FechaSeleccionada.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var lunesSemana = FechaSeleccionada.AddDays(-diasDesdeLunes);

            // Crear días de lunes a sábado
            for (int i = 0; i < 6; i++)
            {
                var dia = lunesSemana.AddDays(i);
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

                // Crear slots de horario para cada día
                diaCalendario.Slots = new ObservableCollection<SlotHorario>();
                foreach (var horario in _horariosDisponibles)
                {
                    var fechaHoraCita = dia.Date + horario;
                    var horaFin = horario.Add(TimeSpan.FromMinutes(30));

                    // ✅ CORRECCIÓN: Buscar citas que caen dentro de este slot
                    var citaEnHorario = response.Citas?.FirstOrDefault(c =>
                    {
                        var horaCita = c.FechaCita.TimeOfDay;
                        return horaCita >= horario && horaCita < horaFin;
                    });

                    diaCalendario.Slots.Add(new SlotHorario
                    {
                        FechaHora = fechaHoraCita,
                        HoraTexto = horario.ToString(@"hh\:mm"),
                        TieneCita = citaEnHorario != null,
                        Cita = citaEnHorario,
                        EsPasado = fechaHoraCita < DateTime.Now
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
                FechaSeleccionada = FechaSeleccionada.AddDays(7);
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
            if (EsVistaDia)
            {
                FechaSeleccionada = FechaSeleccionada.AddDays(-1);
                await CargarVista();

            }
            else if (EsVistaSemana)
            {
                FechaSeleccionada = FechaSeleccionada.AddDays(-7);
                await CargarVistaSemana();
            }
            else
            {
                FechaSeleccionada = FechaSeleccionada.AddMonths(-1);
                await CargarVistaMes();
            }

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
                DiasMes.Add(new DiaCalendario { EsVacio = true });
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

        private async Task CrearCita(SlotHorario slot)
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

            // Navegar a página de crear cita con el horario preseleccionado
            //var crearCitaPage = new CrearCitaPage(slot.FechaHora);
            //await Application.Current.MainPage.Navigation.PushAsync(crearCitaPage);
        }

        private async Task VerDetalleCita(CitaDto cita)
        {
            if (cita == null) return;

            // Navegar a detalle de cita
            // var detallePage = new DetalleCitaPage(cita.Id);
            //await Application.Current.MainPage.Navigation.PushAsync(detallePage);
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
        SemanaSiguiente,
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

        // ✅ NUEVA PROPIEDAD: Información a mostrar en el slot ocupado
        public string InfoCita => TieneCita && Cita != null
            ? $"{Cita.ClienteNombre}\n{Cita.TipoOrden}"
            : string.Empty;

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

        public string ColorFondo => EsVacio ? "Transparent" : (EsPasado ? "#F5F5F5" : (EsHoy ? "#FFEBEE" : "White"));
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