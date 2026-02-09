using CarslineApp.Models;
using System.Diagnostics;
using System.Net.Http.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        /// <summary>
        /// Obtener recordatorios por tipo (1=Primero, 2=Segundo, 3=Tercero)
        /// Versión simplificada para la lista
        /// </summary>
        public async Task<ObtenerRecordatoriosResponse> ObtenerRecordatoriosPorTipoAsync(int tipoRecordatorio)
        {
            try
            {
                if (tipoRecordatorio < 1 || tipoRecordatorio > 3)
                {
                    return new ObtenerRecordatoriosResponse
                    {
                        Success = false,
                        Message = "Tipo de recordatorio inválido"
                    };
                }

                Debug.WriteLine($"📞 Obteniendo recordatorios tipo {tipoRecordatorio}...");

                var response = await _httpClient.GetAsync($"{BaseUrl}/Recordatorios/{tipoRecordatorio}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ObtenerRecordatoriosResponse>();
                    Debug.WriteLine($"✅ Recordatorios obtenidos: {result?.Recordatorios?.Count ?? 0}");
                    return result ?? new ObtenerRecordatoriosResponse { Success = false };
                }

                Debug.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                return new ObtenerRecordatoriosResponse
                {
                    Success = false,
                    Message = $"Error al obtener recordatorios: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Excepción en ObtenerRecordatoriosPorTipoAsync: {ex.Message}");
                return new ObtenerRecordatoriosResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Obtener el detalle completo de un recordatorio específico
        /// </summary>
        public async Task<ObtenerRecordatorioResponse> ObtenerRecordatorioDetalleAsync(int proximoServicioId)
        {
            try
            {
                Debug.WriteLine($"📞 Obteniendo detalle del recordatorio ID: {proximoServicioId}...");

                var response = await _httpClient.GetAsync($"{BaseUrl}/Recordatorios/detalle/{proximoServicioId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ObtenerRecordatorioResponse>();
                    Debug.WriteLine($"✅ Detalle del recordatorio obtenido");
                    return result ?? new ObtenerRecordatorioResponse { Success = false };
                }

                Debug.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                return new ObtenerRecordatorioResponse
                {
                    Success = false,
                    Message = $"Error al obtener detalle: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Excepción en ObtenerRecordatorioDetalleAsync: {ex.Message}");
                return new ObtenerRecordatorioResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Obtener resumen de todos los recordatorios pendientes
        /// </summary>
        public async Task<ResumenRecordatoriosResponse> ObtenerResumenRecordatoriosAsync()
        {
            try
            {
                Debug.WriteLine("📞 Obteniendo resumen de recordatorios...");

                var response = await _httpClient.GetAsync($"{BaseUrl}/Recordatorios/resumen");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResumenRecordatoriosResponse>();
                    Debug.WriteLine($"✅ Resumen obtenido: {result?.TotalPendientes ?? 0} pendientes");
                    return result ?? new ResumenRecordatoriosResponse { Success = false };
                }

                Debug.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                return new ResumenRecordatoriosResponse
                {
                    Success = false,
                    TotalPendientes = 0
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Excepción en ObtenerResumenRecordatoriosAsync: {ex.Message}");
                return new ResumenRecordatoriosResponse
                {
                    Success = false,
                    TotalPendientes = 0
                };
            }
        }

        /// <summary>
        /// Marcar recordatorio como enviado
        /// </summary>
        public async Task<RecordatorioResponse> MarcarRecordatorioEnviadoAsync(
            int proximoServicioId,
            int tipoRecordatorio)
        {
            try
            {
                Debug.WriteLine($"📞 Marcando recordatorio {tipoRecordatorio} como enviado (ID: {proximoServicioId})...");

                var request = new MarcarRecordatorioRequest
                {
                    ProximoServicioId = proximoServicioId,
                    TipoRecordatorio = tipoRecordatorio
                };

                var response = await _httpClient.PutAsJsonAsync(
                    $"{BaseUrl}/Recordatorios/marcar-enviado",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RecordatorioResponse>();
                    Debug.WriteLine($"✅ Recordatorio marcado: {result?.Message}");
                    return result ?? new RecordatorioResponse { Success = true };
                }

                Debug.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                return new RecordatorioResponse
                {
                    Success = false,
                    Message = $"Error: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Excepción en MarcarRecordatorioEnviadoAsync: {ex.Message}");
                return new RecordatorioResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}