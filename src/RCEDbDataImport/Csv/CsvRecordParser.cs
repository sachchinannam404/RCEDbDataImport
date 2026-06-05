namespace RCEDbDataImport.Csv;

public static class CsvRecordParser
{
    public static IReadOnlyList<string> ParseLine(string line, char delimiter = ',')
    {
        if (delimiter == '"' || delimiter == '\r' || delimiter == '\n')
        {
            throw new ArgumentException("Delimiter cannot be a quote or newline character.", nameof(delimiter));
        }

        var fields = new List<string>();
        var field = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var current = line[i];

            if (current == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (current == delimiter && !inQuotes)
            {
                fields.Add(field.ToString());
                field.Clear();
                continue;
            }

            field.Append(current);
        }

        if (inQuotes)
        {
            throw new FormatException("CSV line contains an unterminated quoted field.");
        }

        fields.Add(field.ToString());
        return fields;
    }
}
