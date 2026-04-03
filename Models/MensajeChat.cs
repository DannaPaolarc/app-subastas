namespace app_subastas.Models;

public class MensajeChat
{
    // Nombre del usuario que envia el mensaje
    public string usuario { get; set; } = string.Empty;

    // Contenido del mensaje
    public string contenido { get; set; } = string.Empty;

    // Tipo de mensaje: CHAT, PUJA, SISTEMA, ADMIN
    public string tipo { get; set; } = string.Empty;

    // Hora de envio en formato HH:mm
    public string hora { get; set; } = string.Empty;

    // Identificador de la subasta a la que pertenece el mensaje
    public long subastaId { get; set; }
}