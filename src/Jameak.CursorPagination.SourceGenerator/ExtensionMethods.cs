using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jameak.CursorPagination.SourceGenerator;
internal static class ExtensionMethods
{
    private static readonly SymbolDisplayFormat s_fullyQualifiedWithNullRefModifier =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    private static readonly SymbolDisplayFormat s_fullyQualifiedWithEscape =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    internal static string ToFullyQualifiedWithNullRefModifier(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(s_fullyQualifiedWithNullRefModifier);
    }

    internal static string ToFullyQualified(this ISymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    internal static string ToNameWithGenericsAndEscapedKeywords(this ISymbol symbol)
    {
        return symbol.ToDisplayString(
            new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers));
    }

    internal static string ToStringWithEscapedKeywords(this ISymbol symbol)
    {
        return symbol.ToDisplayString(s_fullyQualifiedWithEscape);
    }

    internal static string GetFullNameWithoutGenericArity(this Type type)
    {
        var index = type.Name.LastIndexOf('`');
        var cleanName = index == -1 ? type.Name : type.Name.Substring(0, index);
        return $"{type.Namespace}.{cleanName}";
    }

    internal static bool IsNullableValueType(this ITypeSymbol symbol)
    {
        return symbol.IsNullable() && symbol.IsValueType;
    }

    internal static bool IsNullable(this ITypeSymbol symbol)
    {
        return symbol.NullableAnnotation == NullableAnnotation.Annotated;
    }

    internal static bool IsNullableOblivious(this ITypeSymbol symbol)
    {
        return symbol.NullableAnnotation == NullableAnnotation.None;
    }

    internal static IEnumerable<AttributeData> FilterWithAttributeType<TAttribute>(this IEnumerable<AttributeData> attributes)
    {
        return attributes.Where(attr => attr.AttributeClass?.ToFullyQualified() == "global::" + typeof(TAttribute).FullName);
    }

    internal static bool TryGetNameOfContent(
        this AttributeArgumentSyntax? syntax,
        GeneratorAttributeSyntaxContext context,
        [NotNullWhen(true)] out string[]? splitNameOfContent,
        out ITypeSymbol? referencedMemberRootType)
    {
        splitNameOfContent = null;
        referencedMemberRootType = null;
        if (syntax?.Expression is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "nameof" } } invocationExpressionSyntax)
        {
            var argument = invocationExpressionSyntax.ArgumentList.Arguments[0];
            var nameOfOperation = context.SemanticModel.Compilation.GetSemanticModel(invocationExpressionSyntax.SyntaxTree)
                .GetOperation(invocationExpressionSyntax) as INameOfOperation;

            var firstSimpleNameNode = HelperMethods.GetFirstSimpleNameSyntaxSyntax(argument.Expression);
            if (firstSimpleNameNode == null)
            {
                return false;
            }

            var isFullNameOf = firstSimpleNameNode.GetFirstToken().IsVerbatimIdentifier();

            var memberPath = new List<ISymbol>();
            var memberRefOperation = nameOfOperation?.ChildOperations.FirstOrDefault() as IMemberReferenceOperation;
            var memberContainingRootType = memberRefOperation?.Member.ContainingType;
            while (memberRefOperation != null)
            {
                memberPath.Add(memberRefOperation.Member);
                memberRefOperation = memberRefOperation.ChildOperations.FirstOrDefault() as IMemberReferenceOperation;
                memberContainingRootType = memberRefOperation == null ? memberContainingRootType : memberRefOperation.Member.ContainingType;
            }

            if (!isFullNameOf)
            {
                memberPath = memberPath.Take(1).ToList();
            }

            memberPath.Reverse();
            splitNameOfContent = memberPath.Select(e => e.ToStringWithEscapedKeywords()).ToArray();
            referencedMemberRootType = memberContainingRootType;
            return true;
        }

        return false;
    }
}
