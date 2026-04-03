// Proposito: Configuracion de URLs para diferentes entornos (desarrollo y produccion)
namespace app_subastas;

public static class Constants
{
#if WINDOWS
    // Desarrollo en Windows local
    public const string BaseUrl = "http://localhost:8080/api";
    public const string WsUrl = "ws://localhost:8080/ws-subastas/websocket";
#elif ANDROID
    // Desarrollo en Android Emulator (10.0.2.2 apunta a localhost del PC)
    public const string BaseUrl = "http://10.0.2.2:8080/api";
    public const string WsUrl = "ws://10.0.2.2:8080/ws-subastas/websocket";
#elif RELEASE
    // Produccion en Render 
    public const string BaseUrl = "https://tu-app.onrender.com/api";
    public const string WsUrl = "wss://tu-app.onrender.com/ws-subastas/websocket";
#else
    // Entorno por defecto
    public const string BaseUrl = "http://localhost:8080/api";
    public const string WsUrl = "ws://localhost:8080/ws-subastas/websocket";
#endif
}