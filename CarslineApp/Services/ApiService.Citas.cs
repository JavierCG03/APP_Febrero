using CarslineApp.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        /// <summary>
        /// Crear una cita con trabajos asociados
        /// </summary>
        public async Task<CrearCitaResponse> CrearCitaConTrabajosAsync(
            CrearCitaConTrabajosRequest request,
            int encargadoCitasId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                    $"{BaseUrl}/Citas/crear-con-trabajos")
                {
                    Content = JsonContent.Create(request)
                };
                httpRequest.Headers.Add("X-User-Id", encargadoCitasId.ToString());

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CrearCitaResponse>();
                    return result ?? new CrearCitaResponse
                    {
                        Success = false,
                        Message = "Error al procesar respuesta"
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Error al crear cita: {errorContent}");

                return new CrearCitaResponse
                {
                    Success = false,
                    Message = $"Error en la solicitud: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Excepción al crear cita: {ex.Message}");
                return new CrearCitaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Obtener citas por fecha específica
        /// </summary>
        public async Task<ObtenerCitasPorFechaResponse> ObtenerCitasPorFechaAsync(DateTime? fecha = null)
        {
            try
            {
                var fechaConsulta = fecha ?? DateTime.Today;
                var url = $"{BaseUrl}/Citas/por-fecha?fecha={fechaConsulta:yyyy-MM-dd}";

                System.Diagnostics.Debug.WriteLine($"📅 Consultando citas para: {fechaConsulta:dd/MMM/yyyy}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    // Deserializar la respuesta completa
                    var apiResponse = JsonSerializer.Deserialize<JsonElement>(json);

                    var citasList = new List<CitaDto>();

                    if (apiResponse.TryGetProperty("citas", out var citasElement))
                    {
                        citasList = JsonSerializer.Deserialize<List<CitaDto>>(
                            citasElement.GetRawText(),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        ) ?? new List<CitaDto>();
                    }

                    var totalCitas = apiResponse.TryGetProperty("totalCitas", out var total)
                        ? total.GetInt32()
                        : citasList.Count;

                    System.Diagnostics.Debug.WriteLine($"✅ Se obtuvieron {totalCitas} cita(s)");

                    return new ObtenerCitasPorFechaResponse
                    {
                        Success = true,
                        Message = totalCitas > 0 ? $"Se encontraron {totalCitas} cita(s)" : "No hay citas para esta fecha",
                        Fecha = fechaConsulta,
                        TotalCitas = totalCitas,
                        Citas = citasList
                    };
                }

                return new ObtenerCitasPorFechaResponse
                {
                    Success = false,
                    Message = "Error al obtener citas",
                    Fecha = fechaConsulta,
                    Citas = new List<CitaDto>()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener citas: {ex.Message}");
                return new ObtenerCitasPorFechaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Fecha = fecha ?? DateTime.Today,
                    Citas = new List<CitaDto>()
                };
            }
        }

        /// <summary>
        /// Obtener detalle completo de una cita con sus trabajos
        /// </summary>
        public async Task<ObtenerCitaResponse> ObtenerDetalleCitaAsync(int citaId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📋 Obteniendo detalle de cita {citaId}");

                var response = await _httpClient.GetAsync($"{BaseUrl}/Citas/{citaId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<JsonElement>(json);

                    if (apiResponse.TryGetProperty("cita", out var citaElement))
                    {
                        var cita = JsonSerializer.Deserialize<CitaDetalleDto>(
                            citaElement.GetRawText(),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        System.Diagnostics.Debug.WriteLine($"✅ Detalle de cita obtenido exitosamente");

                        return new ObtenerCitaResponse
                        {
                            Success = true,
                            Message = "Cita encontrada",
                            Cita = cita
                        };
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ObtenerCitaResponse
                    {
                        Success = false,
                        Message = "Cita no encontrada"
                    };
                }

                return new ObtenerCitaResponse
                {
                    Success = false,
                    Message = "Error al obtener detalle de cita"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener detalle: {ex.Message}");
                return new ObtenerCitaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Cancelar una cita
        /// </summary>
        public async Task<CitaResponse> CancelarCitaAsync(int citaId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🗑️ Cancelando cita {citaId}");

                var response = await _httpClient.PutAsync(
                    $"{BaseUrl}/Citas/cancelar/{citaId}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CitaResponse>();

                    System.Diagnostics.Debug.WriteLine($"✅ Cita cancelada exitosamente");

                    return result ?? new CitaResponse
                    {
                        Success = false,
                        Message = "Error al procesar respuesta"
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();

                return new CitaResponse
                {
                    Success = false,
                    Message = $"Error al cancelar: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cancelar cita: {ex.Message}");
                return new CitaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        /// <summary>
        /// Reagendar una cita existente a una nueva fecha
        /// </summary>
        public async Task<ReagendarCitaResponse> ReagendarCitaAsync(int citaId, DateTime nuevaFecha)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Reagendando cita {citaId} a {nuevaFecha:dd/MMM/yyyy HH:mm}");

                // Validación en el cliente antes de enviar
                if (nuevaFecha <= DateTime.Now)
                {
                    return new ReagendarCitaResponse
                    {
                        Success = false,
                        Message = "La nueva fecha debe ser en el futuro"
                    };
                }

                var request = new ReagendarCitaRequest
                {
                    NuevaFechaCita = nuevaFecha
                };

                var response = await _httpClient.PutAsJsonAsync(
                    $"{BaseUrl}/Citas/reagendar/{citaId}",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ReagendarCitaResponse>();

                    System.Diagnostics.Debug.WriteLine($"✅ Cita reagendada exitosamente");

                    return result ?? new ReagendarCitaResponse
                    {
                        Success = false,
                        Message = "Error al procesar respuesta"
                    };
                }

                // Manejar errores específicos
                var errorJson = await response.Content.ReadAsStringAsync();

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ReagendarCitaResponse>(errorJson);
                    if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Error al reagendar: {errorResponse.Message}");
                        return errorResponse;
                    }
                }
                catch
                {
                    // Si no se puede deserializar, usar mensaje genérico
                }

                return new ReagendarCitaResponse
                {
                    Success = false,
                    Message = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.NotFound => "Cita no encontrada",
                        System.Net.HttpStatusCode.BadRequest => "Fecha inválida o horario ocupado",
                        _ => $"Error al reagendar: {response.StatusCode}"
                    }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Excepción al reagendar cita: {ex.Message}");
                return new ReagendarCitaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Obtener citas de hoy
        /// </summary>
        public async Task<ObtenerCitasPorFechaResponse> ObtenerCitasHoyAsync()
        {
            return await ObtenerCitasPorFechaAsync(DateTime.Today);
        }

        /// <summary>
        /// Obtener citas de mañana
        /// </summary>
        public async Task<ObtenerCitasPorFechaResponse> ObtenerCitasMañanaAsync()
        {
            return await ObtenerCitasPorFechaAsync(DateTime.Today.AddDays(1));
        }

        /// <summary>
        /// Verificar si hay citas para una fecha específica
        /// </summary>
        public async Task<bool> TieneCitasEnFechaAsync(DateTime fecha)
        {
            try
            {
                var response = await ObtenerCitasPorFechaAsync(fecha);
                return response.Success && response.TieneCitas;
            }
            catch
            {
                return false;
            }
        }
    }
}