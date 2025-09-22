using ContextWeaver.Core;

namespace ContextWeaver.Interfaces;

/// <summary>
/// PATRÓN DE DISEÑO: Strategy (Estrategia).
/// Al igual que IFileAnalyzer, esta interfaz define un contrato para la familia de algoritmos
/// que generan reportes. Cada formato de salida (Markdown, XML, etc.) será una estrategia concreta.
///
/// PRINCIPIO DE DISEÑO: Principio de Abierto/Cerrado (OCP) de SOLID.
/// El sistema está "abierto" a la extensión (podemos agregar nuevos generadores de reportes)
/// pero "cerrado" a la modificación (no necesitamos cambiar el CodeAnalyzerService para hacerlo).
/// </summary>
public interface IReportGenerator
{
    string Format { get; }
    string Generate(DirectoryInfo directory, List<FileAnalysisResult> results, Dictionary<string, (int Ca, int Ce, double Instability)> instabilityMetrics);
}