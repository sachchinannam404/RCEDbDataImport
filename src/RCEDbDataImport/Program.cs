using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RCEDbDataImport.Configuration;
using RCEDbDataImport.Csv;
using RCEDbDataImport.Postgres;
using RCEDbDataImport.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables(prefix: "RCEIMPORT_")
    .AddCommandLine(args)
    .Build();

var options = new CsvImportOptions();
configuration.GetSection(CsvImportOptions.SectionName).Bind(options);
configuration.Bind(options);

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConfiguration(configuration.GetSection("Logging"))
        .AddSimpleConsole(console =>
        {
            console.SingleLine = true;
            console.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        });
});

var logger = loggerFactory.CreateLogger("RCEDbDataImport");

try
{
    var service = new CsvImportService(
        new CsvFileReader(),
        new NpgsqlPostgresImporterFactory(),
        loggerFactory.CreateLogger<CsvImportService>());

    var rowCount = await service.ImportAsync(options);
    logger.LogInformation("Import completed successfully with {RowCount} rows.", rowCount);
    return 0;
}
catch (Exception exception)
{
    logger.LogError(exception, "Import failed.");
    return 1;
}
