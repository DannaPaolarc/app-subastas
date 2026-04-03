namespace app_subastas.Models;

public class Usuario
{
    // Identificador unico del usuario
    public long id { get; set; }

    // Nombre completo del usuario
    public string nombre { get; set; } = string.Empty;

    // Correo electronico del usuario
    public string correo { get; set; } = string.Empty;

    // Direccion de envio del usuario
    public string direccion { get; set; } = string.Empty;

    // Numero de telefono del usuario
    public string telefono { get; set; } = string.Empty;

    // Rol del usuario: USUARIO o ADMIN
    public string rol { get; set; } = string.Empty;
}