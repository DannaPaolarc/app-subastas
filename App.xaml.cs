// Clase principal de la aplicacion, punto de entrada
namespace app_subastas
{
    public partial class App : Application
    {
        // Constructor: inicializa componentes
        public App()
        {
            InitializeComponent();
        }

        // Crea la ventana principal de la aplicacion
        // Establece MainPage como pantalla de inicio
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new NavigationPage(new MainPage()));  // ← Envuelve MainPage en NavigationPage
            window.Title = "";
            return window;
        }
    }
}