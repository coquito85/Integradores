using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Services;

/// <summary>
/// Gerencia <c>Configurations/Layouts/&lt;pasta&gt;/</c>: layout.xml, schema.json, customizations.xml,
/// metadata.json e history/v&lt;n&gt; (versionamento). Cadastrar um novo layout não exige recompilação.
/// </summary>
public sealed class LayoutRepository
{
    public const string LayoutFile = "layout.xml";
    public const string SchemaFile = "schema.json";
    public const string CustomizationsFile = "customizations.xml";
    public const string MetadataFile = "metadata.json";
    public const string HistoryFolder = "history";

    private static readonly Regex FolderRegex = new(@"^[A-Za-z0-9][A-Za-z0-9_.\-]*$", RegexOptions.Compiled);
    private static readonly UTF8Encoding Utf8NoBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonDocumentOptions SchemaParseOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public LayoutRepository(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
            throw new ArgumentException("Informe a pasta raiz dos layouts.", nameof(root));
        Root = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public string Root { get; }

    // ------------------------------------------------------------------ consulta
    public IReadOnlyList<LayoutSummary> List()
    {
        var result = new List<LayoutSummary>();
        if (!Directory.Exists(Root)) return result;

        foreach (var dir in Directory.EnumerateDirectories(Root).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(dir);
            var layoutPath = Path.Combine(dir, LayoutFile);
            if (!FolderRegex.IsMatch(name) || !File.Exists(layoutPath)) continue;

            var parse = XmlLayoutParser.ParseText(File.ReadAllText(layoutPath));
            var meta = ReadMetadata(dir, parse.Layout);
            result.Add(new LayoutSummary(name, meta, File.Exists(Path.Combine(dir, SchemaFile))));
        }
        return result;
    }

    /// <summary>Localiza pelo identificador do XML (ex.: NOV2025009), equivalente ao "código do layout" do modelo.</summary>
    public LayoutSummary? FindByIdentifier(string identifier) =>
        List().FirstOrDefault(l => string.Equals(l.Metadata.Identifier, identifier, StringComparison.OrdinalIgnoreCase));

    public LayoutBundle Load(string folderName)
    {
        var dir = ResolveFolder(folderName);
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException($"Layout '{folderName}' não encontrado em {Root}.");

        var layoutPath = Path.Combine(dir, LayoutFile);
        if (!File.Exists(layoutPath))
            throw new FileNotFoundException($"O layout '{folderName}' não possui o arquivo {LayoutFile}.", layoutPath);

        var xml = File.ReadAllText(layoutPath);
        var parse = XmlLayoutParser.ParseText(xml);
        var meta = ReadMetadata(dir, parse.Layout);
        var issues = new List<ValidationIssue>(parse.Issues);

        var customizations = new CustomizationSet { LayoutIdentifier = meta.Identifier, Version = meta.LayoutVersion };
        var customPath = Path.Combine(dir, CustomizationsFile);
        if (File.Exists(customPath))
        {
            try
            {
                customizations = CustomizationSet.Parse(File.ReadAllText(customPath));
                if (customizations.Version != meta.LayoutVersion)
                    issues.Add(ValidationIssue.Info("CUSTOM_VERSION", CustomizationsFile,
                        $"As personalizações foram salvas para a versão {customizations.Version} do layout; a versão atual é {meta.LayoutVersion}."));
            }
            catch (InvalidDataException ex)
            {
                issues.Add(ValidationIssue.Error("CUSTOM_INVALID", CustomizationsFile, ex.Message));
            }
        }

        var schemaPath = Path.Combine(dir, SchemaFile);

        return new LayoutBundle
        {
            FolderName = folderName,
            FolderPath = dir,
            LayoutXmlText = xml,
            Metadata = meta,
            Customizations = customizations,
            Layout = parse.Layout,
            ParseIssues = issues,
            SchemaText = File.Exists(schemaPath) ? File.ReadAllText(schemaPath) : null
        };
    }

    // ------------------------------------------------------------------ cadastro
    public LayoutBundle Create(string folderName, string xmlPath, string? schemaPath, string notes)
    {
        var dir = ResolveFolder(folderName);
        if (Directory.Exists(dir))
            throw new IOException($"Já existe um layout na pasta '{folderName}'.");

        var xml = File.ReadAllText(xmlPath);
        var parse = XmlLayoutParser.ParseText(xml);
        if (parse.Layout == null)
            throw new InvalidDataException(FirstError(parse.Issues));

        string? schema = null;
        if (!string.IsNullOrWhiteSpace(schemaPath))
        {
            schema = File.ReadAllText(schemaPath);
            using (JsonDocument.Parse(schema, SchemaParseOptions)) { } // JsonException se inválido
        }

        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, LayoutFile), xml, Utf8NoBom);
        if (schema != null) File.WriteAllText(Path.Combine(dir, SchemaFile), schema, Utf8NoBom);

        WriteMetadata(dir, new LayoutMetadata
        {
            FriendlyName = parse.Layout.Name,
            Identifier = parse.Layout.Identifier,
            LayoutVersion = 1,
            SchemaVersion = schema == null ? "" : ExtractSchemaVersion(schema),
            UpdatedAt = DateTime.Now,
            Notes = notes ?? ""
        });

        return Load(folderName);
    }

    // ------------------------------------------------------------------ edição e versionamento
    /// <summary>
    /// Salva o XML. Só exige XML bem formado: erros de configuração continuam visíveis na validação.
    /// Com <paramref name="createVersion"/>, os arquivos atuais vão para history\v&lt;n&gt; e a versão é incrementada.
    /// </summary>
    public LayoutBundle SaveLayoutXml(string folderName, string xmlText, bool createVersion)
    {
        var dir = ResolveFolder(folderName);
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException($"Layout '{folderName}' não encontrado.");

        var parse = XmlLayoutParser.ParseText(xmlText);
        if (parse.Layout == null)
            throw new InvalidDataException(FirstError(parse.Issues));

        var meta = ReadMetadata(dir, parse.Layout);

        if (createVersion)
        {
            Archive(dir, meta.LayoutVersion);
            meta.LayoutVersion++;
        }

        if (!string.IsNullOrWhiteSpace(parse.Layout.Identifier)) meta.Identifier = parse.Layout.Identifier;
        meta.UpdatedAt = DateTime.Now;

        File.WriteAllText(Path.Combine(dir, LayoutFile), xmlText, Utf8NoBom);
        WriteMetadata(dir, meta);
        return Load(folderName);
    }

    /// <summary>Grava customizations.xml (arquivo separado do layout), associado ao identificador e à versão.</summary>
    public void SaveCustomizations(LayoutBundle bundle)
    {
        var dir = ResolveFolder(bundle.FolderName);
        bundle.Customizations.LayoutIdentifier = bundle.Metadata.Identifier;
        bundle.Customizations.Version = bundle.Metadata.LayoutVersion;
        File.WriteAllText(Path.Combine(dir, CustomizationsFile), bundle.Customizations.ToXml(), Utf8NoBom);
    }

    // ------------------------------------------------------------------ internos
    private string ResolveFolder(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName) || !FolderRegex.IsMatch(folderName) || folderName.Contains(".."))
            throw new ArgumentException($"Nome de pasta de layout inválido: '{folderName}'.", nameof(folderName));

        var full = Path.GetFullPath(Path.Combine(Root, folderName));
        if (!full.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Nome de pasta de layout inválido: '{folderName}'.", nameof(folderName));
        return full;
    }

    private static LayoutMetadata ReadMetadata(string dir, LayoutConfiguration? layout)
    {
        LayoutMetadata? meta = null;
        var path = Path.Combine(dir, MetadataFile);
        if (File.Exists(path))
        {
            try { meta = JsonSerializer.Deserialize<LayoutMetadata>(File.ReadAllText(path), JsonOptions); }
            catch (JsonException) { meta = null; }
        }

        meta ??= new LayoutMetadata();
        if (string.IsNullOrWhiteSpace(meta.FriendlyName)) meta.FriendlyName = layout?.Name ?? Path.GetFileName(dir);
        if (string.IsNullOrWhiteSpace(meta.Identifier)) meta.Identifier = layout?.Identifier ?? "";
        if (meta.LayoutVersion < 1) meta.LayoutVersion = 1;
        if (meta.UpdatedAt == default) meta.UpdatedAt = File.GetLastWriteTime(Path.Combine(dir, LayoutFile));
        return meta;
    }

    private static void WriteMetadata(string dir, LayoutMetadata meta) =>
        File.WriteAllText(Path.Combine(dir, MetadataFile), JsonSerializer.Serialize(meta, JsonOptions), Utf8NoBom);

    private static void Archive(string dir, int version)
    {
        var target = Path.Combine(dir, HistoryFolder, "v" + version);
        if (Directory.Exists(target)) target += "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
        Directory.CreateDirectory(target);

        foreach (var file in new[] { LayoutFile, SchemaFile, CustomizationsFile, MetadataFile })
        {
            var source = Path.Combine(dir, file);
            if (File.Exists(source)) File.Copy(source, Path.Combine(target, file));
        }
    }

    /// <summary>Versão do Schema guardada junto ao layout: "version" se existir; senão "$id".</summary>
    private static string ExtractSchemaVersion(string schemaText)
    {
        using var doc = JsonDocument.Parse(schemaText, SchemaParseOptions);
        if (doc.RootElement.ValueKind != JsonValueKind.Object) return "";
        foreach (var key in new[] { "version", "$id" })
            if (doc.RootElement.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String)
                return v.GetString() ?? "";
        return "";
    }

    private static string FirstError(IReadOnlyList<ValidationIssue> issues) =>
        issues.FirstOrDefault(i => i.Severity == IssueSeverity.Error)?.Message ?? "XML de layout inválido.";
}
