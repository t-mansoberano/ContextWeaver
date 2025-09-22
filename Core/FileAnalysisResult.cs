namespace ContextWeaver.Core;

/// <summary>
/// BUENA PRÁCTICA: Data Transfer Object (DTO).
/// Esta clase es un POCO (Plain Old CLR Object) cuya única responsabilidad es transportar datos
/// entre las capas de la aplicación (desde los analizadores hasta los generadores de reportes).
///
/// PRINCIPIO DE DISEÑO: ALTA COHESIÓN.
/// La clase solo contiene datos relacionados con el resultado de un análisis de archivo.
/// No tiene lógica de negocio, lo que la hace cohesiva y fácil de entender.
/// </summary>
public class FileAnalysisResult
{
    public string RelativePath { get; set; } = string.Empty;
    public int LinesOfCode { get; set; }
    public string CodeContent { get; set; } = string.Empty;
    public string Language { get; set; } = "plaintext";
    public Dictionary<string, object> Metrics { get; } = new();
    
    // Nueva propiedad para los Usings, para tipado fuerte y fácil acceso.
    public List<string> Usings { get; set; } = new();     
}