# Sprint de Melhorias — UmbLink
> Data: 2026-03-29
> Prioridade de execução: T4 → T2 → T3 → T5 → T1

---

## Tarefa 4 — Upload: Preview imediato de imagem

### Problema
O editor (`Editor.razor`) usa `InputFile` diretamente. Não há preview antes do envio. O usuário só vê o avatar/fundo após o upload completar.

### Solução
- Ao selecionar arquivo via `InputFile`, ler como base64 com `IBrowserFile.OpenReadStream()` e exibir `<img>` de preview imediato (antes de enviar para o servidor)
- Manter o spinner durante o upload; substituir preview local pela URL retornada do servidor ao concluir
- Mensagens de erro amigáveis: arquivo muito grande, tipo inválido, falha de rede
- `MaxFileSize` explícito no `InputFile` (2MB = 2_097_152 bytes)
- Nota Railway: upload salva em `wwwroot/uploads/` (volume montado); sem mudança de storage necessária agora — apenas documentar a limitação

### Arquivos afetados
- `src/UmbLink.Web/Pages/Dashboard/Editor.razor` — seção avatar upload e background upload

---

## Tarefa 2 — Admin: Navegação ativa + consistência visual

### Problema
- `AdminLayout.razor` usa classes Bootstrap (`bg-dark`, `nav-link text-white`) enquanto o resto do app usa CSS variables (`var(--bg-primary)`, `var(--text-primary)`)
- Links do sidebar não destacam o item ativo
- Nenhuma inconsistência funcional — dados são reais via `IAdminService`

### Solução
- Adicionar active state nos links do sidebar usando `NavigationManager.Uri` para comparar rota atual
- Migrar o `AdminLayout` de `bg-dark` Bootstrap para CSS variables do tema (`var(--bg-sidebar)` ou equivalente) para consistência com o resto do app
- Garantir que o sidebar funcione no tema claro e escuro

### Arquivos afetados
- `src/UmbLink.Web/Layouts/AdminLayout.razor`

---

## Tarefa 3 — Wizard: Step unificado (perfil + links + preview)

### Problema
Step 3 tem perfil (título/bio/slug), Step 4 tem links separados. O usuário quer tudo em um único step com preview ao vivo.

### Solução
- Fundir steps 3 e 4 em um único step 3
- Layout responsivo **dois cards + preview**:
  - Card "Perfil": título, slug, bio, AI helper
  - Card "Links": até 3 links opcionais (título + URL cada)
  - Preview (`PagePreview`) à direita no desktop (col-lg-5), oculto no mobile (abaixo dos cards com toggle ou collapsível)
- Ao digitar qualquer link, o `PagePreview` recebe `Links` atualizado reactivamente
- Botão "Criar minha página" ao final do step unificado → vai direto para step 4 (antigo step 5 = sucesso)
- Renumerar: step 1 = perfil type, step 2 = template, step 3 = perfil+links, step 4 = sucesso
- `TotalVisualSteps = 4` → `3` (remover etapa visual de links)
- Responsividade: mobile collapsa preview em baixo, com botão toggle "Ver preview"

### Arquivos afetados
- `src/UmbLink.Web/Pages/Onboarding/Wizard.razor`

---

## Tarefa 5 — Dashboard: Remover blocos + fundo animado

### Problema
- Blocos "Atividade Recente" + "Próximos Passos" (linhas 136–231 de `Dashboard/Index.razor`) sobrecarregam a tela
- Fundo é o padrão `var(--bg-primary)` — sem personalidade visual

### Solução

**Remover:**
- Seção `@* ─── Recent Activity + Next Steps ─── *@` (linhas 136–231) do `Dashboard/Index.razor`
- Remover injeção de `IUserActivityLogService ActivitySvc` e campo `_activityLogs` (se não usado em outro lugar)
- Remover chamada `_activityLogs = await ActivitySvc.GetLogsAsync(...)` no `OnInitializedAsync`

**Fundo animado — Opção A (gradiente escuro + glass morphism):**
- Envolver o conteúdo principal do dashboard em um wrapper com classe `dashboard-bg` que replica o estilo do hero (dark: `#080d1a` com radial gradients)
- Adicionar CSS em `site.css`: `.dashboard-bg` com background escuro e gradientes animados
- No tema claro: versão mais suave (fundo `#f8f9ff` com orbs de baixa opacidade)
- Cards de métricas e de páginas recebem `backdrop-filter: blur(12px)` + fundo semi-transparente (glass morphism)
- Reutilizar `initHeroGradient` do `app.js` — chamar via `IJSRuntime` no `OnAfterRenderAsync` passando ref do wrapper
- Texto e ícones nos cards: forçar contraste adequado com `color: #fff` no tema escuro

### Arquivos afetados
- `src/UmbLink.Web/Pages/Dashboard/Index.razor`
- `src/UmbLink.Web/wwwroot/css/site.css` (adicionar `.dashboard-bg` e `.dashboard-card-glass`)
- `src/UmbLink.Web/wwwroot/js/app.js` (reutilizar `initHeroGradient`, sem alteração necessária)

---

## Tarefa 1 — UX/UI: Auditoria de alinhamento e consistência

### Problemas identificados na exploração
1. **AdminLayout**: usa Bootstrap dark (`bg-dark`, `text-white`) — inconsistente com o tema do app. Resolvido pela T2.
2. **Dashboard cards de métricas**: inline styles repetitivos — extrair classe reutilizável `.metric-card`.
3. **Botões no footer dos page-cards**: mistura de `btn-ghost` e `btn-accent` com estilos inline redundantes.
4. **Wizard**: campos e botões com `max-width` inconsistente entre steps.
5. **Admin/Users**: tabela usa `table-sm` Bootstrap sem customização do tema — pode quebrar em dark mode.

### Solução
- Extrair `.metric-card` em `site.css` para os 4 cards de métricas do dashboard (já que T5 refatora esses cards)
- Garantir que a tabela em `Admin/Users.razor` respeite `var(--text-primary)` e `var(--bg-card)` no dark mode
- Revisar padding/gap nos botões de ação dos page-cards no mobile (garantir touch target ≥ 44px)
- Wizard: unificar `max-width` do container no step unificado (T3 cuida disso)

### Arquivos afetados
- `src/UmbLink.Web/wwwroot/css/site.css`
- `src/UmbLink.Web/Pages/Admin/Users.razor`
- `src/UmbLink.Web/Pages/Dashboard/Index.razor` (já coberto pela T5)

---

## Decisões Técnicas

| Decisão | Escolha |
|---------|---------|
| Fundo animado dashboard | Gradiente escuro + glass morphism (Opção A) |
| Wizard step links | Unificado no step 3 com 2 cards + preview à direita |
| Preview wizard mobile | Colapsível com toggle "Ver preview" |
| Storage de uploads | Mantém `wwwroot/uploads/` (volume Railway) |
| Admin sidebar ativo | Highlight via `NavigationManager.Uri` |

---

## O que NÃO será feito neste sprint
- Gateway de pagamento real
- Verificação DNS de domínio customizado
- Envio de e-mail (SMTP)
- Testes automatizados novos (além dos existentes)
