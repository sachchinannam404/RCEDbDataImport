namespace RCEDbDataImport.Configuration;

public sealed class CsvImportOptions
{
    public const string SectionName = "Import";

    public string ConnectionString { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public string CsvFilePath { get; set; } = string.Empty;

    public char Delimiter { get; set; } = ',';

    public bool HasHeaderRecord { get; set; } = true;

    public string[] Columns { get; set; } = [];

    public int BatchSize { get; set; } = 5_000;
}
