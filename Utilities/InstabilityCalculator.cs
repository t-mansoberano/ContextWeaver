using ContextWeaver.Core;
using System.Text.RegularExpressions;

namespace ContextWeaver.Utilities;

public class InstabilityCalculator
{
    public Dictionary<string, (int Ca, int Ce, double Instability)> Calculate(string rootDirectoryName, List<FileAnalysisResult> results)
    {
        var moduleDependencies = new Dictionary<string, HashSet<string>>();
        var moduleNames = new HashSet<string>();

        // 1. Identificar módulos y sus dependencias eferentes
        foreach (var result in results.Where(r => r.Usings.Any()))
        {
            var pathParts = result.RelativePath.Split('/');
            var currentModule = pathParts.Length > 1 ? pathParts[0] : rootDirectoryName;
            moduleNames.Add(currentModule);

            if (!moduleDependencies.ContainsKey(currentModule))
            {
                moduleDependencies[currentModule] = new HashSet<string>();
            }

            foreach (var usedNamespace in result.Usings)
            {
                var cleanedUsedNamespace = usedNamespace.Split('.')[0];
                var matchingModule = moduleNames.FirstOrDefault(m =>
                    m.Equals(cleanedUsedNamespace, StringComparison.OrdinalIgnoreCase) ||
                    usedNamespace.StartsWith($"{m}.", StringComparison.OrdinalIgnoreCase) ||
                    usedNamespace.Contains($".{m}.", StringComparison.OrdinalIgnoreCase));

                if (matchingModule != null && matchingModule != currentModule)
                {
                    moduleDependencies[currentModule].Add(matchingModule);
                }
            }
        }

        var moduleMetrics = moduleNames.ToDictionary(m => m, m => (Ca: 0, Ce: 0));

        // 2. Calcular Ce (eferentes) y Ca (aferentes)
        foreach (var (module, dependencies) in moduleDependencies)
        {
            moduleMetrics[module] = (moduleMetrics[module].Ca, dependencies.Count);
            foreach (var dependentModule in dependencies)
            {
                if (moduleMetrics.ContainsKey(dependentModule))
                {
                    moduleMetrics[dependentModule] = (moduleMetrics[dependentModule].Ca + 1, moduleMetrics[dependentModule].Ce);
                }
            }
        }

        // 3. Calcular Inestabilidad (I)
        return moduleMetrics.ToDictionary(
            kvp => kvp.Key,
            kvp => {
                var (ca, ce) = kvp.Value;
                double instability = (ca + ce == 0) ? 0.0 : (double)ce / (ca + ce);
                return (ca, ce, instability);
            });
    }
}