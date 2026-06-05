using Microsoft.Extensions.Logging;
using RCEDbDataImport.Configuration;
using RCEDbDataImport.Csv;
using RCEDbDataImport.Postgres;
using RCEDbDataImport.Services;

namespace RCEDbDataImport.Tests;

public sealed class CsvImportServiceTests
{
    [Fact]
    public async Task ImportAsync_UsesHeaderColumnsAndWritesRows()
    {
        var csvReader = new StubCsvFileReader([
            ["id", "name"],
            ["1", "Ada"],
            ["2", ""]
        ]);
        var importerFactory = new CapturingImporterFactory();
        var service = new CsvImportService(csvReader, importerFactory, new ListLogger<CsvImportService>());

        var rowCount = await service.ImportAsync(new CsvImportOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            TableName = "public.people",
            CsvFilePath = "people.csv",
            HasHeaderRecord = true,
            BatchSize = 1
        });

        Assert.Equal(2, rowCount);
        Assert.Equal("Host=localhost;Database=test", importerFactory.ConnectionString);
        Assert.Equal("public.people", importerFactory.TableName);
        Assert.Equal(new[] { "id", "name" }, importerFactory.Columns);
        Assert.Equal(2, importerFactory.Importer.Rows.Count);
        Assert.Equal(new string?[] { "1", "Ada" }, importerFactory.Importer.Rows[0]);
        Assert.Equal(new string?[] { "2", null }, importerFactory.Importer.Rows[1]);
        Assert.True(importerFactory.Importer.Completed);
    }

    [Fact]
    public async Task ImportAsync_UsesConfiguredColumnsWhenNoHeaderExists()
    {
        var csvReader = new StubCsvFileReader([
            ["1", "Ada"]
        ]);
        var importerFactory = new CapturingImporterFactory();
        var service = new CsvImportService(csvReader, importerFactory, new ListLogger<CsvImportService>());

        var rowCount = await service.ImportAsync(new CsvImportOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            TableName = "people",
            CsvFilePath = "people.csv",
            HasHeaderRecord = false,
            Columns = ["id", "name"]
        });

        Assert.Equal(1, rowCount);
        Assert.Equal(new[] { "id", "name" }, importerFactory.Columns);
        Assert.Equal(new string?[] { "1", "Ada" }, importerFactory.Importer.Rows.Single());
    }

    [Fact]
    public async Task ImportAsync_ThrowsWhenFieldCountDoesNotMatchColumns()
    {
        var csvReader = new StubCsvFileReader([
            ["id", "name"],
            ["1"]
        ]);
        var service = new CsvImportService(csvReader, new CapturingImporterFactory(), new ListLogger<CsvImportService>());

        await Assert.ThrowsAsync<FormatException>(() => service.ImportAsync(new CsvImportOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            TableName = "people",
            CsvFilePath = "people.csv",
            HasHeaderRecord = true
        }));
    }

    private sealed class StubCsvFileReader(IReadOnlyList<IReadOnlyList<string>> rows) : ICsvFileReader
    {
        public async IAsyncEnumerable<IReadOnlyList<string>> ReadRowsAsync(string filePath, char delimiter, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                yield return row;
            }
        }
    }

    private sealed class CapturingImporterFactory : IPostgresImporterFactory
    {
        public string? ConnectionString { get; private set; }

        public string? TableName { get; private set; }

        public IReadOnlyList<string> Columns { get; private set; } = [];

        public CapturingImporter Importer { get; } = new();

        public ValueTask<IPostgresImporter> CreateAsync(string connectionString, string tableName, IReadOnlyList<string> columns, CancellationToken cancellationToken = default)
        {
            ConnectionString = connectionString;
            TableName = tableName;
            Columns = columns.ToList();
            return ValueTask.FromResult<IPostgresImporter>(Importer);
        }
    }

    private sealed class CapturingImporter : IPostgresImporter
    {
        public List<IReadOnlyList<string?>> Rows { get; } = [];

        public bool Completed { get; private set; }

        public ValueTask WriteRowAsync(IReadOnlyList<string?> values, CancellationToken cancellationToken = default)
        {
            Rows.Add(values.ToList());
            return ValueTask.CompletedTask;
        }

        public ValueTask CompleteAsync(CancellationToken cancellationToken = default)
        {
            Completed = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }
}
