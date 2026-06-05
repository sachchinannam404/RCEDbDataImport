using RCEDbDataImport.Postgres;

namespace RCEDbDataImport.Tests;

public sealed class PostgresCopyTextEncoderTests
{
    [Fact]
    public void EncodeRow_UsesPostgresCopyTextEscaping()
    {
        var row = PostgresCopyTextEncoder.EncodeRow(["1", null, "line one\nline two", "a\\b", "x\ty"]);

        Assert.Equal("1\t\\N\tline one\\nline two\ta\\\\b\tx\\ty", row);
    }
}
