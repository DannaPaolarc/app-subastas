//  Logica de la pantalla de inicio de sesion
using app_subastas.Views;
using Plugin.Maui.Audio;
using System.Text;
using System.Text.Json;

namespace app_subastas;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // Valida credenciales y redirige segun el rol del usuario
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Validar campos obligatorios
        if (string.IsNullOrWhiteSpace(emailEntry.Text) ||
            string.IsNullOrWhiteSpace(passwordEntry.Text))
        {
            await DisplayAlert("Error", "Ingresa correo y contraseña", "OK");
            return;
        }

        var http = new HttpClient();
        var data = new { correo = emailEntry.Text.Trim(), contrasena = passwordEntry.Text };
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            // Enviar peticion POST al endpoint de login
            var res = await http.PostAsync($"{Constants.BaseUrl}/auth/login", content);

            if (res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(body);

                // Guardar datos del usuario en SessionManager
                SessionManager.Token = doc.RootElement.GetProperty("token").GetString()!;
                SessionManager.Nombre = doc.RootElement.GetProperty("nombre").GetString()!;
                SessionManager.Correo = doc.RootElement.GetProperty("correo").GetString()!;
                SessionManager.Rol = doc.RootElement.GetProperty("rol").GetString()!;
                SessionManager.UsuarioId = doc.RootElement.GetProperty("id").GetInt64();
                SessionManager.Direccion = doc.RootElement.GetProperty("direccion").GetString() ?? "";
                SessionManager.Telefono = doc.RootElement.GetProperty("telefono").GetString() ?? "";

                // Redirigir segun el rol
                if (SessionManager.Rol == "ADMIN")
                {
                    Application.Current!.Windows[0].Page = new NavigationPage(new AdminPage());
                }
                else
                {
                    Application.Current!.Windows[0].Page = new NavigationPage(new HomePage(AudioManager.Current));
                }
            }
            else
            {
                var error = await res.Content.ReadAsStringAsync();
                await DisplayAlert("Error", error, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo conectar: {ex.Message}", "OK");
        }
    }

    // Navega a la pantalla de registro
    private async void OnRegistroClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.RegistroPage());
    }
}