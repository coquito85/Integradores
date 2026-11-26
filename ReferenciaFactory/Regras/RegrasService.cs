using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;
using ReferenciaStandard.Repository;
using System.Globalization;
using System.Reflection;

namespace ReferenciaFactory.Regras
{
    /// <summary>Foto do estado de licenca/versao num instante. So leitura: quem decide e o RegrasService.</summary>
    public sealed class ResultadoRegras
    {
        // licenca
        public bool LicencaAtiva { get; internal set; }
        public bool LicencaValida { get; internal set; }
        public bool RelogioInconsistente { get; internal set; }
        public bool AvisoLicenca { get; internal set; }
        public string? Titular { get; internal set; }
        public DateTime? Validade { get; internal set; }
        public int? DiasRestantes { get; internal set; }
        public string? MotivoLicenca { get; internal set; }
        public string? MensagemAviso { get; internal set; }

        // versao
        public Version VersaoAtual { get; internal set; } = new Version(1, 0, 0, 0);
        public Version? VersaoMinima { get; internal set; }
        public Version? VersaoUltima { get; internal set; }
        public string? UrlDownload { get; internal set; }
        public bool VersaoObrigatoriaPendente { get; internal set; }
        public bool VersaoNovaDisponivel { get; internal set; }
        public string? MotivoVersao { get; internal set; }
        public string? MensagemVersaoNova { get; internal set; }

        /// <summary>Texto para o aviso vermelho: junta os motivos que estao bloqueando o uso.</summary>
        public string? MensagemBloqueio
        {
            get
            {
                var partes = new List<string>();
                if (!LicencaValida && !string.IsNullOrWhiteSpace(MotivoLicenca)) partes.Add(MotivoLicenca!);
                if (VersaoObrigatoriaPendente && !string.IsNullOrWhiteSpace(MotivoVersao)) partes.Add(MotivoVersao!);
                return partes.Count == 0 ? null : string.Join(Environment.NewLine, partes);
            }
        }
    }

    /// <summary>
    /// Avalia as regras de uso definidas em Regras.xml: prazo da licenca, versao minima/ultima e quais
    /// funcionalidades cada regra bloqueia. As regras so desabilitam; a interface nunca habilita por conta delas.
    /// </summary>
    public sealed class RegrasService
    {
        private sealed class EstadoLocal
        {
            public DateTime? UltimaVistaUtc { get; set; }
        }

        private const string MensagemInvalidaPadrao = "Não foi possível validar a licença. Entre em contato com o suporte.";

        private readonly Entid_RegrasDados? _dados;
        private readonly string? _erroCarga;
        private readonly Version _versaoAtual;
        private readonly object _trava = new();
        private readonly string _arquivoEstado = AppPaths.Arquivo("estado.json");
        private EstadoLocal _estado;

        /// <summary>De quanto em quanto tempo a janela deve reavaliar as regras (Licenca/intervaloVerificacaoSegundos).</summary>
        public int IntervaloVerificacaoSegundos { get; }

        private RegrasService(Entid_RegrasDados? dados, string? erroCarga, Version versaoAtual)
        {
            _dados = dados;
            _erroCarga = erroCarga;
            _versaoAtual = versaoAtual;
            _estado = JsonHelper.LerArquivo(_arquivoEstado, new EstadoLocal()) ?? new EstadoLocal();
            IntervaloVerificacaoSegundos = Math.Max(5, dados?.Licenca?.Int("intervaloVerificacaoSegundos") ?? 60);
        }

        /// <summary>
        /// Carrega Regras.xml do recurso embutido. Se nao for possivel ler, o servico bloqueia o uso
        /// (falha fechada): licenca sem regras legiveis nao pode ser tratada como valida.
        /// </summary>
        public static RegrasService Carregar(Version? versaoAtual = null)
        {
            Entid_RegrasDados? dados = null;
            string? erro = null;

            try
            {
                string? xml = FilesRepository.LerEmbutido("Regras");
                if (string.IsNullOrWhiteSpace(xml))
                    erro = "Regras.xml não encontrado." + (FilesRepository.UltimoErro != null ? " " + FilesRepository.UltimoErro : "");
                else
                    dados = XmlHelper.LerRegras(xml);
            }
            catch (Exception ex)
            {
                erro = "Falha ao ler Regras.xml: " + ex.Message;
            }

            Version v = Normalizar(versaoAtual ?? Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0, 0, 0));
            return new RegrasService(dados, erro, v);
        }

        public ResultadoRegras Avaliar() => Avaliar(DateTime.Now);

        public ResultadoRegras Avaliar(DateTime agora)
        {
            var r = new ResultadoRegras { VersaoAtual = _versaoAtual };

            if (_dados == null || _dados.Licenca == null)
            {
                r.LicencaAtiva = true;
                r.LicencaValida = false;
                r.MotivoLicenca = _dados == null
                    ? (_erroCarga ?? MensagemInvalidaPadrao)
                    : MensagemInvalidaPadrao;
                return r;
            }

            AvaliarLicenca(r, _dados.Licenca, agora);
            AvaliarVersao(r, _dados.Versao);
            return r;
        }

        // ── Licenca ─────────────────────────────────────────────────────────
        private void AvaliarLicenca(ResultadoRegras r, Entid_Atributos lic, DateTime agora)
        {
            r.LicencaAtiva = lic.Bool("ativa") ?? true;
            r.Titular = lic.Texto("titular");

            if (!r.LicencaAtiva)
            {
                r.LicencaValida = true;
                return;
            }

            if (!DateTime.TryParseExact(lic.Texto("validade")?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out DateTime validade))
            {
                r.LicencaValida = false;
                r.MotivoLicenca = Formatar(lic.Texto("mensagemInvalida"), MensagemInvalidaPadrao, r);
                return;
            }

            r.Validade = validade.Date;
            r.DiasRestantes = (int)(validade.Date - agora.Date).TotalDays;

            // relogio: se a data do sistema voltou, o prazo nao pode ser avaliado com seguranca
            int tolerancia = Math.Max(0, lic.Int("toleranciaRelogioMinutos") ?? 60);
            r.RelogioInconsistente = RelogioVoltou(agora, tolerancia);

            if (r.RelogioInconsistente)
            {
                r.LicencaValida = false;
                r.MotivoLicenca = Formatar(lic.Texto("mensagemRelogio"),
                    "Data do sistema inconsistente. Corrija o relógio do computador para continuar.", r);
            }
            else if (r.DiasRestantes < 0)
            {
                r.LicencaValida = false;
                r.MotivoLicenca = Formatar(lic.Texto("mensagemExpirada"),
                    "Seu prazo de uso expirou em {validade}. Entre em contato para renovar a licença.", r);
            }
            else
            {
                r.LicencaValida = true;
                int diasAviso = Math.Max(0, lic.Int("diasAviso") ?? 15);
                if (r.DiasRestantes <= diasAviso)
                {
                    r.AvisoLicenca = true;
                    r.MensagemAviso = Formatar(lic.Texto("mensagemAviso"), "Sua licença expira em {dias} dia(s) ({validade}).", r);
                }
            }
        }

        private bool RelogioVoltou(DateTime agora, int toleranciaMinutos)
        {
            DateTime agoraUtc = agora.ToUniversalTime();

            lock (_trava)
            {
                DateTime? ultima = _estado.UltimaVistaUtc;

                if (ultima.HasValue && agoraUtc < ultima.Value.AddMinutes(-toleranciaMinutos))
                    return true;

                // so avanca (e grava) quando passou mais de 1 minuto, para nao escrever em disco a cada verificacao
                if (!ultima.HasValue || agoraUtc > ultima.Value.AddMinutes(1))
                {
                    _estado.UltimaVistaUtc = agoraUtc;
                    JsonHelper.GravarArquivo(_arquivoEstado, _estado, out _);
                }
                return false;
            }
        }

        // ── Versao ──────────────────────────────────────────────────────────
        private void AvaliarVersao(ResultadoRegras r, Entid_Atributos? ver)
        {
            if (ver == null) return;

            r.VersaoMinima = ParseVersao(ver.Texto("minima"));
            r.VersaoUltima = ParseVersao(ver.Texto("ultima"));
            r.UrlDownload = ver.Texto("urlDownload");

            r.VersaoObrigatoriaPendente = r.VersaoMinima != null && _versaoAtual < r.VersaoMinima;
            r.VersaoNovaDisponivel = r.VersaoUltima != null && _versaoAtual < r.VersaoUltima;

            if (r.VersaoObrigatoriaPendente)
                r.MotivoVersao = Formatar(ver.Texto("mensagemObrigatoria"),
                    "Esta versão ({atual}) não é mais suportada. Atualize para a {minima} ou superior.", r);

            if (r.VersaoNovaDisponivel)
                r.MensagemVersaoNova = Formatar(ver.Texto("mensagemNova"),
                    "Nova versão {ultima} disponível (você está na {atual}).", r);
        }

        // ── Bloqueios por funcionalidade ────────────────────────────────────
        public bool Permite(string funcionalidade, out string motivo) => Permite(Avaliar(), funcionalidade, out motivo);

        public bool Permite(ResultadoRegras r, string funcionalidade, out string motivo)
        {
            bool exigeLicenca = true, exigeVersao = true;

            Entid_Atributos? f = _dados?.Bloqueios.FirstOrDefault(b =>
                string.Equals(b.Nome, funcionalidade, StringComparison.OrdinalIgnoreCase));

            if (f != null)
            {
                exigeLicenca = f.Bool("exigeLicenca") ?? true;
                exigeVersao = f.Bool("exigeVersaoMinima") ?? true;
            }

            var motivos = new List<string>();
            if (exigeLicenca && !r.LicencaValida) motivos.Add(r.MotivoLicenca ?? MensagemInvalidaPadrao);
            if (exigeVersao && r.VersaoObrigatoriaPendente) motivos.Add(r.MotivoVersao ?? "Atualize o aplicativo para continuar.");

            motivo = string.Join(Environment.NewLine, motivos);
            return motivos.Count == 0;
        }

        // ── Utilidades ──────────────────────────────────────────────────────
        private static string Formatar(string? modelo, string padrao, ResultadoRegras r)
        {
            string s = string.IsNullOrWhiteSpace(modelo) ? padrao : modelo!;
            return s.Replace("{validade}", r.Validade?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "")
                    .Replace("{dias}", r.DiasRestantes?.ToString(CultureInfo.InvariantCulture) ?? "")
                    .Replace("{titular}", r.Titular ?? "")
                    .Replace("{atual}", r.VersaoAtual.ToString(3))
                    .Replace("{minima}", r.VersaoMinima?.ToString(3) ?? "")
                    .Replace("{ultima}", r.VersaoUltima?.ToString(3) ?? "");
        }

        private static Version? ParseVersao(string? texto) =>
            Version.TryParse(texto?.Trim(), out Version? v) ? Normalizar(v) : null;

        // Version("1.0").ToString(3) lanca excecao: garante sempre 4 componentes
        private static Version Normalizar(Version v) =>
            new(Math.Max(v.Major, 0), Math.Max(v.Minor, 0), Math.Max(v.Build, 0), Math.Max(v.Revision, 0));
    }
}
