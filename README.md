# UmbLink

**Plataforma de link-in-bio profissional** — .NET 8 | Blazor Server | EF Core | SQLite | Bootstrap 5

---

## O que é o UmbLink?

O UmbLink é uma plataforma de link-in-bio no estilo Linktree, desenvolvida com tecnologias modernas do ecossistema .NET. Ela permite que criadores de conteúdo, profissionais e negócios criem uma página pública personalizada com bio, avatar, e uma lista organizada de links — tudo acessível em uma URL amigável como `/p/seu-nome`.

O projeto foi concebido como uma alternativa funcional e completa, com sistema de planos reais com limites aplicados, painel de analytics, temas visuais e um fluxo de onboarding guiado.

**Público-alvo:** criadores de conteúdo, profissionais autônomos, artistas, músicos, desenvolvedores e qualquer pessoa que queira centralizar sua presença digital em um único link.

---

## Funcionalidades principais

O UmbLink oferece um conjunto robusto de funcionalidades divididas entre o produto público e o painel de administração.

**Perfil e editor**
O usuário cria e gerencia sua página de link-in-bio por meio de um editor visual completo. É possível definir título, bio, avatar (com redimensionamento automático para 400×400 WebP), imagem de fundo e um slug personalizado que determina a URL pública da página. As alterações são refletidas em tempo real no preview lateral do editor.

**Links com drag-and-drop**
Links são adicionados, editados, reordenados via arrastar-e-soltar (SortableJS) e ativados ou desativados individualmente. Cada link passa por validação de URL com proteção contra SSRF, bloqueando endereços internos de rede.

**Temas visuais**
Doze temas disponíveis — quatro gratuitos e oito premium — com opções de estilo de botão (outline, rounded, pill, flat), espaçamento, forma do avatar, cores personalizadas de fundo, botão e texto, e tipografia.

**Analytics**
Métricas de visualizações de página e cliques por link, com gráfico diário gerado via Chart.js. O rastreamento acontece de forma assíncrona em segundo plano, sem impacto na experiência do visitante.

**Sistema de planos**
Três planos com limites reais aplicados em tempo real: Free (1 página, 3 links, 2 temas), Pro (3 páginas, 5 links, todos os temas, analytics de 365 dias, domínio customizado) e Business (tudo ilimitado, remoção de branding). Há um sistema de trial de 7 dias por plano, não reutilizável.

**Onboarding guiado**
O wizard de onboarding em 3 passos guia o novo usuário desde a escolha do tipo de perfil e template até a configuração final da página, com preview ao vivo durante o preenchimento.

**Autenticação**
Login e cadastro por e-mail e senha com lockout automático após tentativas falhas. Login via Google OAuth disponível quando as credenciais forem configuradas.

**Redirect com aviso**
Cliques em links para domínios desconhecidos passam por uma página de aviso intermediária antes do redirecionamento, protegendo o visitante de destinos inesperados.

---

## Stack tecnológica

| Camada | Tecnologia |
|--------|------------|
| Runtime | .NET 8 |
| UI | Blazor Server com componentes interativos |
| ORM | Entity Framework Core 8 |
| Banco de dados | SQLite (dev e produção) |
| Identidade | ASP.NET Core Identity |
| CSS | Bootstrap 5.3.3 + Bootstrap Icons 1.11.3 + CSS customizado |
| Gráficos | Chart.js (via JS interop) |
| Drag-and-drop | SortableJS (via JS interop) |
| Processamento de imagem | SixLabors.ImageSharp |
| Validação | FluentValidation |
| Cache | IMemoryCache (padrão) ou Redis (opcional) |
| Testes | xUnit + Moq + FluentAssertions |
| OAuth (opcional) | Google OAuth 2.0 |
| Deploy | Docker + Railway |

---

## Início rápido (desenvolvimento local)

### Pré-requisitos

- .NET 8 SDK instalado
- (Opcional) Credenciais do Google Cloud para OAuth

### Passo a passo

**1. Clone o repositório e restaure as dependências**

```bash
git clone <url-do-repositorio>
cd umblink
dotnet restore
```

**2. Execute o projeto**

```bash
dotnet run --project src/UmbLink.Web/UmbLink.Web.csproj
```

Na primeira execução, o banco de dados SQLite (`umblink.db`) é criado automaticamente, as migrations são aplicadas e os dados de seed (planos, preços e usuários de demonstração) são inseridos. Não é necessário rodar comandos de migração manualmente.

**3. Acesse no navegador**

A aplicação estará disponível em `https://localhost:5001` (ou a porta indicada no terminal).

**4. (Opcional) Habilitar login com Google**

Adicione ao mesmo arquivo:

```json
{
  "Google": {
    "ClientId": "seu-client-id",
    "ClientSecret": "seu-client-secret"
  }
}
```

**5. Rodar os testes**

```bash
dotnet test
```

---

## Estrutura do projeto

O repositório segue uma organização em camadas, separando responsabilidades de forma clara.

**`src/UmbLink.Web`** é o ponto de entrada da aplicação. Contém todos os componentes Blazor, páginas, layouts, o arquivo `Program.cs` com o pipeline completo de middleware e registro de dependências, além dos endpoints de API minimal (tracking, upload de arquivos, autenticação).

**`src/UmbLink.Application`** concentra a lógica de negócio. Aqui ficam os serviços (PageService, LinkService, SubscriptionService, MetricsService, PlanLimitService, AdminService, AuditService), interfaces, DTOs, requests validados com FluentValidation, modelos de domínio como `ThemeDefinitions` e `Templates`, e a fila de processamento em segundo plano para métricas.

**`src/UmbLink.Infrastructure`** é a camada de dados. Contém o `AppDbContext` (EF Core + Identity), todas as entidades do banco, os repositórios com suas interfaces, as migrations e o `DbSeeder` responsável por popular o banco na primeira execução.

**`tests/UmbLink.UnitTests`** contém os testes automatizados. Atualmente cobre o `PlanLimitService` com xUnit, Moq e FluentAssertions.

---

## Painel de administração

O painel admin está disponível em `/admin` e é acessível apenas para usuários com a role `Admin`. Ele oferece:

- **Dashboard geral** com estatísticas globais: total de usuários, páginas publicadas, cliques registrados e receita simulada (soma dos planos ativos).
- **Gestão de usuários** com busca, listagem paginada, visualização do plano atual e ações de suspensão, reativação e alteração de plano diretamente pela interface.
- **Audit log** com registro de todas as ações administrativas e críticas do sistema (criação/exclusão de páginas, alterações de subscription, etc.).

O acesso ao painel é protegido em duas camadas: a política de autorização `AdminOnly` nos componentes Blazor e um middleware customizado que retorna 403 para qualquer requisição a `/admin/*` feita por usuários não autenticados ou sem a role correta.

---

## Credenciais de demonstração

Após a primeira execução, o banco é populado automaticamente com os seguintes usuários de demonstração. Todos usam a senha `Demo@1234`.

| E-mail | Plano | Role |
|--------|-------|------|
| admin@umblink.com | Business | Admin |
| joao@exemplo.com | Pro (trial) | User |
| maria@exemplo.com | Free | User |
| carlos@exemplo.com | Free | User |

---

## Limitações conhecidas

O projeto está em estado funcional para demonstração, mas algumas funcionalidades estão parcialmente implementadas ou simuladas:

- **Pagamento:** o fluxo de checkout em `/plans/checkout` possui UI completa de cartão de crédito, mas o processamento é simulado com um delay artificial. Não há integração com gateway de pagamento real (Stripe, PagSeguro, etc.).
- **Domínio customizado:** a interface de configuração de domínio existe e permite salvar o valor, mas a verificação DNS/CNAME real não está implementada. O alvo de CNAME (`cname.umblink.com`) é um placeholder.
- **E-mail:** as páginas de recuperação e redefinição de senha existem com os formulários funcionais, mas o envio de e-mail via SMTP não está configurado, tornando o fluxo incompleto em ambiente sem configuração adicional.
- **Expiração automática de trial:** o método `ProcessExpiredTrialsAsync` está implementado no `SubscriptionService`, mas não há um job agendado que o execute periodicamente. Trials expirados não são processados automaticamente.
- **Remoção de branding:** a permissão `AllowRemoveBranding` está modelada nos planos e disponível no plano Business, mas a verificação na página pública ainda não está totalmente integrada ao DTO de subscription.

---

## Variáveis de ambiente (produção)

Para deploy em Railway ou ambiente similar, configure as seguintes variáveis:

| Variável | Descrição | Obrigatória |
|----------|-----------|-------------|
| `ConnectionStrings__Default` | Connection string do SQLite com caminho no volume | Sim |
| `ASPNETCORE_URLS` | URL de escuta (Railway injeta automaticamente na porta 8080) | Sim |
| `Google__ClientId` | Client ID do OAuth Google | Não |
| `Google__ClientSecret` | Client Secret do OAuth Google | Não |
| `ConnectionStrings__Redis` | Connection string do Redis para cache distribuído | Não |
