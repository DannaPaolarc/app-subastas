using Plugin.Maui.Audio;
using System.Text;
using System.Text.Json;

namespace app_subastas.Views;

public partial class RegistroPage : ContentPage
{
    public RegistroPage()
    {
        InitializeComponent();
    }

    // Valida los campos y envia peticion de registro al backend
    private async void OnRegistroClicked(object sender, EventArgs e)
    {
        // Validar campos obligatorios
        if (string.IsNullOrWhiteSpace(nombreEntry.Text) ||
            string.IsNullOrWhiteSpace(correoEntry.Text) ||
            string.IsNullOrWhiteSpace(contrasenaEntry.Text) ||
            string.IsNullOrWhiteSpace(direccionEntry.Text))
        {
            await DisplayAlert("Error", "Nombre, correo, contraseña y dirección son obligatorios", "OK");
            return;
        }

        var http = new HttpClient();
        // Crear objeto con los datos del nuevo usuario
        var data = new
        {
            nombre = nombreEntry.Text.Trim(),
            correo = correoEntry.Text.Trim(),
            contrasena = contrasenaEntry.Text,
            direccion = direccionEntry.Text.Trim(),
            telefono = telefonoEntry.Text?.Trim() ?? ""
        };

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            // Enviar peticion POST al endpoint de registro
            var res = await http.PostAsync($"{Constants.BaseUrl}/auth/registro", content);
            var body = await res.Content.ReadAsStringAsync();

            if (res.IsSuccessStatusCode)
            {
                var doc = JsonDocument.Parse(body);

                // Guardar datos del usuario en SessionManager
                SessionManager.Token = doc.RootElement.GetProperty("token").GetString()!;
                SessionManager.Nombre = doc.RootElement.GetProperty("nombre").GetString()!;
                SessionManager.Correo = doc.RootElement.GetProperty("correo").GetString()!;
                SessionManager.Rol = doc.RootElement.GetProperty("rol").GetString()!;
                SessionManager.UsuarioId = doc.RootElement.GetProperty("id").GetInt64();
                SessionManager.Direccion = direccionEntry.Text.Trim();
                SessionManager.Telefono = telefonoEntry.Text?.Trim() ?? "";

                // Mostrar mensaje de exito (sin emoji)
                await DisplayAlert("Cuenta creada", $"Bienvenido {SessionManager.Nombre}!", "OK");

                // Navegar a la pantalla principal
                Application.Current!.Windows[0].Page = new NavigationPage(new HomePage(AudioManager.Current));
            }
            else
            {
                var error = JsonDocument.Parse(body);
                var msg = error.RootElement.GetProperty("error").GetString();
                await DisplayAlert("Error", msg, "OK");
            }
        }
        catch
        {
            await DisplayAlert("Error", "No se pudo conectar al servidor", "OK");
        }
    }

    // Vuelve a la pantalla de login
    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}