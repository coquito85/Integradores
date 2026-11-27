
using System;
using System.Collections.Generic;

namespace ReferenciaStandard.Entidades
{
    /// <summary>Conexao salva: provedor + campos (ou string completa). Senhas ficam criptografadas (DPAPI, usuario atual).</summary>
    public sealed class ConnectionProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "";
        public string ProviderKey { get; set; } = "";
        /// <summary>"fields" (formulario) ou "raw" (string de conexao completa).</summary>
        public string Mode { get; set; } = "fields";
        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();
        public string RawConnectionString { get; set; } = "";

        public ConnectionProfile Clone()
        {
            return new ConnectionProfile
            {
                Id = Id,
                Name = Name,
                ProviderKey = ProviderKey,
                Mode = Mode,
                Fields = new Dictionary<string, string>(Fields),
                RawConnectionString = RawConnectionString
            };
        }
    }
}
