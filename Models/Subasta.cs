namespace app_subastas.Models;

public class Subasta
{
    // Identificador unico de la subasta
    public long id { get; set; }

    // Nombre del producto en subasta
    public string producto { get; set; } = string.Empty;

    // Descripcion del producto
    public string descripcion { get; set; } = string.Empty;

    // URL de la imagen del producto
    public string imageUrl { get; set; } = string.Empty;

    // Precio actual de la subasta (se actualiza con cada oferta)
    public double precioActual { get; set; }

    // Estado: PENDIENTE, ACTIVA, FINALIZADA
    public string estado { get; set; } = string.Empty;

    // Fecha y hora de finalizacion (nullable)
    public DateTime? tiempoFin { get; set; }

    // Nombre del usuario ganador (nullable)
    public string? ganador { get; set; }
}