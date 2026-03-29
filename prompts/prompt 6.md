# UMBLINK — Sprint de Correções: UX/UI, Admin, Wizard, Upload e Dashboard

## Contexto

Você está trabalhando no projeto **Umblink** (umblink.com), um produto SaaS estilo Linktree construído com **.NET 8 / Blazor Server**, Entity Framework Core, SQLite e Railway para deploy. A stack de frontend está em transição do Bootstrap para CSS customizado com GSAP e glass morphism.

Antes de qualquer alteração, faça uma leitura completa dos arquivos `CLAUDE.md` e `STYLE_GUIDE.md` (se existirem) para entender as convenções do projeto. Em seguida, explore a estrutura de pastas para mapear onde vivem os componentes relevantes a cada tarefa.

---

## Tarefa 1 — Auditoria de UX/UI: Alinhamento e Consistência Visual

### Objetivo
Fazer uma varredura em todas as telas do projeto e identificar (e corrigir) problemas de alinhamento, espaçamento e consistência visual.

### O que analisar
- Botões desalinhados (verificar padding, margin, flexbox/grid)
- Elementos que quebram layout em mobile (checar breakpoints)
- Inconsistência de tamanhos de fonte, cores e border-radius entre componentes semelhantes
- Inputs, labels e ícones fora de eixo
- Componentes que não respeitam o tema escuro/claro corretamente

### Como proceder
1. Liste todos os arquivos `.razor` e `.css` das páginas e componentes
2. Para cada tela, descreva os problemas encontrados com localização exata (arquivo + linha aproximada)
3. Aplique as correções diretamente, documentando cada mudança com um comentário `<!-- FIX: descrição -->` no markup ou `/* FIX: descrição */` no CSS
4. Priorize correções que afetam múltiplas telas (ex: componentes reutilizados)

### Critério de aceitação
Nenhum botão, input ou card deve apresentar desalinhamento visível. O layout deve ser coerente em desktop (≥1024px) e mobile (≤768px).

---

## Tarefa 2 — Tela e Fluxo de Admin

### Objetivo
Fazer o fluxo de administração funcionar de ponta a ponta.

### O que investigar
1. Mapear todas as rotas e páginas de admin (ex: `/admin`, `/admin/users`, `/admin/plans`, etc.)
2. Verificar se a autenticação/autorização está corretamente protegendo essas rotas (role `Admin`)
3. Identificar quais componentes/páginas estão incompletos, com dados mockados ou sem integração com o banco
4. Verificar se os serviços e repositórios necessários existem e estão injetados corretamente

### O que implementar/corrigir
- Todas as páginas de admin devem carregar dados reais do banco via EF Core
- A navegação entre as seções do admin deve funcionar sem erros de rota ou autorização
- Se existir uma sidebar/menu de admin, garantir que os itens ativos sejam destacados corretamente
- CRUD de usuários e planos deve estar funcional (ou indicar claramente o que falta)
- Tratar estados de loading e erro nas tabelas/listagens

### Critério de aceitação
Um usuário com role `Admin` consegue acessar o painel, visualizar usuários cadastrados, seus planos e status — sem erros 500, telas em branco ou dados fictícios.

---

## Tarefa 3 — Wizard de Criação de Conta: Links Integrados na Tela de Criação de Página

### Objetivo
Na etapa do wizard onde o usuário cria sua página (bio page), os links que ele adiciona devem aparecer em tempo real na mesma tela, no painel de preview — exatamente como funciona no editor principal.

### O que investigar
1. Localizar o componente wizard de onboarding (provavelmente algo como `Onboarding.razor`, `WizardStep*.razor` ou similar)
2. Identificar a etapa que corresponde à criação da página e adição de links
3. Verificar se o painel de preview já existe nessa etapa ou se está faltando

### O que implementar
- O step de criação de página deve ter um layout **split-view**: formulário à esquerda, preview à direita (mobile: preview abaixo)
- Cada link adicionado deve refletir imediatamente no preview (usar o mesmo padrão de binding reativo já usado no editor principal)
- Reutilizar o componente de preview existente (`LinkPagePreview` ou equivalente) — não criar um novo do zero
- Garantir que o estado dos links no wizard seja persistido corretamente ao avançar para o próximo step

### Critério de aceitação
Durante o wizard, ao adicionar um link, ele aparece instantaneamente no painel de preview ao lado do formulário, sem necessidade de salvar ou navegar.

---

## Tarefa 4 — Upload de Arquivos (Avatar / Imagens)

### Objetivo
Fazer o upload de imagens (avatar de perfil, imagem de capa ou similares) funcionar corretamente.

### O que investigar
1. Localizar todos os componentes que fazem uso de upload (`InputFile`, `IBrowserFile`, ou bibliotecas de terceiros)
2. Verificar o serviço responsável pelo tratamento do arquivo (redimensionamento, salvamento, URL de retorno)
3. Checar se há configuração de tamanho máximo, tipos permitidos e feedback de erro
4. Verificar se o caminho de salvamento existe e tem permissão de escrita no ambiente Railway

### O que corrigir/implementar
- O componente de upload deve mostrar preview imediato da imagem selecionada antes do envio
- Após o upload, a URL da imagem deve ser persistida no banco e refletida na página do usuário
- Tratar erros: arquivo muito grande, tipo inválido, falha de rede — com mensagens amigáveis
- Se o upload estiver salvando localmente mas quebrando no deploy (Railway/containers), migrar para uma solução de storage compatível com ambiente efêmero (base64 em banco, ou indicar claramente a pendência)
- Garantir que o `MaxFileSize` no `InputFile` esteja configurado explicitamente

### Critério de aceitação
O usuário seleciona uma imagem, vê o preview, confirma e a imagem aparece na sua página pública. O fluxo funciona tanto em desenvolvimento local quanto no ambiente de deploy.

---

## Tarefa 5 — Dashboard: Remoção do Bloco de "Próximos Passos / Atividade Recente" + Fundo Animado

### Objetivo
Simplificar o dashboard removendo os blocos de "Próximos Passos" e "Atividade Recente" (que estão sobrecarregando visualmente a tela) e substituir o fundo estático (branco/preto) por um fundo animado moderno, igual ao da tela de Home.

### O que remover
- Identificar e remover os componentes/seções de "Próximos Passos" (Next Steps) e "Atividade Recente" (Recent Activity) do dashboard
- Limpar qualquer serviço, query ou estado que existia exclusivamente para alimentar esses blocos (não remover se forem usados em outro lugar)

### O que implementar: Fundo Animado
- Inspecionar a tela de **Home** (`Home.razor`, `Index.razor` ou equivalente) e extrair o mecanismo de fundo animado (gradiente em movimento, partículas, canvas, GSAP, CSS animation — o que estiver implementado)
- Aplicar o mesmo fundo animado no **Dashboard** (`Dashboard.razor` ou equivalente)
- O fundo deve funcionar nos dois temas (claro e escuro), adaptando as cores conforme as variáveis CSS já definidas
- O conteúdo do dashboard (cards de estatísticas, links recentes, etc.) deve ficar **sobre** o fundo animado com contraste legível — usar `backdrop-filter: blur()` e/ou cards com fundo semi-transparente (glass morphism) se necessário
- A animação não deve causar degradação de performance (checar se já usa `will-change`, `transform` e `requestAnimationFrame` corretamente)

### Critério de aceitação
O dashboard exibe apenas as informações essenciais (estatísticas, links do usuário), sobre um fundo animado igual ao da Home, com boa legibilidade em ambos os temas e sem impacto perceptível de performance.

---

## Instruções Gerais

- **Não quebre o que está funcionando.** Antes de modificar qualquer arquivo, verifique se ele impacta outras páginas ou componentes.
- **Commits atômicos por tarefa.** Cada tarefa acima deve ser tratada de forma independente — resolva e valide uma antes de partir para a próxima.
- **Preserve o padrão de código existente.** Siga as convenções de nomenclatura, injeção de dependência e estrutura de arquivos já estabelecidas no projeto.
- Lembre de atualizar o CLAUDE.md com o que foi feito que julgar necessário.
- **Prioridade de execução sugerida:** Tarefa 4 → Tarefa 2 → Tarefa 3 → Tarefa 5 → Tarefa 1