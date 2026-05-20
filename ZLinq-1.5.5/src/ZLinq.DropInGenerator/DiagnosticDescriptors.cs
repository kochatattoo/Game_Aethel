using Microsoft.CodeAnalysis;

namespace ZLinq;

internal static class DiagnosticDescriptors
{
    const string Category = "ZLinqDropInGenerator";

    public static void ReportDiagnostic(this SourceProductionContext context, DiagnosticDescriptor diagnosticDescriptor, Location location, params object?[]? messageArgs)
    {
        var diagnostic = Diagnostic.Create(diagnosticDescriptor, location, messageArgs);
        context.ReportDiagnostic(diagnostic);
    }

    public static DiagnosticDescriptor Create(int id, string message)
    {
        return Create(id, message, message);
    }

    public static DiagnosticDescriptor Create(int id, string title, string messageFormat)
    {
        return new DiagnosticDescriptor(
            id: "ZL" + id.ToString("000"),
            title: title,
            messageFormat: messageFormat,
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }

    public static DiagnosticDescriptor AttributeNotFound { get; } = Create(
        1,
        "ZLinqDropIn AssemblyAttribute is not found, you need to add like [assembly: ZLinq.ZLinqDropInAttribute(\"ZLinq.DropIn\", ZLinq.DropInGenerateTypes.Array)].");

    public static DiagnosticDescriptor ExtensionNotSupported { get; } = Create(
        2,
        "ZLinqDropInExtension requires implementation of IEnumerable<T> or IValueEnumerable<T, TEnumerator>.");

    public static DiagnosticDescriptor GenericTypeArgumentTooMuch { get; } = Create(
        3,
        "ZLinqDropInExtension requires zero or one generic type argument.");

    public static DiagnosticDescriptor SourceTypeNotFound { get; } = Create(
        4,
        "ZLinqDropInExternalExtensionAttribute sourceTypeFullyQualifiedMetadataName: {0} does not found in compilation.");

    public static DiagnosticDescriptor EnumeratorTypeNotFound { get; } = Create(
        5,
        "ZLinqDropInExternalExtensionAttribute enumeratorTypeFullyQualifiedMetadataName: {0} does not found in compilation.");

    public static DiagnosticDescriptor ElementNotFoundInSourceType { get; } = Create(
        6,
        "ZLinqDropInExternalExtensionAttribute sourceTypeFullyQualifiedMetadataName: {0} type does not implement IEnumerable<T>.");

    public static DiagnosticDescriptor ElementNotFoundInEnumeratorType { get; } = Create(
        7,
        "ZLinqDropInExternalExtensionAttribute enumeratorTypeFullyQualifiedMetadataName: {0} type does not implement IValueEnumerator<T>.");
}
