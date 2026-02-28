using CarslineApp.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {

        public async Task<AgregarRefaccionesCitaResponse> AgregarRefaccionesCitaAsync(AgregarRefaccionesCitaRequest request)
        {
            try
            {
                Debug.WriteLine($"📤 Agregando {request.Refacciones.Count} refacciones al trabajo de cita {request.TrabajoId}");

                var response = await _httpClient.PostAsJsonAsync(
                    $"{BaseUrl}/RefaccionesCompradas/agregar",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content
                        .ReadFromJsonAsync<AgregarRefaccionesCitaResponse>();

                    Debug.WriteLine($"✅ Refacciones de cita agregadas. Total costo: ${result?.TotalCosto:N2}");

                    return result ?? new AgregarRefaccionesCitaResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"❌ Error HTTP {response.StatusCode}: {errorContent}");

                // Intentar deserializar el mensaje de error del servidor
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<AgregarRefaccionesCitaResponse>(
                        errorContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                        return errorResponse;
                }
                catch { /* ignorar si no se puede deserializar */ }

                return new AgregarRefaccionesCitaResponse
                {
                    Success = false,
                    Message = $"Error en la solicitud: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Excepción en AgregarRefaccionesCitaAsync: {ex.Message}");
                return new AgregarRefaccionesCitaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Agregar una sola refacción a un trabajo de cita (método simplificado)
        /// </summary>
        public async Task<AgregarRefaccionesCitaResponse> AgregarRefaccionCitaSimpleAsync(
            int trabajoId,
            string nombreRefaccion,
            int cantidad,
            decimal precio,
            bool orden,
            decimal? precioVenta = null)
        {
            var request = new AgregarRefaccionesCitaRequest
            {
                TrabajoId = trabajoId,
                Orden = orden,
                Refacciones = new List<AgregarRefaccionCitaDto>
                {
                    new AgregarRefaccionCitaDto
                    {
                        Refaccion = nombreRefaccion,
                        Cantidad = cantidad,
                        Precio = precio,
                        PrecioVenta = precioVenta
                    }
                }
            };

            return await AgregarRefaccionesCitaAsync(request);
        }
        /// <summary>
        /// Marcar un trabajo de cita como con refacciones listas
        /// PUT api/RefaccionesCita/{trabajoCitaId}/marcar-listas
        /// </summary>
        public async Task<RefaccionCitaResponse> MarcarRefaccionesListasAsync(
            int trabajoId,
            bool orden = false)
        {
            try
            {
                Debug.WriteLine($"📦 Marcando refacciones como listas para trabajo {trabajoId} (orden={orden})");

                var response = await _httpClient.PutAsync(
                    $"{BaseUrl}/RefaccionesCompradas/{trabajoId}/marcar-listas?orden={orden.ToString().ToLower()}",
                    null);

                var result = await response.Content
                    .ReadFromJsonAsync<RefaccionCitaResponse>();

                if (result != null)
                {
                    if (result.Success)
                        Debug.WriteLine("✅ Refacciones marcadas como listas correctamente");
                    else
                        Debug.WriteLine($"⚠️ No se pudo marcar: {result.Message}");
                    return result;
                }

                return new RefaccionCitaResponse
                {
                    Success = false,
                    Message = $"Error HTTP: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en MarcarRefaccionesListasAsync: {ex.Message}");
                return new RefaccionCitaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Obtener todas las refacciones de un trabajo de cita específico
        /// GET api/RefaccionesCita/trabajo/{trabajoCitaId}
        /// </summary>
        public async Task<ObtenerRefaccionesCitaResponse> ObtenerRefaccionesPorTrabajoCitaAsync(int trabajoId, bool orden = false)
        {
            try
            {
                Debug.WriteLine($"📥 Obteniendo refacciones del trabajo {trabajoId} (orden={orden})");

                // Se agrega ?orden=true o ?orden=false según el valor recibido
                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/RefaccionesCompradas/trabajo/{trabajoId}?orden={orden.ToString().ToLower()}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content
                        .ReadFromJsonAsync<ObtenerRefaccionesCitaResponse>();
                    Debug.WriteLine($"✅ Se obtuvieron {result?.Refacciones.Count ?? 0} refacciones");
                    return result ?? new ObtenerRefaccionesCitaResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta",
                        Refacciones = new List<RefaccionCompradaDto>()
                    };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ObtenerRefaccionesCitaResponse
                    {
                        Success = false,
                        Message = "Trabajo no encontrado",
                        Refacciones = new List<RefaccionCompradaDto>()
                    };
                }

                return new ObtenerRefaccionesCitaResponse
                {
                    Success = false,
                    Message = "No se pudieron obtener las refacciones",
                    Refacciones = new List<RefaccionCompradaDto>()
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en ObtenerRefaccionesPorTrabajoCitaAsync: {ex.Message}");
                return new ObtenerRefaccionesCitaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Refacciones = new List<RefaccionCompradaDto>()
                };
            }
        }
        /// <summary>
        /// Obtener todas las refacciones de una cita completa agrupadas por trabajo
        /// GET api/RefaccionesCita/cita/{citaId}
        /// </summary>
        public async Task<ObtenerRefaccionesPorCitaResponse> ObtenerRefaccionesPorCitaAsync(
            int citaId)
        {
            try
            {
                Debug.WriteLine($"📥 Obteniendo refacciones de la cita {citaId}");

                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/RefaccionesCompradas/cita/{citaId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    // El endpoint devuelve { success, citaId, trabajos: [...] }
                    var apiResponse = JsonSerializer.Deserialize<JsonElement>(json);

                    var resultado = new ObtenerRefaccionesPorCitaResponse
                    {
                        Success = apiResponse.TryGetProperty("success", out var s) && s.GetBoolean(),
                        CitaId = citaId,
                        Trabajos = new List<TrabajoCitaConRefaccionesDto>()
                    };

                    if (apiResponse.TryGetProperty("trabajos", out var trabajosElement))
                    {
                        resultado.Trabajos = JsonSerializer.Deserialize<List<TrabajoCitaConRefaccionesDto>>(
                            trabajosElement.GetRawText(),
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        ) ?? new List<TrabajoCitaConRefaccionesDto>();
                    }

                    resultado.Message = resultado.TieneRefacciones
                        ? $"Se encontraron refacciones en {resultado.Trabajos.Count} trabajo(s)"
                        : "No hay refacciones registradas en esta cita";

                    Debug.WriteLine($"✅ Se obtuvieron {resultado.Trabajos.Count} trabajo(s) con refacciones");

                    return resultado;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ObtenerRefaccionesPorCitaResponse
                    {
                        Success = false,
                        Message = "Cita no encontrada"
                    };
                }

                return new ObtenerRefaccionesPorCitaResponse
                {
                    Success = false,
                    Message = "No se pudieron obtener las refacciones"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en ObtenerRefaccionesPorCitaAsync: {ex.Message}");
                return new ObtenerRefaccionesPorCitaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Eliminar una refacción de un trabajo de cita
        /// DELETE api/RefaccionesCita/{refaccionId}
        /// Solo se pueden eliminar refacciones que no hayan sido transferidas (Transferida = false)
        /// </summary>
        public async Task<RefaccionCitaResponse> EliminarRefaccionCitaAsync(int refaccionId)
        {
            try
            {
                Debug.WriteLine($"🗑️ Eliminando refacción de cita {refaccionId}");

                var response = await _httpClient.DeleteAsync(
                    $"{BaseUrl}/RefaccionesCompradas/{refaccionId}");

                var content = await response.Content
                    .ReadFromJsonAsync<RefaccionCitaResponse>();

                if (content != null)
                {
                    if (content.Success)
                        Debug.WriteLine($"✅ Refacción de cita {refaccionId} eliminada");
                    else
                        Debug.WriteLine($"⚠️ No se pudo eliminar: {content.Message}");

                    return content;
                }

                return new RefaccionCitaResponse
                {
                    Success = false,
                    Message = $"Error HTTP: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en EliminarRefaccionCitaAsync: {ex.Message}");
                return new RefaccionCitaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Actualizar el precio de venta de una refacción de cita
        /// PUT api/RefaccionesCita/{refaccionId}/precio-venta
        /// </summary>
        public async Task<RefaccionCitaResponse> ActualizarPrecioVentaRefaccionCitaAsync(
            int refaccionId,
            decimal nuevoPrecioVenta)
        {
            try
            {
                Debug.WriteLine($"💰 Actualizando precio de venta de refacción de cita {refaccionId} → ${nuevoPrecioVenta:N2}");

                var request = new ActualizarPrecioVentaRefaccionCitaRequest
                {
                    PrecioVenta = nuevoPrecioVenta
                };

                var response = await _httpClient.PutAsJsonAsync(
                    $"{BaseUrl}/RefaccionesCompradas/{refaccionId}/precio-venta",
                    request);

                var result = await response.Content
                    .ReadFromJsonAsync<RefaccionCitaResponse>();

                if (result != null)
                {
                    if (result.Success)
                        Debug.WriteLine($"✅ Precio de venta actualizado correctamente");
                    else
                        Debug.WriteLine($"⚠️ No se pudo actualizar: {result.Message}");

                    return result;
                }

                return new RefaccionCitaResponse
                {
                    Success = false,
                    Message = $"Error HTTP: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en ActualizarPrecioVentaRefaccionCitaAsync: {ex.Message}");
                return new RefaccionCitaResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // ============================================
        // HELPERS DE REFACCIONES POR CITA
        // ============================================

        /// <summary>
        /// Verificar si un trabajo de cita tiene refacciones registradas
        /// </summary>
        public async Task<bool> TrabajoCitaTieneRefaccionesAsync(int trabajoCitaId)
        {
            try
            {
                var response = await ObtenerRefaccionesPorTrabajoCitaAsync(trabajoCitaId);
                return response.Success && response.TieneRefacciones;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtener el total de costo de refacciones de un trabajo de cita
        /// </summary>
        public async Task<decimal> ObtenerTotalRefaccionesCitaAsync(int trabajoCitaId)
        {
            try
            {
                var response = await ObtenerRefaccionesPorTrabajoCitaAsync(trabajoCitaId);
                return response.Success ? response.TotalCosto : 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}