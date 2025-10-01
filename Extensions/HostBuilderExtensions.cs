using ContextWeaver.Analyzers;
using ContextWeaver.Interfaces;
using ContextWeaver.Reporters;
using ContextWeaver.Services;
using ContextWeaver.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ContextWeaver.Extensions;
// Importar el namespace de tu aplicación (cambia 'CodeExtractor' por 'ContextWeaver')

// BUENA PRÁCTICA: Clases de Extensión.
// Extender funcionalidades de clases existentes sin modificarlas.
// En este caso, para encapsular la configuración del IHostBuilder.
public static class HostBuilderExtensions
{
    /// <summary>
    ///     ARQUITECTURA: Composition Root (Raíz de Composición) - Parte de la configuración.
    ///     Este método centraliza la configuración de servicios de la aplicación.
    ///     Es el ÚNICO LUGAR donde las implementaciones concretas se "conectan" a sus abstracciones.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // PRINCIPIO DE DISEÑO: Inyección de Dependencias (DI).
                services.AddSingleton<SettingsProvider>();
                services.AddSingleton<InstabilityCalculator>();
                // Registramos el servicio principal de la aplicación.
                services.AddSingleton<CodeAnalyzerService>();

                // Aquí ocurre la magia del Patrón Strategy y el Principio de Abierto/Cerrado (OCP).
                // Registramos todas las estrategias de análisis de archivos. El contenedor DI
                // las inyectará automáticamente como un IEnumerable<IFileAnalyzer> donde sean requeridas.
                // Para añadir un nuevo analizador de archivos, solo se agrega una línea aquí,
                // sin modificar la lógica existente del CodeAnalyzerService.
                services.AddSingleton<IFileAnalyzer, CSharpFileAnalyzer>();
                services.AddSingleton<IFileAnalyzer, GenericFileAnalyzer>();

                // Hacemos lo mismo para los generadores de reportes.
                // Para añadir un nuevo formato de reporte (XML, YAML, etc.), se registra una nueva
                // implementación de IReportGenerator aquí, sin alterar CodeAnalyzerService.
                services.AddSingleton<IReportGenerator, MarkdownReportGenerator>();
            });
    }
}