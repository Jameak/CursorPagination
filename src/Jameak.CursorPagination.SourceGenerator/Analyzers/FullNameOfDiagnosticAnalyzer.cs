using System.Collections.Immutable;
using Jameak.CursorPagination.Abstractions.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Jameak.CursorPagination.SourceGenerator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class FullNameOfDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private static readonly string s_paginationPropertyAttributeFullName = typeof(PaginationPropertyAttribute).FullName;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DiagnosticHelper.s_suspiciousNameOfRule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeFullNameOfUsage, OperationKind.NameOf);
    }

    private static void AnalyzeFullNameOfUsage(OperationAnalysisContext context)
    {
        switch (context.Operation)
        {
            case INameOfOperation nameofOp:
                AnalyzeNameOf(context, nameofOp);
                break;

            default:
                throw new ArgumentException($"Unexpected operation: {context.Operation.Kind}", nameof(context));
        }
    }

    private static void AnalyzeNameOf(OperationAnalysisContext context, INameOfOperation nameofOp)
    {
        var parent = nameofOp.Parent;
        if (IsLocatedInsidePaginationPropertyAttribute(parent)
            && nameofOp.Syntax is InvocationExpressionSyntax invocationExpressionSyntax)
        {
            var argument = invocationExpressionSyntax.ArgumentList.Arguments.FirstOrDefault();
            if (argument == null)
            {
                return;
            }

            var firstSimpleNameNode = HelperMethods.GetFirstSimpleNameSyntaxSyntax(argument.Expression);

            if (firstSimpleNameNode == null)
            {
                return;
            }

            if (firstSimpleNameNode.GetFirstToken().IsVerbatimIdentifier())
            {
                return;
            }

            if (nameofOp.ChildOperations.FirstOrDefault() is not IMemberReferenceOperation firstMemberRefOperation)
            {
                return;
            }

            if (firstMemberRefOperation.ChildOperations.FirstOrDefault() is not IMemberReferenceOperation)
            {
                return;
            }

            ReportDiagnostic(context, argument.GetLocation());
        }
    }

    private static void ReportDiagnostic(OperationAnalysisContext context, Location location) =>
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticHelper.s_suspiciousNameOfRule, location));

    private static bool IsLocatedInsidePaginationPropertyAttribute(IOperation? operation) =>
        operation != null
        && ((operation.Type != null && IsPaginationPropertyAttribute(operation.Type)) || IsLocatedInsidePaginationPropertyAttribute(operation.Parent));

    private static bool IsPaginationPropertyAttribute(ITypeSymbol symbol) => symbol.ToDisplayString() == s_paginationPropertyAttributeFullName;
}
