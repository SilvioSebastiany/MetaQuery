using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace MetaQuery.Api.Infrastructure;

/// <summary>
/// Transforms route parameters to kebab-case (lowercase with hyphens)
/// Constitution 2.7: APIs Estáveis - rotas em kebab-case
///
/// Example: ConsultaDinamica → consulta-dinamica
///          QueryBuilderTest → query-builder-test
/// </summary>
public partial class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    /// <summary>
    /// Transforms a route value to kebab-case
    /// </summary>
    /// <param name="value">The route value to transform</param>
    /// <returns>The kebab-case version of the value, or null if input is null</returns>
    public string? TransformOutbound(object? value)
    {
        if (value == null)
            return null;

        var str = value.ToString();
        if (string.IsNullOrEmpty(str))
            return str;

        // Insert hyphen before each uppercase letter (except the first)
        // and convert to lowercase
        return KebabCaseRegex().Replace(str, "$1-$2").ToLowerInvariant();
    }

    /// <summary>
    /// Regex pattern to identify word boundaries in PascalCase/camelCase strings
    /// Matches: uppercase letter followed by lowercase, or lowercase followed by uppercase
    /// </summary>
    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex KebabCaseRegex();
}
