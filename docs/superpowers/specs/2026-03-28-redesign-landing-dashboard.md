# Redesign: Landing Page + Dashboard (Light/Dark Theme)

## Summary

Redesign visual completo da Landing Page e Dashboard do UmbLink, introduzindo um sistema de tema light/dark com CSS custom properties, fonte Inter via Google Fonts, animações CSS puras, e visual moderno inspirado em Vercel/Linear (dark) e Notion/Stripe (light).

## Scope

### In scope
- Sistema de tema global (light/dark) via CSS custom properties + `data-theme` attribute
- Toggle de tema na navbar com persistência em `localStorage`
- Landing page: hero com gradiente animado, cards glassmorphism, animações de entrada CSS
- Dashboard Index: cards de páginas redesenhados, responsivos ao tema
- Dashboard Editor: toolbar glass, painel de preview melhorado, visual coeso
- MainLayout: navbar redesenhada com toggle de tema
- PagePreview: atualizado para seguir o novo visual
- Google Fonts (Inter) via CDN

### Out of scope
- Páginas de Auth (Login, Register, ForgotPassword, ResetPassword)
- Páginas de Plans (Index, Checkout, MyPlan)
- Admin pages
- Onboarding Wizard
- ProfilePage pública (será redesenhada em fase posterior)
- GSAP ou qualquer biblioteca JS de animação

## Technical Design

### 1. Sistema de Tema — CSS Custom Properties

Arquivo: `wwwroot/css/site.css`

Definir variáveis em `:root[data-theme="light"]` e `:root[data-theme="dark"]`:

```
Cores:
  --bg-primary        Light: #ffffff       Dark: #0f0f14
  --bg-secondary      Light: #f8f9fc       Dark: #1a1a24
  --bg-card           Light: #ffffff       Dark: #1e1e2a
  --bg-hover          Light: #f3f4f6       Dark: #2a2a3a
  --text-primary      Light: #111827       Dark: #f3f4f6
  --text-secondary    Light: #6b7280       Dark: #9ca3af
  --text-muted        Light: #9ca3af       Dark: #6b7280
  --accent            Light: #6366f1       Dark: #818cf8
  --accent-hover      Light: #4f46e5       Dark: #6366f1
  --border            Light: #e5e7eb       Dark: rgba(255,255,255,0.08)
  --shadow            Light: rgba(0,0,0,0.08) Dark: rgba(0,0,0,0.3)
  --card-shadow       Light: 0 1px 3px var(--shadow) Dark: 0 1px 3px var(--shadow)
  --card-shadow-hover Light: 0 8px 25px var(--shadow) Dark: 0 8px 25px var(--shadow)
  --glass-bg          Light: rgba(255,255,255,0.7)  Dark: rgba(30,30,42,0.7)
  --glass-border      Light: rgba(0,0,0,0.06)       Dark: rgba(255,255,255,0.08)
  --navbar-bg         Light: rgba(255,255,255,0.8)   Dark: rgba(15,15,20,0.8)
```

Default: `data-theme="dark"` (para matching com o hero gradient atual).

### 2. Toggle de Tema — JS Interop

Arquivo: `wwwroot/js/app.js`

Adicionar ao objeto `window.umblink`:

```js
getTheme: () => localStorage.getItem('umblink-theme') || 'dark',
setTheme: (theme) => {
    localStorage.setItem('umblink-theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
},
initTheme: () => {
    const saved = localStorage.getItem('umblink-theme') || 'dark';
    document.documentElement.setAttribute('data-theme', saved);
}
```

Chamar `umblink.initTheme()` inline no `<head>` do `App.razor` (antes do render) para evitar flash of wrong theme.

### 3. App.razor — Mudanças

- Adicionar `<link>` do Google Fonts Inter (weights 400, 500, 600, 700)
- Adicionar `data-theme="dark"` no `<html>`
- Adicionar `<script>` inline mínimo no `<head>` para aplicar tema salvo antes do paint
- Adicionar `<link>` do `site.css` (já existe, mas confirmar ordem — após Bootstrap)

### 4. Landing Page (`Pages/Index.razor`)

#### Hero Section
- Gradiente animado via CSS `@keyframes gradientShift` com `background-size: 400% 400%`
- Cores: transição suave entre indigo, violet, deep-blue, purple
- Mantém visual dark independente do tema (é uma seção de impacto)
- Texto com melhor hierarquia tipográfica: display font bold, tracking tight
- Badge "Novo" com glass effect
- Botões com hover: `transform: translateY(-2px)` + box-shadow glow
- Animação de entrada: `@keyframes fadeInUp` com delays staggered

#### Seção "Como funciona"
- Background: segue o tema (`var(--bg-secondary)`)
- Cards dos 3 passos: glass morphism (`backdrop-filter: blur(12px)`, border semi-transparente)
- Ícones dentro de círculos com gradient sutil
- Animação de entrada: `fadeInUp` com `animation-delay` incremental
- Número do passo como badge com accent color

#### Seção "Recursos"
- Background: `var(--bg-primary)`
- Grid de 6 features em cards com hover lift
- Ícone com cor accent, não cores variadas (coesão visual)
- Hover: `translateY(-4px)` + sombra expandida

#### CTA Final
- Gradiente animado similar ao hero
- Botão grande com glow no hover

#### Footer
- Segue o tema, minimal

### 5. MainLayout (`Layouts/MainLayout.razor`)

- Navbar: `backdrop-filter: blur(12px)`, background semi-transparente (segue tema)
- Logo "UmbLink" com peso bold, cor accent
- Toggle de tema: botão com ícone `bi-sun` / `bi-moon-stars`, transição suave com rotate
- Links de navegação: estilo pill com hover sutil
- Navbar fixa no topo (`position: sticky; top: 0; z-index: 50`)
- Footer: cor muted, segue tema

### 6. Dashboard Index (`Pages/Dashboard/Index.razor`)

- Page header: título + badge do plano, visual limpo
- Cards de páginas:
  - `border-radius: 16px`
  - Sombra suave, hover com lift (`translateY(-2px)`)
  - Border: `1px solid var(--border)`
  - Status badge redesenhado (pill shape, cores mais suaves)
  - Footer com botões ícone mais espaçados e com hover circular
  - Transição suave em tudo: `transition: all 0.2s ease`
- Empty state: ilustração minimalista (via ícone grande + texto), visual acolhedor
- Alert de upgrade: estilo card sutil ao invés de Bootstrap alert
- Modal "Nova Página": cantos arredondados, backdrop blur, segue tema

### 7. Dashboard Editor (`Pages/Dashboard/Editor.razor`)

- Toolbar: sticky top, glass effect (`backdrop-filter: blur`), border-bottom sutil
- Cards de configuração (Info, Links, Aparência):
  - `border-radius: 12px`
  - Border: `var(--border)`
  - Header com peso semibold, sem background (flat)
- Link list items: hover com background sutil, transição suave
- Preview panel: background pattern sutil (dots ou grid) para diferenciar da área de edição
- Modal "Adicionar Link": mesmo estilo da modal de criar página

### 8. PagePreview (`Components/PagePreview.razor`)

- Manter lógica de ThemeConfig existente (cores do usuário)
- Melhorar o container: shadow mais pronunciada, border-radius consistente
- Phone-frame mockup: borda arredondada simulando tela de celular

### 9. Animações CSS

Todas definidas em `site.css`:

```css
@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(20px); }
  to   { opacity: 1; transform: translateY(0); }
}

@keyframes gradientShift {
  0%   { background-position: 0% 50%; }
  50%  { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50%      { transform: translateY(-6px); }
}
```

Utility classes:
- `.animate-fade-in-up` com variantes de delay (`.delay-1`, `.delay-2`, `.delay-3`)
- `.animate-gradient` para backgrounds
- `.hover-lift` para cards com hover translateY

### 10. Classes Utilitárias Customizadas

```css
.glass        { backdrop-filter: blur(12px); background: var(--glass-bg); border: 1px solid var(--glass-border); }
.card-theme   { background: var(--bg-card); border: 1px solid var(--border); border-radius: 16px; box-shadow: var(--card-shadow); transition: all 0.2s ease; }
.card-theme:hover { box-shadow: var(--card-shadow-hover); transform: translateY(-2px); }
.btn-accent   { background: var(--accent); color: white; border: none; border-radius: 10px; transition: all 0.2s ease; }
.btn-accent:hover { background: var(--accent-hover); transform: translateY(-1px); box-shadow: 0 4px 12px rgba(99,102,241,0.3); }
```

## Files to Modify

| File | Change |
|------|--------|
| `src/UmbLink.Web/App.razor` | Google Fonts, data-theme, inline theme init script |
| `src/UmbLink.Web/wwwroot/css/site.css` | CSS variables, animations, utility classes, global styles |
| `src/UmbLink.Web/wwwroot/js/app.js` | Theme toggle functions (getTheme, setTheme, initTheme) |
| `src/UmbLink.Web/Layouts/MainLayout.razor` | Navbar redesign, theme toggle button |
| `src/UmbLink.Web/Pages/Index.razor` | Complete visual redesign with animations |
| `src/UmbLink.Web/Pages/Dashboard/Index.razor` | Card redesign, theme-aware styling |
| `src/UmbLink.Web/Pages/Dashboard/Editor.razor` | Toolbar glass, cards theme-aware, preview bg |
| `src/UmbLink.Web/Components/PagePreview.razor` | Phone-frame container, consistent styling |

## Constraints

- Bootstrap permanece como dependência (grid, utilities básicas), mas visual é customizado via CSS variables
- Sem GSAP ou bibliotecas JS de animação — apenas CSS transitions e @keyframes
- Sem mudanças na lógica de negócio (services, DTOs, etc.)
- Sem mudanças em páginas fora do escopo (Auth, Plans, Admin, Onboarding, ProfilePage)
- Fonte Inter carregada via CDN (Google Fonts), sem self-hosting
- O tema light é o default
