namespace ZLinq;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class ZLinqDropInExtensionAttribute : Attribute;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class ZLinqDropInExternalExtensionAttribute : Attribute
{
    /// <summary>
    /// Gets the namespace where the generated LINQ implementations will be placed.
    /// If empty, the implementations will be generated in the global namespace.
    /// </summary>
    public string GenerateNamespace { get; }

    public string SourceTypeFullyQualifiedMetadataName { get; }

    public string? EnumeratorTypeFullyQualifiedMetadataName { get; }

    /// <summary>
    /// Gets whether the generated LINQ implementations should be public.
    /// When true, the implementations will be generated with public visibility.
    /// When false (default), the implementations will be generated with internal visibility.
    /// </summary>
    public bool GenerateAsPublic { get; set; }

    public ZLinqDropInExternalExtensionAttribute(
        string generateNamespace,
        string sourceTypeFullyQualifiedMetadataName,
        string? enumeratorTypeFullyQualifiedMetadataName = null)
    {
        GenerateNamespace = generateNamespace;
        SourceTypeFullyQualifiedMetadataName = sourceTypeFullyQualifiedMetadataName;
        EnumeratorTypeFullyQualifiedMetadataName = enumeratorTypeFullyQualifiedMetadataName;
    }
}
