namespace app_subastas.Models;

public class UsuarioUI
{
    // Nombre del usuario que aparece en el ranking
    public string nombre { get; set; } = string.Empty;

    // Monto de la oferta del usuario
    public double monto { get; set; }
}