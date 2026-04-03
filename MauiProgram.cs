using Plugin.Maui.Audio;
using app_subastas;
using app_subastas.Views;
using app_subastas.Converters;

public static class MauiProgram
{
    // Crea y configura la aplicacion MAUI
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Usar la aplicacion principal
        builder
            .UseMauiApp<App>();

        // Registrar servicios de inyeccion de dependencias
        builder.Services.AddSingleton(AudioManager.Current);  // Servicio de audio para toda la app
        builder.Services.AddTransient<HomePage>();            // Pagina HomePage como servicio transitorio
        builder.Services.AddSingleton<EstadoColorConverter>(); // Converter para colores de estado

        return builder.Build();
    }
}