using System.Text.Json;
using System.Text.RegularExpressions;
using BetaJsonIntegrator.Core.Configuration;
using BetaJsonIntegrator.Core.Mapping;
using BetaJsonIntegrator.Core.Validation;

namespace BetaJsonIntegrator.Core.Services
{
    public sealed class LayoutMetadata
    {
        public string FriendlyName { get; set; } = "";
        public string Identifier { get; set; } = "";
        public int LayoutVersion { get; set; } = 1;
        public string SchemaVersion { get; set; } = "";
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string Notes { get; set; } = "";
    }

    public sealed class LayoutInfo
    {
        public string FolderName { get; init; } = "";
        public LayoutMetadata Metadata { get; init; } = new();
        public bool HasSchema { get; init; }
    }

    /// <summary>Layout carregado: XML interpretado + personalizações + schema + metadados.</summary>
    public sealed class LayoutBundle
    {
        public string FolderName { get; init; } = "";
        public string FolderPath { get; init; } = "";
        public string LayoutXmlText { get; init; } = "";
        public LayoutConfiguration? Layout { get; init; }
        public List<ValidationIssue> ParseIssues { get; init; } = new();
        public CustomizationSet Customizations { get; init; } = new();
        public string? SchemaText { get; init; }
        public LayoutMetadata Metadata { get; init; } = new();

        public string LayoutXmlPath => Path.Combine(FolderPath, LayoutRepository.LayoutFile);
        public string CustomizationsPath => Path.Combine(FolderPath, LayoutRepository.CustomizationsFile);
        public string SchemaPath => Path.Combine(FolderPath, LayoutRepository.SchemaFile);
        public bool IsUsable => Layout != null && ParseIssues.All(i => i.Severity != IssueSeverity.Error);
        public string DisplayName => string.IsNullOrWhiteSpace(Metadata.FriendlyName) ? (Layout?.Name ?? FolderName) : Metadata.FriendlyName;
    }

    /// <summary>
    /// Gerencia Configurations/Layouts/&lt;Pasta&gt;/{layout.xml, schema.json, customizations.xml, metadata.json}.
    /// Cada alteração do XML/Schema pode arquivar a versão anterior em history/v{n}.
    /// </summary>
    public sealed class LayoutRepository
    {
        public const string LayoutFile = "layout.xml";
        public const string SchemaFile = "schema.json";
        public const string CustomizationsFile = "customizations.xml";
        public const string MetadataFile = "metadata.json";
        private static readonly Regex SafeName = new(@"^[A-Za-z0-9_\-]{1,64}$", RegexOptions.Compiled);
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public string RootFolder { get; }

        public LayoutRepository(string rootFolder)
        {
            RootFolder = rootFolder;
            Directory.CreateDirectory(rootFolder);
        }

        public IReadOnlyList<LayoutInfo> List()
        {
            var list = new List<LayoutInfo>();
            foreach (var dir in Directory.GetDirectories(RootFolder).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
            {
                if (!File.Exists(Path.Combine(dir, LayoutFile))) continue;
                list.Add(new LayoutInfo
                {
                    FolderName = Path.GetFileName(dir),
                    Metadata = ReadMetadata(dir),
                    HasSchema = File.Exists(Path.Combine(dir, SchemaFile))
                });
            }
            return list;
        }

        public LayoutBundle Load(string folderName)
        {
            var path = ResolveFolder(folderName);
            var xmlPath = Path.Combine(path, LayoutFile);
            if (!File.Exists(xmlPath)) throw new FileNotFoundException("layout.xml não encontrado.", xmlPath);

            var xmlText = File.ReadAllText(xmlPath);
            var parse = XmlLayoutParser.ParseText(xmlText);
            var meta = ReadMetadata(path);

            if (parse.Layout != null)
            {
                if (string.IsNullOrWhiteSpace(meta.Identifier)) meta.Identifier = parse.Layout.Identifier;
                if (string.IsNullOrWhiteSpace(meta.FriendlyName)) meta.FriendlyName = parse.Layout.Name;
            }

            var cust = new CustomizationSet { LayoutIdentifier = meta.Identifier, Version = meta.LayoutVersion };
            var custPath = Path.Combine(path, CustomizationsFile);
            var issues = parse.Issues.ToList();
            try
            {
                var loaded = CustomizationSet.Load(custPath);
                cust = loaded;
                if (File.Exists(custPath))
                {
                    if (!string.IsNullOrEmpty(loaded.LayoutIdentifier) &&
                        !string.Equals(loaded.LayoutIdentifier, meta.Identifier, StringComparison.OrdinalIgnoreCase))
                        issues.Add(ValidationIssue.Warning("CUSTOMIZATION_LAYOUT_MISMATCH", "",
                            $"customizations.xml pertence ao layout '{loaded.LayoutIdentifier}', e não a '{meta.Identifier}'."));
                    else if (loaded.Version != meta.LayoutVersion)
                        issues.Add(ValidationIssue.Warning("CUSTOMIZATION_VERSION_MISMATCH", "",
                            $"customizations.xml é da versão {loaded.Version} do layout; a versão atual é {meta.LayoutVersion}. Revise as personalizações."));
                }
                else
                {
                    cust.LayoutIdentifier = meta.Identifier;
                    cust.Version = meta.LayoutVersion;
                }
            }
            catch (System.Xml.XmlException ex)
            {
                issues.Add(ValidationIssue.Error("CUSTOMIZATION_INVALID", CustomizationsFile, "customizations.xml inválido: " + ex.Message));
            }

            var schemaPath = Path.Combine(path, SchemaFile);
            return new LayoutBundle
            {
                FolderName = Path.GetFileName(path),
                FolderPath = path,
                LayoutXmlText = xmlText,
                Layout = parse.Layout,
                ParseIssues = issues,
                Customizations = cust,
                SchemaText = File.Exists(schemaPath) ? File.ReadAllText(schemaPath) : null,
                Metadata = meta
            };
        }

        /// <summary>Cria um novo layout a partir de arquivos existentes.</summary>
        public LayoutBundle Create(string folderName, string layoutXmlPath, string? schemaPath, string? notes = null, string? schemaVersion = null)
        {
            var path = ResolveFolder(folderName);
            if (Directory.Exists(path)) throw new IOException($"Já existe um layout na pasta '{folderName}'.");

            var xml = File.ReadAllText(layoutXmlPath);
            var parse = XmlLayoutParser.ParseText(xml);
            if (parse.Layout == null || parse.Issues.Any(i => i.Severity == IssueSeverity.Error))
                throw new InvalidDataException("O XML informado é inválido: " +
                    string.Join(" | ", parse.Issues.Where(i => i.Severity == IssueSeverity.Error).Select(i => i.Message)));

            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, LayoutFile), xml);
            if (!string.IsNullOrWhiteSpace(schemaPath))
            {
                var schemaText = File.ReadAllText(schemaPath);
                using (JsonSchemaValidator.FromText(schemaText)) { } // valida que é JSON
                File.WriteAllText(Path.Combine(path, SchemaFile), schemaText);
            }

            WriteMetadata(path, new LayoutMetadata
            {
                FriendlyName = parse.Layout.Name,
                Identifier = parse.Layout.Identifier,
                LayoutVersion = 1,
                SchemaVersion = schemaVersion ?? "",
                UpdatedAt = DateTime.Now,
                Notes = notes ?? ""
            });
            return Load(folderName);
        }

        /// <summary>
        /// Salva um novo texto de XML. Se createVersion, arquiva os arquivos atuais em history/v{n}
        /// e incrementa LayoutVersion (as personalizações antigas passam a gerar aviso até revisão).
        /// </summary>
        public LayoutBundle SaveLayoutXml(string folderName, string xmlText, bool createVersion)
        {
            var parse = XmlLayoutParser.ParseText(xmlText);
            if (parse.Layout == null || parse.Issues.Any(i => i.Severity == IssueSeverity.Error))
                throw new InvalidDataException("XML inválido, nada foi salvo: " +
                    string.Join(" | ", parse.Issues.Where(i => i.Severity == IssueSeverity.Error).Select(i => i.Message)));

            var path = ResolveFolder(folderName);
            var meta = ReadMetadata(path);
            if (createVersion)
            {
                Archive(path, meta.LayoutVersion);
                meta.LayoutVersion++;
            }
            File.WriteAllText(Path.Combine(path, LayoutFile), xmlText);
            meta.Identifier = parse.Layout.Identifier;
            meta.FriendlyName = parse.Layout.Name;
            meta.UpdatedAt = DateTime.Now;
            WriteMetadata(path, meta);

            if (createVersion)
            {
                var cust = CustomizationSet.Load(Path.Combine(path, CustomizationsFile));
                cust.LayoutIdentifier = meta.Identifier;
                cust.Version = meta.LayoutVersion;
                if (File.Exists(Path.Combine(path, CustomizationsFile)))
                    cust.Save(Path.Combine(path, CustomizationsFile));
            }
            return Load(folderName);
        }

        public LayoutBundle SaveSchema(string folderName, string schemaText, string? schemaVersion = null)
        {
            using (JsonSchemaValidator.FromText(schemaText)) { }
            var path = ResolveFolder(folderName);
            File.WriteAllText(Path.Combine(path, SchemaFile), schemaText);
            var meta = ReadMetadata(path);
            if (schemaVersion != null) meta.SchemaVersion = schemaVersion;
            meta.UpdatedAt = DateTime.Now;
            WriteMetadata(path, meta);
            return Load(folderName);
        }

        public void SaveCustomizations(LayoutBundle bundle)
        {
            bundle.Customizations.LayoutIdentifier = bundle.Metadata.Identifier;
            bundle.Customizations.Version = bundle.Metadata.LayoutVersion;
            bundle.Customizations.Save(bundle.CustomizationsPath);
        }

        // ---------------- internos ----------------
        private string ResolveFolder(string folderName)
        {
            if (!SafeName.IsMatch(folderName))
                throw new ArgumentException("Nome de pasta inválido. Use letras, números, '_' e '-'.", nameof(folderName));
            var full = Path.GetFullPath(Path.Combine(RootFolder, folderName));
            var root = Path.GetFullPath(RootFolder);
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Caminho fora da pasta de layouts.", nameof(folderName));
            return full;
        }

        private static void Archive(string path, int version)
        {
            var dest = Path.Combine(path, "history", $"v{version}");
            Directory.CreateDirectory(dest);
            foreach (var f in new[] { LayoutFile, SchemaFile, CustomizationsFile, MetadataFile })
            {
                var src = Path.Combine(path, f);
                if (File.Exists(src)) File.Copy(src, Path.Combine(dest, f), true);
            }
        }

        private static LayoutMetadata ReadMetadata(string folderPath)
        {
            var file = Path.Combine(folderPath, MetadataFile);
            if (!File.Exists(file)) return new LayoutMetadata { UpdatedAt = File.GetLastWriteTime(Path.Combine(folderPath, LayoutFile)) };
            try { return JsonSerializer.Deserialize<LayoutMetadata>(File.ReadAllText(file)) ?? new LayoutMetadata(); }
            catch (JsonException) { return new LayoutMetadata(); }
        }

        private static void WriteMetadata(string folderPath, LayoutMetadata meta) =>
            File.WriteAllText(Path.Combine(folderPath, MetadataFile), JsonSerializer.Serialize(meta, JsonOpts));
    }
}
