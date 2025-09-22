using ContextWeaver.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ContextWeaver.Services;

public class SettingsProvider
{
    private readonly AnalysisSettings _defaultSettings;

    public SettingsProvider(IOptions<AnalysisSettings> defaultSettings)
    {
        _defaultSettings = defaultSettings.Value;
    }

    public AnalysisSettings LoadSettingsFor(DirectoryInfo directory)
    {
        var localConfigPath = Path.Combine(directory.FullName, ".contextweaver.json");

        if (!File.Exists(localConfigPath))
        {
            return _defaultSettings;
        }

        Console.WriteLine("✅ Se encontró '.contextweaver.json'. Intentando usarlo para este análisis.");
        try
        {
            using var stream = File.OpenRead(localConfigPath);
            var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
            var section = config.GetSection("AnalysisSettings");

            if (!section.Exists())
            {
                Console.Error.WriteLine("⚠️ La sección 'AnalysisSettings' no existe. Se usará la configuración por defecto.");
                return _defaultSettings;
            }

            var localSettings = section.Get<AnalysisSettings>();
            if (localSettings != null && (localSettings.IncludedExtensions?.Any() == true || localSettings.ExcludePatterns?.Any() == true))
            {
                Console.WriteLine("-> Configuración local aplicada exitosamente.");
                return localSettings;
            }

            Console.WriteLine("-> La configuración local está vacía o incompleta. Se usará la configuración por defecto.");
            return _defaultSettings;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"⚠️ Error al leer '{localConfigPath}': {ex.Message}. Se usará la configuración por defecto.");
            return _defaultSettings;
        }
    }
}