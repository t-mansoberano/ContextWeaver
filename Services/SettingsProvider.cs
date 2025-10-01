using System.Text.Json;
using ContextWeaver.Core;
using Microsoft.Extensions.Configuration;

// <-- Añadir este using

namespace ContextWeaver.Services;

public class SettingsProvider
{
    // El constructor ahora puede estar vacío, ya no depende de IOptions

    public AnalysisSettings LoadSettingsFor(DirectoryInfo directory)
    {
        var localConfigPath = Path.Combine(directory.FullName, ".contextweaver.json");

        if (File.Exists(localConfigPath))
        {
            // Si el archivo ya existe, intentamos leerlo como antes.
            Console.WriteLine("✅ Se encontró '.contextweaver.json'. Usándolo para este análisis.");
            try
            {
                using var stream = File.OpenRead(localConfigPath);
                var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
                var section = config.GetSection("AnalysisSettings");

                if (!section.Exists())
                {
                    Console.Error.WriteLine(
                        "⚠️ La sección 'AnalysisSettings' no existe. Se usará la configuración por defecto.");
                    return DefaultSettings.Get();
                }

                var localSettings = section.Get<AnalysisSettings>();
                if (localSettings != null && (localSettings.IncludedExtensions?.Any() == true ||
                                              localSettings.ExcludePatterns?.Any() == true))
                {
                    Console.WriteLine("-> Configuración local aplicada exitosamente.");
                    return localSettings;
                }

                Console.WriteLine(
                    "-> La configuración local está vacía o incompleta. Se usará la configuración por defecto.");
                return DefaultSettings.Get();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"⚠️ Error al leer '{localConfigPath}': {ex.Message}. Se usará la configuración por defecto.");
                return DefaultSettings.Get();
            }
        }

        // --- NUEVA LÓGICA: El archivo NO existe, así que lo creamos ---
        Console.WriteLine("✅ No se encontró '.contextweaver.json'. Se creará uno nuevo con valores por defecto.");
        var defaultSettings = DefaultSettings.Get();

        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(new { AnalysisSettings = defaultSettings }, options);
            File.WriteAllText(localConfigPath, jsonString);
            Console.WriteLine($"-> Archivo de configuración creado en: {localConfigPath}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"⚠️ No se pudo crear el archivo de configuración: {ex.Message}. Se continuará con la configuración por defecto en memoria.");
        }

        return defaultSettings;
    }
}