namespace RCEDbDataImport.Csv;

public interface ICsvFileReader
{
    IAsyncEnumerable<IReadOnlyList<string>> ReadRowsAsync(string filePath, char delimiter, CancellationToken cancellationToken = default);
}
