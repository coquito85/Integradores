using System.Data;
using System.Text;
using System.Text.Json.Nodes;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Core.Services;
using BetaJsonIntegrator.Core.Validation;
using BetaJsonIntegrator.Data;
using BetaJsonIntegrator.Data.Providers;

// Smoke test de ponta a ponta (primeiro marco funcional da especificação, seção 15.1).
int failures = 0;
void Check(bool ok, string what)
{
    Console.WriteLine((ok ? "  OK   " : "  FALHA ") + what);
    if (!ok) failures++;
}

var sampleRoot = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "Configurations", "Layouts");
var work = Path.Combine(Path.GetTempPath(), "bji-smoke-" + Guid.NewGuid().ToString("N")[..8]);
var layouts = Path.Combine(work, "Layouts");
Directory.CreateDirectory(layouts);
CopyDir(Path.Combine(sampleRoot, "SATUBA"), Path.Combine(layouts, "SATUBA"));

DataTable MakeTable(params (string tp, DateTime dt, string mat, string alien, string adq)[] rows)
{
    var t = new DataTable("TAB_SATUBA");
    t.Columns.Add("TpdeServico", typeof(string));
    t.Columns.Add("DtAto", typeof(DateTime));
    t.Columns.Add("NrMatricula", typeof(string));
    t.Columns.Add("CpfAlienante", typeof(string));
    t.Columns.Add("CpfAdquirente", typeof(string));
    foreach (var r in rows) t.Rows.Add(r.tp, r.dt, r.mat, r.alien, r.adq);
    return t;
}

var repo = new LayoutRepository(layouts);
var svc = new IntegratorService(repo, new FileIntegratorLog(Path.Combine(work, "Logs")));

Console.WriteLine("1) Layouts e parser");
Check(repo.List().Count == 1, "repositório lista 1 layout");
var bundle = svc.LoadLayout("SATUBA");
Check(bundle.IsUsable, "XML SATUBA interpretado sem erros");
var gen = bundle.Layout!.Generates[0];
Check(gen.Tables[0].Columns.Count == 5 && gen.Tables[0].Columns[0].OrderBy, "colunas e orderby lidos");

Console.WriteLine("2) Geração com dados válidos + personalização");
var good = new DataTableProvider().Add("TAB_SATUBA", MakeTable(
    ("B-Escritura", new DateTime(2026, 3, 5), "00123", "11144477735", "52998224725"),
    ("A-Procuração", new DateTime(2026, 3, 6), "00456", "12345678000195", "11144477735")));
var run = svc.Run(bundle, good);
Console.WriteLine(run.Json);
Check(!run.Blocked && run.ErrorCount == 0, $"sem erros (erros={run.ErrorCount})");
Check(run.RecordCount == 2, "2 registros lidos");
var root = JsonNode.Parse(run.Json!)!.AsObject();
var decl = root["declaracoes"]!.AsArray();
Check(decl.Count == 2, "2 declarações geradas");
Check(decl[0]!["tipoServico"]!.GetValue<string>() == "A-Procuração", "orderby aplicado (A antes de B)");
Check(decl[0]!["matricula"]!.GetValue<string>() == "00456", "zeros à esquerda preservados em string");
Check(decl[0]!["adquirentes"]![0]!["participacao"]!.ToJsonString() == "100", "participacao numérica (não texto): " + decl[0]!["adquirentes"]![0]!["participacao"]!.ToJsonString());
Check(decl[0]!["adquirentes"]![0]!["indicadorEspolio"]!.GetValue<bool>() == false, "bool gerado como boolean");
Check(decl[0]!["adquirentes"]![0]!["representantes"]!.AsArray().Count == 0, "representantes = []");
Check(decl[0]!["dataLavraturaRegistroAverbacao"]!.GetValue<string>() == "2026-03-06", "data yyyy-MM-dd");
Check(decl[0]!["naturezaTitulo"]!.GetValue<string>() == "2", "personalização aplicada (naturezaTitulo)");
Check(run.SchemaValidated, "validação por Schema executada");
Check(run.Issues.Any(i => i.Code == "LEGACY_VALUE_AS_COLUMN"), "value legado → coluna reconhecido");

Console.WriteLine("3) Pendência sem personalização (nada é inventado)");
File.Delete(bundle.CustomizationsPath);
var bundleNoCust = svc.LoadLayout("SATUBA");
var blocked = svc.Run(bundleNoCust, good);
Check(blocked.Blocked && blocked.Json == null, "geração bloqueada");
Check(blocked.Pending.Count == 1 && blocked.Pending[0].Path == "declaracoes.naturezaTitulo", "campo pendente listado com caminho");
var preview = svc.Run(bundleNoCust, good, new RunOptions { AllowPending = true });
Check(preview.HasJson && !preview.Json!.Contains("naturezaTitulo"), "prévia não preenche o campo pendente");
Check(preview.Issues.Any(i => i.Code == "REQUIRED_MISSING" && i.Path == "declaracoes[0].naturezaTitulo"), "Schema acusa required ausente com caminho");

Console.WriteLine("4) Schema aponta caminho do erro (CPF inválido, data) ");
bundleNoCust.Customizations.Upsert(new Customization { Name = "naturezaTitulo", Mode = CustomizationModes.Fixed, Value = "9" });
repo.SaveCustomizations(bundleNoCust);
var bad = new DataTableProvider().Add("TAB_SATUBA", MakeTable(("A", new DateTime(2026, 1, 1), "1", "123", "52998224725")));
var runBad = svc.Run(svc.LoadLayout("SATUBA"), bad);
foreach (var i in runBad.Issues.Where(i => i.Severity == IssueSeverity.Error)) Console.WriteLine("    " + i);
Check(runBad.Issues.Any(i => i.Path == "declaracoes[0].alienantes[0].ni" && i.Code == "PATTERN_MISMATCH"), "caminho declaracoes[0].alienantes[0].ni");
Check(runBad.Issues.Any(i => i.Path == "declaracoes[0].naturezaTitulo" && i.Code == "ONEOF_MISMATCH"), "oneOf/const validado");
Check(!runBad.Issues.Any(i => i.Message.Contains("123") || i.Message.Contains("52998224725")), "mensagens não vazam dados");

Console.WriteLine("5) Referência inexistente e coluna ausente na origem");
var xml = bundle.LayoutXmlText.Replace("Columnrefe=\"NrMatricula\"", "Columnrefe=\"NaoExiste\"");
var b2 = repo.SaveLayoutXml("SATUBA", xml, createVersion: true);
Check(b2.Metadata.LayoutVersion == 2 && Directory.Exists(Path.Combine(b2.FolderPath, "history", "v1")), "nova versão arquivada em history/v1");
var r2 = svc.Run(b2, good);
Check(r2.Blocked && r2.Issues.Any(i => i.Code == "INVALID_COLUMN_REFERENCE" && i.Path == "declaracoes.matricula"), "INVALID_COLUMN_REFERENCE com contexto");
Check(!b2.ParseIssues.Any(i => i.Code == "CUSTOMIZATION_VERSION_MISMATCH"), "personalizações acompanham a versão");

Console.WriteLine("6) CSV (; , ISO-8859-1, aspas)");
var csv = Path.Combine(work, "dados.csv");
File.WriteAllText(csv,
    "TpdeServico;DtAto;NrMatricula;CpfAlienante;CpfAdquirente\n" +
    "\"Escritura; pública\";05/03/2026;00123;11144477735;52998224725\n" +
    "Procuração;06/03/2026;00456;12345678000195;\n", Encoding.Latin1);
var b3 = repo.SaveLayoutXml("SATUBA", bundle.LayoutXmlText, createVersion: false);
var r3 = svc.Run(b3, new CsvProvider(csv, new CsvOptions { Delimiter = ';', Encoding = Encoding.Latin1 }));
Check(r3.RecordCount == 2, "CSV: 2 registros");
Check(r3.Json!.Contains("Escritura; pública"), "CSV: aspas e acentos");
Check(r3.Json.Contains("2026-03-05"), "CSV: data dd/MM/yyyy convertida para yyyy-MM-dd");
Check(r3.Issues.Any(i => i.Code == "REQUIRED_MISSING" && i.Path == "declaracoes[1].adquirentes[0].ni"), "CSV: ni ausente apontado em declaracoes[1].adquirentes[0].ni");

Console.WriteLine("7) Exportação sem sobrescrever");
var export = new ExportService(Path.Combine(work, "Output"));
var e1 = export.Export("SATUBA", run);
var e2 = export.Export("SATUBA", run);
Check(e1.Written && File.Exists(e1.JsonPath) && File.Exists(e1.ReportPath), "exportou json + relatório");
Check(!e2.Written && e2.ExistingFiles.Count == 2, "não sobrescreve sem confirmação");
Check(export.Export("SATUBA", run, ExistsPolicy.Timestamp).Written, "política Timestamp preserva o original");
Check(FileIntegratorLog.Mask("cpf 111.444.777-35 e 12345678000195") == "cpf *** e ***", "log mascara CPF/CNPJ");
var logText = File.ReadAllText(Directory.GetFiles(Path.Combine(work, "Logs"))[0]);
Check(!logText.Contains("11144477735") && logText.Contains("Execução iniciada"), "log técnico sem dados pessoais");

Console.WriteLine();
Console.WriteLine(failures == 0 ? "TODOS OS TESTES PASSARAM" : $"{failures} FALHA(S)");
try { Directory.Delete(work, true); } catch { }
return failures == 0 ? 0 : 1;

static void CopyDir(string src, string dst)
{
    Directory.CreateDirectory(dst);
    foreach (var f in Directory.GetFiles(src)) File.Copy(f, Path.Combine(dst, Path.GetFileName(f)), true);
}

