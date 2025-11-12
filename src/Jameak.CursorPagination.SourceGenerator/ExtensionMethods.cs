using Microsoft.CodeAnalysis;

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

    internal static string ToNameWithGenerics(this ISymbol symbol)
    {
        return symbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance));
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
}
