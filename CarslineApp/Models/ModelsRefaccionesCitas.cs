using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CarslineApp.Models
{
    // ============================================
    // MODELOS DE REFACCIONES POR CITA
    // ============================================

    /// <summary>
    /// DTO para una refacción individual de un trabajo de cita
    /// </summary>
    public class RefaccionCitaDto
    {
        public int Id { get; set; }
        public int TrabajoCitaId { get; set; }
        public string Refaccion { get; set; } = string.Empty;
        public int Cantidad { get; set; }

        /// <summary>Precio de costo / compra</summary>
        public decimal Precio { get; set; }

        /// <summary>Precio de venta al cliente (puede ser null si aún no se define)</summary>
        public decimal? PrecioVenta { get; set; }

        /// <summary>Total calculado con precio de costo</summary>
        public decimal TotalCosto => Cantidad * Precio;

        /// <summary>Total calculado con precio de venta (si está definido)</summary>
        public decimal? TotalVenta => PrecioVenta.HasValue ? Cantidad * PrecioVenta.Value : null;

        public DateTime FechaCompra { get; set; }

        /// <summary>false = pendiente de transferir a orden, true = ya fue pasada a una orden</summary>
        public bool Transferida { get; set; }

        /// <summary>ID del trabajo de orden al que fue transferida (si aplica)</summary>
        public int? TrabajoOrdenId { get; set; }

        // Propiedades calculadas para UI
        public string PrecioFormateado => $"${Precio:N2}";
        public string PrecioVentaFormateado => PrecioVenta.HasValue ? $"${PrecioVenta.Value:N2}" : "Sin precio venta";
        public string TotalCostoFormateado => $"${TotalCosto:N2}";
        public string TotalVentaFormateado => TotalVenta.HasValue ? $"${TotalVenta.Value:N2}" : "-";
        public string CantidadTexto => $"{Cantidad}";
        public string EstadoTransferencia => Transferida ? "Transferida" : "Pendiente";
        public string ColorEstado => Transferida ? "#43A047" : "#FB8C00";
    }

    /// <summary>
    /// DTO para agregar una refacción a un trabajo de cita (con INotifyPropertyChanged para binding)
    /// </summary>
    public class AgregarRefaccionCitaDto : INotifyPropertyChanged
    {
        private string _refaccion = string.Empty;
        private int _cantidad = 1;
        private decimal _precio = 0;
        private decimal? _precioVenta = null;

        public string Refaccion
        {
            get => _refaccion;
            set { _refaccion = value; OnPropertyChanged(); }
        }

        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalCalculado));
                OnPropertyChanged(nameof(TotalVentaCalculado));
            }
        }

        public decimal Precio
        {
            get => _precio;
            set
            {
                _precio = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalCalculado));
            }
        }

        public decimal? PrecioVenta
        {
            get => _precioVenta;
            set
            {
                _precioVenta = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalVentaCalculado));
            }
        }

        // Propiedades calculadas
        public decimal TotalCalculado => Cantidad * Precio;
        public decimal? TotalVentaCalculado => PrecioVenta.HasValue ? Cantidad * PrecioVenta.Value : null;
        public string TotalFormateado => $"${TotalCalculado:N2}";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Request para agregar múltiples refacciones a un trabajo de cita
    /// </summary>
    public class AgregarRefaccionesCitaRequest
    {
        public int TrabajoCitaId { get; set; }
        public List<AgregarRefaccionCitaDto> Refacciones { get; set; } = new();

        // Validación cliente
        public bool EsValido =>
            TrabajoCitaId > 0 &&
            Refacciones.Any() &&
            Refacciones.All(r =>
                !string.IsNullOrWhiteSpace(r.Refaccion) &&
                r.Cantidad > 0 &&
                r.Precio > 0);
    }

    /// <summary>
    /// Request para actualizar el precio de venta de una refacción de cita
    /// </summary>
    public class ActualizarPrecioVentaRefaccionCitaRequest
    {
        public decimal PrecioVenta { get; set; }
    }

    /// <summary>
    /// Respuesta al agregar refacciones a un trabajo de cita
    /// </summary>
    public class AgregarRefaccionesCitaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<RefaccionCitaDto> RefaccionesAgregadas { get; set; } = new();
        public int CantidadRefacciones { get; set; }
        public decimal TotalCosto { get; set; }

        // Propiedades calculadas
        public string TotalCostoFormateado => $"${TotalCosto:N2}";
        public bool TieneRefacciones => RefaccionesAgregadas.Any();
    }

    /// <summary>
    /// Respuesta al obtener refacciones de un trabajo de cita
    /// </summary>
    public class ObtenerRefaccionesCitaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TrabajoCitaId { get; set; }
        public string TrabajoCitaNombre { get; set; } = string.Empty;
        public List<RefaccionCitaDto> Refacciones { get; set; } = new();
        public decimal TotalCosto { get; set; }
        public decimal? TotalVenta { get; set; }
        public bool RefaccionesListas { get; set; }

        // Propiedades calculadas
        public string TotalCostoFormateado => $"${TotalCosto:N2}";
        public string TotalVentaFormateado => TotalVenta.HasValue ? $"${TotalVenta.Value:N2}" : "Sin precio venta";
        public bool TieneRefacciones => Refacciones.Any();
        public int CantidadRefacciones => Refacciones.Count;
    }

    /// <summary>
    /// Respuesta genérica para operaciones de refacciones de cita (eliminar, actualizar precio)
    /// </summary>
    public class RefaccionCitaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para trabajos de cita con sus refacciones (usado en el GET por citaId)
    /// </summary>
    public class TrabajoCitaConRefaccionesDto
    {
        public int Id { get; set; }
        public string Trabajo { get; set; } = string.Empty;
        public bool RefaccionesListas { get; set; }
        public List<RefaccionCitaDto> Refacciones { get; set; } = new();

        // Propiedades calculadas
        public decimal TotalCosto => Refacciones.Sum(r => r.TotalCosto);
        public decimal? TotalVenta => Refacciones.All(r => r.TotalVenta.HasValue)
            ? Refacciones.Sum(r => r.TotalVenta ?? 0)
            : null;
        public string TotalCostoFormateado => $"${TotalCosto:N2}";
        public bool TieneRefacciones => Refacciones.Any();
        public int CantidadRefacciones => Refacciones.Count;
    }

    /// <summary>
    /// Respuesta al obtener todas las refacciones de una cita completa
    /// </summary>
    public class ObtenerRefaccionesPorCitaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CitaId { get; set; }
        public List<TrabajoCitaConRefaccionesDto> Trabajos { get; set; } = new();

        // Propiedades calculadas
        public decimal TotalCostoGeneral => Trabajos.Sum(t => t.TotalCosto);
        public string TotalCostoFormateado => $"${TotalCostoGeneral:N2}";
        public bool TieneRefacciones => Trabajos.Any(t => t.TieneRefacciones);
    }

    /// <summary>
    /// ViewModel para gestionar refacciones de cita en una vista
    /// </summary>
    public class RefaccionCitaViewModel : INotifyPropertyChanged
    {
        private RefaccionCitaDto _refaccion;
        private bool _seleccionada;

        public RefaccionCitaViewModel(RefaccionCitaDto refaccion)
        {
            _refaccion = refaccion;
        }

        public RefaccionCitaDto Refaccion
        {
            get => _refaccion;
            set { _refaccion = value; OnPropertyChanged(); }
        }

        public bool Seleccionada
        {
            get => _seleccionada;
            set { _seleccionada = value; OnPropertyChanged(); }
        }

        // Accesos rápidos
        public int Id => Refaccion.Id;
        public string Nombre => Refaccion.Refaccion;
        public string CantidadTexto => Refaccion.CantidadTexto;
        public string PrecioFormateado => Refaccion.PrecioFormateado;
        public string PrecioVentaFormateado => Refaccion.PrecioVentaFormateado;
        public string TotalCostoFormateado => Refaccion.TotalCostoFormateado;
        public bool Transferida => Refaccion.Transferida;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}