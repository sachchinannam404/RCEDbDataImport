using RCEDbDataImport.Postgres;

namespace RCEDbDataImport.Tests;

public sealed class PostgresIdentifierTests
{
    [Theory]
    [InlineData("customers", "\"customers\"")]
    [InlineData("public.customers", "\"public\".\"customers\"")]
    public void QuoteQualifiedName_QuotesValidTableNames(string tableName, string expected)
    {
        Assert.Equal(expected, PostgresIdentifier.QuoteQualifiedName(tableName));
    }

    [Theory]
    [InlineData("public.customers;drop table users")]
    [InlineData("public.customer-orders")]
    [InlineData("public.customers.extra")]
    public void QuoteQualifiedName_RejectsUnsafeTableNames(string tableName)
    {
        Assert.Throws<ArgumentException>(() => PostgresIdentifier.QuoteQualifiedName(tableName));
    }
}
