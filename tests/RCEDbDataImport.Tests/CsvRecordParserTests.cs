using RCEDbDataImport.Csv;

namespace RCEDbDataImport.Tests;

public sealed class CsvRecordParserTests
{
    [Fact]
    public void ParseLine_ReturnsQuotedFieldsAndEscapedQuotes()
    {
        var fields = CsvRecordParser.ParseLine("1,\"Smith, Jane\",\"Says \"\"hello\"\"\"");

        Assert.Equal(new[] { "1", "Smith, Jane", "Says \"hello\"" }, fields);
    }

    [Fact]
    public void ParseLine_ThrowsWhenQuotedFieldIsUnterminated()
    {
        Assert.Throws<FormatException>(() => CsvRecordParser.ParseLine("1,\"unterminated"));
    }
}
