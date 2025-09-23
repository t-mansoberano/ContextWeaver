using System.Text;
using ContextWeaver.Core;
using ContextWeaver.Interfaces;
using ContextWeaver.Utilities;

namespace ContextWeaver.Reporters;

/// <summary>
/// PATRÓN DE DISEÑO: Concrete Strategy (Estrategia Concreta).
/// Implementación de IReportGenerator que sabe cómo construir un reporte en formato Markdown.
///
/// PRINCIPIO DE DISEÑO: ALTA COHESIÓN y SRP.
/// Toda la lógica de formato de Markdown reside exclusivamente en esta clase.
/// </summary>
public class MarkdownReportGenerator : IReportGenerator
{
    public string Format => "markdown";

    public string Generate(DirectoryInfo directory, List<FileAnalysisResult> results, Dictionary<string, (int Ca, int Ce, double Instability)> instabilityMetrics)
    {
        var reportBuilder = new StringBuilder();
        var sortedResults = results.OrderBy(r => r.RelativePath).ToList();

        reportBuilder.Append(GenerateHeader(directory));
        reportBuilder.Append(GenerateHotspots(sortedResults));
        reportBuilder.Append(GenerateInstabilityReport(instabilityMetrics)); // <-- NUEVA SECCIÓN
        reportBuilder.Append(GenerateDirectoryTree(sortedResults, directory.Name));
        reportBuilder.Append(GenerateFileContent(sortedResults));

        return reportBuilder.ToString();
    }

    private string GenerateHeader(DirectoryInfo directory)
    {
        return $"""
                Este archivo es una representación consolidada del código fuente de '{directory.Name}', fusionado en un único documento por ContextWeaver.
                El contenido ha sido procesado para crear un contexto completo para su análisis.

                # Resumen del Archivo

                ## Propósito
                Este archivo contiene una representación empaquetada de los contenidos del repositorio.
                Está diseñado para ser fácilmente consumible por sistemas de IA para análisis, revisión de código u 
                otros procesos automatizados.

                ## Formato del Archivo
                El contenido se organiza de la siguiente manera:
                1. Esta sección de resumen.
                2. Una sección de "Análisis de Hotspots" que identifica archivos clave por métricas.
                3. Una sección de "Análisis de Inestabilidad" que proporciona información arquitectónica.
                4. Un árbol de la estructura de directorios con enlaces clicables a cada archivo.
                5. Múltiples entradas de archivo, cada una de las cuales consta de:
                   - Un encabezado con la ruta del archivo (## Archivo: ruta/al/archivo)
                   - El resumen del "Repo Map" (API pública e importaciones).
                   - El contenido completo del archivo en un bloque de código.

                ## Pautas de Uso
                - Este archivo debe ser tratado como de solo lectura. Cualquier cambio debe realizarse en 
                  los archivos originales del repositorio, no en esta versión empaquetada.
                - Al procesar este archivo, use la ruta del archivo para distinguir entre los diferentes 
                  archivos del repositorio.
                - Tenga en cuenta que este archivo puede contener información sensible. Manéjelo con el mismo 
                  nivel de seguridad que manejaría el repositorio original.

                ## Notas
                - Algunos archivos pueden haber sido excluidos según la configuración de ContextWeaver en `appsettings.json`.
                - Los archivos binarios no se incluyen en esta representación empaquetada.
                - Los archivos se ordenan alfabéticamente por su ruta completa para una ordenación consistente.

                """;
    }

    /// <summary>
    /// Genera la sección de Hotspots, mostrando los top 5 archivos por LOC y por número de imports.
    /// </summary>
    private string GenerateHotspots(List<FileAnalysisResult> results)
    {
        var hotspotsBuilder = new StringBuilder();
        hotspotsBuilder.AppendLine("# 🔥 Análisis de Hotspots");
        hotspotsBuilder.AppendLine();

        // --- Top 5 por Líneas de Código (LOC) ---
        hotspotsBuilder.AppendLine("## 5 Principales Archivos por Líneas de Código (LOC)");
        var topByLoc = results.OrderByDescending(r => r.LinesOfCode).Take(5);
        foreach (var result in topByLoc)
        {
            var headerText = $"File: {result.RelativePath}";
            var anchor = MarkdownHelper.CreateAnchor(headerText);
            hotspotsBuilder.AppendLine($"* **({result.LinesOfCode} LOC)** - [`{result.RelativePath}`](#{anchor})");
        }
        hotspotsBuilder.AppendLine();

        // --- Top 5 por Número de Imports ---
        hotspotsBuilder.AppendLine("## 5 Principales Archivos por Número de Importaciones");
        var topByImports = results
            .Select(r => new {
                Result = r,
                ImportCount = r.Usings.Count // <-- Acceso directo a la propiedad Usings.Count
            })
            .Where(x => x.ImportCount > 0)
            .OrderByDescending(x => x.ImportCount)
            .Take(5);

        foreach (var item in topByImports)
        {
            var headerText = $"File: {item.Result.RelativePath}";
            var anchor = MarkdownHelper.CreateAnchor(headerText);
            hotspotsBuilder.AppendLine($"* **({item.ImportCount} Imports)** - [`{item.Result.RelativePath}`](#{anchor})");
        }
        hotspotsBuilder.AppendLine();

        return hotspotsBuilder.ToString();
    }
    
    private string GenerateInstabilityReport(Dictionary<string, (int Ca, int Ce, double Instability)> instabilityMetrics)
    {
        var reportBuilder = new StringBuilder();
        reportBuilder.AppendLine("# 📊 Análisis de Inestabilidad");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("Esta sección estima la métrica de Inestabilidad (I) para cada módulo de nivel superior (carpeta/proyecto) basándose en sus dependencias (importaciones).");
        reportBuilder.AppendLine("`I = Ce / (Ca + Ce)`");
        reportBuilder.AppendLine("- `Ce` (Eferente): Cuántos otros módulos usa este módulo (apunta hacia afuera).");
        reportBuilder.AppendLine("- `Ca` (Aferente): Cuántos otros módulos dependen de este módulo (apunta hacia adentro).");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("## Resumen de Inestabilidad del Módulo:");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("| Módulo | Ca (Eferente) | Ce (Aferente) | Inestabilidad (I) | Descripción |");
        reportBuilder.AppendLine("|---|---|---|---|---|");

        foreach (var entry in instabilityMetrics.OrderBy(e => e.Key))
        {
            var module = entry.Key;
            var (ca, ce, instability) = entry.Value;
            var description = GetInstabilityDescription(instability);
            reportBuilder.AppendLine($"| `{module}` | {ca} | {ce} | {instability:F2} | {description} |");
        }
        reportBuilder.AppendLine();
        
        reportBuilder.AppendLine("## Guía de Interpretación:");
        reportBuilder.AppendLine("- `I ≈ 0`: Muy estable (muchos dependen de él; depende poco de otros). A menudo son contratos/interfaces principales.");
        reportBuilder.AppendLine("- `I ≈ 1`: Muy inestable (depende de muchos; pocos o ninguno dependen de él). A menudo son implementaciones concretas como UI/adaptadores.");
        reportBuilder.AppendLine("- `I ≈ 0.5`: Estabilidad intermedia.");
        reportBuilder.AppendLine("Idealmente, los módulos estables deben ser abstractos y los inestables concretos. Evite módulos abstractos muy inestables o módulos concretos muy estables.");
        reportBuilder.AppendLine();

        return reportBuilder.ToString();
    }

    /// <summary>
    /// Proporciona una descripción textual de la inestabilidad.
    /// </summary>
    private string GetInstabilityDescription(double instability)
    {
        if (instability <= 0.2) return "Muy estable / Core";
        if (instability >= 0.8) return "Muy inestable / Concreto";
        return "Estabilidad intermedia";
    }

    #region Directory Tree Generation

    private class TreeNode
    {
        public string Name { get; set; }
        public string? Path { get; set; }
        public SortedDictionary<string, TreeNode> Children { get; } = new();
    }

    /// <summary>
    /// Genera la sección de estructura de directorios con un formato de árbol avanzado.
    /// </summary>
    private string GenerateDirectoryTree(List<FileAnalysisResult> results, string rootName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Directory Structure");
        sb.AppendLine();

        var root = BuildTree(results);

        // Nodo raíz
        sb.AppendLine($"- {rootName}/");
        AppendDirectoryStructureWithLinks(root.Children.Values, sb, 1);

        sb.AppendLine();
        return sb.ToString();
    }

    private TreeNode BuildTree(List<FileAnalysisResult> results)
    {
        var root = new TreeNode { Name = "" };
        foreach (var result in results)
        {
            var currentNode = root;
            var pathParts = result.RelativePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < pathParts.Length; i++)
            {
                var part = pathParts[i];
                if (!currentNode.Children.ContainsKey(part))
                {
                    currentNode.Children[part] = new TreeNode { Name = part };
                }
                currentNode = currentNode.Children[part];
                if (i == pathParts.Length - 1)
                {
                    currentNode.Path = result.RelativePath;
                }
            }
        }
        return root;
    }
    
    private void AppendDirectoryStructureWithLinks(IEnumerable<TreeNode> nodes, StringBuilder sb, int level)
    {
        var indent = new string(' ', level * 4);

        // Directorios primero, luego archivos; ambos ordenados por nombre
        var directories = nodes.Where(n => n.Path == null).OrderBy(n => n.Name);
        var files = nodes.Where(n => n.Path != null).OrderBy(n => n.Name);

        foreach (var dir in directories)
        {
            sb.AppendLine($"{indent}- {dir.Name}/");
            AppendDirectoryStructureWithLinks(dir.Children.Values, sb, level + 1);
        }

        foreach (var file in files)
        {
            var headerText = $"File: {file.Path}";
            var anchor = MarkdownHelper.CreateAnchor(headerText);
            sb.AppendLine($"{indent}- [{file.Name}](#{anchor})");
        }
    }

    #endregion

    /// <summary>
    /// Genera el contenido de todos los archivos con sus respectivas métricas y mapa de repositorio.
    /// </summary>
    private string GenerateFileContent(List<FileAnalysisResult> results)
    {
        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("# Archivos");
        contentBuilder.AppendLine();

        foreach (var result in results)
        {
            contentBuilder.AppendLine($"## File: {result.RelativePath}");
            contentBuilder.AppendLine();
            
            // --- NUEVA SECCIÓN DE REPO MAP ---
            if (result.Metrics.TryGetValue("PublicApiSignatures", out object? publicApiObj) && publicApiObj is List<string> publicApi)
            {
                contentBuilder.AppendLine("### Repo Map: Extraer solo firmas públicas y imports de cada archivo");
                contentBuilder.AppendLine("#### API Publica:");
                foreach (var signature in publicApi)
                {
                    contentBuilder.AppendLine(signature);
                }
                contentBuilder.AppendLine(); // Línea en blanco para separación
            }

            if (result.Metrics.TryGetValue("Usings", out object? usingsObj) && usingsObj is List<string> usings)
            {
                contentBuilder.AppendLine("#### Imports:");
                foreach (var singleUsing in usings)
                {
                    contentBuilder.AppendLine($"- {singleUsing}");
                }
                contentBuilder.AppendLine(); // Línea en blanco para separación
            }
            // --- FIN NUEVA SECCIÓN ---

            // Información de métricas existente
            contentBuilder.AppendLine("#### Métricas");
            contentBuilder.AppendLine($"* **Lineas de Código (LOC):** {result.LinesOfCode}");
            // Muestra otras métricas, excluyendo las que ya tratamos explícitamente como "Repo Map"
            foreach (var metric in result.Metrics.Where(m => m.Key != "PublicApiSignatures" && m.Key != "Usings"))
            {
                contentBuilder.AppendLine($"* **{metric.Key}:** {metric.Value}");
            }
            contentBuilder.AppendLine();
            
            // Sección de Código Fuente
            contentBuilder.AppendLine("#### Source Code");
            // Nota: Aquí se muestra el código completo, podrías añadir la lógica para "Fuente: líneas 1-X" si es necesario
            // Por ahora, el "CodeContent" ya tiene todo el código y las "LinesOfCode" te dan el rango.
            contentBuilder.AppendLine("```" + result.Language);
            contentBuilder.AppendLine(result.CodeContent.Trim());
            contentBuilder.AppendLine("```");
            contentBuilder.AppendLine();
        }

        return contentBuilder.ToString();
    }
}