# Beta JSON Integrator

Solução (.sln) com:

| Projeto | Papel |
|---|---|
| `BetaJsonIntegrator.Core` (net8.0) | Parser XML, resolução de campos, geração JSON, validação (Schema + layout), repositório de layouts, exportação, log. Sem dependência de UI nem de pacotes NuGet. |
| `BetaJsonIntegrator.Data` (net8.0) | Provedores: `DataTableProvider` (memória), `CsvProvider`, `MariaDbProvider`, `SqlServerProvider`. Recebem um `DbProviderFactory`. |
| `BetaJsonIntegrator.App` (net10.0-windows) | App WinForms: `frmPrincipal`, `frmLayouts`, `frmConfiguracaoXML`, `frmVisualizadorJSON`, `frmValidacao`. |
| `ReferenciaFactory` | `DatabaseConnection` original (usado em "Testar conexão"). |
| `BetaJsonIntegrator.Tests` | Teste de ponta a ponta sem banco (marco 15.1 da especificação). Substitui o antigo SmokeTest.. |

Teste rápido: `dotnet run --project BetaJsonIntegrator.Tests -- <caminho>\Configurations\Layouts`

## Regras de resolução de campo
1. `value` literal -> 2. `source` / `Columnrefe` -> 3. `default` -> 4. pendente (nunca inventa valor).
* Legado: `value="NomeDaColuna"` sem `source` é reconhecido como coluna (aviso informativo).
* Personalizações (`customizations.xml`) preenchem campos pendentes; `override="true"` substitui também campos já configurados.
* Extensões opcionais de XML: `required`, `configurable`, `default`, `source`, `empty="true"`,
  `groupBy="Col1,Col2"` (array de topo agrupa registros), `alias`+`key`+`parentKey` (array vindo de outra tabela).

## Pastas de execução
`Configurations/Layouts/<Pasta>/{layout.xml, schema.json, customizations.xml, metadata.json, history/vN}`,
`Output/<Pasta>/<Ano>/{declaracoes.json, validation-report.json}`, `Logs/integrator-AAAA-MM-DD.log`.

## Atenção
* `layout.xml` e `schema.json` de SATUBA incluídos são EXEMPLOS baseados na especificação.
  Substitua pelo XML integral e pelo JSON Schema oficial da DOI.
* O validador de Schema embutido cobre os recursos usuais (ver `JsonSchemaValidator.cs`);
  confirme a conformidade com o draft oficial antes de produção.

## Estrutura alinhada ao GenerateBetaAnalytics.slnx
* As telas agora estão em `BetaJsonIntegrator.App` (namespace `BetaJsonIntegrator.App` / `.Forms`).
  Se você já copiou as telas para `GenerateJS`, remova-as de lá para não duplicar.
* `BetaJsonIntegrator.Tests` é um executável de console (sem xunit). Se o seu Tests já usa xunit/MSTest,
  aproveite os cenários de `Program.cs` e mantenha o seu `.csproj`.
* `BetaJsonIntegrator.Orm` e `BetaJsonIntegrator.Tools` não foram alterados (não fazem parte desta entrega).
