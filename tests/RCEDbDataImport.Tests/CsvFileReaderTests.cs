using RCEDbDataImport.Csv;

namespace RCEDbDataImport.Tests;

public sealed class CsvFileReaderTests
{
    [Fact]
    public async Task ReadRowsAsync_ReturnsRecordsWithQuotedNewlines()
    {
        var filePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(filePath, "id,notes\n1,\"line one\nline two\"\n");

        try
        {
            var rows = new List<IReadOnlyList<string>>();
            await foreach (var row in new CsvFileReader().ReadRowsAsync(filePath, ','))
            {
                rows.Add(row);
            }

            Assert.Equal(2, rows.Count);
            Assert.Equal(new[] { "id", "notes" }, rows[0]);
            Assert.Equal(new[] { "1", "line one\nline two" }, rows[1]);
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
