# Design: Melhorias Visuais e Funcionais — Prompt 10
**Data:** 2026-03-29
**Stack:** .NET 8 / Blazor Server / EF Core / SQLite / Bootstrap 5.3.3

---

## Contexto

Projeto UmbLink precisa de 4 grupos de melhorias para entrega:

1. Dashboard dark mode visualmente fraco (cards planos, hover sem profundidade)
2. Admin sem controle de trial por usuário
3. Sem skeletons de carregamento em nenhuma tela
4. Bugs menores de layout, ausência de loading states e confirmação em ações destrutivas

---

## Tarefa 1 — Dashboard Dark Mode

### Problema
- `.page-card` em dark: fundo plano `#151c30`, hover aplica `rgba(255,255,255,0.04)` sem glow ou variação de borda
- Cards admin (`/admin`, `/admin/users`): sem tratamento dark específico, aparência flat
- Sem hierarquia visual entre camadas (página → card → footer do card)

### Solução
Alterações exclusivamente em `src/UmbLink.Web/wwwroot/css/site.css`:

- **`.page-card` dark**: `background: linear-gradient(135deg, #151c30 0%, #1a2440 100%)` (alinhado ao `.card-theme` já existente)
- **`.page-card` hover dark**: `border-color: rgba(85,124,242,0.3)` + `box-shadow: 0 8px 28px rgba(85,124,242,0.15), 0 2px 8px rgba(0,0,0,0.4)` + gradiente levemente elevado
- **`.page-card-footer` dark**: fundo `rgba(0,0,0,0.15)` para diferenciar da body do card
- **`.page-card-body` dark**: sem alteração (herda do card)
- **Admin cards (`.card`)**: já cobertos pelo seletor `[data-theme="dark"] .card` — garantir `transition: all 0.25s ease` presente
- **Variáveis CSS**: adicionar `--skeleton-base` e `--skeleton-highlight` para as duas themes (usadas na Tarefa 3)

### Regras
- Não alterar lógica ou estrutura de componentes
- Todas as mudanças compatíveis com tema light (seletores `[data-theme="dark"]`)

---

## Tarefa 2 — Controle de Trial no Admin

### Decisão de design
Opção **A**: Admin pode conceder trial somente se o usuário ainda não usou aquele plano — respeita a restrição `TrialUsage(UserId, PlanId)`.

### Camadas afetadas

#### DTO (`AdminDto.cs`)
Adicionar campos ao `AdminUserDto`:
```csharp
DateTime? TrialEndsAt,
bool IsOnTrial
```

#### Interface (`IAdminService.cs`)
```csharp
Task<Result<bool>> GrantTrialAsync(Guid adminId, Guid targetUserId, int planId, int durationDays);
```

#### Serviço (`AdminService.cs`)
**Adicionar `ICacheService cache` ao construtor primário** (atualmente ausente em AdminService — `SubscriptionService` já o usa, mas AdminService não tem; necessário para invalidar cache após concessão de trial).

`GrantTrialAsync`:
1. Busca usuário via `userManager.FindByIdAsync` — retorna fail se não encontrado
2. Chama `subRepo.HasUsedTrialAsync(targetUserId, planId)` — retorna fail se já usou
3. Busca plano via `subRepo.GetPlanByIdAsync(planId)` — retorna fail se não encontrado
4. Busca/cria subscription via `subRepo.GetByUserIdAsync` / `subRepo.CreateAsync`
5. Seta `Status = Trial`, `PlanId = planId`, `TrialStartedAt = now`, `TrialEndsAt = now.AddDays(durationDays)`, `UpdatedAt = now`
6. Cria `TrialUsage { UserId = targetUserId, PlanId = planId, Status = TrialStatus.Active, StartedAt = now }`
7. `subRepo.AddTrialUsageAsync(trialUsage)` (método já existe, usado por `SubscriptionService`)
8. `subRepo.SaveChangesAsync()`
9. `cache.RemoveAsync(CacheKeys.UserSubscription(targetUserId))`
10. `audit.LogAsync(adminId, "admin.trial_granted", new { targetUserId, planId, durationDays })`

`GetUsersAsync` enriquece cada `AdminUserDto`:
- Carrega subscription junto com usuário (via `adminUserRepo.GetUsersWithPlanAsync`)
- Popula `TrialEndsAt = sub?.TrialEndsAt`, `IsOnTrial = sub?.Status == SubscriptionStatus.Trial`

#### UI (`Admin/Users.razor`)
- Nova coluna **"Trial"** na tabela: badge "Ativo (Xd)" em verde se ativo, "—" caso contrário
- Novo botão **"Trial"** na coluna de ações
  - Desabilitado se `u.IsOnTrial` (já está em trial) — tooltip "Em trial ativo"
- **Modal de concessão** (inline, sem segundo modal):
  - Seletor de plano: Pro (2) / Business (3) — Free não faz sentido para trial
  - Input numérico: duração em dias (padrão: 7, min: 1, max: 90)
  - Botões: Cancelar / Confirmar
- Toast de sucesso: "Trial concedido com sucesso."
- Toast de erro: mensagem retornada pelo service
- `FormatAction` switch: adicionar `"admin.trial_granted" => "Trial concedido (admin)"`

---

## Tarefa 3 — Skeletons de Carregamento

### CSS (`site.css`)
Novas variáveis e keyframe:

```css
:root, :root[data-theme="light"] {
    --skeleton-base: #e5e7eb;
    --skeleton-highlight: #f3f4f6;
}
:root[data-theme="dark"] {
    --skeleton-base: #1c2540;
    --skeleton-highlight: #243058;
}

@keyframes skeleton-shimmer {
    0%   { background-position: -400px 0; }
    100% { background-position: 400px 0; }
}

.skeleton-block {
    background: linear-gradient(90deg,
        var(--skeleton-base) 25%,
        var(--skeleton-highlight) 50%,
        var(--skeleton-base) 75%);
    background-size: 800px 100%;
    animation: skeleton-shimmer 2s ease-in-out infinite;
    border-radius: 6px;
}

.fade-in-content {
    animation: fadeIn 0.3s ease forwards;
}
```

### Componente `Skeleton.razor`
Arquivo: `src/UmbLink.Web/Shared/Skeleton.razor`

Parâmetros:
- `string Width = "100%"`
- `string Height = "1rem"`
- `string BorderRadius = "6px"`
- `int Lines = 1` — renderiza N blocos empilhados com gap de 0.5rem

### Substituições de `LoadingSpinner`

| Tela | Skeleton gerado |
|------|----------------|
| `Dashboard/Index.razor` | 4 metric cards skeleton + 3 page-card skeleton |
| `Admin/Index.razor` | 8 KPI cards skeleton + 5 linhas de tabela audit |
| `Admin/Users.razor` | 5 linhas de tabela (nome, email, badge, badge, data, botões placeholder) |
| `Dashboard/Metrics.razor` | 3 stat cards + bloco retangular de gráfico (280px height) |

### Regra de transição
Conteúdo real renderizado após `_loading = false` recebe classe `fade-in-content` para opacidade suave 0→1.

**Spinners de botão** (Suspender, Reativar, Confirmar pagamento): **mantidos** — são ações pontuais, correto usar spinner.

---

## Tarefa 4 — Análise Geral + Bug Fixes

### 4.1 Layout de modais

| Local | Problema | Correção |
|-------|----------|----------|
| `Admin/Users.razor` — modal "Alterar Plano" | `h6.fw-bold.mb-1` colado no `<p>` abaixo | `mb-1` → `mb-3`; `font-size: 1rem` no h6 |
| `Plans/Checkout.razor` — título da página | `h4.fw-bold.mb-1` com `p.small.mb-4` OK estruturalmente, mas sem separação visual do card abaixo | Adicionar `border-top: 3px solid var(--accent)` no topo do card para âncora visual |
| Admin inline modal (nova modal de trial) | Garantir padrão: título com `mb-3`, label dos inputs com `form-label-theme` |

### 4.2 Loading states em ações do admin
Adicionar `_suspendingId: Guid?` e `_reactivatingId: Guid?` em `Users.razor`:
- Botão "Suspender": `disabled="@(_suspendingId == u.Id)"` + spinner inline
- Botão "Reativar": `disabled="@(_reactivatingId == u.Id)"` + spinner inline

### 4.3 Confirmação antes de suspender usuário
Usar o `ConfirmModal` existente antes de chamar `SuspendUserAsync` — ação destrutiva (suspender conta) precisa de confirmação explícita.

### 4.4 Audit log — novo action string
`FormatAction` em `Admin/Index.razor`:
```csharp
"admin.trial_granted" => "Trial concedido (admin)",
```

### O que não é tocado
- Gateway de pagamento real (checkout simula com `Task.Delay`)
- Verificação DNS de custom domains
- SMTP / envio de e-mail
- `ProcessExpiredTrialsAsync` scheduled job
- Refactoring de código não relacionado às tarefas

---

## Ordem de implementação

1. CSS: dark mode `page-card` + variáveis skeleton (`site.css`)
2. CSS: `@keyframes skeleton-shimmer` + `.skeleton-block` + `.fade-in-content`
3. Componente `Skeleton.razor`
4. Substituir skeletons: Dashboard/Index → Admin/Index → Admin/Users → Metrics
5. DTO: enriquecer `AdminUserDto` com `TrialEndsAt`, `IsOnTrial`
6. Interface: adicionar `GrantTrialAsync` ao `IAdminService`
7. Service: implementar `GrantTrialAsync` + enriquecer `GetUsersAsync`
8. UI Admin/Users: coluna trial + modal + loading states + `ConfirmModal` para suspend
9. `FormatAction` switch: novo case `"admin.trial_granted"`
10. Modal "Alterar Plano": fix `mb-1` → `mb-3`
11. Checkout: fix separação visual título/card
