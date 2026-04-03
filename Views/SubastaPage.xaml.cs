using Plugin.Maui.Audio;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using app_subastas.Models;

namespace app_subastas;

public partial class SubastaPage : ContentPage
{
    // WebSocket para comunicacion en tiempo real
    private ClientWebSocket? _ws;

    // ID de la subasta actual
    private readonly long _subastaId;

    // Tiempo de finalizacion de la subasta
    private DateTime _tiempoFin;

    // Gestor de audio
    private readonly IAudioManager _audioManager;

    // Reproductor de sonido para pujas
    private IAudioPlayer? _playerPuja;

    // Cliente HTTP para peticiones
    private readonly HttpClient _http;

    // Timer para la cuenta regresiva
    private System.Timers.Timer? _timer;

    // Constructor: inicializa componentes, oculta puja si es admin, carga datos
    public SubastaPage(long id, IAudioManager audio)
    {
        InitializeComponent();
        _subastaId = id;
        _audioManager = audio;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {SessionManager.Token}");

        // Ocultar seccion de puja para administradores
        if (SessionManager.Rol == "ADMIN")
        {
            PujaFrame.IsVisible = false;
        }

        _ = CargarAudio();
        _ = CargarSubasta();
        _ = ConectarWebSocket();
    }

    // Carga los archivos de audio
    private async Task CargarAudio()
    {
        try
        {
            var pujaStream = await FileSystem.OpenAppPackageFileAsync("bid.mp3");
            _playerPuja = _audioManager.CreatePlayer(pujaStream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error audio: {ex.Message}");
        }
    }

    // Carga los datos de la subasta desde el backend
    private async Task CargarSubasta()
    {
        try
        {
            var json = await _http.GetStringAsync($"{Constants.BaseUrl}/subastas/{_subastaId}");
            var subasta = JsonSerializer.Deserialize<Subasta>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (subasta != null)
            {
                TituloText.Text = subasta.producto;
                DescripcionText.Text = subasta.descripcion;
                PrecioActualText.Text = $"${subasta.precioActual:N0}";

                if (!string.IsNullOrEmpty(subasta.imageUrl))
                {
                    ImagenSubasta.Source = subasta.imageUrl;
                }

                if (subasta.tiempoFin.HasValue)
                {
                    _tiempoFin = subasta.tiempoFin.Value;
                    IniciarTimer();
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Inicia el timer de cuenta regresiva
    private void IniciarTimer()
    {
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += (s, e) =>
        {
            var restante = _tiempoFin - DateTime.Now;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (restante.TotalSeconds <= 0)
                {
                    TiempoText.Text = "FINALIZADO";
                    TiempoText.TextColor = Colors.Red;
                    _timer?.Stop();
                }
                else
                {
                    var minutos = (int)restante.TotalMinutes;
                    var segundos = restante.Seconds;
                    TiempoText.Text = $"{minutos:D2}:{segundos:D2}";
                }
            });
        };
        _timer.AutoReset = true;
        _timer.Start();
    }

    // Conecta al WebSocket para recibir mensajes en tiempo real
    private async Task ConectarWebSocket()
    {
        try
        {
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(Constants.WsUrl), CancellationToken.None);

            // Suscribirse al canal de la subasta
            string subscribeMsg = $"SUBSCRIBE\nid:sub-{_subastaId}\ndestination:/topic/subasta/{_subastaId}\n\n\0";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(subscribeMsg);
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            _ = Task.Run(EscucharWebSocket);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
        }
    }

    // Escucha mensajes entrantes del WebSocket
    private async Task EscucharWebSocket()
    {
        var buffer = new byte[4096];
        while (_ws?.State == WebSocketState.Open)
        {
            try
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var raw = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                var jsonStart = raw.IndexOf('{');
                if (jsonStart >= 0)
                {
                    var json = raw.Substring(jsonStart);
                    var msg = JsonSerializer.Deserialize<MensajeChat>(json);
                    if (msg != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            MostrarMensaje(msg);
                        });
                    }
                }
            }
            catch { break; }
        }
    }

    // Muestra un mensaje en el chat
    private void MostrarMensaje(MensajeChat msg)
    {
        var color = msg.tipo == "PUJA" ? Colors.Lime : Colors.White;

        var label = new Label
        {
            Text = $"[{msg.hora}] {msg.usuario}: {msg.contenido}",
            TextColor = color,
            FontSize = 12,
            Margin = new Thickness(0, 2)
        };

        ChatContainer.Children.Add(label);
    }

    // Envia una puja al backend
    private async void OnPujarClicked(object sender, EventArgs e)
    {
        if (!double.TryParse(MontoInput.Text, out double monto))
        {
            await DisplayAlert("Error", "Ingresa un monto valido", "OK");
            return;
        }

        var data = new { monto };
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var res = await _http.PostAsync($"{Constants.BaseUrl}/subastas/{_subastaId}/ofertar", content);

            if (res.IsSuccessStatusCode)
            {
                // Reproducir sonido de puja
                _playerPuja?.Play();
                MontoInput.Text = "";
                await DisplayAlert("Exito", "Puja realizada!", "OK");
            }
            else
            {
                var error = await res.Content.ReadAsStringAsync();
                await DisplayAlert("Error", error, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Envia un mensaje de chat via WebSocket
    private async void OnEnviarMensaje(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MensajeInput.Text)) return;

        var msg = new MensajeChat
        {
            usuario = SessionManager.Nombre,
            contenido = MensajeInput.Text,
            tipo = "CHAT",
            hora = DateTime.Now.ToString("HH:mm"),
            subastaId = _subastaId
        };

        await EnviarMensajeWebSocket(msg);
        MostrarMensaje(msg);
        MensajeInput.Text = "";
    }

    // Envia el mensaje WebSocket al backend
    private async Task EnviarMensajeWebSocket(MensajeChat msg)
    {
        try
        {
            var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(Constants.WsUrl), CancellationToken.None);

            var json = JsonSerializer.Serialize(msg);
            string stompMsg = $"SEND\ndestination:/app/chat.enviar\ncontent-length:{json.Length}\n\n{json}\0";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(stompMsg);
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enviando: {ex.Message}");
        }
    }

    // Vuelve a la pantalla anterior
    private async void OnSalirClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // Limpia recursos al salir de la pantalla
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
        _timer?.Dispose();
        _ws?.Dispose();
    }
}