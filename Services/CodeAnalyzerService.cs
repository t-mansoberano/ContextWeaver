using ContextWeaver.Core;
using ContextWeaver.Interfaces;
using Microsoft.Extensions.Options;

namespace ContextWeaver.Services;
// Necesario para limpiar nombres de namespaces

/// <summary>
/// PATRÓN DE DISEÑO: Context (en el patrón Strategy).
/// Esta clase es el orquestador central. No realiza el trabajo pesado por sí misma,
/// sino que coordina y delega las tareas a las estrategias adecuadas (los analizadores y generadores).
/// </summary>
public class CodeAnalyzerService
{
    private readonly AnalysisSettings _settings;
    private readonly IEnumerable<IFileAnalyzer> _analyzers;
    private readonly IEnumerable<IReportGenerator> _generators;

    /// <summary>
    /// PRINCIPIO DE DISEÑO: Inyección de Dependencias (Dependency Injection) y
    /// Principio de Inversión de Dependencias (DIP) de SOLID.
    /// Esta clase no crea sus propias dependencias (new CSharpAnalyzer(), etc.). En su lugar,
    /// las recibe a través del constructor.
    ///
    /// MÁS IMPORTANTE: Depende de ABSTRACCIONES (IEnumerable<IFileAnalyzer>), no de
    /// implementaciones concretas. Esto es el corazón del BAJO ACOPLAMIENTO y la FLEXIBILIDAD.
    /// </summary>
    public CodeAnalyzerService(
        IOptions<AnalysisSettings> settings,
        IEnumerable<IFileAnalyzer> analyzers,
        IEnumerable<IReportGenerator> generators)
    {
        _settings = settings.Value;
        _analyzers = analyzers;
        _generators = generators;
    }

    /// <summary>
    /// PRINCIPIO DE DISEÑO: Principio de Responsabilidad Única (SRP) de SOLID.
    /// La única responsabilidad de este método es ORQUESTRAR el flujo de trabajo:
    /// 1. Encontrar el generador de reportes correcto.
    /// 2. Encontrar y filtrar los archivos a analizar.
    /// 3. Delegar el análisis de cada archivo a la estrategia correcta.
    /// 4. Delegar la generación del reporte final a la estrategia de reporte.
    /// No sabe NADA sobre Roslyn o sobre cómo formatear Markdown.
    /// </summary>
    public async Task AnalyzeAndGenerateReport(DirectoryInfo directory, FileInfo outputFile, string format)
    {
        var generator = _generators.FirstOrDefault(g => g.Format.Equals(format, StringComparison.OrdinalIgnoreCase));
        if (generator == null)
        {
            Console.Error.WriteLine($"Error: El formato de salida '{format}' no es soportado.");
            return;
        }

        var allFiles = directory.GetFiles("*.*", SearchOption.AllDirectories)
            // 1. Filtrar para EXCLUIR los directorios no deseados.
            .Where(f => !_settings.ExcludePatterns.Any(p => f.FullName.Contains(Path.DirectorySeparatorChar + p + Path.DirectorySeparatorChar)))
            // 2. CORRECCIÓN: Filtrar para INCLUIR solo las extensiones deseadas.
            .Where(f => _settings.IncludedExtensions.Contains(f.Extension.ToLowerInvariant()))
            .ToList();

        var analysisResults = new List<FileAnalysisResult>();
        foreach (var file in allFiles)
        {
            var analyzer = _analyzers.FirstOrDefault(a => a.CanAnalyze(file));
            if (analyzer != null)
            {
                var result = await analyzer.AnalyzeAsync(file);
                // Asegurarse que RelativePath no empieza con '/' y si el archivo está en la raíz,
                // su RelativePath es solo el nombre del archivo.
                result.RelativePath = file.FullName.Replace(directory.FullName, "").Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
                analysisResults.Add(result);
            }
        }
        
        // --- NUEVA LÓGICA: Calcular inestabilidad ---
        var instabilityMetrics = CalculateInstabilityMetrics(directory.Name, analysisResults);
        // --- FIN NUEVA LÓGICA ---

        // Pasamos las métricas de inestabilidad al generador de reportes
        var reportContent = generator.Generate(directory, analysisResults, instabilityMetrics); 
        
        await File.WriteAllTextAsync(outputFile.FullName, reportContent);
        
        Console.WriteLine($"✅ Reporte en formato '{format}' generado exitosamente en: {outputFile.FullName}");
    }

    /// <summary>
    /// Calcula las métricas de inestabilidad (I = Ce / (Ca + Ce)) para cada "módulo" (carpeta principal o proyecto).
    /// Se ignoran archivos que no son de código o no tienen sentencias 'using'.
    /// </summary>
    private Dictionary<string, (int Ca, int Ce, double Instability)> CalculateInstabilityMetrics(string rootDirectoryName, List<FileAnalysisResult> results)
    {
        var moduleDependencies = new Dictionary<string, HashSet<string>>(); // Módulo -> Dependencias eferentes
        var moduleNames = new HashSet<string>();

        // 1. Identificar módulos y sus dependencias eferentes (solo para archivos de código con usings)
        foreach (var result in results)
        {
            // Solo consideramos archivos que tienen usings (presumiblemente archivos de código)
            if (result.Usings == null || !result.Usings.Any())
            {
                continue; // Ignorar archivos sin usings (ej. .md, .html, .json)
            }

            // Determinar el módulo del archivo. Si la ruta relativa solo tiene un componente
            // (ej. "Program.cs" si está en la raíz del proyecto), entonces el módulo es el directorio raíz.
            var pathParts = result.RelativePath.Split('/');
            var currentModule = pathParts.Length > 1 ? pathParts[0] : rootDirectoryName; // Si está en la raíz, el módulo es el nombre del directorio raíz.

            moduleNames.Add(currentModule);

            if (!moduleDependencies.ContainsKey(currentModule))
            {
                moduleDependencies[currentModule] = new HashSet<string>();
            }

            foreach (var usedNamespace in result.Usings)
            {
                // Limpiar el namespace para facilitar la comparación (ej. "Namespace.Sub.Class" -> "Namespace")
                var cleanedUsedNamespace = usedNamespace.Split('.')[0]; 

                // Intentar mapear el namespace usado a un módulo conocido
                // Considerar módulos que empiecen con el mismo nombre o lo contengan como una palabra completa.
                var matchingModule = moduleNames.FirstOrDefault(m =>
                    m.Equals(cleanedUsedNamespace, StringComparison.OrdinalIgnoreCase) || // Coincidencia exacta con el primer segmento
                    usedNamespace.StartsWith($"{m}.", StringComparison.OrdinalIgnoreCase) || // El using empieza con el nombre del módulo y un punto
                    usedNamespace.Contains($".{m}.", StringComparison.OrdinalIgnoreCase)    // El using contiene el nombre del módulo rodeado de puntos
                );

                if (matchingModule != null && matchingModule != currentModule)
                {
                    moduleDependencies[currentModule].Add(matchingModule);
                }
            }
        }
        
        // 2. Inicializar Ca, Ce para todos los módulos que hemos identificado (solo módulos de código)
        var moduleMetrics = new Dictionary<string, (int Ca, int Ce, double Instability)>();
        foreach (var module in moduleNames)
        {
            moduleMetrics[module] = (Ca: 0, Ce: 0, Instability: 0.0);
        }

        // Si no hay módulos con dependencias, retornar un diccionario vacío.
        if (!moduleDependencies.Any())
        {
            return new Dictionary<string, (int Ca, int Ce, double Instability)>();
        }

        // 3. Calcular Ce (eferentes)
        foreach (var entry in moduleDependencies)
        {
            var module = entry.Key;
            var ce = entry.Value.Count;
            moduleMetrics[module] = (moduleMetrics[module].Ca, ce, 0.0); // Actualiza Ce
        }

        // 4. Calcular Ca (aferentes)
        foreach (var outerModule in moduleDependencies.Keys)
        {
            foreach (var dependentModule in moduleDependencies[outerModule])
            {
                if (moduleMetrics.ContainsKey(dependentModule))
                {
                    moduleMetrics[dependentModule] = (moduleMetrics[dependentModule].Ca + 1, moduleMetrics[dependentModule].Ce, 0.0); // Incrementa Ca
                }
            }
        }

        // 5. Calcular Inestabilidad (I)
        var finalInstabilityMetrics = new Dictionary<string, (int Ca, int Ce, double Instability)>();
        foreach (var module in moduleNames) // Iterar sobre moduleNames para incluir módulos con Ca=0, Ce=0
        {
            var (ca, ce, _) = moduleMetrics[module];
            double instability = (ca + ce == 0) ? 0.0 : (double)ce / (ca + ce);
            finalInstabilityMetrics[module] = (ca, ce, instability);
        }

        return finalInstabilityMetrics;
    }
}