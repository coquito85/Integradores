
using Microsoft.Web.WebView2.Core;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using ReferenciaStandard.Repository;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace GenerateJS.Gerenciador
{

    /// <summary>
    /// Ponte JS &lt;-&gt; C#. A tela envia {id, cmd, args}; recebe {id, ok, data|error}.
    /// Eventos do servidor (log, fim de execucao) chegam como {event, data}.
    /// Esta classe so traduz mensagens da tela em chamadas ao <see cref="GeneratorJsExecutor"/> (ExecutorService)
    /// e formata a resposta; nao acessa banco, layout nem disco por conta propria.
    /// </summary>
    internal sealed class Bridge
    {
        private readonly CoreWebView2 _core;
        private readonly frmTelaGeneratorJS _ui;
        private readonly GeneratorJs.GeneratorJsExecutor _exec;

        public Bridge(CoreWebView2 core, frmTelaGeneratorJS ui, GeneratorJs.GeneratorJsExecutor exec)
        {
            _core = core;
            _ui = ui;
            _exec = exec;
        }

        // ------------------------------------------------------------------ transporte
        public async Task HandleAsync(string message)
        {
            int id = 0;
            try
            {
                var root = JObject.Parse(message);
                id = root.Value<int>("id");
                var cmd = root.Value<string>("cmd") ?? "";
                var args = root["args"] as JObject ?? new JObject();

                var data = await DispatchAsync(cmd, args);
                Send(new { id, ok = true, data });
            }
            catch (GeneratorException ex) { Send(new { id, ok = false, error = ex.Message }); }
            catch (OperationCanceledException) { Send(new { id, ok = false, error = "Operacao cancelada." }); }
            catch (Exception ex) { Send(new { id, ok = false, error = "Erro inesperado: " + ex.Message }); }
        }

        private void Send(object payload)
        {
            var text = JsonHelper.SerializarTela(payload);   // serializacao central: ReferenciaStandard.Helpers.JsonHelper
            if (_ui.InvokeRequired) _ui.BeginInvoke(new Action(() => _core.PostWebMessageAsJson(text)));
            else _core.PostWebMessageAsJson(text);
        }

        private void Emit(string name, object data) => Send(new { @event = name, data });

        private static T Arg<T>(JObject args, string name)
        {
            var v = args[name];
            if (v == null || v.Type == JTokenType.Null)
                throw new GeneratorException($"Parametro '{name}' ausente.");
            var valor = JsonHelper.DesserializarTela<T>(v.ToString(Newtonsoft.Json.Formatting.None));
            return valor ?? throw new GeneratorException($"Parametro '{name}' invalido.");
        }

        private static string Str(JObject args, string name, string def = "")
        {
            var v = args[name];
            return v != null && v.Type == JTokenType.String ? (string?)v ?? def : def;
        }

        // ------------------------------------------------------------------ comandos
        private async Task<object?> DispatchAsync(string cmd, JObject a)
        {
            switch (cmd)
            {
                case "app.info":
                    return new
                    {
                        version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "2.0.0",
                        dataFolder = _exec.PastaDados,
                        userLayouts = _exec.PastaLayoutsUsuario,
                        defaultOutput = _exec.PastaSaidaPadrao,
                        layoutErrors = _exec.ErrosDeLayout
                    };

                case "ui.theme":
                    {
                        var hex = Str(a, "caption", "#000000").TrimStart('#');
                        var dark = a.Value<bool?>("dark") == true;
                        if (hex.Length == 6)
                            _ui.SetTitleBar(dark, System.Drawing.Color.FromArgb(Convert.ToInt32(hex, 16) | unchecked((int)0xFF000000)));
                        return null;
                    }
                case "dialog.file":
                    {
                        using var dlg = new OpenFileDialog
                        {
                            Title = "Selecionar arquivo",
                            Filter = Str(a, "filter", "Todos os arquivos (*.*)|*.*"),
                            CheckFileExists = true
                        };
                        var start = Str(a, "path");
                        if (start.Length > 0 && File.Exists(start)) dlg.FileName = start;
                        return dlg.ShowDialog(_ui) == DialogResult.OK ? dlg.FileName : null;
                    }
                // ---- bancos
                case "providers.list":
                    return _exec.Provedores.Select(p => new
                    {
                        key = p.Key,
                        name = p.DisplayName,
                        category = p.Category,
                        abbr = p.Abbr,                    // "CSV", "TXT", "XLS"...
                        supportsRaw = p.SupportsRaw,      // false para arquivos
                        fields = p.Fields.Select(f => new
                        {
                            id = f.Id,
                            label = f.Label,
                            kind = f.Kind,
                            required = f.Required,
                            placeholder = f.Placeholder,
                            @default = f.Default,
                            group = f.Group,              // null = grupo principal; "Avançado" = recolhido
                            filter = f.Filter,            // filtro do OpenFileDialog (kind = "file")
                            hint = f.HintSource?.Invoke() ?? f.Hint,
                            options = f.OptionsSource?.Invoke().Select(o => new { value = o.Value, label = o.Label, installed = o.Installed }),
                            secret = _exec.EhCampoSecreto(f.Id)
                        })
                    }).ToList();

                case "profiles.list":
                    return _exec.ListarConexoes().Select(MaskProfile).ToList();

                case "profiles.save":
                    return MaskProfile(_exec.SalvarConexao(Arg<ConnectionProfile>(a, "profile")));

                case "profiles.delete":
                    _exec.ExcluirConexao(Str(a, "id"));
                    return null;

                case "profiles.test":
                    return await _exec.TestarConexaoAsync(Arg<ConnectionProfile>(a, "profile"));

                case "schema.tables":
                    return await _exec.ListarTabelasAsync(Str(a, "profileId"));

                case "schema.columns":
                    {
                        var cols = await _exec.ListarColunasAsync(Str(a, "profileId"), Str(a, "table"));
                        return cols.Select(c => new { name = c.Name, type = c.Type }).ToList();
                    }

                // ---- layouts
                case "layouts.list":
                    return _exec.ListarLayouts().Select(LayoutSummary).ToList();

                case "layouts.get":
                    return LayoutDetail(_exec.ObterLayout(Str(a, "id")));

                case "layouts.reload":
                    return _exec.RecarregarLayouts().Select(LayoutSummary).ToList();

                case "layouts.import":
                    {
                        using var dlg = new OpenFileDialog { Title = "Importar layout XML", Filter = "Layout XML (*.xml)|*.xml", CheckFileExists = true };
                        if (dlg.ShowDialog(_ui) != DialogResult.OK) return null;
                        return LayoutSummary(_exec.ImportarLayout(dlg.FileName));
                    }

                // ---- geracao
                case "generate.preview":
                    return await _exec.PreviaAsync(Arg<GenerateRequest>(a, "request"));

                case "generate.start":
                    {
                        // O ExecutorService cria o Progress<T> aqui, na thread da tela: os callbacks voltam para ela.
                        var runId = _exec.Iniciar(Arg<GenerateRequest>(a, "request"),
                            (id, entry) => Emit("log", new { runId = id, entry }),
                            (id, result) => Emit("done", new { runId = id, result }));
                        return new { runId };
                    }

                case "generate.cancel":
                    _exec.Cancelar(Str(a, "runId"));
                    return null;

                // ---- historico
                case "history.list":
                    return _exec.Historico();

                case "history.clear":
                    _exec.LimparHistorico();
                    return null;

                // ---- sistema de arquivos
                case "dialog.folder":
                    {
                        var start = Str(a, "path");
                        // Usa o FolderBrowserDialog do Windows Forms
                        using var dlg = new FolderBrowserDialog { Description = "Pasta de destino" };

                        if (start.Length > 0 && Directory.Exists(start))
                            dlg.SelectedPath = start; // No FolderBrowserDialog, a propriedade é SelectedPath

                        // ShowDialog retorna DialogResult (e não bool? como no WPF)
                        return dlg.ShowDialog(_ui) == DialogResult.OK ? dlg.SelectedPath : null;
                    }

                case "file.save":
                    {
                        using var dlg = new SaveFileDialog { Title = "Salvar JSON", Filter = "JSON (*.json)|*.json", FileName = Str(a, "name", "saida.json") };
                        if (dlg.ShowDialog(_ui) != DialogResult.OK) return null;
                        await File.WriteAllTextAsync(dlg.FileName, Str(a, "content"), new System.Text.UTF8Encoding(false));
                        return dlg.FileName;
                    }

                case "shell.open":
                    {
                        var path = Str(a, "path");
                        if (File.Exists(path))
                            Process.Start(new ProcessStartInfo("explorer.exe") { ArgumentList = { "/select,", path }, UseShellExecute = false });
                        else if (Directory.Exists(path))
                            Process.Start(new ProcessStartInfo("explorer.exe") { ArgumentList = { path }, UseShellExecute = false });
                        else throw new GeneratorException("O caminho nao existe mais.");
                        return null;
                    }

                default:
                    throw new GeneratorException($"Comando desconhecido: {cmd}");
            }
        }

        // ------------------------------------------------------------------ DTOs
        /// <summary>Nunca envia senha/string de conexao para a tela.</summary>
        private object MaskProfile(ConnectionProfile p)
        {
            var fields = p.Fields.ToDictionary(kv => kv.Key, kv => _exec.EhCampoSecreto(kv.Key) ? "" : kv.Value);
            var hasSecret = p.Fields.Any(kv => _exec.EhCampoSecreto(kv.Key) && !string.IsNullOrEmpty(kv.Value));
            string summary;
            if (p.Mode == "raw") summary = "string de conexao personalizada";
            else if (p.Fields.TryGetValue("file", out var file) && file.Length > 0) summary = Path.GetFileName(file);
            else if (p.Fields.TryGetValue("folder", out var folder) && folder.Length > 0) summary = folder;
            else
            {
                p.Fields.TryGetValue("server", out var server);
                p.Fields.TryGetValue("database", out var db);
                summary = string.Join(" / ", new[] { server, db }.Where(x => !string.IsNullOrEmpty(x)));
            }
            return new
            {
                id = p.Id,
                name = p.Name,
                providerKey = p.ProviderKey,
                mode = p.Mode,
                fields,
                hasSecret,
                hasRaw = !string.IsNullOrEmpty(p.RawConnectionString),
                summary
            };
        }

        private static object LayoutSummary(LayoutDefinition l) => new
        {
            id = l.Id,
            name = l.Name,
            source = l.Source,
            table = l.TableName,
            alias = l.TableAlias,
            rootName = l.RootField?.Name,
            columns = l.Columns.Count,
            errors = l.ErrorCount,
            warnings = l.WarningCount,
            infos = l.Findings.Count(f => f.Level == IsFindingLevel.Info)
        };

        private static object LayoutDetail(LayoutDefinition l) => new
        {
            id = l.Id,
            name = l.Name,
            source = l.Source,
            file = l.FilePath,
            table = l.TableName,
            alias = l.TableAlias,
            rootName = l.RootField?.Name,
            columns = l.Columns.Select(c => new { name = c.Name, type = c.Type, format = c.Format, description = c.Description, orderBy = c.OrderBy }),
            tree = l.RootField is null ? null : FieldDto(l.RootField),
            findings = l.Findings.Select(f => new { level = f.Level, code = f.Code, message = f.Message, path = f.Path }),
            xml = l.Xml
        };

        private static object FieldDto(LayoutField f) => new
        {
            name = f.Name,
            type = f.Type,
            value = f.Value,
            columnRef = f.ColumnRef,
            format = f.Format,
            children = f.Children.Select(FieldDto).ToList()
        };
    }

}