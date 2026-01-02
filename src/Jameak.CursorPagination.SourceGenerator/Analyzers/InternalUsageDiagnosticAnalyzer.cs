using System.Collections.Immutable;
using Jameak.CursorPagination.Abstractions.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Jameak.CursorPagination.SourceGenerator.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed partial class InternalUsageDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private static readonly string s_internalUsageOnlyAttributeFullName = typeof(InternalUsageOnlyAttribute).FullName;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DiagnosticHelper.s_internalUsageOnlyRule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeInternalClassUsage, OperationKind.MethodReference, OperationKind.Invocation, OperationKind.TypeOf);
    }

    private static void AnalyzeInternalClassUsage(OperationAnalysisContext context)
    {
        switch (context.Operation)
        {
            case IMethodReferenceOperation methodReference:
                AnalyzeMember(context, methodReference.Method);
                break;

            case IInvocationOperation invocation:
                AnalyzeInvocation(context, invocation);
                break;

            case ITypeOfOperation typeOf:
                AnalyzeTypeof(context, typeOf);
                break;

            default:
                throw new ArgumentException($"Unexpected operation: {context.Operation.Kind}", nameof(context));
        }
    }

    private static void AnalyzeMember(OperationAnalysisContext context, ISymbol symbol)
    {
        var containingType = symbol.ContainingType;

        if (HasInternalAttribute(symbol))
        {
            ReportDiagnostic(context, $"{containingType}.{symbol.Name}");
            return;
        }

        if (HasInternalAttribute(containingType))
        {
            ReportDiagnostic(context, containingType);
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, IInvocationOperation invocation)
    {
        foreach (var argument in invocation.TargetMethod.TypeArguments.Where(HasInternalAttribute))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticHelper.s_internalUsageOnlyRule, context.Operation.Syntax.GetLocation(), argument));
        }

        AnalyzeMember(context, invocation.TargetMethod);
    }

    private static void AnalyzeTypeof(OperationAnalysisContext context, ITypeOfOperation typeOf)
    {
        if (HasInternalAttribute(typeOf.TypeOperand))
        {
            ReportDiagnostic(context, typeOf.TypeOperand);
        }
    }

    private static void ReportDiagnostic(OperationAnalysisContext context, object messageArg)
        => context.ReportDiagnostic(
            Diagnostic.Create(DiagnosticHelper.s_internalUsageOnlyRule, context.Operation.Syntax.GetLocation(), messageArg));

    private static bool HasInternalAttribute(ISymbol symbol) => symbol.GetAttributes().Any(a => a.AttributeClass!.ToDisplayString() == s_internalUsageOnlyAttributeFullName);
}
