using AutoIt;
using OrionAutomator.Controller;
using ReferenciaFactory.Engine;
using System.Text.RegularExpressions;

namespace OrionAutomator.Acoes
{
    /// <summary>Tudo o que um executor precisa para agir: controlador AutoIt, variaveis do fluxo e log.</summary>
    public sealed class ContextoExecucao
    {
        private static readonly Regex Variavel = new(@"\{\{\s*([^{}]+?)\s*\}\}", RegexOptions.Compiled);

        public AutoItController AutoIt { get; }
        public Action<string> Log { get; }
        public Dictionary<string, string> Variaveis { get; } = new(StringComparer.OrdinalIgnoreCase);

        public ContextoExecucao(AutoItController autoIt, Action<string> log) { AutoIt = autoIt; Log = log; }

        /// <summary>Troca {{nome}} pelo valor da variavel (variavel inexistente vira texto vazio).</summary>
        public string Resolver(string texto) =>
            Variavel.Replace(texto ?? "", m => Variaveis.TryGetValue(m.Groups[1].Value, out var v) ? v : "");
    }

    /// <summary>Executa uma ou mais "chaves" de executor (atributo executor="..." do Acoes.xml).</summary>
    public interface IExecutorAcao
    {
        IEnumerable<string> Chaves { get; }
        Task ExecutarAsync(string chave, NoFluxo no, ContextoExecucao ctx, CancellationToken ct);
    }

    /// <summary>
    /// CENTRALIZADOR de execucao: liga a chave do XML ao codigo. Acrescentar uma acao nova que ja tenha executor
    /// e so registrar a classe aqui; o catalogo, o painel de propriedades e o cartao nao mudam.
    /// </summary>
    public sealed class RegistroExecutores
    {
        private readonly Dictionary<string, IExecutorAcao> _porChave = new(StringComparer.OrdinalIgnoreCase);

        public RegistroExecutores Registrar(IExecutorAcao executor)
        {
            foreach (var chave in executor.Chaves) _porChave[chave] = executor;
            return this;
        }

        public bool Tem(string? chave) => !string.IsNullOrEmpty(chave) && _porChave.ContainsKey(chave);

        public IExecutorAcao? Buscar(string chave) => _porChave.TryGetValue(chave, out var e) ? e : null;

        public static RegistroExecutores Padrao() => new RegistroExecutores()
            .Registrar(new MouseExecutor())
            .Registrar(new TecladoExecutor())
            .Registrar(new FluxoBasicoExecutor());
    }

    // ══ Mouse (porta do MouseNodeExecutor original) ═══════════════════════════
    public sealed class MouseExecutor : IExecutorAcao
    {
        public IEnumerable<string> Chaves { get; } = new[]
        {
            "mouse.clique", "mouse.mover", "mouse.arrastar", "mouse.scroll", "mouse.palavra", "mouse.texto"
        };

        public async Task ExecutarAsync(string chave, NoFluxo no, ContextoExecucao ctx, CancellationToken ct)
        {
            switch (chave.ToLowerInvariant())
            {
                case "mouse.clique": await ClicarAsync(no, ctx, ct); break;

                case "mouse.mover":
                {
                    int x = no.Int("X"), y = no.Int("Y"), vel = no.Int("velocidade", 10);
                    ctx.Log($"  🖱 Mover mouse para ({x},{y}) vel={vel}");
                    await ctx.AutoIt.MoveMouseAsync(x, y, Velocidade(vel));
                    await Task.Delay(50, ct);
                    break;
                }

                case "mouse.arrastar":
                {
                    int x1 = no.Int("DeX"), y1 = no.Int("DeY"), x2 = no.Int("ParaX"), y2 = no.Int("ParaY"), vel = no.Int("velocidade", 5);
                    ctx.Log($"  🖱 Arrastar de ({x1},{y1}) para ({x2},{y2}) vel={vel}");
                    await ctx.AutoIt.MoveMouseAsync(x1, y1, Velocidade(vel));
                    await ctx.AutoIt.DragAsync(x1, y1, x2, y2, vel);
                    break;
                }

                case "mouse.scroll":
                {
                    int x = no.Int("X"), y = no.Int("Y"), cliques = no.Int("cliques", 3);
                    string dir = no.Texto("direcao", "Down").Equals("Down", StringComparison.OrdinalIgnoreCase) ? "down" : "up";
                    ctx.Log($"  🖱 Scroll {dir} ×{cliques} em ({x},{y})");
                    await ctx.AutoIt.MoveMouseAsync(x, y, 10);
                    await Task.Run(() => AutoItX.MouseWheel(dir, cliques), ct);
                    break;
                }

                case "mouse.palavra":
                {
                    int x = no.Int("X"), y = no.Int("Y");
                    ctx.Log($"  ▣ Selecionar palavra em ({x},{y})");
                    await ctx.AutoIt.ClickAsync(x, y, "left", 2);
                    await Task.Delay(80, ct);
                    break;
                }

                case "mouse.texto":
                {
                    int x1 = no.Int("DeX"), y1 = no.Int("DeY"), x2 = no.Int("AteX"), y2 = no.Int("AteY"), vel = no.Int("velocidade", 5);
                    ctx.Log($"  ▤ Selecionar texto de ({x1},{y1}) até ({x2},{y2})");
                    await ctx.AutoIt.SelectTextAsync(x1, y1, x2, y2, vel);
                    await Task.Delay(80, ct);
                    break;
                }

                default:
                    throw new NotSupportedException($"MouseExecutor não trata '{chave}'.");
            }
        }

        private static async Task ClicarAsync(NoFluxo no, ContextoExecucao ctx, CancellationToken ct)
        {
            int x = no.Int("X"), y = no.Int("Y");

            string posVar = no.Texto("posVar").Trim();
            if (posVar.Length > 0)
            {
                string bruto = ctx.Resolver("{{" + posVar + "}}");
                var partes = bruto.Split(',');
                if (partes.Length == 2 && int.TryParse(partes[0].Trim(), out int vx) && int.TryParse(partes[1].Trim(), out int vy))
                { x = vx; y = vy; }
                else
                    throw new Exception($"Clique: a variável '{posVar}' não contém uma posição válida ('X,Y'). Valor: '{bruto}'");
            }

            string botao = no.Texto("botao", "Left").ToLowerInvariant();
            int cliques = Math.Max(1, no.Int("cliques", 1));
            int vel = no.Int("velocidade", 5);
            ctx.Log($"  🖱 Clique {botao} em ({x},{y}) vel={vel}");

            string janela = no.Texto("janelaAlvo").Trim();
            if (janela.Length > 0) await ctx.AutoIt.ActivateWindowAsync(janela, ct);
            await Task.Delay(80, ct);

            if (x < 0 || y < 0) throw new Exception($"Clique: coordenadas inválidas X={x} Y={y}");
            await ctx.AutoIt.ClickAsync(x, y, botao, cliques, vel);
        }

        private static int Velocidade(int v) => Math.Clamp(v, 1, 100);
    }

    // ══ Teclado ═══════════════════════════════════════════════════════════════
    public sealed class TecladoExecutor : IExecutorAcao
    {
        public IEnumerable<string> Chaves { get; } = new[] { "teclado.digitar", "teclado.tecla" };

        public async Task ExecutarAsync(string chave, NoFluxo no, ContextoExecucao ctx, CancellationToken ct)
        {
            if (chave.Equals("teclado.digitar", StringComparison.OrdinalIgnoreCase))
            {
                string texto = ctx.Resolver(no.Texto("texto"));
                ctx.Log($"  ⌨ Digitar {texto.Length} caractere(s)");
                await ctx.AutoIt.TypeTextAsync(texto, no.Int("atraso", 20));
                return;
            }

            string tecla = no.Texto("tecla").Trim();
            if (tecla.Length == 0) throw new Exception("Tecla especial: informe a tecla.");
            if (HotkeyTranslator.SpecialKeyMaps.TryGetValue(tecla, out var mapa)) tecla = mapa.Trim('{', '}');

            string mods = (no.Bool("ctrl") ? "^" : "") + (no.Bool("shift") ? "+" : "") + (no.Bool("alt") ? "!" : "");
            ctx.Log($"  ⌨ Tecla {(mods.Length > 0 ? mods : "")}{{{tecla}}}");
            await ctx.AutoIt.HotKeyAsync(mods, tecla);
        }
    }

    // ══ Fluxo basico ══════════════════════════════════════════════════════════
    public sealed class FluxoBasicoExecutor : IExecutorAcao
    {
        public IEnumerable<string> Chaves { get; } = new[] { "fluxo.inicio", "fluxo.fim", "fluxo.aguardar", "fluxo.comentario" };

        public async Task ExecutarAsync(string chave, NoFluxo no, ContextoExecucao ctx, CancellationToken ct)
        {
            if (chave.Equals("fluxo.aguardar", StringComparison.OrdinalIgnoreCase))
            {
                int ms = Math.Max(0, no.Int("tempoMs", 1000));
                ctx.Log($"  ⏳ Aguardar {ms} ms");
                await Task.Delay(ms, ct);
            }
            // inicio, fim e comentario nao fazem nada aqui (o motor trata o "fim")
        }
    }

    // ══ Motor ═════════════════════════════════════════════════════════════════
    /// <summary>
    /// Percorre a cadeia de nos a partir do "Início", valida as propriedades pelas regras do XML (tipo/min/max/lista)
    /// e chama o executor registrado. Eventos sao disparados na thread de quem chamou ExecutarAsync (a da tela).
    /// </summary>
    public sealed class MotorFluxo
    {
        private CancellationTokenSource? _cts;
        private volatile bool _pausado;

        public bool EmExecucao { get; private set; }
        public bool Pausado => _pausado;

        public event Action<NoFluxo>? NoAlterado;
        public event Action<string>? Log;
        public event Action? EstadoMudou;   // iniciou, pausou, retomou, terminou

        public void Pausar() { if (EmExecucao && !_pausado) { _pausado = true; Log?.Invoke("⏸ Execução pausada."); EstadoMudou?.Invoke(); } }
        public void Retomar() { if (EmExecucao && _pausado) { _pausado = false; Log?.Invoke("▶ Execução retomada."); EstadoMudou?.Invoke(); } }
        public void Parar() { if (EmExecucao) { _cts?.Cancel(); _pausado = false; } }

        public async Task ExecutarAsync(ModeloFluxo modelo, CatalogoAcoes catalogo, RegistroExecutores registro, AutoItController autoIt)
        {
            if (EmExecucao) return;
            EmExecucao = true;
            _pausado = false;
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            var ctx = new ContextoExecucao(autoIt, m => Log?.Invoke(m));

            foreach (var n in modelo.Nos) { n.Estado = EstadoNo.Normal; NoAlterado?.Invoke(n); }
            EstadoMudou?.Invoke();
            Log?.Invoke("▶ Executando fluxo...");

            try
            {
                var atual = EncontrarInicio(modelo, catalogo);
                if (atual == null) throw new Exception("O fluxo não tem nó de Início.");

                var visitados = new HashSet<string>();
                while (atual != null)
                {
                    ct.ThrowIfCancellationRequested();
                    while (_pausado) await Task.Delay(100, ct);

                    if (!visitados.Add(atual.Id))
                        throw new Exception("Ciclo detectado: um nó foi alcançado duas vezes (laços ainda não são suportados).");

                    var def = catalogo.Buscar(atual.AcaoId)
                        ?? throw new Exception($"Ação '{atual.AcaoId}' não existe no catálogo (Acoes.xml).");
                    var executor = registro.Buscar(def.Executor)
                        ?? throw new Exception($"'{def.Rotulo}': executor '{def.Executor}' não está registrado.");

                    Log?.Invoke($"▶ {atual.Titulo(def)}");
                    bool continuar = await ExecutarNoAsync(atual, def, executor, ctx, ct);
                    if (!continuar) return;

                    if (def.Executor.Equals("fluxo.fim", StringComparison.OrdinalIgnoreCase)) break;
                    atual = modelo.Buscar(atual.ProximoId);
                }

                Log?.Invoke("✓ Execução concluída com sucesso.");
            }
            catch (OperationCanceledException) { Log?.Invoke("⛔ Execução interrompida pelo usuário."); }
            catch (Exception ex) { Log?.Invoke($"❌ Erro: {ex.Message}"); }
            finally
            {
                EmExecucao = false;
                _pausado = false;
                _cts?.Dispose();
                _cts = null;
                EstadoMudou?.Invoke();
            }
        }

        /// <summary>false = interromper o fluxo (erro com emErro=Stop).</summary>
        private async Task<bool> ExecutarNoAsync(NoFluxo no, DefinicaoAcao def, IExecutorAcao executor, ContextoExecucao ctx, CancellationToken ct)
        {
            string politica = no.Texto("emErro", "Stop");
            int tentativas = politica.Equals("Retry", StringComparison.OrdinalIgnoreCase) ? 3 : 1;

            no.Estado = EstadoNo.Executando;
            NoAlterado?.Invoke(no);

            for (int t = 1; t <= tentativas; t++)
            {
                try
                {
                    foreach (var p in def.Propriedades)
                    {
                        string motivo = p.Validar(no.Texto(p.Nome));
                        if (motivo.Length > 0) throw new Exception(motivo);
                    }

                    await executor.ExecutarAsync(def.Executor, no, ctx, ct);
                    no.Estado = EstadoNo.Sucesso;
                    NoAlterado?.Invoke(no);
                    return true;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    Log?.Invoke($"  ❌ {def.Rotulo}: {ex.Message}" + (t < tentativas ? $"  (tentativa {t}/{tentativas})" : ""));
                    if (t < tentativas) await Task.Delay(500, ct);
                }
            }

            no.Estado = EstadoNo.Erro;
            NoAlterado?.Invoke(no);

            if (politica.Equals("Continue", StringComparison.OrdinalIgnoreCase))
            {
                Log?.Invoke("  ⊘ Erro ignorado (Em caso de erro = Continue).");
                return true;
            }
            throw new Exception($"Fluxo interrompido em '{no.Titulo(def)}'.");
        }

        private static NoFluxo? EncontrarInicio(ModeloFluxo modelo, CatalogoAcoes catalogo)
        {
            var inicio = modelo.Nos.FirstOrDefault(n => catalogo.Buscar(n.AcaoId)?.Executor == "fluxo.inicio");
            if (inicio != null) return inicio;

            var alvos = modelo.Nos.Where(n => n.ProximoId != null).Select(n => n.ProximoId!).ToHashSet();
            return modelo.Nos.FirstOrDefault(n => !alvos.Contains(n.Id));
        }
    }
}
