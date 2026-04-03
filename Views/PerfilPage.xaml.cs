//
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace app_subastas.Views;

public partial class PerfilPage : ContentPage
{
    // Cliente HTTP para peticiones al backend
    private readonly HttpClient _http;

    // Constructor: inicializa componentes y carga datos del usuario desde SessionManager
    public PerfilPage()
    {
        InitializeComponent();

        _http = new HttpClient();
        // Agregar token JWT al header de autorizacion
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {SessionManager.Token}");

        // Cargar datos actuales del usuario en los campos
        NombreEntry.Text = SessionManager.Nombre;
        CorreoEntry.Text = SessionManager.Correo;
        DireccionEntry.Text = SessionManager.Direccion;
        TelefonoEntry.Text = SessionManager.Telefono;
    }

    // Actualiza los datos del perfil en el backend
    private async void OnActualizarClicked(object sender, EventArgs e)
    {
        // Validar que el nombre no este vacio
        if (string.IsNullOrWhiteSpace(NombreEntry.Text))
        {
            await DisplayAlert("Error", "El nombre es obligatorio", "OK");
            return;
        }

        // Validar formato del correo si se proporciono
        if (!string.IsNullOrWhiteSpace(CorreoEntry.Text) && !IsValidEmail(CorreoEntry.Text))
        {
            await DisplayAlert("Error", "Correo electronico no valido", "OK");
            return;
        }

        // Validar formato del telefono si se proporciono
        if (!string.IsNullOrWhiteSpace(TelefonoEntry.Text) && !IsValidPhone(TelefonoEntry.Text))
        {
            await DisplayAlert("Error", "Telefono debe contener solo numeros", "OK");
            return;
        }

        // Crear objeto con los datos a actualizar
        var data = new
        {
            nombre = NombreEntry.Text,
            correo = CorreoEntry.Text,
            direccion = DireccionEntry.Text,
            telefono = TelefonoEntry.Text,
            contrasena = string.IsNullOrWhiteSpace(PasswordEntry.Text) ? null : PasswordEntry.Text
        };

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            // Enviar peticion PUT al backend
            var res = await _http.PutAsync($"{Constants.BaseUrl}/usuarios/{SessionManager.UsuarioId}", content);

            if (res.IsSuccessStatusCode)
            {
                var response = await res.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(response);

                // Actualizar SessionManager con los nuevos datos
                SessionManager.Nombre = doc.RootElement.GetProperty("nombre").GetString() ?? NombreEntry.Text;
                SessionManager.Correo = doc.RootElement.GetProperty("correo").GetString() ?? CorreoEntry.Text;
                SessionManager.Direccion = doc.RootElement.GetProperty("direccion").GetString() ?? DireccionEntry.Text;
                SessionManager.Telefono = doc.RootElement.GetProperty("telefono").GetString() ?? TelefonoEntry.Text;

                await DisplayAlert("Exito", "Perfil actualizado correctamente", "OK");
                await Navigation.PopAsync(); // Volver a la pantalla anterior
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

    // Cierra sesion y vuelve a la pantalla de login
    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        SessionManager.Cerrar();
        Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
    }

    // Vuelve a la pantalla anterior sin guardar cambios
    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // Valida el formato de un correo electronico
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // Valida el formato de un numero de telefono (solo numeros, opcional + al inicio)
    private bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(phone, @"^\+?[0-9]{7,15}$");
    }
}