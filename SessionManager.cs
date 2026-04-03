using Microsoft.Maui.Storage;

namespace app_subastas;

public static class SessionManager
{
    // Token JWT del usuario autenticado
    public static string Token
    {
        get => Preferences.Get("auth_token", string.Empty);
        set => Preferences.Set("auth_token", value);
    }

    // Nombre completo del usuario
    public static string Nombre
    {
        get => Preferences.Get("user_nombre", string.Empty);
        set => Preferences.Set("user_nombre", value);
    }

    // Correo electronico del usuario
    public static string Correo
    {
        get => Preferences.Get("user_correo", string.Empty);
        set => Preferences.Set("user_correo", value);
    }

    // Rol del usuario: USUARIO o ADMIN
    public static string Rol
    {
        get => Preferences.Get("user_rol", string.Empty);
        set => Preferences.Set("user_rol", value);
    }

    // Identificador unico del usuario
    public static long UsuarioId
    {
        get => Preferences.Get("user_id", 0L);
        set => Preferences.Set("user_id", value);
    }

    // Direccion de envio del usuario
    public static string Direccion
    {
        get => Preferences.Get("user_direccion", string.Empty);
        set => Preferences.Set("user_direccion", value);
    }

    // Numero de telefono del usuario
    public static string Telefono
    {
        get => Preferences.Get("user_telefono", string.Empty);
        set => Preferences.Set("user_telefono", value);
    }

    // Indica si hay un usuario logueado (token no vacio)
    public static bool EstaLogueado => !string.IsNullOrEmpty(Token);

    // Elimina todos los datos de sesion (cierra sesion)
    public static void Cerrar()
    {
        Preferences.Remove("auth_token");
        Preferences.Remove("user_nombre");
        Preferences.Remove("user_correo");
        Preferences.Remove("user_rol");
        Preferences.Remove("user_id");
        Preferences.Remove("user_direccion");
        Preferences.Remove("user_telefono");
    }
}