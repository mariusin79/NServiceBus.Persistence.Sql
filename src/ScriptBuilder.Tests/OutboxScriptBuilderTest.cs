using System.IO;
using System.Text;
using ApprovalTests;
using ApprovalTests.Namers;
using NServiceBus.Persistence.Sql.ScriptBuilder;
using NUnit.Framework;

[TestFixture]
public class OutboxScriptBuilderTest
{
    [Test]
    [TestCase(BuildSqlVariant.MsSqlServer)]
    [TestCase(BuildSqlVariant.MySql)]
    [TestCase(BuildSqlVariant.Oracle)]
    public void BuildCreateScript(BuildSqlVariant sqlVariant)
    {
        var builder = new StringBuilder();
        using (var writer = new StringWriter(builder))
        {
            OutboxScriptBuilder.BuildCreateScript(writer,sqlVariant);
        }
        var script = builder.ToString();
        if (sqlVariant == BuildSqlVariant.MsSqlServer)
        {
            SqlValidator.Validate(script);
        }
        using (ApprovalResults.ForScenario(sqlVariant))
        {
            Approvals.Verify(script);
        }
    }

    [Test]
    [TestCase(BuildSqlVariant.MsSqlServer)]
    [TestCase(BuildSqlVariant.MySql)]
    [TestCase(BuildSqlVariant.Oracle)]
    public void BuildDropScript(BuildSqlVariant sqlVariant)
    {
        var builder = new StringBuilder();
        using (var writer = new StringWriter(builder))
        {
            OutboxScriptBuilder.BuildDropScript(writer, sqlVariant);
        }
        var script = builder.ToString();
        if (sqlVariant == BuildSqlVariant.MsSqlServer)
        {
            SqlValidator.Validate(script);
        }
        using (ApprovalResults.ForScenario(sqlVariant))
        {
            Approvals.Verify(script);
        }
    }
}