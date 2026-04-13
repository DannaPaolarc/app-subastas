using Plugin.Maui.Audio;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using app_subastas.Models;

namespace app_subastas;

public partial class SubastaPage : ContentPage
{
    private ClientWebSocket? _ws;
    private readonly long _subastaId;
    private DateTime _tiempoFin;
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _playerPuja;
    private readonly HttpClient _http;
    private System.Timers.Timer? _timer;

    public SubastaPage(long id, IAudioManager audio)
    {
        InitializeComponent();
        _subastaId = id;
        _audioManager = audio;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {SessionManager.Token}");

        if (SessionManager.Rol == "ADMIN")
        {
            PujaFrame.IsVisible = false;
        }

        _ = CargarAudio();
        _ = CargarSubasta();
        _ = ConectarWebSocket();
    }

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

    private void IniciarTimer()
    {
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += (s, e) =>
        {
            var segundosRestantes = (_tiempoFin - DateTime.UtcNow).TotalSeconds;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (segundosRestantes <= 0)
                {
                    TiempoText.Text = "FINALIZADO";
                    TiempoText.TextColor = Colors.Red;
                    _timer?.Stop();
                }
                else
                {
                    var minutos = (int)(segundosRestantes / 60);
                    var segundos = (int)(segundosRestantes % 60);
                    TiempoText.Text = $"{minutos:D2}:{segundos:D2}";
                }
            });
        };
        _timer.AutoReset = true;
        _timer.Start();
    }

    private async Task ConectarWebSocket()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== 1. CONECTANDO WEBSOCKET ===");
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(Constants.WsUrl), CancellationToken.None);
            System.Diagnostics.Debug.WriteLine("=== 2. WEBSOCKET CONECTADO ===");

            string subscribeMsg = $"SUBSCRIBE\ndestination:/topic/subasta/{_subastaId}\nid:0\n\n\0";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(subscribeMsg);
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            System.Diagnostics.Debug.WriteLine($"=== 3. SUSCRIPCION ENVIADA ===");

            _ = Task.Run(EscucharWebSocket);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR: {ex.Message}");
        }
    }

    private async Task EscucharWebSocket()
    {
        var buffer = new byte[4096];
        while (_ws?.State == WebSocketState.Open)
        {
            try
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var raw = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                System.Diagnostics.Debug.WriteLine($"=== MENSAJE RECIBIDO: {raw}");

                var msg = JsonSerializer.Deserialize<MensajeChat>(raw);
                if (msg != null)
                {
                    MainThread.BeginInvokeOnMainThread(() => MostrarMensaje(msg));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== ERROR: {ex.Message}");
                break;
            }
        }
    }

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

    private async void OnEnviarMensaje(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MensajeInput.Text)) return;

        var msg = new MensajeChat
        {
            usuario = SessionManager.Nombre,
            contenido = MensajeInput.Text,
            hora = DateTime.Now.ToString("HH:mm"),
            subastaId = _subastaId
        };

        var json = JsonSerializer.Serialize(msg);
        string stompMsg = $"SEND\ndestination:/app/chat.enviar\ncontent-length:{json.Length}\n\n{json}\0";
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(stompMsg);

        try
        {
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            System.Diagnostics.Debug.WriteLine("=== MENSAJE ENVIADO ===");
            MostrarMensaje(msg);
            MensajeInput.Text = "";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR: {ex.Message}");
        }
    }

    private async void OnSalirClicked(object sender, EventArgs e)
    {
        if (_ws != null)
        {
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            _ws.Dispose();
        }
        await Navigation.PopAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
        _timer?.Dispose();
        _ws?.Dispose();
    }
}