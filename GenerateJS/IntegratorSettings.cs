using System.ComponentModel;
using System.Data.Common;
using System.Drawing.Design;
using System.Text;
using BetaJsonIntegrator.Core.Services;
using BetaJsonIntegrator.Data.Providers;

namespace GenerateJS
{
    public static class FontesDados
    {
        public const string Csv = "Arquivo CSV";
        public const string Banco = "Banco de dados";

        public static readonly string[] Names = { Csv, Banco };
    }

    /// <summary>Providers de banco suportados (nomes dos drivers, como no ProviderName do GeneratorJS).</summary>
    public static class DbProviders
    {
        public const string MySql = "MySqlConnector";
        public const string SqlServer = "Microsoft.Data.SqlClient";
        public const string Access = "System.Data.OleDb";

        public static readonly string[] Names = { MySql, SqlServer, Access };

        /// <summary>Reconhece o nome digitado (sem diferenciar maiúsculas) e devolve MySql ou SqlServer; null se não suportado.</summary>
        public static string? Resolver(string? nome) => (nome ?? "").Trim().ToLowerInvariant() switch
        {
            "mysqlconnector" or "mysql.data.mysqlclient" or "mariadb" or "mysql" => MySql,
            "microsoft.data.sqlclient" or "system.data.sqlclient" or "sqlserver" or "sql server" => SqlServer,
            "system.data.oledb" or "oledb" or "access" or "ace" => Access,
            _ => null
        };
    }

    public static class CsvChoices
    {
        public static readonly string[] Delimitadores =
            { "; (ponto e vírgula)", ", (vírgula)", "| (barra vertical)", "Tab" };

        public static readonly string[] Codificacoes = { "UTF-8", "Windows-1252", "ISO-8859-1" };
    }

    /// <summary>Propriedade exibida somente quando a Origem escolhida for a indicada.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AplicaSeAttribute : Attribute
    {
        public AplicaSeAttribute(string fonte) => Fonte = fonte;
        public string Fonte { get; }
    }

    // ------------------------------------------------------------------ combos do PropertyGrid
    public abstract class ChoiceConverter : StringConverter
    {
        protected abstract IReadOnlyList<string> Choices(ITypeDescriptorContext? context);

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context) =>
            new StandardValuesCollection(Choices(context).ToArray());
    }

    public sealed class OrigemConverter : ChoiceConverter
    {
        protected override IReadOnlyList<string> Choices(ITypeDescriptorContext? context) => FontesDados.Names;
    }

    public sealed class DelimitadorConverter : ChoiceConverter
    {
        protected override IReadOnlyList<string> Choices(ITypeDescriptorContext? context) => CsvChoices.Delimitadores;
    }

    public sealed class CodificacaoConverter : ChoiceConverter
    {
        protected override IReadOnlyList<string> Choices(ITypeDescriptorContext? context) => CsvChoices.Codificacoes;
    }

    /// <summary>Lista as configurações (&lt;generate&gt;) do layout identificado pelo LayoutCode.</summary>
    public sealed class GenerateNameConverter : ChoiceConverter
    {
        protected override IReadOnlyList<string> Choices(ITypeDescriptorContext? context) =>
            (context?.Instance as IntegratorSettings)?.AvailableGenerates ?? new List<string>();
    }

    // ------------------------------------------------------------------ editores de caminho
    public sealed class CsvFileEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context) => UITypeEditorEditStyle.Modal;

        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Arquivo CSV de origem",
                Filter = "CSV/Texto (*.csv;*.txt)|*.csv;*.txt|Todos os arquivos (*.*)|*.*"
            };
            if (value is string atual && File.Exists(atual)) dlg.FileName = atual;
            return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : value;
        }
    }

    public sealed class FolderEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context) => UITypeEditorEditStyle.Modal;

        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
        {
            using var dlg = new FolderBrowserDialog { Description = "Pasta de destino da exportação" };
            if (value is string atual && Directory.Exists(atual)) dlg.SelectedPath = atual;
            return dlg.ShowDialog() == DialogResult.OK ? dlg.SelectedPath : value;
        }
    }

    /// <summary>
    /// Tudo o que o usuário configura antes de gerar: fonte de dados (CSV ou banco) e geração
    /// (layout, configuração/generate, prévia, destino). Editado no diálogo de Configurações (PropertyGrid);
    /// mostra só os campos da Origem escolhida e identifica se o layout (bundle) existe.
    /// Fica em memória: a string de conexão nunca é gravada em arquivo (spec. seção 14).
    /// </summary>
    public sealed class IntegratorSettings : ICustomTypeDescriptor
    {
        private const string CatFonte = "Fonte de dados";
        private const string CatGeracao = "Geração";

        private string _layoutCode = "";
        private LayoutBundle? _layout;

        // ============================== FONTE DE DADOS ==============================
        [Category(CatFonte)]
        [DisplayName("Origem")]
        [Description("Onde estão os dados: arquivo CSV ou banco de dados.")]
        [TypeConverter(typeof(OrigemConverter))]
        [RefreshProperties(RefreshProperties.All)]
        public string Origem { get; set; } = FontesDados.Csv;

        [Category(CatFonte)]
        [AplicaSe(FontesDados.Csv)]
        [DisplayName("Arquivo")]
        [Description("Caminho do arquivo CSV de origem.")]
        [Editor(typeof(CsvFileEditor), typeof(UITypeEditor))]
        public string Arquivo { get; set; } = "";

        [Category(CatFonte)]
        [AplicaSe(FontesDados.Csv)]
        [DisplayName("Delimitador")]
        [Description("Separador de campos do CSV.")]
        [TypeConverter(typeof(DelimitadorConverter))]
        public string Delimitador { get; set; } = CsvChoices.Delimitadores[0];

        [Category(CatFonte)]
        [AplicaSe(FontesDados.Csv)]
        [DisplayName("Codificação")]
        [Description("Codificação de caracteres do arquivo.")]
        [TypeConverter(typeof(CodificacaoConverter))]
        public string Codificacao { get; set; } = CsvChoices.Codificacoes[0];

        [Category(CatFonte)]
        [AplicaSe(FontesDados.Csv)]
        [DisplayName("Primeira linha é o cabeçalho")]
        [Description("Marque se a primeira linha do CSV contém os nomes das colunas.")]
        public bool PrimeiraLinhaCabecalho { get; set; } = true;

        [Category(CatFonte)]
        [AplicaSe(FontesDados.Banco)]
        [DisplayName("ConnectionString")]
        [Description("String de conexão do banco. Fica apenas em memória e é exibida mascarada.")]
        [PasswordPropertyText(true)]
        public string ConnectionString { get; set; } = "";

        [Category(CatFonte)]
        [AplicaSe(FontesDados.Banco)]
        [DisplayName("ProviderName")]
        [Description("Driver do banco (digite): MySqlConnector (MariaDB/MySQL), Microsoft.Data.SqlClient (SQL Server) ou System.Data.OleDb (Access, ex.: Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\dados\\base.accdb;).")]
        public string ProviderName { get; set; } = DbProviders.MySql;

        [Category(CatFonte)]
        [AplicaSe(FontesDados.Banco)]
        [DisplayName("TableSource")]
        [Description("Opcional. Substitui o nome da tabela definido no XML (table/@name). Aceita schema.tabela.")]
        public string TableSource { get; set; } = "";

        [Category(CatFonte)]
        [AplicaSe(FontesDados.Banco)]
        [DisplayName("FiltersSource")]
        [Description("Opcional. Filtros de igualdade separados por vírgula, ex.: Ano=2026,Cartorio=1. Enviados como parâmetros SQL.")]
        public string FiltersSource { get; set; } = "";

        [Category(CatFonte)]
        [AplicaSe(FontesDados.Banco)]
        [DisplayName("CommandTimeout")]
        [Description("Tempo máximo, em segundos, de cada consulta.")]
        public int CommandTimeout { get; set; } = 120;

        // ============================== GERAÇÃO ==============================
        [Category(CatGeracao)]
        [DisplayName("LayoutCode")]
        [Description("Identificador do layout (atributo identifier do XML, ex.: NOV2025009). Deve existir em Configurations\\Layouts.")]
        [RefreshProperties(RefreshProperties.All)]
        public string LayoutCode
        {
            get => _layoutCode;
            set
            {
                _layoutCode = value ?? "";
                ResolverLayout();
            }
        }

        [Category(CatGeracao)]
        [DisplayName("Configuração")]
        [Description("Qual <generate> do layout será gerado. A lista vem do layout identificado pelo LayoutCode.")]
        [TypeConverter(typeof(GenerateNameConverter))]
        public string Generate { get; set; } = "";

        [Category(CatGeracao)]
        [DisplayName("Gerar prévia com pendências")]
        [Description("Gera o JSON mesmo com campos pendentes (eles não são preenchidos). Uma prévia não pode ser exportada.")]
        public bool GerarPrevia { get; set; }

        [Category(CatGeracao)]
        [DisplayName("ArquivosDestino")]
        [Description("Opcional. Pasta de destino da exportação. Vazio usa a pasta Output do aplicativo.")]
        [Editor(typeof(FolderEditor), typeof(UITypeEditor))]
        public string ArquivosDestino { get; set; } = "";

        // ============================== apoio (não exibido) ==============================
        /// <summary>Resolve um LayoutCode em um layout carregado (fornecido por quem edita as configurações).</summary>
        [Browsable(false)]
        public Func<string, LayoutBundle?>? LayoutResolver { get; set; }

        [Browsable(false)]
        public LayoutBundle? Layout => _layout;

        [Browsable(false)]
        public bool TemLayout => _layout != null;

        [Browsable(false)]
        public IReadOnlyList<string> AvailableGenerates =>
            _layout?.Layout?.Generates.Select(g => g.Name).ToList() ?? new List<string>();

        [Browsable(false)]
        public bool EhBanco => Origem == FontesDados.Banco;

        /// <summary>Access não aceita "SELECT 1" sem FROM; o teste de conexão só abre a conexão.</summary>
        [Browsable(false)]
        public bool EhAccess => DbProviders.Resolver(ProviderName) == DbProviders.Access;

        [Browsable(false)]
        public bool BancoConfigurado => !string.IsNullOrWhiteSpace(ConnectionString);

        public IntegratorSettings Clone() => (IntegratorSettings)MemberwiseClone();

        /// <summary>Define o layout ativo (usado pela tela principal ao carregar um layout).</summary>
        public void DefinirLayout(LayoutBundle? bundle)
        {
            _layout = bundle;
            _layoutCode = bundle?.Metadata.Identifier ?? "";
            AjustarGenerate();
        }

        /// <summary>Recarrega o layout a partir do LayoutCode atual, usando o <see cref="LayoutResolver"/>.</summary>
        public void ResolverLayout()
        {
            _layout = null;
            var code = _layoutCode.Trim();
            if (code.Length > 0 && LayoutResolver != null)
            {
                try { _layout = LayoutResolver(code); }
                catch (Exception ex) when (ex is IOException or ArgumentException) { _layout = null; }
            }
            AjustarGenerate();
        }

        private void AjustarGenerate()
        {
            var nomes = AvailableGenerates;
            if (nomes.Count == 0) Generate = "";
            else if (!nomes.Contains(Generate, StringComparer.OrdinalIgnoreCase)) Generate = nomes[0];
        }

        // ============================== resumo, validação e criação ==============================
        /// <summary>Resumo sem credenciais para a tela principal e para as mensagens.</summary>
        public string Resumo()
        {
            var partes = new List<string>();

            if (EhBanco)
            {
                var banco = $"Origem: banco ({ProviderName})" + (BancoConfigurado ? "" : " — não configurado");
                if (!string.IsNullOrWhiteSpace(TableSource)) banco += $", tabela {TableSource.Trim()}";
                if (!string.IsNullOrWhiteSpace(FiltersSource)) banco += $", filtros {FiltersSource.Trim()}";
                partes.Add(banco);
            }
            else
            {
                partes.Add("Origem: CSV" + (string.IsNullOrWhiteSpace(Arquivo)
                    ? " (arquivo não informado)"
                    : $" ({Path.GetFileName(Arquivo)})"));
            }

            partes.Add("Configuração: " + (string.IsNullOrWhiteSpace(Generate) ? "(nenhuma)" : Generate));
            partes.Add("Prévia: " + (GerarPrevia ? "sim" : "não"));
            return string.Join("  •  ", partes);
        }

        public DbProviderFactory CreateFactory() => DbProviders.Resolver(ProviderName) switch
        {
            DbProviders.MySql => MySqlConnector.MySqlConnectorFactory.Instance,
            DbProviders.SqlServer => Microsoft.Data.SqlClient.SqlClientFactory.Instance,
            DbProviders.Access => System.Data.OleDb.OleDbFactory.Instance,
            _ => throw new InvalidOperationException(MensagemProvider())
        };

        private string MensagemProvider() =>
            $"ProviderName '{ProviderName}' não suportado. Use {DbProviders.MySql}, {DbProviders.SqlServer} ou {DbProviders.Access}.";

        public IDataProvider CreateProvider()
        {
            if (!EhBanco)
            {
                return new CsvProvider(Arquivo.Trim(), new CsvOptions
                {
                    Delimiter = ObterDelimitador(),
                    Encoding = ObterCodificacao(),
                    HasHeader = PrimeiraLinhaCabecalho
                });
            }

            var factory = CreateFactory();
            var cs = ConnectionString.Trim();
            return DbProviders.Resolver(ProviderName) switch
            {
                DbProviders.SqlServer => new SqlServerProvider(factory, cs) { CommandTimeoutSeconds = CommandTimeout },
                DbProviders.Access => new AccessProvider(factory, cs) { CommandTimeoutSeconds = CommandTimeout },
                _ => new MariaDbProvider(factory, cs) { CommandTimeoutSeconds = CommandTimeout }
            };
        }

        /// <summary>TableSource e FiltersSource só valem para origem de banco.</summary>
        public RunOptions CreateOptions() => new RunOptions
        {
            GenerateName = string.IsNullOrWhiteSpace(Generate) ? null : Generate,
            AllowPending = GerarPrevia,
            TableSource = EhBanco && !string.IsNullOrWhiteSpace(TableSource) ? TableSource.Trim() : null,
            Filters = EhBanco ? ParseFilters() : null
        };

        private char ObterDelimitador()
        {
            var d = Delimitador ?? "";
            if (d.StartsWith(",")) return ',';
            if (d.StartsWith("|")) return '|';
            if (d.StartsWith("Tab", StringComparison.OrdinalIgnoreCase)) return '\t';
            return ';';
        }

        private Encoding ObterCodificacao()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Codificacao switch
            {
                "Windows-1252" => Encoding.GetEncoding(1252),
                "ISO-8859-1" => Encoding.Latin1,
                _ => new UTF8Encoding(false)
            };
        }

        /// <summary>FiltersSource "campo=valor,campo2=valor2" → filtros parametrizados. Lança FormatException se inválido.</summary>
        public IReadOnlyList<DataFilter> ParseFilters()
        {
            var result = new List<DataFilter>();
            if (string.IsNullOrWhiteSpace(FiltersSource)) return result;

            foreach (var part in FiltersSource.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                int i = part.IndexOf('=');
                if (i <= 0)
                    throw new FormatException($"Filtro inválido: '{part}'. Use campo=valor, separados por vírgula.");

                var column = part[..i].Trim();
                var value = part[(i + 1)..].Trim();
                if (value.Length >= 2 && (value[0] == '\'' || value[0] == '"') && value[^1] == value[0])
                    value = value[1..^1];

                if (!SqlIdentifier.IsValid(column))
                    throw new FormatException($"Nome de campo inválido no filtro: '{column}'.");

                result.Add(new DataFilter(column, value));
            }
            return result;
        }

        /// <summary>Valida o necessário para conectar (usado também pelo botão "Testar origem").</summary>
        public string? ValidateConnectionOnly()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                return "A string de conexão é obrigatória.";
            if (DbProviders.Resolver(ProviderName) == null)
                return MensagemProvider();
            return null;
        }

        /// <summary>Validações executadas ao gravar e antes de gerar.</summary>
        public List<string> Validate()
        {
            var erros = new List<string>();

            if (EhBanco)
            {
                var conexao = ValidateConnectionOnly();
                if (conexao != null) erros.Add(conexao);

                if (!string.IsNullOrWhiteSpace(TableSource) && !SqlIdentifier.IsValid(TableSource.Trim()))
                    erros.Add("TableSource inválido: use apenas letras, números e '_' (opcionalmente schema.tabela).");

                try { ParseFilters(); }
                catch (FormatException ex) { erros.Add(ex.Message); }

                if (CommandTimeout < 1)
                    erros.Add("CommandTimeout deve ser maior que zero (segundos).");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Arquivo))
                    erros.Add("Informe o arquivo CSV de origem.");
                else if (!File.Exists(Arquivo.Trim()))
                    erros.Add("O arquivo CSV informado não existe.");
            }

            if (string.IsNullOrWhiteSpace(LayoutCode))
                erros.Add("Informe o código do layout (LayoutCode).");
            else if (_layout == null)
                erros.Add($"Não existe layout para o código '{LayoutCode.Trim()}'. Cadastre o layout ou informe um código válido.");
            else if (string.IsNullOrWhiteSpace(Generate) || !AvailableGenerates.Contains(Generate, StringComparer.OrdinalIgnoreCase))
                erros.Add("Escolha a Configuração (generate) do layout.");

            if (!string.IsNullOrWhiteSpace(ArquivosDestino) && !Path.IsPathRooted(ArquivosDestino.Trim()))
                erros.Add("ArquivosDestino deve ser um caminho completo (ex.: D:\\Saida).");

            return erros;
        }

        // ============================== ICustomTypeDescriptor ==============================
        // Mostra só as propriedades da Origem escolhida. Usa GetType() (e não "this") nas chamadas
        // ao TypeDescriptor para evitar recursão infinita.
        private bool AplicaAoContexto(PropertyDescriptor pd)
        {
            var attr = pd.Attributes.OfType<AplicaSeAttribute>().FirstOrDefault();
            return attr == null || attr.Fonte == Origem;
        }

        public PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
        {
            var todas = attributes == null
                ? TypeDescriptor.GetProperties(GetType())
                : TypeDescriptor.GetProperties(GetType(), attributes);

            var visiveis = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor pd in todas)
                if (AplicaAoContexto(pd)) visiveis.Add(pd);

            return new PropertyDescriptorCollection(visiveis.ToArray());
        }

        public PropertyDescriptorCollection GetProperties() => GetProperties(null);

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(GetType());
        public string? GetClassName() => TypeDescriptor.GetClassName(GetType());
        public string? GetComponentName() => null;
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(GetType());
        public EventDescriptor? GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(GetType());
        public PropertyDescriptor? GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(GetType());
        public object? GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(GetType(), editorBaseType);
        public EventDescriptorCollection GetEvents(Attribute[]? attributes) => TypeDescriptor.GetEvents(GetType(), attributes);
        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(GetType());
        public object? GetPropertyOwner(PropertyDescriptor? pd) => this;
    }
}
