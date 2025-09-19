namespace ContextWeaver;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// BUENA PRÁCTICA: Clase de Utilidad Estática.
/// Contiene lógica pura y sin estado para calcular métricas de código C#.
/// El uso de la API de Roslyn es una técnica avanzada para el análisis estático.
/// </summary>
public static class CSharpMetricsCalculator
{
    public static int CalculateCyclomaticComplexity(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return 1;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        
        // PATRÓN DE DISEÑO: Visitor (Visitante).
        // CSharpSyntaxWalker es una implementación del patrón Visitor que nos permite
        // recorrer el árbol de sintaxis del código y ejecutar lógica en nodos específicos.
        var walker = new ComplexityWalker();
        walker.Visit(root);
        
        return walker.Complexity;
    }

    private class ComplexityWalker : CSharpSyntaxWalker
    {
        public int Complexity { get; private set; } = 1;

        public override void VisitIfStatement(IfStatementSyntax node) { Complexity++; base.VisitIfStatement(node); }
        public override void VisitForEachStatement(ForEachStatementSyntax node) { Complexity++; base.VisitForEachStatement(node); }
        public override void VisitForStatement(ForStatementSyntax node) { Complexity++; base.VisitForStatement(node); }
        public override void VisitWhileStatement(WhileStatementSyntax node) { Complexity++; base.VisitWhileStatement(node); }
        public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node) { Complexity++; base.VisitCaseSwitchLabel(node); }
        public override void VisitConditionalExpression(ConditionalExpressionSyntax node) { Complexity++; base.VisitConditionalExpression(node); }
        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.LogicalAndExpression) || node.IsKind(SyntaxKind.LogicalOrExpression))
            {
                Complexity++;
            }
            base.VisitBinaryExpression(node);
        }
    }
}