using System.Security.Cryptography;
using System.Text;
using ReferenciaStandard.Entidades;
using ReferenciaStandard.Helpers;

namespace ReferenciaFactory.Banco;

public static class SecretBox
{
    private const string Prefix = "dpapi:";

    public static string Protect(string plain)
    {
        if (string.IsNullOrEmpty(plain) || plain.StartsWith(Prefix, StringComparison.Ordinal)) return plain;
        var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(plain), null, DataProtectionScope.CurrentUser);
        return Prefix + Convert.ToBase64String(bytes);
    }

    public static string Unprotect(string stored)
    {
        if (string.IsNullOrEmpty(stored) || !stored.StartsWith(Prefix, StringComparison.Ordinal)) return stored;
        try
        {
            var bytes = ProtectedData.Unprotect(Convert.FromBase64String(stored[Prefix.Length..]), null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (CryptographicException)
        {
            throw new GeneratorException("Nao foi possivel descriptografar a senha salva (outro usuario/maquina?). Edite a conexao e informe a senha novamente.");
        }
    }
}

/// <summary>Persistencia das conexoes em %AppData%. Segredos sao criptografados antes de gravar.</summary>
public sealed class ProfileStore
{
    private readonly string _file;
    private readonly object _lock = new();

    public ProfileStore(string folder)
    {
        Directory.CreateDirectory(folder);
        _file = Path.Combine(folder, "connections.json");
    }

    /// <summary>Perfis como gravados (segredos ainda criptografados).</summary>
    public List<ConnectionProfile> LoadStored()
    {
        lock (_lock)
        {
            return JsonHelper.LerArquivo(_file, new List<ConnectionProfile>());
        }
    }

    /// <summary>Perfil pronto para conectar (segredos descriptografados).</summary>
    public ConnectionProfile GetDecrypted(string id)
    {
        var p = LoadStored().FirstOrDefault(x => x.Id == id)
                ?? throw new GeneratorException("Conexao nao encontrada. Selecione ou cadastre uma conexao.");
        return Decrypt(p);
    }

    public static ConnectionProfile Decrypt(ConnectionProfile stored)
    {
        var p = stored.Clone();
        foreach (var key in p.Fields.Keys.Where(ProviderCatalog.IsSecretField).ToList())
            p.Fields[key] = SecretBox.Unprotect(p.Fields[key]);
        if (p.RawConnectionString is not null) p.RawConnectionString = SecretBox.Unprotect(p.RawConnectionString);
        return p;
    }

    /// <summary>Salva (cria ou atualiza). Segredo vazio em perfil existente = manter o anterior.</summary>
    public ConnectionProfile Save(ConnectionProfile incoming)
    {
        lock (_lock)
        {
            var all = LoadStored();
            var existing = all.FirstOrDefault(x => x.Id == incoming.Id);
            var p = incoming.Clone();
            if (string.IsNullOrWhiteSpace(p.Name)) throw new GeneratorException("Informe um nome para a conexao.");
            if (!ProviderCatalog.TryGet(p.ProviderKey, out _)) throw new GeneratorException("Provedor de banco invalido.");

            foreach (var key in p.Fields.Keys.Where(ProviderCatalog.IsSecretField).ToList())
            {
                if (string.IsNullOrEmpty(p.Fields[key]) && existing is not null && existing.Fields.TryGetValue(key, out var old))
                    p.Fields[key] = old;
                else
                    p.Fields[key] = SecretBox.Protect(p.Fields[key]);
            }
            if (existing is not null && existing.ProviderKey == p.ProviderKey)
                foreach (var kv in existing.Fields.Where(k => ProviderCatalog.IsSecretField(k.Key) && !p.Fields.ContainsKey(k.Key)))
                    p.Fields[kv.Key] = kv.Value;
            if (string.IsNullOrEmpty(p.RawConnectionString) && existing?.RawConnectionString is not null && p.Mode == "raw")
                p.RawConnectionString = existing.RawConnectionString;
            else if (p.RawConnectionString is not null)
                p.RawConnectionString = SecretBox.Protect(p.RawConnectionString);

            if (existing is null) all.Add(p); else all[all.IndexOf(existing)] = p;
            Gravar(all);
            return p;
        }
    }

    public void Delete(string id)
    {
        lock (_lock)
        {
            var all = LoadStored();
            all.RemoveAll(x => x.Id == id);
            Gravar(all);
        }
    }

    private void Gravar(List<ConnectionProfile> all)
    {
        if (!JsonHelper.GravarArquivo(_file, all, out var erro))
            throw new GeneratorException("Nao foi possivel gravar as conexoes: " + erro);
    }
}
