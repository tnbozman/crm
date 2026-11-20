using Crm.SharedKernel.Errors;

namespace Crm.__Module__.Validation;

/// <summary>The example item's rules, shared by the request DTO (boundary) and the model (invariant).</summary>
internal static class ItemRules
{
    public const string Name = nameof(Name);

    public const int NameMaxLength = 200;

    /// <summary>Returns the name trimmed, or throws a 400 validation error for a blank or over-long name.</summary>
    public static string RequireValidName(string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        return trimmed.Length is > 0 and <= NameMaxLength
            ? trimmed
            : throw new RequestValidationException(new Dictionary<string, string[]>
            {
                [Name] = [$"Name is required and at most {NameMaxLength} characters."],
            });
    }
}
