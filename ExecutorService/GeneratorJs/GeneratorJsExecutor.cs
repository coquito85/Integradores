
using ReferenciaFactory.Banco;
using ReferenciaFactory.Gerador;
using ReferenciaFactory.Layouts;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using ReferenciaStandard.Repository;

namespace ExecutorService.GeneratorJs
{
    /// <summary>
    /// Ponto unico de entrada do GeneratorJS. A tela (GenerateJS) so conversa com esta classe: ela recebe a ordem,
    /// aciona o ReferenciaFactory (regras, bancos, geracao do JSON) e o ReferenciaStandard (arquivos, XML, caminhos)
    /// e devolve o resultado. A tela nunca acessa banco, layout ou disco diretamente.
    /// </summary>
    public sealed class GeneratorJsExecutor
    {
        private readonly LayoutCatalog _layouts;
        private readonly ProfileStore _profiles;
        private readonly GeneratorEngine _engine;
        private readonly HistoricoStore _historico;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, CancellationTokenSource> _execucoes = new();

        public GeneratorJsExecutor()
        {
            Directory.CreateDirectory(AppPaths.GeneratorJs);
            _layouts = new LayoutCatalog();
            _profiles = new ProfileStore(AppPaths.GeneratorJs);
            _engine = new GeneratorEngine(_layouts, _profiles);
            _historico = new HistoricoStore(AppPaths.GeneratorJs);
        }

        // ------------------------------------------------------------------ informacoes
        public string PastaDados => AppPaths.GeneratorJs;
        public string PastaLayoutsUsuario => AppPaths.LayoutsUsuario;
        public string PastaSaidaPadrao => AppPaths.SaidaPadrao;
        public IReadOnlyList<string> ErrosDeLayout => _layouts.LoadErrors;

        // ------------------------------------------------------------------ bancos
        public IReadOnlyList<ProviderInfo> Provedores => ProviderCatalog.All;

        public bool EhCampoSecreto(string id) => ProviderCatalog.IsSecretField(id);

        public List<ConnectionProfile> ListarConexoes() => _profiles.LoadStored();

        public ConnectionProfile SalvarConexao(ConnectionProfile conexao) => _profiles.Save(conexao);

        public void ExcluirConexao(string id) => _profiles.Delete(id);

        /// <summary>Testa a conexao do formulario; campos secretos em branco reaproveitam os da conexao ja salva.</summary>
        public Task<TestResult> TestarConexaoAsync(ConnectionProfile rascunho) =>
            DbSession.TestAsync(MesclarSegredos(rascunho));

        public async Task<List<string>> ListarTabelasAsync(string conexaoId)
        {
            using var s = await DbSession.OpenAsync(_profiles.GetDecrypted(conexaoId));
            var tabelas = await SchemaReader.ListTablesAsync(s);
            return tabelas.Select(t => t.FullName).ToList();
        }

        public async Task<List<ColumnRef>> ListarColunasAsync(string conexaoId, string tabela)
        {
            using var s = await DbSession.OpenAsync(_profiles.GetDecrypted(conexaoId));
            var tabelas = await SchemaReader.ListTablesAsync(s);
            var t = tabelas.FirstOrDefault(x => x.FullName.Equals(tabela, StringComparison.OrdinalIgnoreCase))
                    ?? throw new GeneratorException($"Tabela '{tabela}' nao encontrada.");
            return await SchemaReader.ListColumnsAsync(s, t);
        }

        private ConnectionProfile MesclarSegredos(ConnectionProfile rascunho)
        {
            var salva = _profiles.LoadStored().FirstOrDefault(x => x.Id == rascunho.Id);
            if (salva is null) return rascunho;

            var dec = ProfileStore.Decrypt(salva);
            if (dec.ProviderKey == rascunho.ProviderKey)
            {
                foreach (var chave in dec.Fields.Keys.Where(ProviderCatalog.IsSecretField))
                    if (!rascunho.Fields.TryGetValue(chave, out var v) || string.IsNullOrEmpty(v))
                        rascunho.Fields[chave] = dec.Fields[chave];
                if (rascunho.Mode == "raw" && string.IsNullOrEmpty(rascunho.RawConnectionString))
                    rascunho.RawConnectionString = dec.RawConnectionString;
            }
            return rascunho;
        }

        // ------------------------------------------------------------------ layouts (XML vem do ReferenciaStandard)
        public IReadOnlyList<LayoutDefinition> ListarLayouts() => _layouts.All;

        public LayoutDefinition ObterLayout(string id) => _layouts.Get(id);

        public IReadOnlyList<LayoutDefinition> RecarregarLayouts()
        {
            _layouts.Reload();
            return _layouts.All;
        }

        public LayoutDefinition ImportarLayout(string arquivoXml)
        {
            return _layouts.Import(arquivoXml);
        }

        // ------------------------------------------------------------------ geracao
        public Task<PreviewResult> PreviaAsync(GenerateRequest pedido, CancellationToken ct = default) =>
            _engine.PreviewAsync(pedido, 5, ct);

        /// <summary>
        /// Inicia a geracao em segundo plano e devolve o id da execucao. <paramref name="aoLogar"/> e
        /// <paramref name="aoConcluir"/> voltam na thread de quem chamou (a da tela) quando ela tem SynchronizationContext.
        /// </summary>
        public string Iniciar(GenerateRequest pedido, Action<string, LogEntry> aoLogar, Action<string, GenerateResult> aoConcluir)
        {
            var runId = Guid.NewGuid().ToString("N");
            var cts = new CancellationTokenSource();
            _execucoes[runId] = cts;

            var layout = _layouts.Find(pedido.LayoutId);
            var conexao = _profiles.LoadStored().FirstOrDefault(p => p.Id == pedido.ProfileId);
            var registro = new RunRecord
            {
                LayoutId = pedido.LayoutId,
                LayoutName = layout?.Name ?? pedido.LayoutId,
                ProfileName = conexao?.Name ?? "?",
                Mode = pedido.Mode
            };

            // Progress<T> criado na thread da tela: os callbacks voltam para ela.
            var progresso = new Progress<LogEntry>(entrada => aoLogar(runId, entrada));

            _ = Task.Run(async () =>
            {
                var resultado = await _engine.RunAsync(pedido, progresso, cts.Token);
                registro.Status = resultado.Success ? "ok" : resultado.Cancelled ? "cancelled" : "error";
                registro.Rows = resultado.Rows;
                registro.ElapsedMs = resultado.ElapsedMs;
                registro.Message = resultado.Error;
                registro.Files = resultado.Files;
                try { _historico.Adicionar(registro); } catch { /* historico e opcional */ }
                _execucoes.TryRemove(runId, out _);
                cts.Dispose();
                aoConcluir(runId, resultado);
            });

            return runId;
        }

        public void Cancelar(string runId)
        {
            if (_execucoes.TryGetValue(runId, out var cts)) cts.Cancel();
        }

        // ------------------------------------------------------------------ historico
        public List<RunRecord> Historico() => _historico.Listar();

        public void LimparHistorico() => _historico.Limpar();
    }
}
