using Npgsql;

namespace RCEDbDataImport.Postgres;

public sealed class NpgsqlPostgresImporterFactory : IPostgresImporterFactory
{
    public async ValueTask<IPostgresImporter> CreateAsync(
        string connectionString,
        string tableName,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(connectionString);
        NpgsqlTransaction? transaction = null;

        try
        {
            await connection.OpenAsync(cancellationToken);
            transaction = await connection.BeginTransactionAsync(cancellationToken);

            var quotedTable = PostgresIdentifier.QuoteQualifiedName(tableName);
            var quotedColumns = string.Join(", ", columns.Select(PostgresIdentifier.Quote));
            var copyCommand = $"COPY {quotedTable} ({quotedColumns}) FROM STDIN";
            var writer = await connection.BeginTextImportAsync(copyCommand, cancellationToken);

            return new NpgsqlPostgresImporter(connection, transaction, writer);
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }

            await connection.DisposeAsync();
            throw;
        }
    }

    private sealed class NpgsqlPostgresImporter(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        TextWriter writer) : IPostgresImporter
    {
        private bool completed;

        public async ValueTask WriteRowAsync(IReadOnlyList<string?> values, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await writer.WriteLineAsync(PostgresCopyTextEncoder.EncodeRow(values));
        }

        public async ValueTask CompleteAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await writer.FlushAsync();
            await writer.DisposeAsync();
            await transaction.CommitAsync(cancellationToken);
            completed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (!completed)
            {
                try
                {
                    await writer.DisposeAsync();
                }
                finally
                {
                    await transaction.RollbackAsync();
                }
            }

            await transaction.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
