using System.Diagnostics;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using ReferenciaFactory.Classes;
using ReferenciaFactory.Layouts;

namespace ReferenciaFactory.Gerador
{
    public sealed class GeneratorEngine
    {
        private readonly LayoutCatalog _layouts;
        private readonly ProfileStore _profiles;

        public GeneratorEngine(LayoutCatalog layouts, ProfileStore profiles)
        {
            _layouts = layouts;
            _profiles = profiles;
        }

        private sealed record Prepared(LayoutDefinition Layout, BuiltQuery Query, List<string> Warnings);

        private static async Task<Prepared> PrepareAsync(DatabaseConnection s, LayoutDefinition layout, GenerateRequest req,
            int? limit, CancellationToken ct)
        {
            if (layout.ErrorCount > 0)
                throw new GeneratorException($"O layout '{layout.Id}' tem {layout.ErrorCount} erro(s) de validacao. Corrija o XML na tela Layouts antes de gerar.");

            var warnings = new List<string>();
            var table = string.IsNullOrWhiteSpace(req.Table) ? layout.TableName : req.Table!.Trim();

            SchemaMap? map = null;
            try
            {
                map = await s.ResolveAsync(table, layout.Columns.Select(c => c.Name), ct).ConfigureAwait(false);
               
                if (map is null) warnings.Add("Catalogo vazio: usando os nomes do layout sem conferir a tabela.");
                else if (map.Columns is null) warnings.Add("Colunas da tabela nao puderam ser lidas: usando os nomes do layout.");
            }
            catch (GeneratorException) { throw; }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                warnings.Add($"Nao foi possivel ler o catalogo do banco ({ex.Message}). Usando os nomes do layout.");
            }

            var query = QueryBuilder.Build(layout, s.Dialect, req.Table, req.Filters, map, limit);
            return new Prepared(layout, query, warnings);
        }

        private static Dictionary<string, object?> ToRow(BuiltQuery q, object?[] values)
        {
            var row = new Dictionary<string, object?>(q.Columns.Count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < q.Columns.Count && i < values.Length; i++)
                row[q.Columns[i].Name] = values[i] is DBNull ? null : values[i];
            return row;
        }

        private static string FileBase(GenerateRequest req, LayoutDefinition layout)
        {
            var now = DateTime.Now;
            var name = string.IsNullOrWhiteSpace(req.FileName) ? "{layout}_{date}_{time}" : req.FileName.Trim();
            name = name.Replace("{layout}", layout.Id, StringComparison.OrdinalIgnoreCase)
                       .Replace("{date}", now.ToString("yyyyMMdd"), StringComparison.OrdinalIgnoreCase)
                       .Replace("{time}", now.ToString("HHmmss"), StringComparison.OrdinalIgnoreCase);
            if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) name = name[..^5];
            var invalid = Path.GetInvalidFileNameChars();
            return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
        }

        // ---------------------------------------------------------------- previa
        public async Task<PreviewResult> PreviewAsync(GenerateRequest req, int rows, CancellationToken ct)
        {
            var layout = _layouts.Get(req.LayoutId);
            var profile = _profiles.GetDecrypted(req.ProfileId);
            using var s = await DatabaseConnection.OpenAsync(profile, ct).ConfigureAwait(false);

            var prep = await PrepareAsync(s, layout, req, rows, ct).ConfigureAwait(false);
            var result = new PreviewResult
            {
                Sql = prep.Query.Sql,
                Parameters = prep.Query.Params.Select(p => $"{p.Name} = {p.Value}").ToList(),
                Columns = layout.Columns.Select(c => c.Name).ToList(),
                Warnings = prep.Warnings
            };

            try { result.TotalRows = await s.ScalarLongAsync(prep.Query.CountSql, prep.Query.Params, ct).ConfigureAwait(false); }
            catch (Exception ex) when (ex is not OperationCanceledException) { result.Warnings.Add("Nao foi possivel contar os registros: " + ex.Message); }

            var sink = new JsonSink(layout.RootField!.Name, toFiles: false, null, "preview", 0);
            var stats = new MapStats();
            var options = new MapOptions { EmptyArrayAsNull = req.EmptyArrayAsNull };
            long n = 0;
            try
            {
                await foreach (var values in s.QueryAsync(prep.Query.Sql, prep.Query.Params, ct).ConfigureAwait(false))
                {
                    n++;
                    var row = ToRow(prep.Query, values);
                    result.Rows.Add(layout.Columns.Select(c => ValueConverter.Display(c.Type, row[c.Name], c.Format)).ToList());
                    sink.Write(JsonHelper.MapRow(layout, row, options, stats, n, ValueConverter.ToJson));
                }
                sink.Complete();
                result.Json = sink.VirtualJson ?? "";
            }
            catch { sink.Abort(); throw; }
            finally { sink.Dispose(); }

            result.Warnings.AddRange(stats.Issues);
            return result;
        }

        // ---------------------------------------------------------------- geracao
        public async Task<GenerateResult> RunAsync(GenerateRequest req, IProgress<LogEntry> log, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var result = new GenerateResult();
            JsonSink? sink = null;

            try
            {
                var toFiles = !string.Equals(req.Mode, "virtual", StringComparison.OrdinalIgnoreCase);
                if (toFiles && string.IsNullOrWhiteSpace(req.OutputFolder))
                    throw new GeneratorException("Escolha a pasta de destino dos arquivos (ou use o modo Virtual).");

                var layout = _layouts.Get(req.LayoutId);
                var profile = _profiles.GetDecrypted(req.ProfileId);

                log.Report(LogEntry.Info($"Layout: {layout.Name} ({layout.Id})"));
                log.Report(LogEntry.Info($"Conectando em {profile.Name} ({ProviderCatalog.Get(profile.ProviderKey).DisplayName})..."));
                using var s = await DatabaseConnection.OpenAsync(profile, ct).ConfigureAwait(false);
                log.Report(LogEntry.Ok("Conectado."));

                log.Report(LogEntry.Info("Conferindo tabela e colunas do layout no banco..."));
                var prep = await PrepareAsync(s, layout, req, null, ct).ConfigureAwait(false);
                foreach (var w in prep.Warnings) log.Report(LogEntry.Warn(w));
                log.Report(LogEntry.Info("SQL: " + prep.Query.Sql));

                long total = 0;
                try { total = await s.ScalarLongAsync(prep.Query.CountSql, prep.Query.Params, ct).ConfigureAwait(false); }
                catch (Exception ex) when (ex is not OperationCanceledException) { log.Report(LogEntry.Warn("Contagem indisponivel: " + ex.Message)); }
                log.Report(LogEntry.Info(total > 0 ? $"{total:N0} registro(s) a processar." : "Processando..."));

                sink = new JsonSink(layout.RootField!.Name, toFiles, req.OutputFolder, FileBase(req, layout), req.MaxPerFile);
                var stats = new MapStats();
                var options = new MapOptions { EmptyArrayAsNull = req.EmptyArrayAsNull };

                long n = 0;
                var lastReport = Stopwatch.StartNew();
                await foreach (var values in s.QueryAsync(prep.Query.Sql, prep.Query.Params, ct).ConfigureAwait(false))
                {
                    n++;
                    sink.Write(JsonHelper.MapRow(layout, ToRow(prep.Query, values), options, stats, n, ValueConverter.ToJson));
                    if (lastReport.ElapsedMilliseconds >= 400)
                    {
                        lastReport.Restart();
                        int? pct = total > 0 ? (int)Math.Min(99, n * 100 / total) : null;
                        log.Report(LogEntry.Info($"{n:N0} registro(s) processado(s)...", pct));
                    }
                }

                sink.Complete();
                result.Rows = sink.TotalRows;
                result.Files = sink.Files.ToList();
                result.Json = sink.VirtualJson;
                result.EmptyByColumn = stats.EmptyByColumn;
                result.Issues = stats.Issues;
                result.IssueCount = stats.IssueCount;
                result.Success = true;

                foreach (var f in result.Files)
                    log.Report(LogEntry.Ok($"Arquivo gerado: {f.Path} ({f.Rows:N0} registro(s), {f.Bytes / 1024.0:N1} KB)"));
                if (!toFiles) log.Report(LogEntry.Ok($"JSON virtual gerado ({result.Rows:N0} registro(s))."));
                if (stats.IssueCount > 0) log.Report(LogEntry.Warn($"{stats.IssueCount:N0} valor(es) nao puderam ser convertidos e foram gravados como null. Veja o resumo."));
                log.Report(LogEntry.Ok($"Concluido em {sw.Elapsed.TotalSeconds:N1}s.", 100));
            }
            catch (OperationCanceledException)
            {
                sink?.Abort();
                result.Cancelled = true;
                result.Error = "Cancelado pelo usuario. Arquivos parciais foram removidos.";
                log.Report(LogEntry.Warn(result.Error));
            }
            catch (GeneratorException ex)
            {
                sink?.Abort();
                result.Error = ex.Message;
                log.Report(LogEntry.Error(ex.Message));
            }
            catch (Exception ex)
            {
                sink?.Abort();
                result.Error = "Erro inesperado: " + ex.Message;
                log.Report(LogEntry.Error(result.Error));
            }
            finally
            {
                sink?.Dispose();
                result.ElapsedMs = sw.ElapsedMilliseconds;
            }
            return result;
        }
    }

}

