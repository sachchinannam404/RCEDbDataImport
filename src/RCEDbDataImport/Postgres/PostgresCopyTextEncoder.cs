using System.Text;

namespace RCEDbDataImport.Postgres;

public static class PostgresCopyTextEncoder
{
    public static string EncodeRow(IReadOnlyList<string?> values)
    {
        return string.Join('\t', values.Select(EncodeValue));
    }

    public static string EncodeValue(string? value)
    {
        if (value is null)
        {
            return "\\N";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(character switch
            {
                '\\' => "\\\\",
                '\t' => "\\t",
                '\n' => "\\n",
                '\r' => "\\r",
                _ => character.ToString()
            });
        }

        return builder.ToString();
    }
}
