using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.SourceGenerator.Helpers;
using Jameak.CursorPagination.SourceGenerator.Poco;
using Microsoft.CodeAnalysis;
#pragma warning disable RS2008 // Analyzer release tracking

namespace Jameak.CursorPagination.SourceGenerator;
internal static class DiagnosticHelper
{
    private const string CouldNotGenerateForTypeId = "JAMCP0001";
    private const string NoPropertiesDefinedId = "JAMCP0002";
    private const string DuplicatePropertiesDefinedId = "JAMCP0003";
    private const string PropertyIsWriteOnlyId = "JAMCP0004";
    private const string CannotFindPropertyId = "JAMCP0005";
    private const string PaginationStrategyCannotBeDeclaredWithBothPaginationAttributesId = "JAMCP0006";
    private const string PropertyIsNullableId = "JAMCP0007";
    private const string PropertyIsNotNullableButHasNullCoalesceDefinedId = "JAMCP0008";
    private const string PropertyGetterIsNotAccessibleId = "JAMCP0009";
    private const string DuplicatePropertyOrderDefinedId = "JAMCP0010";
    private const string ClassIsMissingPartialKeywordId = "JAMCP0011";
    private const string NestedClassIsNotSupportedId = "JAMCP0012";
    private const string InternalUsageOnlyId = "JAMCP0013";
    private const string UnboundGenericNotSupportedId = "JAMCP0014";
    private const string GenericGeneratorClassNotSupportedRule = "JAMCP0015";
    private const string RequiredTypeIsErrorKindRule = "JAMCP0016";
    private const string GeneralPaginatedTypeIsUnsupportedRule = "JAMCP0017";
    private const string KeySetPropertyIsNullableValueTypeId = "JAMCP0018";

    private static readonly DiagnosticDescriptor s_couldNotGenerateForTypeRule = new(
        id: CouldNotGenerateForTypeId,
        title: "Could not generate for type",
        messageFormat: "Unknown error. Could not generate pagination strategy for '{0}'.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_noPropertiesDefinedRule = new(
        id: NoPropertiesDefinedId,
        title: "No properties defined",
        messageFormat: "No pagination properties defined for type '{0}'",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_duplicatePropertiesDefinedRule = new(
        id: DuplicatePropertiesDefinedId,
        title: "Duplicate properties defined",
        messageFormat: "Property with name '{0}' has been defined as pagination property on '{1}' multiple times",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_propertyIsWriteOnlyRule = new(
        id: PropertyIsWriteOnlyId,
        title: "Property is write-only",
        messageFormat: "Property with name '{0}' is write-only",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_cannotFindPropertyRule = new(
        id: CannotFindPropertyId,
        title: "Cannot find property",
        messageFormat: "Cannot find property or field with name '{0}' on type '{1}', or it is not accessible from the pagination strategy type",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_paginationStrategyCannotBeDeclaredWithBothPaginationAttributesRule = new(
        id: PaginationStrategyCannotBeDeclaredWithBothPaginationAttributesId,
        title: "Pagination strategy cannot be declared with both KeySet and Offset pagination attributes",
        messageFormat: "Pagination strategy with type '{0}' cannot be declared with both '{1}' and '{2}'",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_propertyIsNullableRule = new(
        id: PropertyIsNullableId,
        title: "Configured pagination property is nullable",
        messageFormat: "Property '{0}' is nullable type '{1}'. This will likely work if your ORM behaves but is not generally supported by this library. It is recommended that you instead create a non-nullable computed property or add a null-coalesce expression to the PaginationPropertyAttribute for this property.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_propertyIsNotNullableButHasNullCoalesceDefinedRule = new(
        id: PropertyIsNotNullableButHasNullCoalesceDefinedId,
        title: "Non-nullable pagination property has null-coalesce configured",
        messageFormat: "Property '{0}' is non-nullable type '{1}' but has a null-coalesce expression '{2}'. This is likely unnecessary and may cause your ORM to generate less-optimal SQL.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_propertyGetterIsNotAccessibleRule = new(
        id: PropertyGetterIsNotAccessibleId,
        title: "Property getter is not accessible",
        messageFormat: "Property with name '{0}' has getter which is not accessible from the pagination strategy type",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_duplicatePropertyOrderDefinedRule = new(
        id: DuplicatePropertyOrderDefinedId,
        title: "Duplicate property order defined",
        messageFormat: "Multiple properties have been defined with the order-value '{0}'",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_classIsMissingPartialKeywordRule = new(
        id: ClassIsMissingPartialKeywordId,
        title: "Class is missing partial keyword",
        messageFormat: "The class '{0}' is annotated with generator attribute but is not partial",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_nestedClassIsNotSupportedRule = new(
        id: NestedClassIsNotSupportedId,
        title: "Nested class is not supported",
        messageFormat: "The class '{0}' is a nested class which is not supported",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor s_internalUsageOnlyRule = new(
        id: InternalUsageOnlyId,
        title: "Internal CursorPagination API usage",
        messageFormat: "{0} is an internal API that supports the library infrastructure and not subject to the same compatibility standards as public APIs. It may be changed or removed without notice in any release.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor s_unboundGenericNotSupportedRule = new(
        id: UnboundGenericNotSupportedId,
        title: "Unbound generic type not supported",
        messageFormat: "Cannot generate '{0}' because the configured pagination type '{1}' is an unbound generic type. Source generation for unbound generics is not supported.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor s_genericGeneratorClassNotSupportedRule = new(
        id: GenericGeneratorClassNotSupportedRule,
        title: "Generic strategy type not supported",
        messageFormat: "Cannot generate pagination strategy for '{0}' because source generation for generic strategy-types is not supported",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor s_requiredTypeIsErrorKindRule = new(
        id: RequiredTypeIsErrorKindRule,
        title: "Type is error kind",
        messageFormat: "Cannot get data for the symbol '{0}'. This is likely because the code has compilation errors.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor s_generalPaginatedTypeIsUnsupportedRule = new(
        id: GeneralPaginatedTypeIsUnsupportedRule,
        title: "Paginated type is not supported kind",
        messageFormat: "The paginated type '{0}' is not supported for source generation",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor s_keySetPropertyIsNullableValueTypeRule = new(
        id: KeySetPropertyIsNullableValueTypeId,
        title: "Configured KeySet pagination property is nullable value type",
        messageFormat: "Property '{0}' is nullable value type '{1}'. This will likely work if no 'null' values exist in your data and your ORM behaves, but is not generally supported by this library. If a 'null'-value is ever encountered the Cursor-constructor will throw KeySetCursorNullValueException.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor FindById(string id)
    {
        return id switch
        {
            CouldNotGenerateForTypeId => s_couldNotGenerateForTypeRule,
            NoPropertiesDefinedId => s_noPropertiesDefinedRule,
            DuplicatePropertiesDefinedId => s_duplicatePropertiesDefinedRule,
            PropertyIsWriteOnlyId => s_propertyIsWriteOnlyRule,
            CannotFindPropertyId => s_cannotFindPropertyRule,
            PaginationStrategyCannotBeDeclaredWithBothPaginationAttributesId => s_paginationStrategyCannotBeDeclaredWithBothPaginationAttributesRule,
            PropertyIsNullableId => s_propertyIsNullableRule,
            PropertyIsNotNullableButHasNullCoalesceDefinedId => s_propertyIsNotNullableButHasNullCoalesceDefinedRule,
            PropertyGetterIsNotAccessibleId => s_propertyGetterIsNotAccessibleRule,
            DuplicatePropertyOrderDefinedId => s_duplicatePropertyOrderDefinedRule,
            ClassIsMissingPartialKeywordId => s_classIsMissingPartialKeywordRule,
            NestedClassIsNotSupportedId => s_nestedClassIsNotSupportedRule,
            UnboundGenericNotSupportedId => s_unboundGenericNotSupportedRule,
            GenericGeneratorClassNotSupportedRule => s_genericGeneratorClassNotSupportedRule,
            RequiredTypeIsErrorKindRule => s_requiredTypeIsErrorKindRule,
            GeneralPaginatedTypeIsUnsupportedRule => s_generalPaginatedTypeIsUnsupportedRule,
            KeySetPropertyIsNullableValueTypeId => s_keySetPropertyIsNullableValueTypeRule,
            _ => throw new ArgumentException(nameof(id), $"Unknown id: {id}")
        };
    }

    public static CacheableDiagnosticInfo CreateCouldNotGenerateForTypeDiagnostic(
        EquatableArray<CacheableLocation>? locations,
        string typeMarkedWithGeneratorAttribute)
    {
        return new CacheableDiagnosticInfo(CouldNotGenerateForTypeId, locations, [typeMarkedWithGeneratorAttribute]);
    }

    public static CacheableDiagnosticInfo CreateNoPropertiesDefinedDiagnostic(
        EquatableArray<CacheableLocation> locations,
        string typeMarkedWithGeneratorAttribute)
    {
        return new CacheableDiagnosticInfo(NoPropertiesDefinedId, locations, [typeMarkedWithGeneratorAttribute]);
    }

    public static CacheableDiagnosticInfo CreateDuplicatePropertiesDefinedDiagnostic(
        EquatableArray<CacheableLocation> locations,
        string duplicatePropertyName,
        string typeMarkedWithGeneratorAttribute)
    {
        return new CacheableDiagnosticInfo(DuplicatePropertiesDefinedId, locations, [duplicatePropertyName, typeMarkedWithGeneratorAttribute]);
    }

    public static CacheableDiagnosticInfo CreatePropertyIsWriteOnlyDiagnostic(
        EquatableArray<CacheableLocation> locations,
        string writeOnlyPropertyName)
    {
        return new CacheableDiagnosticInfo(PropertyIsWriteOnlyId, locations, [writeOnlyPropertyName]);
    }

    public static CacheableDiagnosticInfo CreatePropertyGetterIsNotAccessibleDiagnostic(
        EquatableArray<CacheableLocation> locations,
        string notAccessiblePropertyName)
    {
        return new CacheableDiagnosticInfo(PropertyGetterIsNotAccessibleId, locations, [notAccessiblePropertyName]);
    }

    public static CacheableDiagnosticInfo CreateCannotFindPropertyDiagnostic(
        EquatableArray<CacheableLocation> locations,
        string propertyName,
        string targetType)
    {
        return new CacheableDiagnosticInfo(CannotFindPropertyId, locations, [propertyName, HelperMethods.TrimGlobalAlias(targetType)]);
    }

    public static CacheableDiagnosticInfo CreatePaginationStrategyCannotBeDeclaredWithBothPaginationAttributesDiagnostic(
        EquatableArray<CacheableLocation> locations,
        string typeMarkedWithGeneratorAttribute)
    {
        return new CacheableDiagnosticInfo(PaginationStrategyCannotBeDeclaredWithBothPaginationAttributesId, locations, [typeMarkedWithGeneratorAttribute, nameof(KeySetPaginationStrategyAttribute), nameof(OffsetPaginationStrategyAttribute)]);
    }

    public static CacheableDiagnosticInfo CreatePropertyIsNullableDiagnostic(
        EquatableArray<CacheableLocation> locations,
        string propertyName,
        string propertyType)
    {
        return new CacheableDiagnosticInfo(PropertyIsNullableId, locations, [propertyName, HelperMethods.TrimGlobalAlias(propertyType)]);
    }

    public static CacheableDiagnosticInfo CreateNonNullablePropertyHasNullCoalesceDefinedDiagnostic(
        EquatableArray<CacheableLocation> locations,
        string propertyName,
        string propertyType,
        string nullCoalesceRhsValue)
    {
        return new CacheableDiagnosticInfo(PropertyIsNotNullableButHasNullCoalesceDefinedId, locations, [propertyName, HelperMethods.TrimGlobalAlias(propertyType), nullCoalesceRhsValue]);
    }

    public static CacheableDiagnosticInfo CreateDuplicatePropertyOrderDefinedDiagnostic(
        EquatableArray<CacheableLocation> locations,
        string duplicateOrderValue)
    {
        return new CacheableDiagnosticInfo(DuplicatePropertyOrderDefinedId, locations, [duplicateOrderValue]);
    }

    public static CacheableDiagnosticInfo CreateClassIsMissingPartialKeywordDiagnostic(
        EquatableArray<CacheableLocation>? locations,
        string typeMarkedWithGeneratorAttribute)
    {
        return new CacheableDiagnosticInfo(ClassIsMissingPartialKeywordId, locations, [typeMarkedWithGeneratorAttribute]);
    }

    public static CacheableDiagnosticInfo CreateNestedClassIsNotSupportedDiagnostic(
        EquatableArray<CacheableLocation>? locations,
        string typeMarkedWithGeneratorAttribute)
    {
        return new CacheableDiagnosticInfo(NestedClassIsNotSupportedId, locations, [typeMarkedWithGeneratorAttribute]);
    }

    public static CacheableDiagnosticInfo CreateUnboundGenericIsNotSupportedDiagnostic(
        EquatableArray<CacheableLocation>? locations,
        string typeMarkedWithGeneratorAttribute,
        string paginationTargetTypeName)
    {
        return new CacheableDiagnosticInfo(UnboundGenericNotSupportedId, locations, [typeMarkedWithGeneratorAttribute, paginationTargetTypeName]);
    }

    public static CacheableDiagnosticInfo CreateGenericClassIsNotSupportedDiagnostic(
        EquatableArray<CacheableLocation>? locations,
        string typeMarkedWithGeneratorAttribute)
    {
        return new CacheableDiagnosticInfo(GenericGeneratorClassNotSupportedRule, locations, [typeMarkedWithGeneratorAttribute]);
    }

    public static CacheableDiagnosticInfo CreateRequiredTypeIsErrorKindDiagnostic(
        EquatableArray<CacheableLocation>? locations,
        string typeWithError)
    {
        return new CacheableDiagnosticInfo(RequiredTypeIsErrorKindRule, locations, [typeWithError]);
    }

    public static CacheableDiagnosticInfo CreateGeneralPaginatedTypeIsUnsupportedDiagnostic(
        EquatableArray<CacheableLocation>? locations,
        string paginationTargetTypeName)
    {
        return new CacheableDiagnosticInfo(GeneralPaginatedTypeIsUnsupportedRule, locations, [paginationTargetTypeName]);
    }

    public static CacheableDiagnosticInfo CreateKeySetPropertyIsNullableValueTypeDiagnostic(
        EquatableArray<CacheableLocation> locations,
        string propertyName,
        string propertyType)
    {
        return new CacheableDiagnosticInfo(KeySetPropertyIsNullableValueTypeId, locations, [propertyName, HelperMethods.TrimGlobalAlias(propertyType)]);
    }
}
