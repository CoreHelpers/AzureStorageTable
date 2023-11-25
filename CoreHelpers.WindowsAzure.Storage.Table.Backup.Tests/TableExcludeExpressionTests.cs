using CoreHelpers.WindowsAzure.Storage.Table.Backup.Internal;

namespace CoreHelpers.WindowsAzure.Storage.Table.Backup.Tests;

public class TableExcludeExpressionTests
{
    [Fact]
    public void VerifyFullTableNameTests()
    {
        var validator = new TabledExcludeExpressionValidator(new string[] { "demoTable", "secondTable" });
        Assert.True(validator.IsTableExcluded("demoTable"));
        Assert.True(validator.IsTableExcluded("demotable"));
        Assert.True(validator.IsTableExcluded("DEMOTABLE"));
        Assert.True(validator.IsTableExcluded("secondtable"));
        Assert.False(validator.IsTableExcluded("demo*"));
        Assert.False(validator.IsTableExcluded("*Table"));
        Assert.False(validator.IsTableExcluded("otherTable"));
    }
    
    [Fact]
    public void VerifyRegExPatternTableNameTests()
    {
        var validator = new TabledExcludeExpressionValidator(new string[] { "demo.*" });
        Assert.True(validator.IsTableExcluded("demoTable"));
        Assert.True(validator.IsTableExcluded("demotable"));
        Assert.True(validator.IsTableExcluded("DEMOTABLE"));
        Assert.True(validator.IsTableExcluded("demo*"));
        Assert.False(validator.IsTableExcluded("*Table"));
        Assert.False(validator.IsTableExcluded("otherTable"));
    }
}