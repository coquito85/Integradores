using ReferenciaStandard.Helpers;

namespace ReferenciaFactory.Layouts
{
    /// <summary>Resultado da conferência da licença do cartório.</summary>
    public sealed record LicenseStatus(bool Valid, bool Bypass, string Message, CartorioLicense? License)
    {
        /// <summary>Layouts liberados para uso: os da licença válida (ou todos, só em modo de desenvolvimento).</summary>
        public bool Permite(string? layoutId) => Valid ? License!.TemLayout(layoutId) : Bypass;
    }

    /// <summary>
    /// Guarda e confere a licença do cartório (cartorio.lic) na pasta de dados do aplicativo.
    /// Hoje a fonte é o arquivo; quando o banco do aplicativo existir, troque só esta classe
    /// (ex.: DbCartorioStore) e mantenha o contrato (Status / Reload / Import).
    /// </summary>
    public sealed class CartorioStore
    {
        /// <summary>
        /// Chave PÚBLICA do fornecedor (gerada pela LicenseTool: comando "chaves"). Cole aqui o conteúdo de public.xml.
        /// Enquanto estiver como "COLE_AQUI", nenhuma licença é aceita.
        /// </summary>
        private const string PublicKeyXml = "COLE_AQUI";

        public const string FileName = "cartorio.lic";

        private readonly string _file;
        private readonly object _lock = new();
        private LicenseStatus _status = new(false, false, "", null);

        public CartorioStore(string folder)
        {
            Directory.CreateDirectory(folder);
            _file = Path.Combine(folder, FileName);
            Reload();
        }

        public LicenseStatus Status { get { lock (_lock) return _status; } }

        public LicenseStatus Reload()
        {
            var s = Evaluate(_file);
#if DEBUG
            // Só em desenvolvimento: sem licença válida, libera todos os layouts para você poder trabalhar.
            if (!s.Valid) s = s with { Bypass = true, Message = s.Message + " (modo desenvolvimento: licença não exigida)" };
#endif
            lock (_lock) _status = s;
            return s;
        }

        /// <summary>Confere o arquivo escolhido e só então o copia para a pasta de dados.</summary>
        public LicenseStatus Import(string sourceFile)
        {
            var s = Evaluate(sourceFile);
            if (!s.Valid) throw new GeneratorException(s.Message);
            File.Copy(sourceFile, _file, overwrite: true);
            return Reload();
        }

        private static LicenseStatus Evaluate(string path)
        {
            if (!File.Exists(path))
                return new(false, false, "Nenhuma licença do cartório importada.", null);

            CartorioLicense lic;
            try { lic = CartorioLicense.Read(path); }
            catch (Exception ex) { return new(false, false, "Arquivo de licença ilegível: " + ex.Message, null); }

            if (PublicKeyXml == "COLE_AQUI")
                return new(false, false, "Chave pública do fornecedor não configurada no aplicativo (CartorioStore.PublicKeyXml).", lic);

            if (!lic.Verify(PublicKeyXml))
                return new(false, false, "Licença inválida: a assinatura não confere (arquivo alterado ou de outro emissor).", lic);

            var validade = lic.ValidadeData();
            if (!string.IsNullOrWhiteSpace(lic.Validade) && validade is null)
                return new(false, false, "Licença inválida: data de validade ilegível.", lic);
            if (validade is { } v && DateTime.Today > v)
                return new(false, false, $"Licença vencida em {v:dd/MM/yyyy}. Peça uma renovação ao fornecedor.", lic);

            return new(true, false, "Licença válida.", lic);
        }
    }
}