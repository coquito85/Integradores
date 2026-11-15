# Beta JSON Integrator — Core e Data implementados conforme a especificação

## O que veio no pacote
| Projeto | Conteúdo |
|---|---|
| `BetaJsonIntegrator.Core` | Leitor XML (com proteção XXE), mapeamento/personalizações, gerador JSON, validação de layout e de JSON Schema, serviços (repositório de layouts, integrador, exportação, log). |
| `BetaJsonIntegrator.Data` | `IDataProvider` + provedores CSV, MariaDB, SQL Server e em memória. Sem pacotes NuGet. |
| `BetaJsonIntegrator.Tests` | xUnit: XML SATUBA, mapeamento, geração, conversões, Schema, CSV, SELECT parametrizado. |
| `Configurations/Layouts/SATUBA` | `layout.xml` = XML do modelo, **sem alteração**; `metadata.json`. |
| `GenerateJS` (App) | A UI já chamava esta API. Única mudança: configuração do banco em diálogo separado (abaixo). |

## Do modelo (GeneratorJS) para o novo desenho
| Modelo | Agora |
|---|---|
| `DefinirFileLayout` (switch fixo `NOV2025009`→SATUBA) | `LayoutRepository` + pasta por layout; `FindByIdentifier("NOV2025009")` |
| XML embutido como resource | `Configurations/Layouts/<layout>/layout.xml` (sem recompilar) |
| `ScraperXmlSelectQuery` (SQL com CAST/FORMAT por provider) | Provedores montam `SELECT` só com as colunas do XML, identificadores validados/entre aspas; formatos aplicados no Core |
| `TableSource` / `FiltersSource` (concatenados no SQL) | `RunOptions.TableSource` / `RunOptions.Filters` (filtros como **parâmetros**) |
| `ProcessarFieldEstrutura` (Newtonsoft, 1 linha) | `JsonGenerator` (System.Text.Json), arrays aninhados, tipos corretos |
| `ScraperXmlFielsValidate` (lança exceção) | `LayoutValidator` com lista de ocorrências e caminho |
| Campo sem origem vira `null` calado | Campo **pendente** → bloqueia / personalização em `customizations.xml` |
| Sem validação de saída | `JsonSchemaValidator` + relatório `validation-report.json` |

## Pontos de atenção
1. **Não foi compilado**: o ambiente de preparação não tem SDK .NET. Rode `dotnet build` e `dotnet test`; se algo não compilar, me envie a mensagem.
2. **`schema.json` não veio nos uploads.** Copie o Schema DOI para `Configurations/Layouts/SATUBA/schema.json`; sem ele a execução avisa "sem Schema".
3. O validador de Schema é próprio (subconjunto draft-07 usado pelo Schema DOI; o que não avalia é listado como Info). A spec prevê uma biblioteca: dá para trocar mantendo a assinatura de `JsonSchemaValidator.Validate`.
4. XML legado: `value="cpfcnpjAdquirente"` em `ni` é tratado como **coluna** (Info `LEGACY_VALUE_AS_COLUMN`); `formate="yyyy-mm-dd"` é lido como `yyyy-MM-dd` (Aviso `CFG_FORMAT_MM`, pois `mm` = minutos em .NET).
5. O layout SATUBA tem grafias que diferem só por maiúscula (`Participacao`/`participacao`, `indicadorRepresentante`/`IndicadorRepresentante`). O sistema avisa (`CFG_NAME_CASE`); confirme contra o Schema antes de produção. O XML não foi alterado.
6. Arrays aninhados geram um elemento por registro. Agrupar vários adquirentes/alienantes em uma declaração (chave de associação) é a evolução prevista na seção 8.
7. Decimais sem `culture`: o último separador é o decimal; `"1.234"` é lido como 1,234. Use `culture="pt-BR"` no campo para milhar.
8. A `.slnx` lista `BetaJsonIntegrator.Orm` e `BetaJsonIntegrator.Tools`, que não estão no .rar nem na especificação. Se existirem aí, mantenha; senão remova as duas linhas.
9. Core/Data usam `net8.0` (spec); o App continua `net10.0-windows`.

## Tela de Configurações (fonte de dados + geração)
A tela principal ficou só com as ações (**Gerar JSON, Ver JSON, Ver validação, Exportar**), um resumo da configuração e as mensagens. O botão **Configurações...** abre `frmConfiguracao` (PropertyGrid, como no GeneratorJS) com **Gravar**, **Cancelar** e **Testar origem**.

**Fonte de dados** (mostra só os campos da Origem escolhida):
| Origem | Campos |
|---|---|
| Arquivo CSV | Arquivo, Delimitador, Codificação, Primeira linha é o cabeçalho |
| Banco de dados | ConnectionString (mascarada, só em memória), ProviderName (texto livre: `MySqlConnector` ou `Microsoft.Data.SqlClient`; também aceita `MySql.Data.MySqlClient`, `System.Data.SqlClient`), TableSource, FiltersSource (`campo=valor,campo2=valor2`, como parâmetros SQL), CommandTimeout |

**Geração:** LayoutCode (ao gravar, valida se existe layout para o código), Configuração (lista os `<generate>` do layout encontrado), Gerar prévia com pendências, ArquivosDestino (vazio = pasta `Output`).

Ao gravar, as validações rodam e listam os erros (layout inexistente, arquivo CSV inexistente, conexão vazia, filtro inválido etc.). A mesma validação roda antes de gerar. Se o LayoutCode apontar outro layout cadastrado, ele é carregado. Mudar a configuração descarta o resultado anterior, para não exportar um JSON de outra origem.

Ficaram de fora `ColumnUpdateSource`, `LayoutValidate` e `Generate` do modelo: o documento não define atualização na origem, e a validação já é obrigatória no fluxo.
