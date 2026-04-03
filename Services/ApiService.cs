using app_subastas.Models;
using System.Text.Json;

namespace app_subastas.Services;

public class ApiService
{
    // Cliente HTTP para realizar peticiones al backend
    private readonly HttpClient _http = new HttpClient();

    /*
     * Obtiene todas las subastas activas desde el backend
     * URL: GET /api/subastas/activas
     * Retorna: Lista de subastas con estado ACTIVA
     * Si hay error, retorna lista vacia
     */
    public async Task<List<Subasta>> GetSubastasActivas()
    {
        try
        {
            // Realizar peticion GET al endpoint de subastas activas
            var res = await _http.GetStringAsync($"{Constants.BaseUrl}/subastas/activas");

            // Deserializar respuesta JSON a lista de objetos Subasta
            var subastas = JsonSerializer.Deserialize<List<Subasta>>(res, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true  // Ignorar mayusculas/minusculas en nombres de propiedades
            }) ?? new List<Subasta>();

            return subastas;
        }
        catch (Exception ex)
        {
            // Registrar error en consola y retornar lista vacia
            Console.WriteLine($"Error en GetSubastasActivas: {ex.Message}");
            return new List<Subasta>();
        }
    }
}