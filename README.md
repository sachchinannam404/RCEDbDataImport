# RCE Database Data Import

A .NET 8 console application that imports CSV rows into a configurable PostgreSQL table using PostgreSQL `COPY` text import.

## Configuration

Configuration can be provided through `appsettings.json`, environment variables prefixed with `RCEIMPORT_`, or command-line arguments.

```json
{
  "Import": {
    "ConnectionString": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres",
    "TableName": "public.target_table",
    "CsvFilePath": "data/sample_customers.csv",
    "Delimiter": ",",
    "HasHeaderRecord": true,
    "Columns": [],
    "BatchSize": 5000
  }
}
```

When `HasHeaderRecord` is `true`, the first CSV line supplies column names. When it is `false`, set `Columns` to the target table columns in file order.

The solution includes `data/sample_customers.csv`, and the default `appsettings.json` points to that sample file. Create a matching table before running the default import, for example:

```sql
CREATE TABLE public.target_table (
  id text,
  name text,
  email text,
  signup_date text,
  notes text
);
```

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
