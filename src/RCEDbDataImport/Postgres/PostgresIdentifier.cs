using System.Text.RegularExpressions;

namespace RCEDbDataImport.Postgres;

public static partial class PostgresIdentifier
{
    public static string QuoteQualifiedName(string qualifiedName)
    {
        var parts = qualifiedName.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length is < 1 or > 2)
        {
            throw new ArgumentException("Table name must be either a table or schema.table identifier.", nameof(qualifiedName));
        }

        return string.Join('.', parts.Select(Quote));
    }

    public static string Quote(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || !IdentifierRegex().IsMatch(identifier))
        {
            throw new ArgumentException($"'{identifier}' is not a valid PostgreSQL identifier.", nameof(identifier));
        }

        return $"\"{identifier}\"";
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex IdentifierRegex();
}
