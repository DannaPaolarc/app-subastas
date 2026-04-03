namespace app_subastas.Models;

public class Oferta
{
    // Monto de la oferta
    public double monto { get; set; }

    // Usuario que realizo la oferta
    public Usuario usuario { get; set; } = new Usuario();
}