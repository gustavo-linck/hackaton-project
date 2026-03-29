# UMBLINK — Hotfix + UX: Admin Dashboard, Upload, IA e Melhorias de Experiência

## Contexto

Projeto **Umblink** (.NET 8 / Blazor Server, EF Core, SQLite, Railway). Leia `CLAUDE.md` e `STYLE_GUIDE.md` antes de qualquer alteração. As tarefas são independentes entre si — resolva uma por vez, compile e valide antes de avançar.

---

## Tarefa 1 — Corrigir o Dashboard da tela `/admin`

### Objetivo
O painel administrativo em `/admin` não está carregando os dados corretamente. Fazer o dashboard funcionar de ponta a ponta com dados reais do banco.

### O que investigar
1. Localizar a página de admin (`Admin.razor`, `AdminDashboard.razor` ou equivalente) e os serviços que ela injeta
2. Verificar se as queries ao banco via EF Core estão corretas (sem erros de navegação de propriedades, includes ausentes ou contexto disposed)
3. Checar se a rota `/admin` está protegida com a role correta e se o usuário de teste possui essa role no banco
4. Verificar o ciclo de vida do componente — dados sendo carregados em `OnInitializedAsync` corretamente?
5. Inspecionar o console do servidor por exceções silenciadas (try/catch engolindo erros sem log)

### O que corrigir
- Garantir que todas as métricas do dashboard (total de usuários, páginas criadas, links, planos ativos, etc.) sejam buscadas do banco e exibidas corretamente
- Tratar estados de loading (exibir skeleton ou spinner enquanto carrega) e erro (mensagem amigável se a query falhar)
- Se alguma métrica não tiver dado no banco ainda, exibir `0` — nunca tela em branco ou exceção

### Critério de aceitação
Acessar `/admin` com um usuário de role `Admin` exibe o dashboard com dados reais, sem erro 500, tela em branco ou exceção no log do servidor.

---

## Tarefa 2 — Remover Upload de Imagem (manter apenas link)

### Objetivo
O upload de imagem não está funcionando. Remover completamente essa funcionalidade e manter apenas a opção de inserir URL de imagem via link, que já existe e funciona.

### O que localizar e remover
1. Todos os componentes `InputFile` ou equivalentes usados para upload de avatar/capa
2. O serviço/método responsável por processar o arquivo (leitura de `IBrowserFile`, salvamento, etc.)
3. Qualquer botão, tab, toggle ou seção de UI que ofereça a opção de "fazer upload" vs "usar link"
4. Referências a `MaxFileSize`, `OnChange` de arquivo, streams de leitura de imagem
5. Imports e injeções de dependência que existam exclusivamente para o upload

### O que manter
- O campo de input de URL de imagem (link) deve permanecer intacto e funcional
- O preview de imagem via URL deve continuar funcionando normalmente

### Como remover
- Remover o código, não comentar
- Se a UI tinha um seletor "Upload / URL", simplificar para mostrar apenas o campo de URL diretamente, sem seletor
- Garantir que o layout da seção de imagem fique limpo após a remoção (sem espaços vazios ou elementos órfãos)

### Critério de aceitação
Não existe mais nenhuma interface de upload de arquivo no projeto. O usuário vê apenas um campo para inserir uma URL de imagem, que funciona normalmente.

---

## Tarefa 3 — Remover Funcionalidades de Inteligência Artificial

### Objetivo
Remover completamente todas as funcionalidades de IA que foram implementadas mas não estão funcionando, sem deixar rastros de UI ou código morto.

### O que localizar e remover
1. Qualquer botão, seção ou modal que chame recursos de IA (ex: "Gerar com IA", "Sugestão automática", "Bio gerada por IA", etc.)
2. Os serviços, classes e métodos que fazem chamadas à API de IA (OpenAI, Anthropic ou similar)
3. Configurações de API key de IA em `appsettings.json` — remover a entrada, sem expor nenhum valor sensível
4. Injeções de dependência registradas exclusivamente para os serviços de IA
5. Imports e using statements que fiquem órfãos após as remoções
6. Qualquer pacote NuGet adicionado exclusivamente para IA — remover do `.csproj`

### Como remover
- Remover o código completamente, não comentar
- Após remover cada serviço, verificar se ele é referenciado em outros lugares antes de deletar o arquivo
- Garantir que o projeto compila sem erros após cada remoção
- Verificar se menus de navegação ou sidebars tinham itens de IA — remover esses itens

### Critério de aceitação
O projeto compila e roda sem erros. Não existe nenhuma referência visual ou de código a funcionalidades de IA.

---

## Tarefa 4 — Melhorias de UX/UI e Fluxos

### Objetivo
Com o projeto limpo e funcional após as tarefas anteriores, realizar uma passagem completa de UX/UI para elevar a qualidade da experiência em todas as telas e fluxos principais.

---

### 4.1 — Auditoria Visual Geral

Varrer todos os arquivos `.razor` e `.css` e corrigir:

- **Alinhamento:** botões, inputs, labels e ícones fora de eixo (flexbox/grid incorreto)
- **Espaçamento:** padding/margin inconsistentes entre componentes semelhantes
- **Tipografia:** tamanhos de fonte e pesos sem hierarquia clara
- **Border-radius e cores:** inconsistências entre cards, botões e inputs de mesma família
- **Tema:** elementos que não adaptam corretamente entre modo claro e escuro
- **Mobile:** layout quebrando abaixo de 768px — checar todos os breakpoints

Documentar cada correção com `/* FIX: descrição */` no CSS ou `<!-- FIX: descrição -->` no markup.

---

### 4.2 — Feedback e Estados de Interface

Em todas as ações do usuário (salvar, excluir, publicar, copiar link, etc.):

- Adicionar **feedback visual imediato**: toast de sucesso/erro, spinner no botão durante a operação, desabilitar o botão enquanto processa (evitar cliques duplos)
- Ações destrutivas (excluir link, excluir página) devem ter **confirmação antes de executar** — modal ou inline confirm, não `window.confirm` nativo
- Formulários devem mostrar **validação inline** nos campos (erro embaixo do input, não só um alert genérico no topo)
- Estados vazios (nenhum link cadastrado, nenhuma página criada) devem ter uma **empty state** com mensagem e CTA claro — não tela em branco

---

### 4.3 — Fluxo de Onboarding / Wizard

- O wizard deve ter uma **barra de progresso clara** indicando em qual step o usuário está e quantos restam
- Cada step deve ter título e subtítulo explicando o que o usuário precisa fazer
- O botão "Avançar" deve ser desabilitado enquanto os campos obrigatórios do step atual não estiverem preenchidos
- Deve existir um botão "Voltar" funcional nos steps intermediários
- O step de criação de página deve exibir um **preview em tempo real** dos links sendo adicionados (split-view: formulário à esquerda, preview à direita — mobile: preview abaixo)
- Ao finalizar o wizard, redirecionar para o dashboard com uma **mensagem de boas-vindas** e o link da página pública em destaque

---

### 4.4 — Editor de Página (Split-view)

- O painel de preview deve permanecer **fixo** enquanto o painel esquerdo rola — usar `position: sticky` ou layout fixo no container
- Qualquer alteração no formulário (nome, bio, link adicionado/removido, tema alterado) deve refletir no preview **sem delay perceptível**
- O botão "Salvar" deve ter estado de loading e confirmar o sucesso com toast — não apenas recarregar a página
- O campo de slug deve validar em tempo real se está disponível (debounce de 500ms + indicador visual: ✓ disponível / ✗ em uso)
- Adicionar botão "Ver página pública" que abre a página em nova aba — visível e acessível no topo do editor

---

### 4.5 — Fluxo de Autenticação

- Login e cadastro devem ter **loading state** no botão de submit
- Erros de autenticação (senha errada, email não encontrado, email já cadastrado) devem aparecer como mensagem inline abaixo do formulário — não como alert
- Após login bem-sucedido, redirecionar para o dashboard (não para a home pública)
- Após cadastro, iniciar o wizard de onboarding automaticamente — sem etapa manual de navegação
- A tela de recuperação de senha deve existir e ser acessível pelo link "Esqueci minha senha" na tela de login

---

### 4.6 — Navegação e Estrutura Geral

- O menu/sidebar deve indicar claramente a **rota ativa** com destaque visual
- Links de navegação quebrados ou que levam a páginas 404 devem ser identificados e corrigidos ou removidos
- A página pública do usuário (`/p/{slug}`) deve ter **meta tags** básicas (title, description, og:image) com os dados da página — importante para compartilhamento em redes sociais
- Botão de copiar o link da página (`umblink.com/p/{slug}`) deve estar acessível e visível no dashboard e no editor

---

### 4.7 — Painel Admin (UX)

Após corrigir o funcionamento do dashboard admin (Tarefa 1), melhorar a experiência:

- Tabelas de usuários/planos devem ter **busca e filtro** básicos (por nome, email, plano)
- Ações em massa ou por linha devem ter confirmação antes de executar
- Paginação nas listagens com mais de 20 itens
- Indicadores de status claros (usuário ativo/inativo, plano trial/pago) com badges coloridos

---

### Critério de aceitação geral da Tarefa 4
Todos os fluxos principais (onboarding, edição de página, autenticação, navegação) funcionam de ponta a ponta sem confusão ou perda de estado. O usuário recebe feedback visual em toda ação relevante. Nenhuma tela apresenta desalinhamento, estado vazio sem tratamento ou botão sem resposta.

---

## Instruções Gerais

- **Compile após cada tarefa** antes de avançar (`dotnet build`)
- **Não quebre o que está funcionando** — se remover um método, confirme que nada mais o referencia
- **Sem código comentado** — remoção limpa em todas as tarefas
- **Sem mocks** — exceto pagamento, tudo deve usar dados reais do banco
- **Ordem sugerida:** Tarefa 2 → Tarefa 3 → Tarefa 1 → Tarefa 4