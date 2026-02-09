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
        public string TipoServicio { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }

        // Propiedades calculadas
        public string FechaFormateada => FechaCita.ToString("dd/MMM/yyyy");
        public string HoraFormateada => FechaCita.ToString("hh:mm tt");
        public string FechaHoraCompleta => $"{FechaFormateada} - {HoraFormateada}";
    }

    /// <summary>
    /// DTO de cita con detalle completo incluyendo trabajos
    /// </summary>
    public class CitaDetalleDto
    {
        public int Id { get; set; }
        public DateTime FechaCita { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public string VehiculoInfo { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public string TipoServicio { get; set; } = string.Empty;
        public string EncargadoNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public List<TrabajoCitaDto> Trabajos { get; set; } = new();

        // Propiedades calculadas
        public string FechaFormateada => FechaCita.ToString("dd/MMM/yyyy");
        public string HoraFormateada => FechaCita.ToString("hh:mm tt");
        public bool TieneTrabajos => Trabajos.Any();
        public int CantidadTrabajos => Trabajos.Count;
    }

    /// <summary>
    /// DTO de trabajo asociado a una cita
    /// </summary>
    public class TrabajoCitaDto
    {
        public int Id { get; set; }
        public string Trabajo { get; set; } = string.Empty;
        public string? IndicacionesTrabajo { get; set; }

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
}