using System.Data;
using System.Text;
using System.Text.Json.Nodes;
using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Generator;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Core.Services;
using BetaJsonIntegrator.Core.Validation;
using BetaJsonIntegrator.Data.Providers;
using Xunit;

namespace BetaJsonIntegrator.Tests;

/// <summary>Primeiro marco (spec. 15.1): XML SATUBA + registros em memória → JSON → validação → exportação.</summary>
public class SatubaTests
{
    private static string LayoutPath => Path.Combine(AppContext.BaseDirectory, "TestData", "SATUBA.layout.xml");

    private static DataTable SampleData()
    {
        var t = new DataTable();
        foreach (var name in new[]
                 {
                     "TpdeServico", "DtAto", "TipoAto", "Folha", "NrMatricula", "DtJur", "FormPgto", "TPOp", "TpImovel",
                     "TpLogradouro", "Logradouro", "Numero", "municipio", "Cep", "CpfAlienante", "cpfcnpjAdquirente", "numeroLivro"
                 })
            t.Columns.Add(name, typeof(string));

        var r = t.NewRow();
        r["TpdeServico"] = "1"; r["DtAto"] = "15/03/2026"; r["TipoAto"] = "1"; r["Folha"] = "12";
        r["NrMatricula"] = "000123"; r["DtJur"] = "2026-03-10"; r["FormPgto"] = "1"; r["TPOp"] = "1";
        r["TpImovel"] = "1"; r["TpLogradouro"] = "RUA"; r["Logradouro"] = "A"; r["Numero"] = "10";
        r["municipio"] = "CENTRO"; r["Cep"] = "57000000"; r["CpfAlienante"] = "11111111111";
        r["cpfcnpjAdquirente"] = "22222222222"; r["numeroLivro"] = "2";
        t.Rows.Add(r);
        return t;
    }

    private static LayoutBundle SaturbaBundle(out string folder)
    {
        folder = Path.Combine(Path.GetTempPath(), "bji-" + Guid.NewGuid().ToString("N"));
        var repo = new LayoutRepository(folder);
        return repo.Create("SATUBA", LayoutPath, null, "teste");
    }

    [Fact]
    public void Parser_reads_legacy_satuba_xml()
    {
        var parse = XmlLayoutParser.ParseText(File.ReadAllText(LayoutPath));

        Assert.NotNull(parse.Layout);
        Assert.Equal("NOV2025009", parse.Layout!.Identifier);
        var gen = Assert.Single(parse.Layout.Generates);
        Assert.Equal("TAB_SATUBA", Assert.Single(gen.Tables).Alias);
        Assert.NotEmpty(gen.JsonDefinitions);
        Assert.DoesNotContain(parse.Issues, i => i.Severity == IssueSeverity.Error);
    }

    [Fact]
    public void Satuba_has_no_pending_fields_and_treats_legacy_value_as_column()
    {
        var parse = XmlLayoutParser.ParseText(File.ReadAllText(LayoutPath));
        var gen = parse.Layout!.Generates[0];

        var result = new LayoutValidator().Validate(gen, new CustomizationSet());

        Assert.False(result.HasErrors);
        Assert.Empty(result.Plan.Pending);
        Assert.Contains(result.Issues, i => i.Code == "LEGACY_VALUE_AS_COLUMN");

        var ni = result.Plan.Fields.Keys.First(f => f.Path == "declaracoes.adquirentes.ni");
        Assert.Equal(FieldKind.Column, result.Plan.Get(ni)!.Kind);
        Assert.Equal("cpfcnpjAdquirente", result.Plan.Get(ni)!.Column);

        var reps = result.Plan.Fields.Keys.First(f => f.Path == "declaracoes.adquirentes.representantes");
        Assert.Equal(FieldKind.EmptyContainer, result.Plan.Get(reps)!.Kind);
    }

    [Fact]
    public void Generates_typed_nested_json()
    {
        var bundle = SaturbaBundle(out var folder);
        try
        {
            var svc = new IntegratorService(new LayoutRepository(folder));
            var run = svc.Run(bundle, new DataTableProvider(SampleData()));

            Assert.False(run.Blocked);
            Assert.Equal(0, run.ErrorCount);
            Assert.Equal(1, run.RecordCount);

            var root = JsonNode.Parse(run.Json!)!;
            var d = root["declaracoes"]![0]!;
            Assert.Equal("000123", (string?)d["matricula"]);                 // zeros à esquerda preservados
            Assert.Equal("2026-03-15", (string?)d["dataLavraturaRegistroAverbacao"]);
            Assert.Equal("22222222222", (string?)d["adquirentes"]![0]!["ni"]);
            Assert.Equal(0, d["adquirentes"]![0]!["representantes"]!.AsArray().Count);
            Assert.Equal(100m, (decimal)d["adquirentes"]![0]!["Participacao"]!); // número, não texto
            Assert.Equal("2708907", (string?)d["codigoIbge"]);
        }
        finally { Directory.Delete(folder, true); }
    }

    [Fact]
    public void Pending_field_blocks_generation_and_customization_resolves_it()
    {
        var xml = """
            <Layout name="T" identifier="X1">
              <generate name="G"><table name="Tab" alias="T1"><selection>
                <Column name="A" type="string"/></selection></table></generate>
              <Jsonfields name="J"><Refealias name="T1">
                <field name="items" type="array">
                  <field name="a" type="string" source="A"/>
                  <field name="natureza" type="string" required="true" configurable="true"/>
                </field></Refealias></Jsonfields>
            </Layout>
            """;
        var parse = XmlLayoutParser.ParseText(xml);
        var gen = parse.Layout!.Generates[0];

        var before = new LayoutValidator().Validate(gen, new CustomizationSet());
        Assert.Single(before.Plan.Pending);
        Assert.True(before.HasErrors);

        var set = new CustomizationSet();
        set.Upsert(new Customization { Name = "natureza", Mode = CustomizationModes.Fixed, Value = "2" });
        var after = new LayoutValidator().Validate(gen, set);
        Assert.Empty(after.Plan.Pending);
        Assert.False(after.HasErrors);
    }

    [Theory]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("100,5", 100.5)]
    [InlineData("100", 100)]
    [InlineData("1,234.50", 1234.50)]
    public void Decimal_conversion_is_invariant_and_numeric(string text, double expected)
    {
        Assert.True(JsonValueConverter.TryConvert(FieldTypes.Decimal, text, null, null, out var node, out _));
        Assert.Equal((decimal)expected, (decimal)node!);
    }

    [Fact]
    public void Invalid_date_reports_error_without_echoing_the_value()
    {
        Assert.False(JsonValueConverter.TryConvert(FieldTypes.DateTime, "30/02/2026", null, null, out _, out var error));
        Assert.DoesNotContain("30/02", error);
    }

    [Fact]
    public void Schema_validator_reports_paths_for_required_type_and_additional()
    {
        const string schema = """
            { "type":"object","additionalProperties":false,
              "properties":{ "declaracoes":{ "type":"array","items":{ "$ref":"#/definitions/D" } } },
              "definitions":{ "D":{ "type":"object","additionalProperties":false,"required":["ni","tipo"],
                "properties":{ "ni":{"type":"string"},
                               "tipo":{"oneOf":[{"const":"1"},{"const":"2"}]},
                               "participacao":{"type":"number"} } } } }
            """;
        const string json = """{ "declaracoes":[ { "ni": 5, "tipo":"9", "Participacao": 100 } ] }""";

        var result = JsonSchemaValidator.Validate(json, schema);
        var paths = result.Issues.Select(i => i.Path).ToList();

        Assert.Contains("declaracoes[0].ni", paths);          // tipo errado
        Assert.Contains("declaracoes[0].tipo", paths);        // fora do oneOf/const
        Assert.Contains("declaracoes[0].Participacao", paths); // propriedade extra (grafia)
        Assert.Contains(result.Issues, i => i.Message.Contains("participacao"));
    }

    [Fact]
    public void Csv_provider_reads_quoted_fields_and_orders()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "B;A\r\n\"x;y\";2\r\nz;1\r\n", new UTF8Encoding(false));
            var table = new CsvProvider(file, new CsvOptions { Delimiter = ';' }).Load(new DataRequest
            {
                Columns = new[] { new SourceColumn("B"), new SourceColumn("A", orderBy: true) }
            });

            Assert.Equal(2, table.Rows.Count);
            Assert.Equal("1", table.Rows[0]["A"]);
            Assert.Equal("x;y", table.Rows[1]["B"]);
        }
        finally { File.Delete(file); }
    }

    private sealed class FakeFactory : System.Data.Common.DbProviderFactory { }

    [Fact]
    public void Db_select_quotes_identifiers_and_parameterizes_filters()
    {
        var provider = new SqlServerProvider(new FakeFactory(), "Server=x");
        var select = provider.BuildSelect(new DataRequest
        {
            TableName = "dbo.Tab",
            Columns = new[] { new SourceColumn("A", orderBy: true), new SourceColumn("B") },
            Filters = new[] { new DataFilter("A", "x'; DROP TABLE Tab;--") }
        });

        Assert.Equal("SELECT [A], [B] FROM [dbo].[Tab] WHERE [A] = @p0 ORDER BY [A]", select.Sql);
        Assert.DoesNotContain("DROP", select.Sql);
        Assert.Single(select.Parameters);

        Assert.Throws<ArgumentException>(() => provider.BuildSelect(new DataRequest
        {
            TableName = "Tab; DROP TABLE X",
            Columns = new[] { new SourceColumn("A") }
        }));
    }
}
