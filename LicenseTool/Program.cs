using System.Security.Cryptography;
using ReferenciaFactory.Layouts;

// Ferramenta do FORNECEDOR (não distribuir ao cliente). Guarde private.xml em local seguro e fora do Git.
//
//   dotnet run -- chaves
//   dotnet run -- emitir --priv private.xml --out cartorio.lic --id SATUBA --nome "Cartório de Satuba"
//        --cns 00.000-0 --ibge 2704302 --resp "Nome do Tabelião" --cargo Tabelião --email x@y.com
//        --validade 2027-11-27 --layouts NOV2025009,OUTRO_MODELO
//   dotnet run -- conferir --pub public.xml --file cartorio.lic

if (args.Length == 0) { Uso(); return 1; }

var cmd = args[0].ToLowerInvariant();
var o = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
for (int i = 1; i < args.Length - 1; i += 2)
    if (args[i].StartsWith("--")) o[args[i][2..]] = args[i + 1];

string Opt(string k, string def = "") => o.TryGetValue(k, out var v) ? v : def;
string Need(string k) => o.TryGetValue(k, out var v) && v.Length > 0 ? v : throw new ArgumentException($"Falta --{k}");

try
{
    switch (cmd)
    {
        case "chaves":
            {
                using var rsa = RSA.Create(3072);
                File.WriteAllText("private.xml", rsa.ToXmlString(true));
                File.WriteAllText("public.xml", rsa.ToXmlString(false));
                Console.WriteLine("Gerados private.xml (SECRETO: só você guarda) e public.xml.");
                Console.WriteLine("Cole o conteúdo de public.xml em CartorioStore.PublicKeyXml (entre aspas triplas \"\"\" ... \"\"\").");
                return 0;
            }
        case "emitir":
            {
                var lic = new CartorioLicense
                {
                    CartorioId = Need("id"),
                    Nome = Need("nome"),
                    Cns = Opt("cns"),
                    CodigoIbge = Opt("ibge"),
                    ResponsavelNome = Need("resp"),
                    ResponsavelCargo = Opt("cargo"),
                    ResponsavelEmail = Opt("email"),
                    EmitidoEm = DateTime.Today.ToString("yyyy-MM-dd"),
                    Validade = Opt("validade"),
                    Layouts = Need("layouts").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                };
                lic.Sign(File.ReadAllText(Need("priv")));
                var saida = Opt("out", "cartorio.lic");
                lic.Write(saida);
                Console.WriteLine($"Licença emitida: {saida} ({lic.Layouts.Count} layout(s), validade {(lic.Validade == "" ? "sem validade" : lic.Validade)}).");
                return 0;
            }
        case "conferir":
            {
                var lic = CartorioLicense.Read(Need("file"));
                var ok = lic.Verify(File.ReadAllText(Need("pub")));
                Console.WriteLine(ok ? "Assinatura OK." : "ASSINATURA INVÁLIDA.");
                return ok ? 0 : 2;
            }
        default: Uso(); return 1;
    }
}
catch (Exception ex) { Console.Error.WriteLine("Erro: " + ex.Message); return 1; }

static void Uso() => Console.WriteLine("Comandos: chaves | emitir --priv --out --id --nome --resp --layouts [--cns --ibge --cargo --email --validade] | conferir --pub --file");