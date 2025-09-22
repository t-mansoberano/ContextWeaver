using ContextWeaver.Core;

namespace ContextWeaver.Interfaces;

/// <summary>
/// PATRÓN DE DISEÑO: Strategy (Estrategia).
/// Esta interfaz define el "contrato" para una familia de algoritmos: los analizadores de archivos.
/// Cualquier clase que sepa cómo analizar un tipo de archivo debe implementar esta interfaz.
///
/// PRINCIPIO DE DISEÑO: Abstracción y BAJO ACOPLAMIENTO.
/// El servicio principal (CodeAnalyzerService) no conocerá los detalles de cómo se analiza un
/// archivo C# o TypeScript, solo interactuará con esta abstracción. Esto desacopla
/// la lógica de orquestación de la lógica de análisis específica.
/// </summary>
public interface IFileAnalyzer
{
    bool CanAnalyze(FileInfo file);
    Task<FileAnalysisResult> AnalyzeAsync(FileInfo file);
}