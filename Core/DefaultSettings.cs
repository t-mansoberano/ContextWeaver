namespace ContextWeaver.Core;

/// <summary>
/// Proporciona los valores de configuración por defecto para la aplicación.
/// </summary>
public static class DefaultSettings
{
    public static AnalysisSettings Get() => new()
    {
        // CAMBIO: Se usa new string[] en lugar de new List<string>
        IncludedExtensions = new string[]
        {
            ".cs", ".csproj", ".sln", ".json", ".ts", ".html", ".scss", ".css", ".md"
        },
        // CAMBIO: Se usa new string[] en lugar de new List<string>
        ExcludePatterns = new string[]
        {
            "bin", "obj", "node_modules", ".angular", ".vs", "dist", "wwwroot", "Publish", "packages", "Scripts", "Content"
        }
    };
}