# 🛠️ Umblink — Prompt de Melhorias do Sistema

> Leia este arquivo inteiro antes de começar qualquer implementação. Execute cada item em sequência, confirmando o que foi feito antes de avançar para o próximo. Ao final de cada item, liste os arquivos modificados.

---

## Contexto do Projeto

Umblink é um SaaS no estilo Linktree construído com **.NET 8 / Blazor Server**, **Entity Framework Core**, **SQLite** e **Railway** para deploy. O sistema tem autenticação por e-mail/senha e Google OAuth, planos Freemium (Free, Pro, Business) com trial de 7 dias e editor de páginas com preview em tempo real.

---

## Item 1 — Criação de Conta: Permitir adicionar links durante o onboarding

**Problema:** No fluxo de criação de conta, o usuário só consegue definir nome e bio. Não há etapa para já adicionar links à página.

**O que fazer:**
- Identificar o componente/página de onboarding (provavelmente `Register.razor` ou `Onboarding.razor`).
- Adicionar uma **etapa extra no fluxo de cadastro** (após definir nome/bio) onde o usuário pode adicionar até 3 links iniciais (título + URL).
- Esses links devem ser salvos junto com a criação da página padrão do usuário.
- A etapa deve ser opcional — ter um botão "Pular por agora" que leva direto ao Dashboard.
- Validar as URLs inseridas (formato básico) antes de salvar.
- O visual deve seguir o padrão já existente no sistema.

---

## Item 2 — Painel Admin: Dashboard funcional com gestão de usuários

**Problema:** A rota `/admin` não funciona. O painel admin é praticamente vazio, sem utilidade real.

**O que fazer:**
- Corrigir a rota `/admin` para que carregue um **Dashboard administrativo** funcional.
- O dashboard deve exibir:
  - Total de usuários cadastrados
  - Usuários ativos nos últimos 7 e 30 dias
  - Distribuição de planos (Free / Pro / Business / Trial)
  - Páginas criadas (total e por status: publicado/rascunho)
  - Receita estimada (baseada nos planos ativos)
- Na tela `/admin/users`, adicionar as seguintes ações por usuário:
  - Ver plano atual e histórico de planos
  - Ver faturas associadas
  - Ver logs de atividade (ver Item 3)
  - Forçar troca de plano manualmente (com justificativa)
  - Enviar mensagem/notificação ao usuário (pode ser simples, via flag no banco)
- Proteger todas as rotas `/admin/*` com verificação de role `Admin`. Retornar 403 ou redirecionar caso o usuário não seja admin.

---

## Item 3 — Sistema de Logs de Atividade do Usuário

**Problema:** Provavelmente não existe um sistema de logs estruturado, o que impede diagnóstico de problemas por parte do admin.

**O que fazer:**
- Verificar se já existe algum mecanismo de log no sistema (buscar por `ILogger`, tabelas de log no banco, etc.).
- Criar uma entidade `UserActivityLog` com os campos:
  ```
  Id, UserId, Action (string), Details (string/json), IpAddress, CreatedAt
  ```
- Registrar logs nas seguintes ações:
  - Login com sucesso / falha
  - Criação/edição/exclusão de página
  - Adição/remoção de link
  - Troca de plano
  - Upload de avatar
  - Qualquer erro inesperado capturado
- Criar um `IUserActivityLogService` e injetá-lo nos serviços relevantes.
- Exibir esses logs na tela de detalhe do usuário no painel admin (Item 2).
- **Não logar dados sensíveis** (senhas, tokens, dados de cartão).

---

## Item 4 — IA na Criação de Conta: Investigar e corrigir

**Problema:** A funcionalidade de IA durante o onboarding não está funcionando.

**O que fazer:**
- Localizar onde a IA é chamada no fluxo de criação de conta (buscar por chamadas HTTP à API da OpenAI/Anthropic ou qualquer serviço de IA configurado).
- Verificar:
  - A chave de API está configurada corretamente nas variáveis de ambiente?
  - O serviço de IA está sendo injetado corretamente via DI?
  - Há algum erro sendo silenciado (try/catch sem log)?
  - O endpoint correto está sendo chamado?
- Adicionar logs detalhados (ver Item 3) em volta da chamada de IA para capturar erros.
- Testar a chamada isoladamente e corrigir o que estiver errado.
- Se a chave de API não estiver configurada no ambiente de dev, documentar no `README.md` ou `CLAUDE.md` como configurá-la.

---

## Item 5 — Tema Light: Corrigir legibilidade do texto na Home

**Problema:** No tema light, o texto "Todos os seus links em um só lugar" na home está com cor escura sobre fundo escuro, prejudicando a leitura.

**O que fazer:**
- Localizar o componente da home (provavelmente `Home.razor` ou `Index.razor`).
- Identificar o elemento com esse texto e verificar a cor aplicada.
- No tema light, garantir que o texto tenha contraste adequado — usar `color: var(--text-primary)` ou equivalente que respeite o tema.
- Verificar se o problema é causado por uma classe CSS herdada que força cor escura independente do tema.
- Testar nos dois temas (light e dark) para confirmar que o contraste está correto em ambos.
- Seguir o padrão WCAG AA (contraste mínimo de 4.5:1 para texto normal).

---

## Item 6 — Arquitetura: Mover consultas diretas ao banco para Repositories

**Problema:** Alguns serviços estão fazendo consultas diretamente no `DbContext`, violando a separação de responsabilidades.

**O que fazer:**
- Fazer um levantamento nos arquivos de `Services/` buscando por uso direto de `_context.`, `DbContext`, ou `_dbContext.` para queries (`.Where(`, `.FirstOrDefault(`, `.ToList(`, etc.).
- Para cada serviço que faz acesso direto:
  1. Criar (ou complementar) o `Repository` correspondente em `Repositories/`.
  2. Mover a query para o repository, expondo um método com nome semântico (ex: `GetActivePagesByUserId`).
  3. No serviço, injetar o repository via interface e chamar o método.
  4. O serviço deve ficar responsável apenas por **validações de negócio**, orquestração e tratamento de erros.
- Seguir o padrão de interface `IXRepository` já usado no projeto (se existir).
- Registrar os novos repositories no `Program.cs` / `Startup.cs`.
- **Não alterar comportamento** — apenas mover a lógica, mantendo os testes passando (se houver).

---

## Item 7 — Upload de Avatar: Corrigir erro no upload de imagem

**Problema:** O upload de imagem de avatar na edição de página está falhando.

**O que fazer:**
- Localizar o componente de edição de página e o handler de upload de imagem.
- Reproduzir o erro com um arquivo `.png` e `.jpg` e capturar a mensagem de erro completa.
- Verificar:
  - O tamanho máximo permitido está configurado corretamente (tanto no Blazor quanto no servidor)?
  - Os tipos MIME aceitos incluem `image/png`, `image/jpeg`, `image/webp`?
  - O caminho de salvamento existe e tem permissão de escrita?
  - Em produção (Railway), o armazenamento em disco é efêmero? Se sim, considerar salvar em base64 no banco ou usar um serviço externo.
- Corrigir o erro encontrado.
- Exibir mensagens de erro amigáveis ao usuário caso o upload falhe (ex: "Arquivo muito grande", "Formato não suportado").
- Aceitar: `.png`, `.jpg`, `.jpeg`, `.webp`. Tamanho máximo sugerido: 2MB.

---

## Item 8 — Footer: Tornar fixo na parte inferior da tela

**Problema:** O footer some em páginas com pouco conteúdo, exigindo scroll para visualizá-lo, o que quebra o layout.

**O que fazer:**
- Localizar o componente de layout principal (provavelmente `MainLayout.razor`).
- Ajustar o layout para que o footer fique sempre visível na parte inferior da viewport, **sem sobrepor o conteúdo scrollável**.
- Usar a abordagem de `min-height: 100vh` com flexbox no layout:
  ```css
  .layout-wrapper {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
  }
  .layout-content {
    flex: 1;
  }
  footer {
    flex-shrink: 0;
  }
  ```
- Garantir que o footer **não** use `position: fixed` (o que sobreporia o conteúdo) — a abordagem flexbox é a correta.
- Testar em páginas com muito conteúdo (o scroll deve funcionar normalmente) e em páginas com pouco conteúdo (footer deve aparecer na base da tela).

---

## Item 9 — Trial Gratuito: Exigir cadastro de cartão antes de ativar

**Problema:** O trial gratuito é ativado com um único clique, sem nenhuma barreira. O correto é coletar dados de cartão primeiro (informando que não haverá cobrança até o fim do trial).

**O que fazer:**
- Localizar o botão/ação de ativar trial no sistema.
- Substituir o comportamento atual por um **fluxo de tela dedicado**:
  1. Tela/modal de introdução: destacar os benefícios do trial e informar claramente: *"Seu cartão não será cobrado durante os 7 dias de trial. A cobrança só ocorre após o período terminar, e você pode cancelar a qualquer momento."*
  2. Formulário de cartão de crédito (campos: número, nome, validade, CVV) — pode ser mockado visualmente, mas deve parecer real e validar formato.
  3. Botão de confirmação: "Iniciar meu Trial Gratuito".
- Após o fluxo, ativar o trial normalmente como hoje.
- O estado do cartão (mockado) deve ser salvo no banco como `PaymentMethod` vinculado ao usuário.
- Exibir mensagem de confirmação positiva ao finalizar: *"Trial ativado! Você tem 7 dias para explorar tudo."*

---

## Item 10 — Modais: Tamanho fixo independente do conteúdo

**Problema:** Algumas modais mudam de tamanho conforme o conteúdo interno muda (ex: ao trocar de etapa dentro da modal), causando um efeito visual ruim.

**O que fazer:**
- Identificar as modais que apresentam esse comportamento (provavelmente as de criação/edição de página, planos, etc.).
- Aplicar tamanho fixo via CSS nas modais afetadas:
  ```css
  .modal-dialog {
    width: 560px;        /* ou o tamanho adequado */
    min-height: 400px;   /* altura mínima consistente */
    max-height: 85vh;
    overflow-y: auto;    /* scroll interno se necessário */
  }
  ```
- O conteúdo interno deve ocupar o espaço disponível, mas a modal não deve redimensionar ao trocar de etapa/conteúdo.
- Testar nas modais identificadas para confirmar comportamento estável.

---

## Item 11 — Dashboard do Usuário: Enriquecer com mais informações úteis

**Problema:** O dashboard atual está simples, com pouco conteúdo e baixo valor percebido.

**O que fazer:**
- Manter o que já existe, mas adicionar as seguintes seções:

  **Visão Geral Rápida (cards no topo):**
  - Total de visualizações de todas as páginas (últimos 30 dias)
  - Total de cliques em links (últimos 30 dias)
  - Página mais visitada
  - Link mais clicado

  **Minhas Páginas (lista melhorada):**
  - Exibir thumbnail/preview da página (pode ser apenas o avatar + nome)
  - Status (Publicado / Rascunho)
  - Contagem de links
  - Visualizações nos últimos 7 dias
  - Botão rápido de editar

  **Atividade Recente:**
  - Listagem dos últimos eventos do usuário (links adicionados, página editada, etc.) — usando o sistema de logs do Item 3.

  **Dicas / Próximos Passos (se página incompleta):**
  - "Adicione uma foto de perfil"
  - "Publique sua página"
  - "Adicione seu primeiro link"
  - Exibir apenas o que ainda não foi feito.

- O layout deve ser em grid responsivo, usando o estilo visual já existente no sistema.

---

## Item 12 — Configurações da Conta: Criar tela dedicada

**Problema:** Não existe uma tela de configurações da conta do usuário.

**O que fazer:**
- Criar a rota `/account/settings` com uma página `AccountSettings.razor`.
- Adicionar link para essa página no menu de navegação (no dropdown do nome do usuário ou em "Meu Plano").
- A página deve ter as seguintes seções:

  **Perfil:**
  - Nome de exibição
  - E-mail (somente leitura se for OAuth, editável se for e-mail/senha)
  - Foto de perfil da conta (diferente do avatar da página)
  - Salvar alterações

  **Segurança:**
  - Trocar senha (apenas para usuários com e-mail/senha)
  - Exibir provedor de login (E-mail ou Google)
  - Sessões ativas (opcional)

  **Plano e Faturamento:**
  - Plano atual com data de renovação ou fim do trial
  - Botão para fazer upgrade
  - Histórico de faturas (mesmo que mockado)
  - Método de pagamento cadastrado (ver Item 9)

  **Zona de Perigo:**
  - Botão "Excluir minha conta" com modal de confirmação (pedir para digitar o e-mail para confirmar).
  - Ao excluir: remover usuário, páginas, links e logs associados.

---

## Item 13 — Login e Cadastro: Padronizar layout dos labels

**Problema:** No formulário de cadastro/login, o label "E-mail" está posicionado ao lado do campo, enquanto "Senha" está acima. O padrão deve ser uniforme.

**O que fazer:**
- Localizar os componentes `Login.razor` e `Register.razor` (ou equivalentes).
- Padronizar **todos** os campos de formulário com o label **acima** do input:
  ```html
  <div class="form-group">
    <label for="email">E-mail</label>
    <input type="email" id="email" class="form-control" />
  </div>
  ```
- Verificar se há outros formulários no sistema com o mesmo problema (ex: criação de página, configurações) e padronizar também.
- Manter espaçamento consistente entre label e input (sugestão: `margin-bottom: 4px` no label).

---

## Item 14 — Seletor de Imagem de Background: Melhorar qualidade visual dos thumbnails

**Problema:** As imagens disponíveis para uso como background na tela de edição aparecem com qualidade ruim no seletor.

**O que fazer:**
- Localizar o componente do seletor de imagens de background (na tela de edição da página).
- Verificar a fonte das imagens: são assets locais ou URLs externas?
- Se forem assets locais:
  - Verificar se as imagens estão sendo redimensionadas via CSS de forma distorcida (ex: `width: 100%` sem `object-fit: cover`).
  - Garantir que os thumbnails usem `object-fit: cover` e tenham dimensões adequadas (mínimo 120x80px renderizado).
  - Se as imagens originais forem de baixa resolução, substituir por versões de maior qualidade (mínimo 800x600px para backgrounds).
- Se forem URLs externas:
  - Garantir que as URLs apontam para versões de alta qualidade (ex: Unsplash com parâmetros `?w=800&q=80`).
- O seletor deve exibir os thumbnails com bordas arredondadas, sem distorção, com um estado de hover/selecionado claramente visível.

---

## Item 15 — Header: Exibir link para o Painel Admin quando o usuário for admin

**Problema:** Usuários com role `Admin` não têm acesso rápido ao painel administrativo pelo header.

**O que fazer:**
- Localizar o componente de navegação principal (provavelmente `NavMenu.razor` ou `MainLayout.razor`).
- Verificar o usuário autenticado atual e sua role — usar o `AuthenticationState` ou `IAuthorizationService` já presente no projeto.
- Se o usuário tiver role `Admin`, exibir um item extra no header com o label **"Admin"** (ou ícone de engrenagem/escudo + "Admin") linkando para `/admin`.
- O item deve aparecer de forma discreta mas visível — sugestão: destaque sutil diferente dos itens comuns (ex: cor levemente diferente ou um badge/ícone pequeno).
- **Não exibir** esse item para usuários comuns — a condição deve ser puramente client-side após autenticação, mas a proteção real da rota continua sendo server-side (ver Item 2).
- Exemplo de implementação Blazor:
  ```razor
  <AuthorizeView Roles="Admin">
      <Authorized>
          <a href="/admin" class="nav-link nav-admin">⚙ Admin</a>
      </Authorized>
  </AuthorizeView>
  ```

---

## Checklist de Finalização

Ao concluir todos os itens, verificar:

- [ ] Nenhum `Console.WriteLine` ou `console.log` de debug foi deixado no código
- [ ] Todos os novos serviços/repositories estão registrados no DI (`Program.cs`)
- [ ] Nenhuma query direta ao banco ficou em serviços (Item 6)
- [ ] O sistema compila sem warnings críticos
- [ ] As rotas novas (`/admin`, `/account/settings`) estão protegidas por autenticação/autorização
- [ ] O sistema foi testado em tema light e dark
- [ ] O sistema foi testado em mobile (responsividade)
- [ ] O `CLAUDE.md` foi atualizado com qualquer nova variável de ambiente necessária

---