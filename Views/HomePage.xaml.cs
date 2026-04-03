using app_subastas.Models;
using app_subastas.Services;
using Plugin.Maui.Audio;
using System.Text.Json;

namespace app_subastas.Views;

public partial class HomePage : ContentPage
{
    // Servicio para llamadas a la API
    private readonly ApiService _api;

    // Gestor de audio para sonidos de pujas
    private readonly IAudioManager _audio;

    // Cliente HTTP para peticiones directas
    private readonly HttpClient _http;

    // Constructor: inicializa componentes, carga subastas e historial
    public HomePage(IAudioManager audioManager)
    {
        InitializeComponent();
        _audio = audioManager;
        _api = new ApiService();
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {SessionManager.Token}");

        _ = CargarSubastas();
        _ = CargarHistorial();
    }

    // Carga las subastas activas desde el backend y las muestra en la lista
    private async Task CargarSubastas()
    {
        try
        {
            var subastas = await _api.GetSubastasActivas();
            SubastasList.ItemsSource = subastas;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Carga el historial de subastas finalizadas desde el backend
    private async Task CargarHistorial()
    {
        try
        {
            var json = await _http.GetStringAsync($"{Constants.BaseUrl}/subastas/todas");
            var todas = JsonSerializer.Deserialize<List<Subasta>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Subasta>();

            var historial = todas.Where(s => s.estado == "FINALIZADA").ToList();
            HistorialList.ItemsSource = historial;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error historial: {ex.Message}");
        }
    }

    // Cuando el usuario selecciona una subasta, navega a la pantalla de detalle
    private async void OnSubastaSeleccionada(object sender, SelectionChangedEventArgs e)
    {
        var subasta = e.CurrentSelection.FirstOrDefault() as Subasta;
        if (subasta != null)
        {
            await Navigation.PushAsync(new SubastaPage(subasta.id, _audio));
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    // Navega a la pantalla de perfil del usuario
    private async void OnPerfilClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PerfilPage());
    }
}