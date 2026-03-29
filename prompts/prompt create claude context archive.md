# Prompt: Full System Audit → Update CLAUDE.md

Use este prompt no início de uma sessão dedicada ao mapeamento. Cole diretamente no Claude Code CLI.

---

## INSTRUÇÃO PRINCIPAL

Você vai realizar uma auditoria completa do projeto e atualizar o arquivo `CLAUDE.md` com contexto denso e estruturado. O objetivo é que futuras sessões consumam menos tokens — o CLAUDE.md será a fonte de verdade que substituirá exploração repetitiva da codebase.

**Não interrompa para perguntar. Execute tudo em sequência. Ao final, escreva o CLAUDE.md.**

---

## FASE 1 — Mapeamento de estrutura

Execute os seguintes comandos e armazene os resultados mentalmente:

```bash
# Estrutura geral do projeto
find . -type f \( -name "*.cs" -o -name "*.razor" -o -name "*.json" -o -name "*.csproj" \) \
  | grep -v "bin/" | grep -v "obj/" | grep -v ".git/" \
  | sort > /tmp/audit_files.txt && cat /tmp/audit_files.txt

# Projetos e dependências
find . -name "*.csproj" | xargs grep -l "" | head -20
cat $(find . -name "*.csproj" | head -5)

# Migrations existentes
find . -path "*/Migrations/*.cs" -name "*.cs" | sort

# Pacotes NuGet usados
find . -name "*.csproj" | xargs grep -h "PackageReference" | sort -u
```

---

## FASE 2 — Mapeamento de modelos e banco de dados

```bash
# Entidades/modelos de domínio
find . -path "*/Models/*.cs" -o -path "*/Entities/*.cs" -o -path "*/Domain/*.cs" \
  | grep -v "bin/" | grep -v "obj/" | xargs grep -l "class " 2>/dev/null

# DbContext(s)
find . -name "*Context*.cs" | grep -v "bin/" | grep -v "obj/"
cat $(find . -name "*Context*.cs" | grep -v "bin/" | grep -v "obj/" | head -3)

# Enums relevantes
find . -name "*.cs" | grep -v "bin/" | grep -v "obj/" | xargs grep -l "^public enum" 2>/dev/null
```

Leia os arquivos de modelos encontrados, identificando:
- Propriedades de cada entidade
- Relacionamentos (FKs, navegação)
- Colunas JSON (se houver)
- Índices ou constraints relevantes

---

## FASE 3 — Mapeamento de serviços e lógica de negócio

```bash
# Serviços
find . -path "*/Services/*.cs" | grep -v "bin/" | grep -v "obj/" | sort

# Interfaces
find . -path "*/Interfaces/*.cs" | grep -v "bin/" | grep -v "obj/" | sort

# Injeção de dependência (registros)
find . -name "Program.cs" -o -name "Startup.cs" | grep -v "bin/" | grep -v "obj/"
cat $(find . -name "Program.cs" | grep -v "bin/" | grep -v "obj/" | head -1)
```

Leia cada serviço e anote:
- Responsabilidade principal (1 frase)
- Métodos públicos principais
- Dependências injetadas
- Regras de negócio não óbvias

---

## FASE 4 — Mapeamento de UI (Blazor)

```bash
# Páginas Blazor
find . -name "*.razor" | grep -v "bin/" | grep -v "obj/" | sort

# Componentes compartilhados
find . -path "*/Shared/*.razor" | grep -v "bin/" | grep -v "obj/"

# Layouts
find . -name "*Layout*.razor" | grep -v "bin/" | grep -v "obj/"

# Routes configuradas
grep -r "@page" $(find . -name "*.razor" | grep -v "bin/" | grep -v "obj/") \
  | sed 's/.*@page //' | sort
```

Para cada página, anote: rota, propósito, componentes principais usados.

---

## FASE 5 — Mapeamento de configuração e infraestrutura

```bash
# Configurações
cat appsettings.json 2>/dev/null || find . -name "appsettings*.json" | head -5 | xargs cat

# Variáveis de ambiente usadas no código
grep -r "Environment.GetEnvironmentVariable\|IConfiguration\|GetValue\|GetConnectionString" \
  $(find . -name "*.cs" | grep -v "bin/" | grep -v "obj/") \
  | grep -v "//.*:" | head -40

# Arquivos de deploy/infra
find . -name "Dockerfile" -o -name "docker-compose*" -o -name "railway.toml" \
  -o -name ".github" -type d 2>/dev/null | head -10
cat $(find . -name "Dockerfile" | head -1) 2>/dev/null
cat $(find . -name "railway.toml" -o -name "railway.json" | head -1) 2>/dev/null
```

---

## FASE 6 — Mapeamento de autenticação e autorização

```bash
# Auth
grep -r "AddAuthentication\|AddIdentity\|AddAuthorization\|GoogleAuth\|JwtBearer" \
  $(find . -name "*.cs" | grep -v "bin/" | grep -v "obj/") | head -20

# Atributos de autorização
grep -rn "\[Authorize\]\|\[AllowAnonymous\]" \
  $(find . -name "*.razor" -o -name "*.cs" | grep -v "bin/" | grep -v "obj/") | head -30

# Roles/Policies definidos
grep -r "AddPolicy\|RequireRole\|RequireClaim" \
  $(find . -name "*.cs" | grep -v "bin/" | grep -v "obj/") | head -20
```

---

## FASE 7 — Mapeamento de cache, background jobs e integrações externas

```bash
# Redis / IMemoryCache
grep -rn "IMemoryCache\|IDistributedCache\|StackExchange.Redis\|AddRedis\|AddStackExchangeRedis" \
  $(find . -name "*.cs" | grep -v "bin/" | grep -v "obj/") | head -20

# Background services
find . -name "*.cs" | grep -v "bin/" | grep -v "obj/" \
  | xargs grep -l "BackgroundService\|IHostedService" 2>/dev/null

# Chamadas HTTP externas
grep -rn "HttpClient\|IHttpClientFactory\|AddHttpClient" \
  $(find . -name "*.cs" | grep -v "bin/" | grep -v "obj/") | head -20
```

---

## FASE 8 — Identificar dívidas técnicas e TODOs

```bash
# TODOs e FIXMEs
grep -rn "TODO\|FIXME\|HACK\|XXX\|TEMP\|BUG" \
  $(find . -name "*.cs" -o -name "*.razor" | grep -v "bin/" | grep -v "obj/") \
  | grep -v "//.*\*" | head -50

# Erros conhecidos / exception handling custom
grep -rn "catch\|throw\|Exception" \
  $(find . -name "*.cs" | grep -v "bin/" | grep -v "obj/") \
  | grep "throw new\|catch (Exception\|catch (var" | head -30
```

---

## FASE 9 — Estado atual de features e fluxos principais

Com base em tudo que você leu, responda internamente:

1. **Quais features estão 100% implementadas?**
2. **Quais estão parcialmente implementadas (stub, mock, incompleto)?**
3. **Quais são mencionadas em TODO mas não existem no código?**
4. **Quais fluxos end-to-end estão funcionais** (ex: cadastro → criação de link → visualização pública)?

---

## FASE 10 — ESCREVER O CLAUDE.md

Agora, com todas as informações coletadas, **reescreva completamente o arquivo `CLAUDE.md`** usando a estrutura abaixo. Seja denso e técnico — este arquivo é para consumo de IA, não humano. Prefira listas compactas a prosa. Cada linha deve carregar informação real, não genérica.

```markdown
# CLAUDE.md — Umblink System Context
> Gerado em: [DATA]
> Stack: .NET 8 / Blazor Server / EF Core / SQLite / Railway

---

## 1. Estrutura de projetos

[Liste cada projeto .csproj com sua responsabilidade em 1 linha]
[Ex: Umblink.Web — Blazor Server app, entry point, UI + controllers]

---

## 2. Domínio — Entidades principais

### [NomeEntidade]
- Tabela: `nome_tabela`
- Campos-chave: id (Guid), campo1 (tipo), campo2 (tipo)
- JSON columns: nomeColuna → tipo desserializado
- Relacionamentos: FK para X, navegação para Y (1:N)
- Regras: [regras de negócio críticas]

[Repita para cada entidade]

---

## 3. DbContext

- Arquivo: `Caminho/NomeContext.cs`
- DbSets: [lista compacta]
- Configurações especiais: [índices, value conversions, seed data]
- Connection string key: `[chave no appsettings]`
- Provider: SQLite (dev) / [outro em prod?]

---

## 4. Serviços — Responsabilidades

| Serviço | Responsabilidade | Dependências-chave |
|---------|-----------------|-------------------|
| NomeServiço | Faz X | IRepo, ICache |
| ... | ... | ... |

### Regras de negócio não-óbvias
- [Regra 1: onde está, o que faz]
- [Regra 2: ...]

---

## 5. Injeção de dependência (Program.cs)

[Apenas registros não-triviais ou custom. Ignore AddRazorComponents, AddBlazor padrão.]
- `AddScoped<INomeServiço, NomeServiço>()`
- `AddSingleton<...>` — motivo se relevante
- Pipeline de middleware (ordem importa): [lista]

---

## 6. Páginas e rotas Blazor

| Rota | Arquivo .razor | Propósito |
|------|---------------|-----------|
| `/` | Index.razor | Landing page pública |
| `/dashboard` | Dashboard.razor | Painel do usuário logado |
| ... | ... | ... |

### Componentes compartilhados importantes
- `NomeComponente.razor` — [o que faz, onde é usado]

---

## 7. Autenticação e autorização

- Provider: [Identity / JWT / Cookie / OAuth]
- Google OAuth: [configurado? campos necessários no appsettings?]
- Política padrão: [AllowAnonymous global ou Authorize global?]
- Roles existentes: [lista]
- Páginas que requerem auth: [lista de rotas]

---

## 8. Cache

- Tipo: [IMemoryCache / Redis / nenhum]
- Keys usadas: [lista de cache keys existentes]
- TTLs configurados: [onde, quanto]
- Serviços que usam cache: [lista]

---

## 9. Configuração e variáveis de ambiente

### appsettings.json — chaves relevantes
```json
{
  "ConnectionStrings": { "Default": "..." },
  "Google": { "ClientId": "", "ClientSecret": "" },
  "Stripe": { "PublicKey": "", "SecretKey": "" }
}
```

### Variáveis obrigatórias em produção (Railway)
- `VARNAME` — [descrição]
- ...

---

## 10. Deploy e infra

- Plataforma: Railway
- Build: [Dockerfile? nixpacks?]
- Porta: [configurada onde?]
- Banco em prod: [SQLite no volume / migrado para Postgres?]
- Migrações: aplicadas [automaticamente no startup / manualmente]

---

## 11. Features — Estado atual

### ✅ Implementadas e funcionais
- [Feature 1]
- [Feature 2]

### 🔶 Parcialmente implementadas
- [Feature X] — falta: [o que está faltando]
- [Feature Y] — mock em: [arquivo/método]

### ❌ Planejadas / não iniciadas
- [Feature Z] — mencionada em: [arquivo/TODO]

---

## 12. Fluxos end-to-end funcionais

### Fluxo: [Nome do fluxo]
1. Usuário acessa `[rota]`
2. `[Serviço]` chama `[método]`
3. [Próximo passo]
4. Resultado: [o que acontece]

[Repita para cada fluxo principal]

---

## 13. Dívidas técnicas e TODOs ativos

- `[Arquivo:linha]` — [descrição do TODO]
- [Padrão ruim conhecido]: [onde está, impacto]

---

## 14. Convenções do projeto

- Nomenclatura: [PascalCase para X, camelCase para Y, etc]
- Padrão de erros: [como erros são tratados e retornados]
- Padrão de validação: [DataAnnotations / FluentValidation / custom]
- CSS/estilo: [Bootstrap sendo removido, GSAP sendo adotado, classes custom em ...]
- Commits: [convenção se houver]

---

## 15. Guia de tarefas comuns (para sessões futuras)

### Adicionar nova entidade
1. Criar model em `[caminho]`
2. Adicionar DbSet em `[NomeContext]`
3. Criar migration: `dotnet ef migrations add NomeMigration`
4. Criar serviço em `[caminho]` implementando `[interface padrão]`
5. Registrar em `Program.cs`

### Adicionar nova página Blazor
1. Criar `[NomePagina].razor` em `[caminho]`
2. Adicionar `@page "/rota"`
3. Injetar serviços necessários com `@inject`
4. Adicionar link no `[NaveBar/Menu component]` se necessário

### Rodar localmente
```bash
[comandos exatos para rodar o projeto]
```

### Aplicar migrations
```bash
dotnet ef database update --project [caminho]
```
```

---

## INSTRUÇÕES FINAIS

1. Preencha **todas** as seções com dados reais do código — nunca deixe `[placeholder]` vazio
2. Se uma seção não se aplica, escreva `N/A — [motivo em 1 linha]`
3. Priorize **precisão** sobre completude — melhor omitir algo do que escrever errado
4. O arquivo final deve ter entre **200 e 500 linhas** — denso mas não redundante
5. Após escrever o CLAUDE.md, execute `wc -l CLAUDE.md` e confirme o tamanho
6. Informe ao usuário: total de arquivos analisados, principais descobertas, e qualquer inconsistência grave encontrada