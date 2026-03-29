# Umblink — Sprint de melhorias e nova feature de IA

## Contexto do projeto

Stack: .NET 8 + Blazor Server, EF Core, SQLite, Railway.  
Padrão atual: serviços em `Services/`, entidades em `Models/`, DbContext direto nos serviços.  
Rotas relevantes: `/onboarding`, `/editor/{pageId}`, `/p/{slug}`, `/r/{linkId}`, `/auth/login`.

---

## 1. Refatoração de arquitetura — camada de repositório

**Objetivo:** mover toda manipulação de dados para uma camada de repositório (`Infrastructure/Repositories/`), deixando os serviços responsáveis apenas por validações, regras de negócio e orquestração.

### O que fazer

1. Criar pasta `Infrastructure/Repositories/` com interfaces e implementações:
   - `IPageRepository` — CRUD de páginas, busca por slug, listagem por usuário
   - `ILinkRepository` — CRUD de links, reordenação, busca por página
   - `IUserRepository` — busca por ID/email, atualização de perfil
   - `ISubscriptionRepository` — plano atual, histórico de assinatura
   - `IAnalyticsRepository` — inserção de views/clicks, agregações por período
   - `IAuditLogRepository` — inserção de logs de auditoria

2. Cada repositório recebe `AppDbContext` via injeção. Nenhuma lógica de negócio aqui — apenas queries e persistência.

3. Refatorar os serviços existentes:
   - Remover toda chamada direta a `_context.SaveChanges()` e queries LINQ dos serviços
   - Substituir por chamadas aos repositórios
   - Manter nos serviços: validações, cálculo de limites por plano, lógica de upgrade/downgrade, disparo de eventos

4. Registrar todos os repositórios no `Program.cs` com `AddScoped<IXRepository, XRepository>()`.

5. Garantir que os repositórios não retornam `IQueryable` — sempre `Task<T>` ou `Task<IEnumerable<T>>` para não vazar o ORM para cima.

### Exemplo de contrato esperado

```csharp
public interface IPageRepository
{
    Task<Page?> GetByIdAsync(Guid id);
    Task<Page?> GetBySlugAsync(string slug);
    Task<IEnumerable<Page>> GetByUserIdAsync(string userId);
    Task<Page> CreateAsync(Page page);
    Task UpdateAsync(Page page);
    Task DeleteAsync(Guid id);
    Task<bool> SlugExistsAsync(string slug, Guid? excludePageId = null);
}
```

---

## 2. Limites de caracteres nos campos de texto

**Objetivo:** impedir entradas excessivamente longas que possam causar problemas de layout na página pública e de armazenamento.

### Limites definidos

| Campo | Limite |
|---|---|
| Título da página | 60 caracteres |
| Bio / descrição | 200 caracteres |
| Título de um link | 80 caracteres |
| URL de um link | 2048 caracteres |
| Slug | 30 caracteres |
| Nome de usuário / display name | 50 caracteres |

### O que fazer

1. Adicionar atributos de validação nas entidades (`[MaxLength(60)]`) para gerar constraints no banco via EF.
2. Criar migration para aplicar os constraints existentes.
3. Nos componentes Blazor do editor (`Editor.razor`, `OnboardingStep3.razor` e similares):
   - Adicionar `maxlength` nos inputs HTML
   - Exibir contador de caracteres em tempo real: `"42 / 200"` alinhado à direita abaixo do campo
   - Estilo do contador: texto muted enquanto abaixo do limite, vermelho quando ≥ 90% do limite
4. No serviço, validar tamanho antes de persistir e retornar erro estruturado se exceder (defesa dupla).

---

## 3. Validação e allowlist de URLs de links

**Objetivo:** evitar que usuários adicionem links maliciosos (phishing, malware, javascript:, data:, etc.) e dar segurança a quem clica.

### Validações obrigatórias (backend — `LinkService` ou `ILinkRepository` via serviço)

1. Parsear a URL com `Uri.TryCreate()` — rejeitar qualquer coisa que não seja URI válida.
2. Aceitar apenas esquemas `https://` e `http://`. Bloquear: `javascript:`, `data:`, `file:`, `ftp:`, `vbscript:` e qualquer outro.
3. Bloquear domínios internos: `localhost`, `127.0.0.1`, faixas `192.168.x.x`, `10.x.x.x`, `172.16-31.x.x`.
4. Rejeitar URLs com credenciais embutidas (`user:pass@dominio.com`).
5. Comprimento máximo: 2048 caracteres.

### Allowlist de domínios populares (UX — no editor)

Criar componente `LinkSuggestionsPanel` que exibe atalhos de plataformas comuns para preencher a URL automaticamente. Quando o usuário clica num atalho, preenche o campo URL com o prefixo da plataforma e move o foco para o campo de título.

Plataformas a incluir (com ícone SVG simples ou texto):

```
Instagram, TikTok, YouTube, Twitter/X, LinkedIn, Facebook,
WhatsApp, Telegram, GitHub, Spotify, Pinterest, Twitch,
Behance, Dribbble, Medium, Substack, Discord, E-mail (mailto:)
```

O campo URL ainda aceita qualquer URL válida — a allowlist é apenas atalho de UX, não restrição.

### Aviso ao visitante no redirect

No endpoint `/r/{linkId}` (antes do redirect), adicionar cabeçalho `Referrer-Policy: no-referrer` e, se o domínio não estiver na lista de plataformas conhecidas, exibir uma tela de aviso intermediária simples:

```
"Você está sendo redirecionado para: [domínio]
Este link foi adicionado por um usuário do Umblink.
[Continuar] [Cancelar]"
```

Plataformas da lista conhecida fazem redirect direto sem tela de aviso.

---

## 4. Upload de avatar e temas com foto de fundo

### 4a. Upload de avatar próprio

**Objetivo:** além da URL de avatar, permitir upload direto de imagem.

1. Criar endpoint `POST /api/upload/avatar` que:
   - Aceita `multipart/form-data` com o arquivo
   - Valida: tamanho máximo 2 MB, tipos aceitos `image/jpeg`, `image/png`, `image/webp`
   - Redimensiona para 400×400px mantendo aspect ratio (usar `SixLabors.ImageSharp`)
   - Salva em `wwwroot/uploads/avatars/{userId}_{timestamp}.webp`
   - Retorna a URL pública do arquivo

2. No editor, adicionar dois modos no campo de avatar:
   - "URL externa" (comportamento atual)
   - "Fazer upload" — botão que abre file picker, faz POST para o endpoint acima, preenche o campo com a URL retornada

3. Limitar 1 avatar por usuário — ao fazer novo upload, deletar o arquivo anterior.

### 4b. Temas com foto de fundo

**Objetivo:** além de cores sólidas e gradientes, permitir fundos com imagem.

1. Adicionar campo `BackgroundImageUrl` (nullable) em `ThemeConfig` (JSON column na tabela de páginas).

2. Na página pública `/p/{slug}`, se `BackgroundImageUrl` preenchido:
   - Aplicar como `background-image` com `background-size: cover`, `background-position: center`
   - Adicionar overlay semitransparente para garantir legibilidade dos links

3. No editor, nova aba ou seção "Fundo":
   - Opção 1: cor sólida (atual)
   - Opção 2: gradiente (atual)
   - Opção 3: foto de fundo — sub-opções:
     - Galeria de fotos curadas (unsplash URLs hardcoded, ~12 opções por categoria: natureza, abstrato, cidade, minimalista)
     - Upload próprio (mesmo endpoint de avatar, pasta `backgrounds/`)

4. No preview ao vivo do editor, refletir a imagem de fundo em tempo real.

---

## 5. Correção do redirect de links

**Problema:** ao clicar num link, a URL montada inclui o host do ambiente atual (ex: `https://localhost:7190/r/{linkId}`), em vez de sempre usar o domínio de produção ou o domínio configurado.

**Causa provável:** a URL de redirect está sendo montada com `NavigationManager.BaseUri` ou `HttpContext.Request.Host` no lugar errado.

### O que fazer

1. No endpoint `/r/{linkId}` (controller ou minimal API), o redirect deve ser feito diretamente para a `Url` do link — sem montar URL intermediária:
   ```csharp
   return Results.Redirect(link.Url, permanent: false);
   ```

2. Se em algum lugar do código a URL de clique está sendo gerada para exibição (ex: no editor ou no dashboard de métricas), usar uma configuração centralizada `AppSettings:PublicBaseUrl` ao invés de `Request.Host`. Definir essa variável no Railway com o valor `https://umblink.com`.

3. Verificar e corrigir todos os lugares onde `NavigationManager.BaseUri` ou `Request.Host` é concatenado com `/r/` — substituir por `publicBaseUrl` vindo de `IConfiguration`.

---

## 6. Correção do layout do formulário de login

**Problema:** no formulário `/auth/login`, o label "Email" aparece dentro da linha do input (como placeholder sobreposto ou inline), enquanto o label "Senha" segue o padrão correto de label acima + input abaixo.

### O que fazer

Padronizar todos os campos do formulário de login (e de registro, se o mesmo problema existir) para o padrão:

```html
<div class="mb-3">
    <label for="email" class="form-label">Email</label>
    <input type="email" id="email" class="form-control" @bind="model.Email" placeholder="seu@email.com" />
</div>
```

Garantir que nenhum campo usa `placeholder` no lugar do `label` como texto visível principal. O `placeholder` deve ser apenas exemplo/dica, nunca o único identificador do campo.

Aplicar o mesmo padrão na tela de registro e recuperação de senha.

---

## 7. Feature de IA no onboarding — gerador assistido (opcional)

**Objetivo:** no onboarding, na etapa de definição de título/bio/slug (passo 3), oferecer um botão "Gerar com IA" que é opcional — o usuário pode preencher manualmente se preferir.

### UX esperada

```
Etapa 3 de 4 — Sobre você

[Campo: Título da sua página]         ← preenchível manualmente
[Campo: Bio / descrição]              ← preenchível manualmente
[Campo: Slug]                         ← preenchível manualmente

──────────────────────────────────────
  Quer uma ajuda? Descreva quem você é em uma frase:
  [______________________________________________]  ← textarea livre
  [Gerar com IA]
──────────────────────────────────────

[Continuar →]
```

Quando o usuário clica em "Gerar com IA":
1. Exibir loading spinner no lugar do botão
2. Chamar endpoint backend `POST /api/ai/generate-profile`
3. Preencher os campos Título, Bio e Slug com o resultado (editável após)
4. Exibir mensagem sutil: "Gerado com IA — edite à vontade antes de continuar"

### Backend — endpoint `POST /api/ai/generate-profile`

```csharp
// Request
public record GenerateProfileRequest(string UserDescription, string ProfileType);

// Response  
public record GenerateProfileResponse(string Title, string Bio, string SlugSuggestion);
```

**Integração com Groq (free tier — sem necessidade de cartão):**

1. Adicionar `Groq:ApiKey` em `appsettings.json` / variável de ambiente no Railway
2. Criar `GroqService` que faz HTTP POST para `https://api.groq.com/openai/v1/chat/completions`
3. Modelo recomendado: `llama-3.3-70b-versatile` (rápido e gratuito)

```csharp
public class GroqService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public async Task<GenerateProfileResponse> GenerateProfileAsync(string userDescription, string profileType)
    {
        var prompt = $"""
            O usuário quer criar uma página de links estilo Linktree.
            Tipo de perfil: {profileType}
            Descrição do usuário: {userDescription}
            
            Retorne APENAS um JSON válido, sem markdown, sem explicação:
            {{
              "title": "nome/título da página (máx 60 chars, pode ser nome + profissão)",
              "bio": "bio curta e engajante (máx 200 chars, primeira pessoa)",
              "slug": "slug-sem-espacos-e-sem-acentos (máx 30 chars, lowercase, hifens)"
            }}
            """;

        var body = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 200,
            temperature = 0.7
        };

        _http.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _http.PostAsJsonAsync(
            "https://api.groq.com/openai/v1/chat/completions", body);

        var result = await response.Content.ReadFromJsonAsync<GroqResponse>();
        var json = result.Choices[0].Message.Content;
        return JsonSerializer.Deserialize<GenerateProfileResponse>(json)!;
    }
}
```

4. Registrar `GroqService` como `AddHttpClient<GroqService>()` no `Program.cs`
5. Verificar se o slug sugerido está disponível — se não, appender número aleatório de 3 dígitos
6. Timeout de 10 segundos na chamada — se falhar, retornar erro 503 e a UI exibe "Não foi possível gerar, tente preencher manualmente"

### Registro no Groq (para o time)

1. Acesse `console.groq.com`
2. Crie conta gratuita (sem cartão de crédito)
3. Vá em API Keys → Create API Key
4. Adicione como variável de ambiente `GROQ__APIKEY` no Railway

### Segurança

- Rate limit por usuário: máximo 3 chamadas de IA por sessão de onboarding (evitar abuso)
- Sanitizar a resposta da IA antes de usar — nunca confiar diretamente no JSON sem validar tamanhos
- Logar uso no `AuditLog` com tipo `AiProfileGenerated`

---

## Ordem de execução sugerida

1. **Arquitetura** (item 1) — fazer primeiro pois afeta tudo
2. **Correções rápidas** (itens 5 e 6) — 30 minutos cada, fechar logo
3. **Limites de caracteres** (item 2) — simples, alta segurança
4. **Validação de URLs** (item 3) — importante para segurança da plataforma
5. **Upload de avatar + temas com foto** (item 4) — mais trabalhoso, deixar pra quando a base estiver sólida
6. **Feature de IA** (item 7) — implementar por último, com a arquitetura já refatorada

---

## Notas para o Claude Code

- Antes de qualquer mudança estrutural, ler os arquivos existentes de serviços e o DbContext para entender o padrão atual
- Não quebrar testes existentes se houver — adaptar os mocks para os novos repositórios
- Manter retrocompatibilidade com dados existentes no SQLite — criar migrations, não recriar o banco
- Para o ImageSharp: `dotnet add package SixLabors.ImageSharp`
- Para o Groq: não há SDK oficial .NET — usar `HttpClient` diretamente como mostrado acima
- Commitar em branches separadas por item se possível, para facilitar revisão