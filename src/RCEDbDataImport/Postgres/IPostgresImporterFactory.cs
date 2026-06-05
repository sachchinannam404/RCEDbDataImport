namespace RCEDbDataImport.Postgres;

public interface IPostgresImporterFactory
{
    ValueTask<IPostgresImporter> CreateAsync(string connectionString, string tableName, IReadOnlyList<string> columns, CancellationToken cancellationToken = default);
}
