namespace ContextWeaver;

using System.Text.RegularExpressions;

/// <summary>
/// BUENA PRÁCTICA: Clase de Utilidad Estática.
/// Encapsula una lógica muy específica y reutilizable: la creación de anclas de Markdown.
/// Esto mantiene la lógica de formato fuera del generador de reportes, siguiendo el SRP.
/// </summary>
public static class MarkdownHelper
{
    public static string CreateAnchor(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var anchor = text.Trim().ToLowerInvariant();
        anchor = Regex.Replace(anchor, @"[^a-z0-9\s-]", "");
        anchor = Regex.Replace(anchor, @"[\s-]+", "-");
        return anchor.Trim('-');
    }
}