namespace RCEDbDataImport.Postgres;

public interface IPostgresImporter : IAsyncDisposable
{
    ValueTask WriteRowAsync(IReadOnlyList<string?> values, CancellationToken cancellationToken = default);

    ValueTask CompleteAsync(CancellationToken cancellationToken = default);
}
