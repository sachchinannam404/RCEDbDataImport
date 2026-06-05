# RCE Database Data Import

A .NET 8 console application that imports CSV rows into a configurable PostgreSQL table using PostgreSQL `COPY` text import.

## Configuration

Configuration can be provided through `appsettings.json`, environment variables prefixed with `RCEIMPORT_`, or command-line arguments.

```json
{
  "Import": {
    "ConnectionString": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres",
    "TableName": "public.target_table",
    "CsvFilePath": "data.csv",
    "Delimiter": ",",
    "HasHeaderRecord": true,
    "Columns": [],
    "BatchSize": 5000
  }
}
```

When `HasHeaderRecord` is `true`, the first CSV line supplies column names. When it is `false`, set `Columns` to the target table columns in file order.

## Running

```bash
dotnet run --project src/RCEDbDataImport -- \
  --Import:ConnectionString "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres" \
  --Import:TableName "public.customers" \
  --Import:CsvFilePath "./customers.csv"
```

Environment variable example:

```bash
RCEIMPORT_Import__TableName=public.customers dotnet run --project src/RCEDbDataImport
```

## Testing

```bash
dotnet test
```

Run tests with coverage collection:

```bash
dotnet test --collect:"XPlat Code Coverage"
```
