using System.Text;
using ContextWeaver;

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
        This file is a merged representation of the codebase for '{directory.Name}', combined into a single document by ContextWeaver.
        The content has been processed to create a comprehensive context for analysis.

        # File Summary
        
        ## Purpose
        This file contains a packed representation of the repository's contents.
        It is designed to be easily consumable by AI systems for analysis, code review,
        or other automated processes.
        
        ## File Format
        The content is organized as follows:
        1. This summary section
        2. A "Hotspots" section identifying key files by metrics
        3. An "Instability Analysis" section providing architectural insights
        4. A directory structure tree with clickable links to each file
        5. Multiple file entries, each consisting of:
           a. A header with the file path (## File: path/to/file)
           b. The "Repo Map" summary (public API and imports)
           c. The full contents of the file in a code block
        
        ## Usage Guidelines
        - This file should be treated as read-only. Any changes should be made to the
          original repository files, not this packed version.
        - When processing this file, use the file path to distinguish
          between different files in the repository.
        - Be aware that this file may contain sensitive information. Handle it with
          the same level of security as you would the original repository.
        
        ## Notes
        - Some files may have been excluded based on ContextWeaver's configuration in `appsettings.json`.
        - Binary files are not included in this packed representation.
        - Files are sorted alphabetically by their full path for consistent ordering.
        
        """;
    }

    /// <summary>
    /// Genera la sección de Hotspots, mostrando los top 5 archivos por LOC y por número de imports.
    /// </summary>
    private string GenerateHotspots(List<FileAnalysisResult> results)
    {
        var hotspotsBuilder = new StringBuilder();
        hotspotsBuilder.AppendLine("# 🔥 Hotspots Analysis");
        hotspotsBuilder.AppendLine();

        // --- Top 5 por Líneas de Código (LOC) ---
        hotspotsBuilder.AppendLine("## Top 5 Files by Lines of Code (LOC)");
        var topByLoc = results.OrderByDescending(r => r.LinesOfCode).Take(5);
        foreach (var result in topByLoc)
        {
            var headerText = $"File: {result.RelativePath}";
            var anchor = MarkdownHelper.CreateAnchor(headerText);
            hotspotsBuilder.AppendLine($"* **({result.LinesOfCode} LOC)** - [`{result.RelativePath}`](#{anchor})");
        }
        hotspotsBuilder.AppendLine();

        // --- Top 5 por Número de Imports ---
        hotspotsBuilder.AppendLine("## Top 5 Files by Number of Imports");
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
        reportBuilder.AppendLine("# 📊 Instability Analysis (Optional)");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("This section estimates the Instability (I) metric for each top-level module (folder/project) based on its dependencies (imports).");
        reportBuilder.AppendLine("`I = Ce / (Ca + Ce)`");
        reportBuilder.AppendLine("- `Ce` (Efferent): How many other modules this module *uses* (points outwards).");
        reportBuilder.AppendLine("- `Ca` (Afferent): How many other modules *depend on* this module (point inwards).");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("## Module Instability Overview:");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("| Module | Ca (Afferent) | Ce (Efferent) | Instability (I) | Description |");
        reportBuilder.AppendLine("|---|---|---|---|---|");

        foreach (var entry in instabilityMetrics.OrderBy(e => e.Key))
        {
            var module = entry.Key;
            var (ca, ce, instability) = entry.Value;
            var description = GetInstabilityDescription(instability);
            reportBuilder.AppendLine($"| `{module}` | {ca} | {ce} | {instability:F2} | {description} |");
        }
        reportBuilder.AppendLine();
        
        reportBuilder.AppendLine("## Interpretation Guide:");
        reportBuilder.AppendLine("- `I ≈ 0`: Very stable (many depend on it; depends little on others). Often core contracts/interfaces.");
        reportBuilder.AppendLine("- `I ≈ 1`: Very unstable (depends on many; few or none depend on it). Often concrete implementations like UI/adapters.");
        reportBuilder.AppendLine("- `I ≈ 0.5`: Intermediate stability.");
        reportBuilder.AppendLine("Ideally, stable modules should be abstract, and unstable modules concrete. Avoid highly abstract, unstable modules, or highly concrete, stable modules.");
        reportBuilder.AppendLine();

        return reportBuilder.ToString();
    }

    /// <summary>
    /// Proporciona una descripción textual de la inestabilidad.
    /// </summary>
    private string GetInstabilityDescription(double instability)
    {
        if (instability <= 0.2) return "Very Stable / Core";
        if (instability >= 0.8) return "Very Unstable / Concrete";
        return "Intermediate Stability";
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
        var treeBuilder = new StringBuilder();
        treeBuilder.AppendLine("# Directory Structure");
        treeBuilder.AppendLine();

        treeBuilder.AppendLine("<pre>"); // Envuelve el árbol en <pre> para respetar el indentado
        
        var root = BuildTree(results);
        treeBuilder.AppendLine($"/{rootName}/");

        RenderNode(root, treeBuilder, "", true);

        treeBuilder.AppendLine("</pre>"); // Cierra la etiqueta <pre>
        
        treeBuilder.AppendLine();
        return treeBuilder.ToString();
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
    
    private void RenderNode(TreeNode node, StringBuilder builder, string indent, bool isLast)
    {
        var children = node.Children.Values.ToList();
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            bool isCurrentLast = (i == children.Count - 1);
            
            builder.Append(indent);
            builder.Append(isCurrentLast ? "└── " : "├── ");

            if (child.Path != null)
            {
                var headerText = $"File: {child.Path}";
                var anchor = MarkdownHelper.CreateAnchor(headerText);
                builder.AppendLine($"📄 [<a href=\"#{anchor}\">{child.Name}</a>]");
            }
            else
            {
                builder.AppendLine($"📁 {child.Name}/");
                RenderNode(child, builder, indent + (isCurrentLast ? "    " : "│   "), isCurrentLast);
            }
        }
    }
    #endregion

    /// <summary>
    /// Genera el contenido de todos los archivos con sus respectivas métricas y mapa de repositorio.
    /// </summary>
    private string GenerateFileContent(List<FileAnalysisResult> results)
    {
        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("# Files");
        contentBuilder.AppendLine();

        foreach (var result in results)
        {
            contentBuilder.AppendLine($"## File: {result.RelativePath}");
            contentBuilder.AppendLine();
            
            // --- NUEVA SECCIÓN DE REPO MAP ---
            if (result.Metrics.TryGetValue("PublicApiSignatures", out object? publicApiObj) && publicApiObj is List<string> publicApi)
            {
                contentBuilder.AppendLine("### Repo Map: Extraer solo firmas públicas y imports de cada archivo");
                contentBuilder.AppendLine("#### Public API:");
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
            contentBuilder.AppendLine("#### Metrics");
            contentBuilder.AppendLine($"* **Lines of Code (LOC):** {result.LinesOfCode}");
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