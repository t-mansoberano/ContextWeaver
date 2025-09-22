using ContextWeaver.Core;
using ContextWeaver.Interfaces;

namespace ContextWeaver.Analyzers;

/// <summary>
/// PATRÓN DE DISEÑO: Concrete Strategy (Estrategia Concreta).
/// Esta clase es una implementación "genérica" de IFileAnalyzer para archivos de texto
/// que no requieren un análisis complejo, solo conteo de líneas y extracción de contenido.
///
/// PRINCIPIO DE DISEÑO: ALTA COHESIÓN y SRP.
/// Su única responsabilidad es manejar un conjunto predefinido de extensiones de archivo de texto.
/// </summary>
public class GenericFileAnalyzer : IFileAnalyzer
{
    private readonly string[] _supportedExtensions = { ".ts", ".js", ".html", ".css", ".scss", ".json", ".md", ".csproj", ".sln" };
    private readonly Dictionary<string, string> _languageMap = new()
    {
        { ".ts", "typescript" }, { ".js", "javascript" }, { ".html", "html" },
        { ".css", "css" }, { ".scss", "scss" }, { ".json", "json" },
        { ".md", "markdown" }, { ".csproj", "xml" }, { ".sln", "plaintext" }
    };

    public bool CanAnalyze(FileInfo file) => _supportedExtensions.Contains(file.Extension.ToLower());

    public async Task<FileAnalysisResult> AnalyzeAsync(FileInfo file)
    {
        var content = await File.ReadAllTextAsync(file.FullName);
        return new FileAnalysisResult
        {
            LinesOfCode = content.Split('\n').Length,
            CodeContent = content,
            Language = _languageMap.GetValueOrDefault(file.Extension.ToLower(), "plaintext")
        };
    }
}