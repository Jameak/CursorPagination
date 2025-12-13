using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.SourceGenerator.Helpers;
using Jameak.CursorPagination.SourceGenerator.Poco;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jameak.CursorPagination.SourceGenerator;
internal static class HelperMethods
{

    internal static readonly AssemblyName s_generatorAssemblyName = typeof(KeySetPaginationClassBuilder).Assembly.GetName();
#if IS_CI_TEST_BUILD || DEBUG
    internal static string GeneratedCodeAttribute => $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{s_generatorAssemblyName.Name}\", \"LOCALBUILD\")]";
#else
    internal static string GeneratedCodeAttribute => $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{s_generatorAssemblyName.Name}\", \"{s_generatorAssemblyName.Version}\")]";
#endif

    internal static bool TypeIsCompareToSpecialCase(string typeFullName)
    {
        const string GuidFullName = "global::System.Guid";
        const string StringFullName = "string";
        const string NullableStringFullName = "string?";
        const string BoolFullName = "bool";
        const string NullableBoolFullName = "bool?";
        return typeFullName == StringFullName
            || typeFullName == NullableStringFullName
            || typeFullName == GuidFullName
            || typeFullName == BoolFullName
            || typeFullName == NullableBoolFullName;
    }

    internal static string Indent(int level)
    {
        return new string(' ', level * 4);
    }

    internal static string CreateApplyOrderByMethodBody(EquatableArray<PropertyConfiguration> propertyConfigurations, int indentLevel)
    {
        return $$"""
            {{Indent(indentLevel)}}switch (paginationDirection)
            {{Indent(indentLevel)}}{
            {{Indent(indentLevel)}}    case global::{{typeof(PaginationDirection).FullName}}.{{nameof(PaginationDirection.Forward)}}:
            {{Indent(indentLevel)}}        {
            {{string.Join("\n", propertyConfigurations.Select((e, index) => Indent(indentLevel + 3) + CreateOrderByOrThenByString(e, index, PaginationDirection.Forward)))}}
            {{Indent(indentLevel)}}            return orderedQueryable;
            {{Indent(indentLevel)}}        }
            {{Indent(indentLevel)}}    case global::{{typeof(PaginationDirection).FullName}}.{{nameof(PaginationDirection.Backward)}}:
            {{Indent(indentLevel)}}        {
            {{string.Join("\n", propertyConfigurations.Select((e, index) => Indent(indentLevel + 3) + CreateOrderByOrThenByString(e, index, PaginationDirection.Backward)))}}
            {{Indent(indentLevel)}}            return orderedQueryable;
            {{Indent(indentLevel)}}        }
            {{Indent(indentLevel)}}    default:
            {{Indent(indentLevel)}}        throw new global::System.ArgumentException("Invalid pagination direction specified.", nameof(paginationDirection));
            {{Indent(indentLevel)}}}
            """;

        static string CreateOrderByOrThenByString(PropertyConfiguration propertyConfig, int index, PaginationDirection direction)
        {
            var orderMethod = direction switch
            {
                PaginationDirection.Forward when index == 0 && propertyConfig.Direction == PaginationOrdering.Ascending => "global::System.Linq.Queryable.OrderBy(queryable, ",
                PaginationDirection.Forward when index == 0 && propertyConfig.Direction == PaginationOrdering.Descending => "global::System.Linq.Queryable.OrderByDescending(queryable, ",
                PaginationDirection.Forward when index != 0 && propertyConfig.Direction == PaginationOrdering.Ascending => "global::System.Linq.Queryable.ThenBy(orderedQueryable, ",
                PaginationDirection.Forward when index != 0 && propertyConfig.Direction == PaginationOrdering.Descending => "global::System.Linq.Queryable.ThenByDescending(orderedQueryable, ",
                PaginationDirection.Backward when index == 0 && propertyConfig.Direction == PaginationOrdering.Ascending => "global::System.Linq.Queryable.OrderByDescending(queryable, ",
                PaginationDirection.Backward when index == 0 && propertyConfig.Direction == PaginationOrdering.Descending => "global::System.Linq.Queryable.OrderBy(queryable, ",
                PaginationDirection.Backward when index != 0 && propertyConfig.Direction == PaginationOrdering.Ascending => "global::System.Linq.Queryable.ThenByDescending(orderedQueryable, ",
                PaginationDirection.Backward when index != 0 && propertyConfig.Direction == PaginationOrdering.Descending => "global::System.Linq.Queryable.ThenBy(orderedQueryable, ",
                _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Unhandled direction value: direction={direction} & index={index} & propertyConfig.Direction={propertyConfig.Direction}")
            };

            return $"{(index == 0 ? "var " : "")}orderedQueryable = {orderMethod}(obj => {GetPropertyValue(propertyConfig)}));";

            static string GetPropertyValue(PropertyConfiguration property)
            {
                if (property.NullCoalesceRhs != null)
                {
                    return ($"(obj.{property.PropertyAccessor} ?? {property.NullCoalesceRhs})");
                }

                return $"obj.{property.PropertyAccessor}";
            }
        }
    }

    internal static BaseExtractedData CreateUnspecificGenerationErrorResult(INamedTypeSymbol generatorClassSymbol)
    {
        var locations = CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations);
        return new BaseExtractedData(
                generatorClassSymbol.Name,
                [DiagnosticHelper.CreateCouldNotGenerateForTypeDiagnostic(locations, generatorClassSymbol.Name)],
                [],
                locations);
    }

    internal static IEnumerable<ISymbol> GetAccessiblePropertyAndFieldMembers(ITypeSymbol symbol, ITypeSymbol accessibleWithinSymbol, GeneratorAttributeSyntaxContext context)
    {
        return GetAllMembers(symbol)
            .Where(e => e is { IsStatic: false, Kind: SymbolKind.Property } or IFieldSymbol { IsStatic: false, AssociatedSymbol: null })
            .Where(e => context.SemanticModel.Compilation.IsSymbolAccessibleWithin(e, accessibleWithinSymbol))
            .GroupBy(e => e.Name).Select(e => e.First());
    }

    internal static IEnumerable<IMethodSymbol> GetAccessibleMethodsWithAttribute<TAttribute>(ITypeSymbol symbol, GeneratorAttributeSyntaxContext context)
    {
        return GetAllMembers(symbol)
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary
                && context.SemanticModel.Compilation.IsSymbolAccessibleWithin(m, symbol)
                && m.GetAttributes().FilterWithAttributeType<TAttribute>().Any());
    }

    internal static IEnumerable<ISymbol> GetAllMembers(ITypeSymbol symbol)
    {
        var members = symbol.GetMembers();

        if (symbol.TypeKind == TypeKind.Interface)
        {
            var interfaceMembers = symbol.AllInterfaces.SelectMany(GetAllMembers);
            return members.Concat(interfaceMembers);
        }

        return symbol.BaseType == null ? members : members.Concat(GetAllMembers(symbol.BaseType));
    }

    internal static bool HasErrors(ImmutableArray<TypedConstant> constructorArgs)
    {
        return constructorArgs.Any(arg => arg.Kind == TypedConstantKind.Error);
    }

    internal static bool IsErrorKind(INamedTypeSymbol symbol)
    {
        return symbol.Kind == SymbolKind.ErrorType
            || symbol.TypeArguments.Any(e => e is INamedTypeSymbol named && IsErrorKind(named));
    }

    internal static object? GetArgumentValue(TypedConstant argument)
    {
        return GetArgumentValue(argument, null);
    }

    internal static object? GetArgumentValue(TypedConstant argument, Type? targetType)
    {
        return argument.Kind switch
        {
            _ when argument.IsNull => null,
            TypedConstantKind.Enum when targetType != null => Enum.IsDefined(targetType, argument.Value) ? Enum.ToObject(targetType, argument.Value) : null,
            TypedConstantKind.Primitive => argument.Value,
            TypedConstantKind.Type => argument.Value,
            TypedConstantKind.Array => argument.Values.Select(value => GetArgumentValue(value, targetType)).ToArray(),
            _ => null
        };
    }

    internal static string GetAccessibility(ITypeSymbol symbol)
    {
        return symbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.Private => "private",
            _ => throw new InvalidOperationException($"Unknown accessibility: {symbol.DeclaredAccessibility}")
        };
    }

    internal static string CreateMd5Hash(string toHash)
    {
        var input = Encoding.UTF8.GetBytes(toHash);

        byte[] hashed;
        using (var hasher = MD5.Create())
        {
            hashed = hasher.ComputeHash(input);
        }

        var sb = new StringBuilder();
        for (var i = 0; i < hashed.Length; i++)
        {
            sb.Append(hashed[i].ToString("x2"));
        }

        return sb.ToString();
    }

    internal static string? GetEnclosingNamespace(INamedTypeSymbol generatorClassSymbol)
    {
        var ns = generatorClassSymbol.ContainingNamespace.ToFullyQualified();
        return ns == "<global namespace>" ? null : ns.Replace("global::", "");
    }

    internal static string SanitizeToValidFilename(string input) => input.Replace('@', '_');

    internal static string TrimGlobalAlias(string typeFullName) => typeFullName.StartsWith("global::") ? typeFullName.Substring("global::".Length) : typeFullName;

    internal static SimpleNameSyntax? GetFirstSimpleNameSyntaxSyntax(ExpressionSyntax argumentExpression)
    {
        return argumentExpression
            .DescendantNodesAndSelf()
            .OfType<SimpleNameSyntax>()
            .FirstOrDefault(sn =>
                sn.Parent is not AliasQualifiedNameSyntax aliasSyntax ||
                aliasSyntax.Alias != sn);
    }
}
