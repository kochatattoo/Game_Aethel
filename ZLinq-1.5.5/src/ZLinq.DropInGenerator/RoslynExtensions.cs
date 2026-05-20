using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace ZLinq;

public static class RoslynExtensions
{
    public static string FormatGenericConstraints(this INamedTypeSymbol namedType)
    {
        if (!namedType.IsGenericType)
        {
            return "";
        }

        if (namedType.TypeParameters.Length == 0) // nested type, get to root
        {
            do
            {
                namedType = namedType.ContainingType;
            } while (namedType != null && namedType.TypeParameters.Length == 0);
        }

        if (namedType == null) return "";

        var builder = new StringBuilder();

        foreach (var typeParameter in namedType.TypeParameters)
        {
            if (!HasAnyConstraints(typeParameter))
            {
                continue;
            }

            builder.Append($"where {typeParameter.Name} : ");

            var constraints = new List<string>();

            if (typeParameter.HasReferenceTypeConstraint)
            {
                constraints.Add("class");
            }
            else if (typeParameter.HasUnmanagedTypeConstraint)
            {
                constraints.Add("unmanaged");
            }
            else if (typeParameter.HasValueTypeConstraint)
            {
                constraints.Add("struct");
            }

            foreach (var constraintType in typeParameter.ConstraintTypes)
            {
                constraints.Add(constraintType.ToDisplayString());
            }

            if (typeParameter.HasNotNullConstraint)
            {
                constraints.Add("notnull");
            }

            if (typeParameter.HasConstructorConstraint)
            {
                constraints.Add("new()");
            }

            builder.Append(string.Join(", ", constraints));
        }

        return builder.ToString();
    }

    static bool HasAnyConstraints(ITypeParameterSymbol typeParameter)
    {
        return typeParameter.HasReferenceTypeConstraint ||
               typeParameter.HasValueTypeConstraint ||
               typeParameter.HasNotNullConstraint ||
               typeParameter.HasConstructorConstraint ||
               typeParameter.HasUnmanagedTypeConstraint ||
               typeParameter.ConstraintTypes.Any();
    }

    public static Location? GetConstructorParameterLocation(this AttributeData attributeData, int index)
    {
        var location = (attributeData.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax)?.ArgumentList?.Arguments[index].GetLocation();
        return location;
    }
}
