using System.Globalization;
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.SourceGenerator.Poco;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Jameak.CursorPagination.SourceGenerator.HelperMethods;

namespace Jameak.CursorPagination.SourceGenerator.Extractors;
internal abstract partial class BaseExtractor
{
    private sealed class PaginationPropertyExtractionHelper
    {
        private const char MemberAccessSeparator = '.';

        private readonly GeneratorAttributeSyntaxContext _context;
        private readonly INamedTypeSymbol _generatorClassSymbol;
        private readonly ITypeSymbol _paginationTargetType;
        private readonly PaginationKind _paginationKind;
        private readonly List<CacheableDiagnosticInfo> _errors = [];
        private readonly List<CacheableDiagnosticInfo> _warnings = [];
        private readonly List<string> _alreadyErroredProps = [];

        public PaginationPropertyExtractionHelper(
            GeneratorAttributeSyntaxContext context,
            INamedTypeSymbol generatorClassSymbol,
            ITypeSymbol paginationTargetType,
            PaginationKind paginationKind)
        {
            _context = context;
            _generatorClassSymbol = generatorClassSymbol;
            _paginationTargetType = paginationTargetType;
            _paginationKind = paginationKind;
        }

        public (List<PropertyConfiguration> finalProperties, IReadOnlyList<CacheableDiagnosticInfo> errors, IReadOnlyList<CacheableDiagnosticInfo> warnings) Extract()
        {
            var tempProperties = new List<TemporaryPropertyConfiguration>();
            foreach (var attrData in _generatorClassSymbol.GetAttributes().FilterWithAttributeType<PaginationPropertyAttribute>())
            {
                ExtractPaginationPropertyData(attrData, ref tempProperties);
            }

            if (tempProperties.Count == 0)
            {
                _errors.Add(DiagnosticHelper.CreateNoPropertiesDefinedDiagnostic(CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations), _generatorClassSymbol.Name));
                return ([], _errors, _warnings);
            }

            ReportDuplicatePropertyNames(tempProperties, out var tempPropertiesWithoutDuplicates);

            var finalProperties = RetrievePropertyDetails(tempPropertiesWithoutDuplicates);

            ReportPropertiesWithDuplicateOrder(finalProperties);

            return (finalProperties, _errors, _warnings);
        }

        private void ExtractPaginationPropertyData(
            AttributeData paginationPropertyAttributeData,
            ref List<TemporaryPropertyConfiguration> properties)
        {
            var constructorArgs = paginationPropertyAttributeData.ConstructorArguments;
            if (HasErrors(constructorArgs))
            {
                return;
            }

            // Assumes PaginationPropertyAttribute only has a single constructor.
            var paginationOrder = GetArgumentValue(constructorArgs[0]) as int?;
            var paginationPropertyName = HandlePaginationPropertyPath(paginationPropertyAttributeData);
            var paginationDirection = GetArgumentValue(constructorArgs[2], typeof(PaginationOrdering)) as PaginationOrdering?;
            var nullCoalesceRhs = GetArgumentValue(constructorArgs[3]) as string;

            if (paginationOrder.HasValue && paginationPropertyName != null && paginationPropertyName.Length > 0 && paginationDirection.HasValue)
            {
                properties.Add(new TemporaryPropertyConfiguration(paginationOrder.Value, paginationPropertyName, paginationDirection.Value, nullCoalesceRhs));
            }
        }

        private string[]? HandlePaginationPropertyPath(AttributeData paginationPropertyAttributeData)
        {
            var attributeArgumentSyntax = (IReadOnlyList<AttributeArgumentSyntax>?)((AttributeSyntax?)paginationPropertyAttributeData.ApplicationSyntaxReference?.GetSyntax())?.ArgumentList?.Arguments;

            var propertyNameSyntax = attributeArgumentSyntax![1];

            if (propertyNameSyntax.TryGetNameOfContent(_context, out var splitNameOfContent, out var referencedMemberRootType))
            {
                if (referencedMemberRootType != null && !referencedMemberRootType.Equals(_paginationTargetType, SymbolEqualityComparer.Default))
                {
                    _warnings.Add(DiagnosticHelper.CreateNameOfReferencesDifferentTypeDiagnostic(
                        CacheableLocation.CreateFromLocations([propertyNameSyntax.GetLocation()]),
                        referencedMemberRootType.ToFullyQualified(),
                        _paginationTargetType.ToFullyQualified()));
                }

                return splitNameOfContent;
            }

            return (GetArgumentValue(paginationPropertyAttributeData.ConstructorArguments[1]) as string)
                ?.Split(MemberAccessSeparator)
                .ToArray();
        }

        private void ReportDuplicatePropertyNames(List<TemporaryPropertyConfiguration> tempProperties, out List<TemporaryPropertyConfiguration> tempPropertiesWithoutDuplicates)
        {
            var duplicateProperties = tempProperties.GroupBy(e => e.PropertyNameFullName).Where(e => e.Count() > 1).ToList();
            foreach (var property in duplicateProperties)
            {
                _alreadyErroredProps.Add(property.Key);
                _errors.Add(DiagnosticHelper.CreateDuplicatePropertiesDefinedDiagnostic(CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations), property.Key, _generatorClassSymbol.Name));
            }

            tempPropertiesWithoutDuplicates = tempProperties.GroupBy(e => e.PropertyNameFullName).Where(e => e.Count() == 1).SelectMany(e => e).ToList();
        }

        private void ReportPropertiesWithDuplicateOrder(List<PropertyConfiguration> finalProperties)
        {
            var propertiesWithDuplicateOrder = finalProperties.GroupBy(e => e.Order).Where(e => e.Count() > 1).ToList();
            if (propertiesWithDuplicateOrder.Any())
            {
                foreach (var groupingKey in propertiesWithDuplicateOrder)
                {
                    _errors.Add(DiagnosticHelper.CreateDuplicatePropertyOrderDefinedDiagnostic(CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations), groupingKey.Key.ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        private List<PropertyConfiguration> RetrievePropertyDetails(List<TemporaryPropertyConfiguration> tempProperties)
        {
            var finalProperties = new List<PropertyConfiguration>();
            foreach (var tempProperty in tempProperties)
            {
                var prop = RetrievePropertyDetailsRecursive(tempProperty, 0, _paginationTargetType, string.Empty);
                if (prop != null)
                {
                    finalProperties.Add(prop);
                }
            }

            if (finalProperties.Count != tempProperties.Count)
            {
                foreach (var unfoundPropName in tempProperties.Select(e => e.PropertyNameFullName).Except(finalProperties.Select(e => e.PropertyAccessor)).Except(_alreadyErroredProps))
                {
                    _errors.Add(DiagnosticHelper.CreateCannotFindPropertyDiagnostic(CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations), unfoundPropName, _paginationTargetType.ToFullyQualified()));
                }
            }

            return finalProperties;
        }

        private PropertyConfiguration? RetrievePropertyDetailsRecursive(TemporaryPropertyConfiguration tempProperty, int depth, ITypeSymbol enclosingType, string parentMemberPath)
        {
            var member = GetAccessiblePropertyAndFieldMembers(enclosingType, _generatorClassSymbol, _context)
                .Select(member => (found: tempProperty.PropertyNamePath[depth] == member.ToStringWithEscapedKeywords(), member))
                .Where(e => e.found)
                .Select(e => e.member)
                .SingleOrDefault();

            if (member == null)
            {
                return null;
            }

            switch (member)
            {
                case IPropertySymbol propertySymbol:
                    if (propertySymbol.GetMethod == null)
                    {
                        _alreadyErroredProps.Add(member.Name);
                        _errors.Add(DiagnosticHelper.CreatePropertyIsWriteOnlyDiagnostic(CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations), member.Name));
                        return null;
                    }

                    if (IsSymbolNotAccessibleThenReportError(member, propertySymbol.GetMethod))
                    {
                        return null;
                    }

                    return HandleRecursion(propertySymbol.Type);
                case IFieldSymbol fieldSymbol:
                    if (IsSymbolNotAccessibleThenReportError(member, fieldSymbol))
                    {
                        return null;
                    }

                    return HandleRecursion(fieldSymbol.Type);
                default:
                    // Error is reported elsewhere.
                    return null;
            }

            PropertyConfiguration? HandleRecursion(ITypeSymbol enclosingType)
            {
                ReportPropertyTypeDiagnostics(enclosingType, tempProperty);
                var memberPath = parentMemberPath + member.ToStringWithEscapedKeywords();

                // Depth is 0-indexed, length is not, so we +1
                if (tempProperty.PropertyNamePath.Length > depth + 1)
                {
                    var accessor = (enclosingType.IsNullable(), tempProperty.NullCoalesceRhs != null) switch
                    {
                        (true, true) => "?.",
                        (true, false) => "!.",
                        (false, _) => ".",
                    };
                    return RetrievePropertyDetailsRecursive(tempProperty, depth + 1, enclosingType, memberPath + accessor);
                }

                return CreateFinalConfiguration(tempProperty, enclosingType, memberPath);
            }

            static PropertyConfiguration CreateFinalConfiguration(
                TemporaryPropertyConfiguration config,
                ITypeSymbol memberTypeSymbol,
                string memberAccessor)
            {
                return new PropertyConfiguration(
                            config.Order,
                            memberAccessor,
                            config.Direction,
                            memberTypeSymbol.ToFullyQualifiedWithNullRefModifier(),
                            config.NullCoalesceRhs,
                            memberTypeSymbol.IsNullableValueType());
            }
        }

        private void ReportPropertyTypeDiagnostics(
            ITypeSymbol typeSymbol,
            TemporaryPropertyConfiguration config)
        {
            if (typeSymbol.IsNullableValueType() && config.NullCoalesceRhs == null && _paginationKind == PaginationKind.KeySet)
            {
                _warnings.Add(DiagnosticHelper.CreateKeySetPropertyIsNullableValueTypeDiagnostic(
                    CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations),
                    config.PropertyNameFullName,
                    typeSymbol.ToFullyQualifiedWithNullRefModifier()));
            }
            else if (typeSymbol.IsNullable() && config.NullCoalesceRhs == null)
            {
                _warnings.Add(DiagnosticHelper.CreatePropertyIsNullableDiagnostic(
                    CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations),
                    config.PropertyNameFullName,
                    typeSymbol.ToFullyQualifiedWithNullRefModifier()));
            }

            if (!typeSymbol.IsNullable() && !typeSymbol.IsNullableOblivious() && config.NullCoalesceRhs != null)
            {
                _warnings.Add(DiagnosticHelper.CreateNonNullablePropertyHasNullCoalesceDefinedDiagnostic(
                    CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations),
                    config.PropertyNameFullName,
                    typeSymbol.ToFullyQualifiedWithNullRefModifier(),
                    config.NullCoalesceRhs));
            }

        }

        private bool IsSymbolNotAccessibleThenReportError(
                ISymbol member,
                ISymbol symbolToCheck)
        {
            if (!_context.SemanticModel.Compilation.IsSymbolAccessibleWithin(symbolToCheck, _generatorClassSymbol))
            {
                _alreadyErroredProps.Add(member.Name);
                _errors.Add(DiagnosticHelper.CreatePropertyGetterIsNotAccessibleDiagnostic(CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations), member.Name));
                return true;
            }

            return false;
        }

        private sealed record TemporaryPropertyConfiguration
        {
            public int Order { get; }
            public string[] PropertyNamePath { get; }
            public PaginationOrdering Direction { get; }
            public string? NullCoalesceRhs { get; }

            public TemporaryPropertyConfiguration(
                int order,
                string[] propertyNamePath,
                PaginationOrdering direction,
                string? nullCoalesceRhs)
            {
                Order = order;
                PropertyNamePath = propertyNamePath;
                Direction = direction;
                NullCoalesceRhs = nullCoalesceRhs;
            }

            public string PropertyNameFullName => string.Join(MemberAccessSeparator.ToString(), PropertyNamePath);
        }
    }
}
