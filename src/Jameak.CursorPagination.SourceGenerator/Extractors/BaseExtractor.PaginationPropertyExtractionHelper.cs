using System.Globalization;
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.SourceGenerator.Poco;
using Microsoft.CodeAnalysis;
using static Jameak.CursorPagination.SourceGenerator.HelperMethods;

namespace Jameak.CursorPagination.SourceGenerator.Extractors;
internal abstract partial class BaseExtractor
{
    private class PaginationPropertyExtractionHelper
    {
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

            ReportDuplicatePropertyNames(tempProperties);

            var finalProperties = RetrievePropertyDetails(tempProperties);

            ReportPropertiesWithDuplicateOrder(finalProperties);

            return (finalProperties, _errors, _warnings);
        }

        private static void ExtractPaginationPropertyData(
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
            var paginationPropertyName = GetArgumentValue(constructorArgs[1]) as string;
            var paginationDirection = GetArgumentValue(constructorArgs[2], typeof(PaginationOrdering)) as PaginationOrdering?;
            var nullCoalesceRhs = GetArgumentValue(constructorArgs[3]) as string;

            if (paginationOrder.HasValue && paginationPropertyName != null && paginationDirection.HasValue)
            {
                properties.Add(new TemporaryPropertyConfiguration(paginationOrder.Value, paginationPropertyName, paginationDirection.Value, nullCoalesceRhs));
            }
        }

        private void ReportDuplicatePropertyNames(List<TemporaryPropertyConfiguration> tempProperties)
        {
            var duplicateProperties = tempProperties.GroupBy(e => e.PropertyName).Where(e => e.Count() > 1).ToList();
            if (duplicateProperties.Any())
            {
                foreach (var property in duplicateProperties)
                {
                    _alreadyErroredProps.Add(property.Key);
                    _errors.Add(DiagnosticHelper.CreateDuplicatePropertiesDefinedDiagnostic(CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations), property.Key, _generatorClassSymbol.Name));
                }
            }
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
            var tempPropConfigDict = tempProperties.GroupBy(e => e.PropertyName).Where(e => e.Count() == 1).SelectMany(e => e).ToDictionary(e => e.PropertyName, e => e);
            var validMembers = GetAccessiblePropertyAndFieldMembers(_paginationTargetType, _generatorClassSymbol, _context);
            var finalProperties = new List<PropertyConfiguration>();

            foreach (var (_, member, config) in validMembers
                .Select(member => (found: tempPropConfigDict.TryGetValue(member.Name, out var config), member, config))
                .Where(e => e.found))
            {
                if (member is IPropertySymbol propertySymbol)
                {
                    if (propertySymbol.GetMethod == null)
                    {
                        _alreadyErroredProps.Add(member.Name);
                        _errors.Add(DiagnosticHelper.CreatePropertyIsWriteOnlyDiagnostic(CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations), member.Name));
                        continue;
                    }

                    if (IsSymbolNotAccessibleThenReportError(member, propertySymbol.GetMethod))
                    {
                        continue;
                    }

                    ReportPropertyTypeDiagnostics(propertySymbol.Type, config);
                    finalProperties.Add(CreateFinalConfiguration(config, member, propertySymbol.Type));
                }
                else if (member is IFieldSymbol fieldSymbol)
                {
                    if (IsSymbolNotAccessibleThenReportError(member, fieldSymbol))
                    {
                        continue;
                    }

                    ReportPropertyTypeDiagnostics(fieldSymbol.Type, config);
                    finalProperties.Add(CreateFinalConfiguration(config, member, fieldSymbol.Type));
                }
            }

            if (finalProperties.Count != tempPropConfigDict.Count)
            {
                foreach (var unfoundPropName in tempPropConfigDict.Select(e => e.Key).Except(finalProperties.Select(e => e.PropertyName)).Except(_alreadyErroredProps))
                {
                    _errors.Add(DiagnosticHelper.CreateCannotFindPropertyDiagnostic(CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations), unfoundPropName, _paginationTargetType.ToFullyQualified()));
                }
            }

            return finalProperties;

            static PropertyConfiguration CreateFinalConfiguration(TemporaryPropertyConfiguration config, ISymbol memberSymbol, ITypeSymbol memberTypeSymbol)
            {
                return new PropertyConfiguration(
                            config.Order,
                            memberSymbol.ToStringWithEscapedKeywords(),
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
                    config.PropertyName,
                    typeSymbol.ToFullyQualifiedWithNullRefModifier()));
            }
            else if (typeSymbol.IsNullable() && config.NullCoalesceRhs == null)
            {
                _warnings.Add(DiagnosticHelper.CreatePropertyIsNullableDiagnostic(
                    CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations),
                    config.PropertyName,
                    typeSymbol.ToFullyQualifiedWithNullRefModifier()));
            }

            if (!typeSymbol.IsNullable() && !typeSymbol.IsNullableOblivious() && config.NullCoalesceRhs != null)
            {
                _warnings.Add(DiagnosticHelper.CreateNonNullablePropertyHasNullCoalesceDefinedDiagnostic(
                    CacheableLocation.CreateFromLocations(_generatorClassSymbol.Locations),
                    config.PropertyName,
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

        private record TemporaryPropertyConfiguration
        {
            public int Order { get; }
            public string PropertyName { get; }
            public PaginationOrdering Direction { get; }
            public string? NullCoalesceRhs { get; }

            public TemporaryPropertyConfiguration(
                int order,
                string propertyName,
                PaginationOrdering direction,
                string? nullCoalesceRhs)
            {
                Order = order;
                PropertyName = propertyName;
                Direction = direction;
                NullCoalesceRhs = nullCoalesceRhs;
            }
        }
    }
}
