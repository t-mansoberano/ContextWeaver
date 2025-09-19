namespace ContextWeaver;

/// <summary>
/// BUENA PRÁCTICA: Options Pattern.
/// Esta es una clase POCO (Plain Old CLR Object) que se usa para vincular
/// fuertemente las secciones del archivo appsettings.json. Esto proporciona
/// seguridad de tipos al acceder a la configuración.
/// </summary>
public class AnalysisSettings
{
    public string[] IncludedExtensions { get; set; } = [];
    public string[] ExcludePatterns { get; set; } = [];
}