// Logica de la pantalla de administracion
using app_subastas.Models;
using Plugin.Maui.Audio;
using System.Text.Json;

namespace app_subastas.Views;

public partial class AdminPage : ContentPage
{
    // Cliente HTTP para peticiones al backend
    private readonly HttpClient _http;

    // Constructor
    public AdminPage()
    {
        InitializeComponent();
        _http = new HttpClient();
        // Agregar token JWT al header de autorizacion
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {SessionManager.Token}");

        // Cargar datos asincronicamente sin bloquear la UI
        _ = CargarDatos();
    }

    /*
     * Carga las subastas activas y el historial de subastas finalizadas
     * Obtiene datos del endpoint /api/subastas/todas
     */
    private async Task CargarDatos()
    {
        try
        {
            var json = await _http.GetStringAsync($"{Constants.BaseUrl}/subastas/todas");
            var todas = JsonSerializer.Deserialize<List<Subasta>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Subasta>();

            // Filtrar subastas activas
            var activas = todas.Where(s => s.estado == "ACTIVA").ToList();
            // Filtrar subastas finalizadas para historial
            var historial = todas.Where(s => s.estado == "FINALIZADA").ToList();

            // Asignar a las CollectionView
            SubastasActivasList.ItemsSource = activas;
            HistorialList.ItemsSource = historial;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    /*
     * Crea una nueva subasta y la inicia automaticamente
     * Endpoint: POST /api/subastas/crear y POST /api/subastas/{id}/iniciar
     */
    private async void CrearSubasta(object sender, EventArgs e)
    {
        // Validar campos obligatorios
        if (string.IsNullOrWhiteSpace(TituloEntry.Text) || string.IsNullOrWhiteSpace(PrecioEntry.Text))
        {
            await DisplayAlert("Error", "Titulo y precio requeridos", "OK");
            return;
        }

        // Validar precio
        if (!double.TryParse(PrecioEntry.Text, out double precio))
        {
            await DisplayAlert("Error", "Precio invalido", "OK");
            return;
        }

        // Obtener duracion (default 30 minutos)
        int duracion = 30;
        if (!string.IsNullOrWhiteSpace(DuracionEntry.Text))
            int.TryParse(DuracionEntry.Text, out duracion);

        // Obtener URL de imagen o usar placeholder
        string imagenUrl = ImagenUrlEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(imagenUrl))
            imagenUrl = "https://via.placeholder.com/300";

        // Crear objeto con datos de la nueva subasta
        var nuevaSubasta = new
        {
            producto = TituloEntry.Text.Trim(),
            descripcion = DescripcionEntry.Text?.Trim() ?? "",
            imageUrl = imagenUrl,
            precioInicial = precio,
            incrementoMinimo = 100
        };

        var json = JsonSerializer.Serialize(nuevaSubasta);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            // Crear subasta
            var res = await _http.PostAsync($"{Constants.BaseUrl}/subastas/crear", content);

            if (res.IsSuccessStatusCode)
            {
                var responseBody = await res.Content.ReadAsStringAsync();
                var subastaCreada = JsonSerializer.Deserialize<Subasta>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Iniciar subasta automaticamente
                await _http.PostAsync($"{Constants.BaseUrl}/subastas/{subastaCreada.id}/iniciar?minutos={duracion}", null);

                await DisplayAlert("Exito", $"Subasta '{TituloEntry.Text}' creada", "OK");

                // Limpiar campos del formulario
                TituloEntry.Text = "";
                DescripcionEntry.Text = "";
                PrecioEntry.Text = "";
                ImagenUrlEntry.Text = "";
                DuracionEntry.Text = "30";

                // Recargar listas
                await CargarDatos();
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

    /*
     * Abre la pantalla de chat de una subasta especifica
     * El administrador puede ver el chat en vivo de cualquier subasta activa
     */
    private async void AbrirChatSubasta(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is long id)
        {
            var audioManager = AudioManager.Current;
            await Navigation.PushAsync(new SubastaPage(id, audioManager));
        }
    }

    // Navega a la pantalla de perfil
    private async void OnPerfilClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PerfilPage());
    }

    // Cierra sesion y vuelve a la pantalla de login
    private async void OnSalirClicked(object sender, EventArgs e)
    {
        SessionManager.Cerrar();
        Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
    }
}