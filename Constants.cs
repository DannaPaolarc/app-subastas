// Proposito: Configuracion de URLs para diferentes entornos (desarrollo y produccion)
namespace app_subastas;

public static class Constants
{
    // FORZADO - Usar Render
    public const string BaseUrl = "https://api-subastas-backup.onrender.com/api";
    public const string WsUrl = "wss://api-subastas-backup.onrender.com/ws-subastas/websocket";
}