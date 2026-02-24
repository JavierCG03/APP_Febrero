using System.ComponentModel.DataAnnotations;

namespace CarslineApp.Models
{
    /// <summary>
    /// Request para crear una cita con trabajos
    /// </summary>
    public class CrearCitaConTrabajosRequest
    {
        [Required]
        public int TipoOrdenId { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int VehiculoId { get; set; }

        [Required]
        public DateTime FechaCita { get; set; }

        public int? TipoServicioId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un trabajo")]
        public List<TrabajoCrearDto> Trabajos { get; set; } = new();
    }

    /// <summary>
    /// Response al crear una cita
    /// </summary>
    public class CrearCitaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CitaId { get; set; }
        public DateTime FechaCita { get; set; }
        public int TotalTrabajos { get; set; }
    }

    /// <summary>
    /// DTO de cita simplificado para listado
    /// </summary>
    public class CitaDto
    {
        public int Id { get; set; }
        public DateTime FechaCita { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public string VehiculoInfo { get; set; } = string.Empty;
        public string TipoOrden { get; set; } = string.Empty;
    }


    /// <summary>
    /// DTO de trabajo asociado a una cita
    /// </summary>
    public class TrabajoCitaDto
    {
        public int Id { get; set; }
        public string Trabajo { get; set; } = string.Empty;
        public string? IndicacionesTrabajo { get; set; }
        public bool RefaccionesListas { get; set; }  

        // Propiedades calculadas
        public bool TieneIndicaciones => !string.IsNullOrWhiteSpace(IndicacionesTrabajo);
    }

    /// <summary>
    /// Response para obtener citas por fecha
    /// </summary>
    public class ObtenerCitasPorFechaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int TotalCitas { get; set; }
        public List<CitaDto> Citas { get; set; } = new();

        // Propiedades calculadas
        public bool TieneCitas => Citas.Any();
        public string FechaFormateada => Fecha.ToString("dd/MMM/yyyy");
    }

    /// <summary>
    /// Response para obtener detalle de una cita
    /// </summary>
    public class ObtenerCitaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CitaDetalleDto? Cita { get; set; }
    }

    /// <summary>
    /// Response genérico para operaciones de citas
    /// </summary>
    public class CitaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    /// <summary>
    /// DTO para la respuesta del endpoint de detalle de cita
    /// </summary>
    public class CitaDetalleDto
    {
        public int ClienteId { get; set; }
        public int VehiculoId { get; set; }
        public int TipoOrdenId { get; set; }
        public int TipoServicioId { get; set; }
        public int Id { get; set; }
        public DateTime FechaCita { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public string DireccionCliente { get; set; } = string.Empty;
        public string RfcCliente { get; set; } = string.Empty;
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string VinVehiculo { get; set; } = string.Empty;
        public string PlacasVehiculo { get; set; } = string.Empty;
        public string TipoOrdenNombre { get; set; } = string.Empty;
        public string TipoServicioNombre { get; set; } = string.Empty;
        public List<TrabajoDetalleDto> Trabajos { get; set; } = new List<TrabajoDetalleDto>();
    }

    /// <summary>
    /// DTO para trabajo dentro del detalle de cita
    /// </summary>
    public class TrabajoDetalleDto
    {
        public int Id { get; set; }
        public string Trabajo { get; set; } = string.Empty;
        public string IndicacionesTrabajo { get; set; } = string.Empty;
        public bool TieneIndicaciones => !string.IsNullOrWhiteSpace(IndicacionesTrabajo);

    }
    /// <summary>
    /// Request para reagendar una cita
    /// </summary>
    public class ReagendarCitaRequest
    {
        [Required(ErrorMessage = "La nueva fecha es requerida")]
        public DateTime NuevaFechaCita { get; set; }
    }

    /// <summary>
    /// Response al reagendar una cita
    /// </summary>
    public class ReagendarCitaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CitaId { get; set; }
        public DateTime FechaAnterior { get; set; }
        public DateTime FechaNueva { get; set; }
    }

    // Nuevo DTO — agrégalo al final del archivo
    public class CitaConTrabajosDto
    {
        public int Id { get; set; }
        public int TipoOrdenId { get; set; }
        public int VehiculoId { get; set; }
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public DateTime FechaCita { get; set; }
        public List<TrabajoCitaDto> Trabajos { get; set; } = new();

        // Propiedades calculadas
        public string FechaFormateada => FechaCita.ToString("dd/MMM/yyyy");
        public string HoraFormateada => FechaCita.ToString("hh:mm tt");
        public int TotalTrabajos => Trabajos.Count;
    }
}