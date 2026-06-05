using Microsoft.Extensions.Logging;
using RCEDbDataImport.Configuration;
using RCEDbDataImport.Csv;
using RCEDbDataImport.Postgres;

namespace RCEDbDataImport.Services;

public sealed class CsvImportService(
    ICsvFileReader csvFileReader,
    IPostgresImporterFactory importerFactory,
    ILogger<CsvImportService> logger)
{
    public async Task<int> ImportAsync(CsvImportOptions options, CancellationToken cancellationToken = default)
    {
        ValidateOptions(options);

        logger.LogInformation("Starting CSV import from {CsvFilePath} to PostgreSQL table {TableName}.", options.CsvFilePath, options.TableName);

        var columns = options.Columns.ToList();
        var importedRows = 0;
        await using var enumerator = csvFileReader.ReadRowsAsync(options.CsvFilePath, options.Delimiter, cancellationToken).GetAsyncEnumerator(cancellationToken);

        if (options.HasHeaderRecord)
        {
            if (!await enumerator.MoveNextAsync())
            {
                logger.LogWarning("CSV file {CsvFilePath} is empty; no rows were imported.", options.CsvFilePath);
                return 0;
            }

            columns = enumerator.Current.Select(column => column.Trim()).ToList();
            ValidateColumns(columns);
        }
        else
        {
            ValidateColumns(columns);
        }

        await using var importer = await importerFactory.CreateAsync(options.ConnectionString, options.TableName, columns, cancellationToken);

        while (await enumerator.MoveNextAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = enumerator.Current;
            if (row.Count != columns.Count)
            {
                throw new FormatException($"CSV row {importedRows + 1} contains {row.Count} fields but {columns.Count} columns were configured.");
            }

            await importer.WriteRowAsync(row.Select(value => string.IsNullOrEmpty(value) ? null : value).ToList(), cancellationToken);
            importedRows++;

            if (importedRows % options.BatchSize == 0)
            {
                logger.LogInformation("Imported {ImportedRows} rows into {TableName} so far.", importedRows, options.TableName);
            }
        }

        await importer.CompleteAsync(cancellationToken);
        logger.LogInformation("Finished CSV import. Imported {ImportedRows} rows into {TableName}.", importedRows, options.TableName);
        return importedRows;
    }

    private static void ValidateOptions(CsvImportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new ArgumentException("A PostgreSQL connection string is required.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.TableName))
        {
            throw new ArgumentException("A target table name is required.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.CsvFilePath))
        {
            throw new ArgumentException("A CSV file path is required.", nameof(options));
        }

        if (options.BatchSize <= 0)
        {
            throw new ArgumentException("BatchSize must be greater than zero.", nameof(options));
        }

        _ = PostgresIdentifier.QuoteQualifiedName(options.TableName);
    }

    private static void ValidateColumns(IReadOnlyCollection<string> columns)
    {
        if (columns.Count == 0)
        {
            throw new ArgumentException("At least one column is required when the CSV file has no header row.");
        }

        foreach (var column in columns)
        {
            _ = PostgresIdentifier.Quote(column);
        }
    }
}
