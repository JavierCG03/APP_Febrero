using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CarslineApp.Services
{
    /// <summary>
    /// Servicio para decodificar VINs usando la API gratuita de NHTSA (gobierno de EE.UU.)
    /// No requiere registro ni API key.
    /// Funciona con la mayoría de vehículos vendidos en México (Nissan, Chevrolet,
    /// VW, Toyota, Honda, Ford, etc.) porque comparten catálogo con EE.UU.
    /// </summary>
    public class VinDecoderService
    {
        private const string BaseUrl =
            "https://vpic.nhtsa.dot.gov/api/vehicles/DecodeVinValues/{0}?format=json";

        private static readonly HttpClient _http = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        // ──────────────────────────────────────────────────────────────────────
        //  Método principal
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Decodifica un VIN de 17 caracteres.
        /// Retorna null si el VIN no pudo decodificarse o hubo error de red.
        /// </summary>
        public async Task<VinDecodedResult?> DecodificarVinAsync(string vin)
        {
            if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
                return null;

            try
            {
                var url = string.Format(BaseUrl, vin.Trim().ToUpper());
                var response = await _http.GetFromJsonAsync<NhtsaResponse>(url);

                if (response?.Results == null || response.Results.Length == 0)
                    return null;

                var r = response.Results[0];

                // Verificar que el VIN se decodificó sin errores críticos
                // ErrorCode "0" = limpio, "6" = VIN parcial decodificado
                if (r.ErrorCode != "0" && r.ErrorCode != "6" &&
                    !string.IsNullOrEmpty(r.ErrorCode))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[VIN] Error NHTSA: {r.ErrorCode} - {r.ErrorText}");
                }

                // Construir el resultado limpio
                var resultado = new VinDecodedResult
                {
                    VIN = vin.ToUpper(),
                    Marca = LimpiarTexto(r.Make),
                    Modelo = LimpiarTexto(r.Model),
                    Anio = ParseAnio(r.ModelYear),
                    TipoVehiculo = LimpiarTexto(r.VehicleType),
                    NumCilindros = LimpiarTexto(r.EngineCylinders),
                    Displacement = LimpiarTexto(r.DisplacementL),
                    TipoCombust = LimpiarTexto(r.FuelTypePrimary),
                    Transmision = LimpiarTexto(r.TransmissionStyle),
                    TraccionTipo = LimpiarTexto(r.DriveType),
                    NumPuertas = LimpiarTexto(r.Doors),
                    PaisOrigen = LimpiarTexto(r.PlantCountry),
                    ErrorTexto = r.ErrorCode == "0" ? null : r.ErrorText,
                    DecodificadoCorrectamente = r.ErrorCode == "0"
                };

                System.Diagnostics.Debug.WriteLine(
                    $"[VIN] Decodificado: {resultado.Anio} {resultado.Marca} {resultado.Modelo}");

                return resultado;
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("[VIN] Timeout al decodificar VIN");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VIN] Error: {ex.Message}");
                return null;
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Helpers
        // ──────────────────────────────────────────────────────────────────────

        private static string LimpiarTexto(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();

        private static int ParseAnio(string? texto)
        {
            if (int.TryParse(texto, out int anio) && anio > 1980)
                return anio;
            return DateTime.Now.Year;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Modelos de respuesta NHTSA (solo campos que nos interesan)
        // ──────────────────────────────────────────────────────────────────────

        private class NhtsaResponse
        {
            [JsonPropertyName("Results")]
            public NhtsaVehicle[]? Results { get; set; }
        }

        private class NhtsaVehicle
        {
            [JsonPropertyName("ErrorCode")]
            public string? ErrorCode { get; set; }

            [JsonPropertyName("ErrorText")]
            public string? ErrorText { get; set; }

            [JsonPropertyName("Make")]
            public string? Make { get; set; }

            [JsonPropertyName("Model")]
            public string? Model { get; set; }

            [JsonPropertyName("ModelYear")]
            public string? ModelYear { get; set; }

            [JsonPropertyName("VehicleType")]
            public string? VehicleType { get; set; }

            [JsonPropertyName("EngineCylinders")]
            public string? EngineCylinders { get; set; }

            [JsonPropertyName("DisplacementL")]
            public string? DisplacementL { get; set; }

            [JsonPropertyName("FuelTypePrimary")]
            public string? FuelTypePrimary { get; set; }

            [JsonPropertyName("TransmissionStyle")]
            public string? TransmissionStyle { get; set; }

            [JsonPropertyName("DriveType")]
            public string? DriveType { get; set; }

            [JsonPropertyName("Doors")]
            public string? Doors { get; set; }

            [JsonPropertyName("PlantCountry")]
            public string? PlantCountry { get; set; }
        }
    }

    public class VinDecodedResult
    {
        public string VIN { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string TipoVehiculo { get; set; } = string.Empty;
        public string NumCilindros { get; set; } = string.Empty;
        public string Displacement { get; set; } = string.Empty;   // en litros
        public string TipoCombust { get; set; } = string.Empty;
        public string Transmision { get; set; } = string.Empty;
        public string TraccionTipo { get; set; } = string.Empty;
        public string NumPuertas { get; set; } = string.Empty;
        public string PaisOrigen { get; set; } = string.Empty;
        public string? ErrorTexto { get; set; }
        public bool DecodificadoCorrectamente { get; set; }

        /// <summary>Resumen para mostrar en la UI antes de confirmar.</summary>
        public string ResumenVehiculo =>
            $"{Anio} {Marca} {Modelo}".Trim();
    }
}