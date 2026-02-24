using CarslineApp.Models;
using CarslineApp.Services;
using System.Collections.Specialized;
using System.Diagnostics;

namespace CarslineApp.Views
{
    public partial class RecordatorioDetallePage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly int _recordatorioId;
        private readonly int _tipoRecordatorio;
        private RecordatorioServicioDto _recordatorioDetalle;
        private bool _isLoading;

        public RecordatorioDetallePage(int recordatorioId, int tipoRecordatorio)
        {
            InitializeComponent();

            _apiService = new ApiService();
            _recordatorioId = recordatorioId;
            _tipoRecordatorio = tipoRecordatorio;

            Title = "Detalle del Recordatorio";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarDetalleRecordatorio();
        }

        private async Task CargarDetalleRecordatorio()
        {
            try
            {
                _isLoading = true;
                loadingIndicator.IsRunning = true;
                loadingIndicator.IsVisible = true;
                contenidoDetalle.IsVisible = false;

                Debug.WriteLine($"?? Cargando detalle del recordatorio ID: {_recordatorioId}");

                // Llamar al API para obtener el detalle completo
                var response = await _apiService.ObtenerRecordatorioDetalleAsync(_recordatorioId);

                if (response.Success && response.Recordatorios != null && response.Recordatorios.Count > 0)
                {
                    _recordatorioDetalle = response.Recordatorios[0];
                    Debug.WriteLine($"? Detalle cargado para cliente: {_recordatorioDetalle.ClienteNombre}");

                    // Mostrar los datos
                    MostrarDatos();
                }
                else
                {
                    Debug.WriteLine($"?? No se encontró el recordatorio: {response.Message}");
                    await DisplayAlert("Advertencia", response.Message ?? "No se encontró el recordatorio", "OK");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error al cargar detalle: {ex.Message}");
                await DisplayAlert("Error", "No se pudo cargar el detalle del recordatorio", "OK");
                await Navigation.PopAsync();
            }
            finally
            {
                _isLoading = false;
                loadingIndicator.IsRunning = false;
                loadingIndicator.IsVisible = false;
                contenidoDetalle.IsVisible = true;
            }
        }

        private void MostrarDatos()
        {
            if (_recordatorioDetalle == null)
            {
                Debug.WriteLine("?? No hay datos del recordatorio para mostrar");
                return;
            }

            try
            {
                // Cliente
                lblClienteNombre.Text = _recordatorioDetalle.ClienteNombre;
                lblClienteTelefono.Text = !string.IsNullOrEmpty(_recordatorioDetalle.Telefono)
                    ? _recordatorioDetalle.Telefono
                    : "Sin teléfono";
                lblClienteTelefonoCasa.Text = _recordatorioDetalle.TelefonoCasa ?? "Sin teléfono";
                lblClienteCorreo.Text = _recordatorioDetalle.Correo;

                // Vehículo
                lblVehiculoInfo.Text = _recordatorioDetalle.InfoVehiculo;
                lblVIN.Text = _recordatorioDetalle.VIN;
                lblPlacas.Text = _recordatorioDetalle.Placas;

                // Último servicio
                lblUltimoServicio.Text = _recordatorioDetalle.UltimoServicioRealizado;
                lblFechaUltimoServicio.Text = _recordatorioDetalle.FechaUltimoServicio.ToString("dd/MMM/yyyy");
                lblUltimoKilometraje.Text = $"{_recordatorioDetalle.UltimoKilometraje:N0} km";

                // Próximo servicio
                lblProximoServicio.Text = _recordatorioDetalle.TipoProximoServicio;
                lblFechaProximoServicio.Text = _recordatorioDetalle.FechaProximoServicioFormateada;
                lblProximoKilometraje.Text = _recordatorioDetalle.KilometrajeProximoServicio.HasValue
                    ? $"{_recordatorioDetalle.KilometrajeProximoServicio:N0} km"
                    : "N/A";

                // Calcular días
                var diasDesdeUltimo = (DateTime.Today - _recordatorioDetalle.FechaUltimoServicio.Date).Days;
                var diasParaProximo = _recordatorioDetalle.DiasParaProximoServicio ?? -1;

                lblDiasDesdeUltimo.Text = $"{diasDesdeUltimo} días";
                lblDiasParaProximo.Text = $"{diasParaProximo} días";

                Debug.WriteLine("? Datos mostrados correctamente en la UI");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error al mostrar datos: {ex.Message}");
            }
        }

        #region Métodos de Comunicación

        /// <summary>
        /// Enviar mensaje de WhatsApp con recordatorio de servicio
        /// </summary>

        private async void OnEnviarWhatsAppClicked(object sender, EventArgs e)
        {
            if (_recordatorioDetalle == null) return;

            var telefono = !string.IsNullOrEmpty(_recordatorioDetalle.Telefono)
                ? _recordatorioDetalle.Telefono
                : _recordatorioDetalle.TelefonoCasa;

            if (string.IsNullOrWhiteSpace(telefono))
            {
                await DisplayAlert("?? Advertencia", "No hay número de teléfono disponible", "OK");
                return;
            }

            try
            {
                Debug.WriteLine($"?? Abriendo WhatsApp para: {telefono}");

                // Limpiar el número de teléfono
                var telefonoLimpio = LimpiarNumeroTelefono(telefono);

                // Agregar código de país si no lo tiene (México +52)
                if (!telefonoLimpio.StartsWith("+"))
                {
                    telefonoLimpio = "+52" + telefonoLimpio;
                }

                // Generar mensaje personalizado
                var mensaje = GenerarMensajeRecordatorio();

                // URL de WhatsApp con mensaje pre-escrito
                var whatsappUrl = $"https://wa.me/{telefonoLimpio.Replace("+", "")}?text={Uri.EscapeDataString(mensaje)}";

                Debug.WriteLine($"?? URL WhatsApp: {whatsappUrl}");

                // Abrir WhatsApp
                await Launcher.OpenAsync(new Uri(whatsappUrl));

                Debug.WriteLine("? WhatsApp abierto exitosamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error al abrir WhatsApp: {ex.Message}");
                await DisplayAlert("? Error", $"No se pudo abrir WhatsApp: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Llamar al teléfono móvil
        /// </summary>
        private async void OnLlamarTelefonoClicked(object sender, EventArgs e)
        {
            if (_recordatorioDetalle == null) return;

            if (string.IsNullOrWhiteSpace(_recordatorioDetalle.Telefono))
            {
                await DisplayAlert("?? Advertencia", "No hay número de teléfono móvil disponible", "OK");
                return;
            }

            await RealizarLlamada(_recordatorioDetalle.Telefono);
        }

        /// <summary>
        /// Llamar al teléfono de casa
        /// </summary>
        private async void OnLlamarTelefonoCasaClicked(object sender, EventArgs e)
        {
            if (_recordatorioDetalle == null) return;

            if (string.IsNullOrWhiteSpace(_recordatorioDetalle.TelefonoCasa))
            {
                await DisplayAlert("?? Advertencia", "No hay número de teléfono de casa disponible", "OK");
                return;
            }

            await RealizarLlamada(_recordatorioDetalle.TelefonoCasa);
        }

        /// <summary>
        /// Enviar correo electrónico con recordatorio
        /// </summary>
        private async void OnEnviarCorreoClicked(object sender, EventArgs e)
        {
            if (_recordatorioDetalle == null) return;

            if (string.IsNullOrWhiteSpace(_recordatorioDetalle.Correo) || _recordatorioDetalle.Correo == "Sin Correo Registrado")
            {
                await DisplayAlert("?? Advertencia", "No hay correo electrónico disponible", "OK");
                return;
            }

            try
            {
                Debug.WriteLine($"?? Abriendo correo para: {_recordatorioDetalle.Correo}");

                // Generar asunto y cuerpo del correo
                var asunto = $"Recordatorio de Servicio - {_recordatorioDetalle.InfoVehiculo}";
                var cuerpo = GenerarMensajeRecordatorio();

                // Crear el mensaje de email
                var message = new EmailMessage
                {
                    Subject = asunto,
                    Body = cuerpo,
                    To = new List<string> { _recordatorioDetalle.Correo }
                };

                // Abrir cliente de correo
                await Email.ComposeAsync(message);

                Debug.WriteLine("? Cliente de correo abierto");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error al abrir correo: {ex.Message}");
                await DisplayAlert("? Error", $"No se pudo abrir el cliente de correo: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Realizar llamada telefónica
        /// </summary>
        private async Task RealizarLlamada(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                await DisplayAlert("?? Advertencia", "No hay número de teléfono disponible", "OK");
                return;
            }

            try
            {
                Debug.WriteLine($"?? Realizando llamada a: {telefono}");

                // Limpiar el número
                var telefonoLimpio = LimpiarNumeroTelefono(telefono);

                // PhoneDialer de MAUI
                if (PhoneDialer.Default.IsSupported)
                {
                    PhoneDialer.Default.Open(telefonoLimpio);
                    Debug.WriteLine("? Llamada iniciada");
                }
                else
                {
                    await DisplayAlert("? Error", "El dispositivo no soporta llamadas telefónicas", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error al realizar llamada: {ex.Message}");
                await DisplayAlert("? Error", $"No se pudo realizar la llamada: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Generar mensaje personalizado para el recordatorio
        /// </summary>
        private string GenerarMensajeRecordatorio()
        {
            if (_recordatorioDetalle == null) return string.Empty;

            var tipoRecordatorioTexto = _tipoRecordatorio switch
            {
                1 => "Primer recordatorio",
                2 => "Segundo recordatorio",
                3 => "Recordatorio final",
                _ => "recordatorio"
            };

            var Final = " y mantener su garantía.";
            if (_recordatorioDetalle.TipoProximoServicio == "Servicio Externo")
            {
                Final = ".";
            }
            var mensaje = $@"*RECORDATORIO DE SERVICIO*

Estimado(a) *{_recordatorioDetalle.ClienteNombre}*,

Le recordamos que su vehículo *{_recordatorioDetalle.InfoVehiculo}* tiene programado su próximo servicio.

*INFORMACIÓN DEL SERVICIO*
• Tipo: {_recordatorioDetalle.TipoProximoServicio}
• Fecha: {_recordatorioDetalle.FechaProximoServicioFormateada}
• Kilometraje: {(_recordatorioDetalle.KilometrajeProximoServicio.HasValue ? $"{_recordatorioDetalle.KilometrajeProximoServicio:N0} km" : "N/A")} 
• Faltan {_recordatorioDetalle.DiasParaProximoServicio} días

Este es su *{tipoRecordatorioTexto}*.
Le recomendamos agendar su cita para conservar su vehículo en óptimas condiciones{Final}

*CONTACTO*
Teléfono: 771 183 9338

¡Gracias por su preferencia!";


              return mensaje;

        }

        /// <summary>
        /// Limpia un número de teléfono quitando caracteres no numéricos
        /// </summary>
        private string LimpiarNumeroTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return string.Empty;

            // Mantener el + si está al inicio, quitar todo lo demás excepto números
            var limpio = new string(telefono.Where(c => char.IsDigit(c) || c == '+').ToArray());

            // Asegurar que el + solo esté al inicio
            if (limpio.Contains('+'))
            {
                limpio = "+" + limpio.Replace("+", "");
            }

            return limpio;
        }

        #endregion

        #region Marcar como Enviado
        private async void AgendarCitaClicked(object sender, EventArgs e)
        {

            await DisplayAlert("Agendar Cita", "Selecciona un Horario Disponible en la agenda", "OK");
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new AgendaCitas(0, _recordatorioDetalle.ClienteId,_recordatorioDetalle.VehiculoId ));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo abrir la agenda de citas: {ex.Message}",
                    "OK");
            }
        }
        private async void OnMarcarEnviadoClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert(
                "Confirmar",
                "¿Marcar este recordatorio como enviado?",
                "Sí",
                "No");

            if (confirmar)
            {
                try
                {
                    loadingIndicator.IsRunning = true;
                    loadingIndicator.IsVisible = true;

                    var response = await _apiService.MarcarRecordatorioEnviadoAsync(
                        _recordatorioId,
                        _tipoRecordatorio);

                    if (response.Success)
                    {
                        await DisplayAlert(" Éxito", "Recordatorio marcado como enviado", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert(" Error", response.Message, "OK");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($" Error al marcar enviado: {ex.Message}");
                    await DisplayAlert(" Error", "No se pudo marcar el recordatorio", "OK");
                }
                finally
                {
                    loadingIndicator.IsRunning = false;
                    loadingIndicator.IsVisible = false;
                }
            }
        }

        #endregion
    }
}