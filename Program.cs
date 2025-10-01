using System.CommandLine;
using ContextWeaver.Extensions;
using ContextWeaver.Services;
using Microsoft.Extensions.DependencyInjection;


// ARQUITECTURA: Top-Level Statements.
// Este archivo ahora actúa como el punto de entrada directo de la aplicación.
// Todo el código aquí se ejecuta dentro del "Main" implícito.

// BUENA PRÁCTICA: Usar una librería robusta como System.CommandLine para la
// interfaz de línea de comandos (CLI). Evita parsear 'args' manualmente,
// lo que es propenso a errores y poco mantenible.
var rootCommand = new RootCommand("Herramienta de análisis y extracción de código para LLMs.");

var directoryOption = new Option<DirectoryInfo>(
    new[] { "-d", "--directorio" },
    // La función getDefaultValue se ejecuta si el usuario no provee este parámetro.
    // "." es una forma universal de referirse al directorio actual.
    () => new DirectoryInfo("."),
    "El directorio raíz del proyecto a analizar. Por defecto, es el directorio actual.");
// Ya no se necesita la propiedad { IsRequired = true }, porque ahora tiene un valor por defecto.

var outputOption = new Option<FileInfo>(
    new[] { "-o", "--output" },
    () => new FileInfo("analysis_report.md"),
    "El archivo de salida para el reporte consolidado.");

var formatOption = new Option<string>(
    new[] { "-f", "--format" },
    () => "markdown",
    "El formato del reporte de salida.");

rootCommand.AddOption(directoryOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(formatOption);

rootCommand.SetHandler(async (directory, outputFile, format) =>
{
    // BUENA PRÁCTICA: Usar el "Generic Host" de .NET para configurar la aplicación.
    // Llamamos al método de extensión para crear el host.
    var host = HostBuilderExtensions.CreateHostBuilder(args).Build();
    var service = host.Services.GetRequiredService<CodeAnalyzerService>();
    await service.AnalyzeAndGenerateReport(directory, outputFile, format);
}, directoryOption, outputOption, formatOption);

// La llamada final al comando ejecuta la lógica.
return await rootCommand.InvokeAsync(args);