namespace RCEDbDataImport.Csv;

public sealed class CsvFileReader : ICsvFileReader
{
    public async IAsyncEnumerable<IReadOnlyList<string>> ReadRowsAsync(
        string filePath,
        char delimiter,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        var recordBuilder = new System.Text.StringBuilder();

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                yield break;
            }

            if (recordBuilder.Length > 0)
            {
                recordBuilder.AppendLine();
            }

            recordBuilder.Append(line);

            if (!IsCompleteRecord(recordBuilder.ToString()))
            {
                continue;
            }

            yield return CsvRecordParser.ParseLine(recordBuilder.ToString(), delimiter);
            recordBuilder.Clear();
        }

        if (recordBuilder.Length > 0)
        {
            throw new FormatException("CSV file ended before a quoted field was closed.");
        }
    }

    private static bool IsCompleteRecord(string record)
    {
        var inQuotes = false;

        for (var i = 0; i < record.Length; i++)
        {
            if (record[i] != '\"')
            {
                continue;
            }

            if (inQuotes && i + 1 < record.Length && record[i + 1] == '\"')
            {
                i++;
                continue;
            }

            inQuotes = !inQuotes;
        }

        return !inQuotes;
    }
}
