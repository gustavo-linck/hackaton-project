# Redesign Landing + Dashboard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Introduce a light/dark theme system with CSS custom properties, redesign the Landing page and Dashboard with modern glassmorphism, animations, and Inter font.

**Architecture:** CSS custom properties on `[data-theme]` drive all colors. JS interop handles toggle + localStorage persistence. Each page uses utility classes (`.glass`, `.card-theme`, `.btn-accent`, `.animate-fade-in-up`) built on these variables. Bootstrap stays for grid/utilities but visual identity comes from custom CSS.

**Tech Stack:** Blazor Server (.NET 8), CSS custom properties, CSS animations (@keyframes), Google Fonts (Inter), Bootstrap 5.3.3 (grid only), vanilla JS (theme toggle).

---

## File Map

| File | Action | Responsibility |
|------|--------|---------------|
| `src/UmbLink.Web/wwwroot/css/site.css` | Rewrite | CSS variables, animations, utility classes, global reset |
| `src/UmbLink.Web/wwwroot/js/app.js` | Modify | Add theme toggle functions (getTheme, setTheme, initTheme) |
| `src/UmbLink.Web/App.razor` | Modify | Google Fonts link, data-theme attribute, inline theme init script |
| `src/UmbLink.Web/Layouts/MainLayout.razor` | Rewrite | Redesigned navbar with theme toggle, sticky, glass effect |
| `src/UmbLink.Web/Pages/Index.razor` | Rewrite | Hero with animated gradient, glassmorphism cards, CSS animations |
| `src/UmbLink.Web/Pages/Dashboard/Index.razor` | Modify | Theme-aware cards, improved hover, redesigned empty state |
| `src/UmbLink.Web/Pages/Dashboard/Editor.razor` | Modify | Glass toolbar, theme-aware cards, preview panel background |
| `src/UmbLink.Web/Components/PagePreview.razor` | Modify | Phone-frame container with improved shadow |

---

### Task 1: CSS Theme System + Global Styles

**Files:**
- Rewrite: `src/UmbLink.Web/wwwroot/css/site.css`

- [ ] **Step 1: Write CSS custom properties for light and dark themes**

Replace the entire contents of `site.css` with:

```css
/* ===== Google Fonts (loaded via App.razor <link>) ===== */

/* ===== CSS Reset & Base ===== */
*, *::before, *::after {
    box-sizing: border-box;
}

html, body {
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    margin: 0;
    padding: 0;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
}

/* ===== Theme Variables ===== */
:root,
:root[data-theme="light"] {
    --bg-primary: #ffffff;
    --bg-secondary: #f8f9fc;
    --bg-card: #ffffff;
    --bg-hover: #f3f4f6;
    --text-primary: #111827;
    --text-secondary: #6b7280;
    --text-muted: #9ca3af;
    --accent: #6366f1;
    --accent-hover: #4f46e5;
    --accent-glow: rgba(99, 102, 241, 0.25);
    --border: #e5e7eb;
    --shadow: rgba(0, 0, 0, 0.08);
    --shadow-lg: rgba(0, 0, 0, 0.12);
    --glass-bg: rgba(255, 255, 255, 0.7);
    --glass-border: rgba(0, 0, 0, 0.06);
    --navbar-bg: rgba(255, 255, 255, 0.8);
    --modal-bg: rgba(0, 0, 0, 0.4);
    --success: #10b981;
    --warning: #f59e0b;
    --danger: #ef4444;
    --info: #3b82f6;
    color-scheme: light;
}

:root[data-theme="dark"] {
    --bg-primary: #0f0f14;
    --bg-secondary: #1a1a24;
    --bg-card: #1e1e2a;
    --bg-hover: #2a2a3a;
    --text-primary: #f3f4f6;
    --text-secondary: #9ca3af;
    --text-muted: #6b7280;
    --accent: #818cf8;
    --accent-hover: #6366f1;
    --accent-glow: rgba(129, 140, 248, 0.25);
    --border: rgba(255, 255, 255, 0.08);
    --shadow: rgba(0, 0, 0, 0.3);
    --shadow-lg: rgba(0, 0, 0, 0.5);
    --glass-bg: rgba(30, 30, 42, 0.7);
    --glass-border: rgba(255, 255, 255, 0.08);
    --navbar-bg: rgba(15, 15, 20, 0.8);
    --modal-bg: rgba(0, 0, 0, 0.6);
    --success: #34d399;
    --warning: #fbbf24;
    --danger: #f87171;
    --info: #60a5fa;
    color-scheme: dark;
}

body {
    background: var(--bg-primary);
    color: var(--text-primary);
    transition: background 0.3s ease, color 0.3s ease;
}

/* ===== Animations ===== */
@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(20px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

@keyframes fadeIn {
    from { opacity: 0; }
    to   { opacity: 1; }
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

@keyframes pulse-glow {
    0%, 100% { box-shadow: 0 0 0 0 var(--accent-glow); }
    50%      { box-shadow: 0 0 20px 4px var(--accent-glow); }
}

@keyframes spin-slow {
    from { transform: rotate(0deg); }
    to   { transform: rotate(360deg); }
}

.animate-fade-in-up {
    animation: fadeInUp 0.6s ease forwards;
    opacity: 0;
}
.animate-fade-in-up.delay-1 { animation-delay: 0.1s; }
.animate-fade-in-up.delay-2 { animation-delay: 0.2s; }
.animate-fade-in-up.delay-3 { animation-delay: 0.3s; }
.animate-fade-in-up.delay-4 { animation-delay: 0.4s; }
.animate-fade-in-up.delay-5 { animation-delay: 0.5s; }

.animate-fade-in {
    animation: fadeIn 0.5s ease forwards;
    opacity: 0;
}

.animate-gradient {
    background-size: 400% 400%;
    animation: gradientShift 8s ease infinite;
}

.animate-float {
    animation: float 3s ease-in-out infinite;
}

/* ===== Utility Classes ===== */
.glass {
    backdrop-filter: blur(12px);
    -webkit-backdrop-filter: blur(12px);
    background: var(--glass-bg);
    border: 1px solid var(--glass-border);
}

.card-theme {
    background: var(--bg-card);
    border: 1px solid var(--border);
    border-radius: 16px;
    box-shadow: 0 1px 3px var(--shadow);
    transition: all 0.25s ease;
}

.card-theme:hover {
    box-shadow: 0 8px 25px var(--shadow-lg);
    transform: translateY(-2px);
}

.card-theme-static {
    background: var(--bg-card);
    border: 1px solid var(--border);
    border-radius: 16px;
    box-shadow: 0 1px 3px var(--shadow);
}

.hover-lift {
    transition: all 0.25s ease;
}
.hover-lift:hover {
    transform: translateY(-4px);
    box-shadow: 0 12px 30px var(--shadow-lg);
}

.btn-accent {
    background: var(--accent);
    color: white;
    border: none;
    border-radius: 10px;
    padding: 0.5rem 1.25rem;
    font-weight: 600;
    font-size: 0.875rem;
    transition: all 0.2s ease;
    cursor: pointer;
    text-decoration: none;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
}
.btn-accent:hover {
    background: var(--accent-hover);
    color: white;
    transform: translateY(-1px);
    box-shadow: 0 4px 12px var(--accent-glow);
}

.btn-accent-lg {
    padding: 0.75rem 2rem;
    font-size: 1rem;
    border-radius: 12px;
}

.btn-accent-outline {
    background: transparent;
    color: var(--accent);
    border: 2px solid var(--accent);
    border-radius: 10px;
    padding: 0.5rem 1.25rem;
    font-weight: 600;
    font-size: 0.875rem;
    transition: all 0.2s ease;
    cursor: pointer;
    text-decoration: none;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
}
.btn-accent-outline:hover {
    background: var(--accent);
    color: white;
    transform: translateY(-1px);
}

.btn-ghost {
    background: transparent;
    color: var(--text-secondary);
    border: 1px solid var(--border);
    border-radius: 10px;
    padding: 0.4rem 0.75rem;
    font-size: 0.8rem;
    transition: all 0.2s ease;
    cursor: pointer;
    text-decoration: none;
    display: inline-flex;
    align-items: center;
    gap: 0.375rem;
}
.btn-ghost:hover {
    background: var(--bg-hover);
    color: var(--text-primary);
    border-color: var(--text-muted);
}

.text-theme-primary { color: var(--text-primary); }
.text-theme-secondary { color: var(--text-secondary); }
.text-theme-muted { color: var(--text-muted); }
.text-accent { color: var(--accent); }
.bg-theme-primary { background: var(--bg-primary); }
.bg-theme-secondary { background: var(--bg-secondary); }
.bg-theme-card { background: var(--bg-card); }

.section-padding { padding: 5rem 0; }

/* ===== Navbar ===== */
.navbar-theme {
    position: sticky;
    top: 0;
    z-index: 50;
    backdrop-filter: blur(12px);
    -webkit-backdrop-filter: blur(12px);
    background: var(--navbar-bg);
    border-bottom: 1px solid var(--border);
    padding: 0.75rem 1.5rem;
    transition: background 0.3s ease;
}

.navbar-theme .nav-brand {
    font-weight: 700;
    font-size: 1.25rem;
    color: var(--accent);
    text-decoration: none;
}

.navbar-theme .nav-link-theme {
    color: var(--text-secondary);
    text-decoration: none;
    font-weight: 500;
    font-size: 0.875rem;
    padding: 0.375rem 0.875rem;
    border-radius: 8px;
    transition: all 0.2s ease;
}
.navbar-theme .nav-link-theme:hover {
    color: var(--text-primary);
    background: var(--bg-hover);
}

.theme-toggle {
    background: var(--bg-hover);
    border: 1px solid var(--border);
    border-radius: 8px;
    padding: 0.375rem 0.5rem;
    cursor: pointer;
    color: var(--text-secondary);
    font-size: 1rem;
    transition: all 0.3s ease;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    line-height: 1;
}
.theme-toggle:hover {
    color: var(--accent);
    border-color: var(--accent);
}
.theme-toggle i {
    transition: transform 0.3s ease;
}
.theme-toggle:hover i {
    transform: rotate(20deg);
}

/* ===== Modal Overrides ===== */
.modal-theme-backdrop {
    position: fixed;
    inset: 0;
    background: var(--modal-bg);
    backdrop-filter: blur(4px);
    -webkit-backdrop-filter: blur(4px);
    z-index: 1050;
    display: flex;
    align-items: center;
    justify-content: center;
    animation: fadeIn 0.2s ease;
}

.modal-theme {
    background: var(--bg-card);
    border: 1px solid var(--border);
    border-radius: 16px;
    box-shadow: 0 20px 60px var(--shadow-lg);
    width: 100%;
    max-width: 480px;
    margin: 1rem;
    animation: fadeInUp 0.3s ease;
}

.modal-theme-header {
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid var(--border);
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.modal-theme-header h5 {
    margin: 0;
    font-weight: 600;
    color: var(--text-primary);
}

.modal-theme-body {
    padding: 1.5rem;
}

.modal-theme-footer {
    padding: 1rem 1.5rem;
    border-top: 1px solid var(--border);
    display: flex;
    justify-content: flex-end;
    gap: 0.5rem;
}

/* ===== Form Overrides ===== */
.form-control-theme {
    background: var(--bg-primary);
    border: 1px solid var(--border);
    border-radius: 10px;
    color: var(--text-primary);
    padding: 0.5rem 0.75rem;
    font-size: 0.875rem;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
}
.form-control-theme:focus {
    outline: none;
    border-color: var(--accent);
    box-shadow: 0 0 0 3px var(--accent-glow);
}
.form-control-theme::placeholder {
    color: var(--text-muted);
}

.form-label-theme {
    font-size: 0.8rem;
    font-weight: 600;
    color: var(--text-secondary);
    margin-bottom: 0.375rem;
}

/* ===== Badge ===== */
.badge-theme {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    padding: 0.25rem 0.625rem;
    border-radius: 50px;
    font-size: 0.7rem;
    font-weight: 600;
}
.badge-success {
    background: rgba(16, 185, 129, 0.1);
    color: var(--success);
}
.badge-muted {
    background: var(--bg-hover);
    color: var(--text-muted);
}
.badge-accent {
    background: rgba(99, 102, 241, 0.1);
    color: var(--accent);
}

/* ===== Hero (always dark, independent of theme) ===== */
.hero-section {
    background: linear-gradient(135deg, #0f0c29, #302b63, #24243e, #1a1045);
    background-size: 400% 400%;
    animation: gradientShift 8s ease infinite;
    color: #ffffff;
    position: relative;
    overflow: hidden;
}

.hero-section::before {
    content: '';
    position: absolute;
    inset: 0;
    background: radial-gradient(ellipse at 30% 20%, rgba(99, 102, 241, 0.15), transparent 60%),
                radial-gradient(ellipse at 70% 80%, rgba(168, 85, 247, 0.1), transparent 60%);
    pointer-events: none;
}

.hero-nav {
    position: relative;
    z-index: 10;
    padding: 1.25rem 2rem;
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.hero-content {
    position: relative;
    z-index: 10;
    min-height: 70vh;
    display: flex;
    align-items: center;
    justify-content: center;
    text-align: center;
    padding: 2rem 1.5rem 4rem;
}

.hero-title {
    font-size: clamp(2rem, 5vw, 3.5rem);
    font-weight: 800;
    line-height: 1.1;
    letter-spacing: -0.02em;
    margin-bottom: 1.25rem;
}

.hero-title .highlight {
    background: linear-gradient(135deg, #fbbf24, #f59e0b);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
}

.hero-subtitle {
    font-size: 1.125rem;
    color: rgba(255, 255, 255, 0.6);
    max-width: 520px;
    margin: 0 auto 2.5rem;
    line-height: 1.6;
}

.hero-badge {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    background: rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(8px);
    border: 1px solid rgba(255, 255, 255, 0.15);
    border-radius: 50px;
    padding: 0.5rem 1.25rem;
    font-size: 0.85rem;
    font-weight: 500;
    margin-bottom: 1.5rem;
    color: #ffffff;
}

.hero-cta-primary {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    background: #6366f1;
    color: #ffffff;
    border: none;
    border-radius: 12px;
    padding: 0.875rem 2rem;
    font-size: 1rem;
    font-weight: 600;
    text-decoration: none;
    transition: all 0.25s ease;
}
.hero-cta-primary:hover {
    background: #4f46e5;
    color: #ffffff;
    transform: translateY(-2px);
    box-shadow: 0 8px 25px rgba(99, 102, 241, 0.35);
}

.hero-cta-secondary {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    background: transparent;
    color: #ffffff;
    border: 1px solid rgba(255, 255, 255, 0.3);
    border-radius: 12px;
    padding: 0.875rem 2rem;
    font-size: 1rem;
    font-weight: 500;
    text-decoration: none;
    transition: all 0.25s ease;
}
.hero-cta-secondary:hover {
    background: rgba(255, 255, 255, 0.1);
    color: #ffffff;
    border-color: rgba(255, 255, 255, 0.5);
    transform: translateY(-2px);
}

/* ===== Landing Sections ===== */
.section-steps, .section-features, .section-cta {
    position: relative;
}

.section-title {
    font-size: 1.875rem;
    font-weight: 700;
    letter-spacing: -0.01em;
    color: var(--text-primary);
    margin-bottom: 0.5rem;
}

.section-subtitle {
    color: var(--text-secondary);
    font-size: 1rem;
    margin-bottom: 3rem;
}

.step-card {
    background: var(--bg-card);
    border: 1px solid var(--border);
    border-radius: 16px;
    padding: 2rem 1.5rem;
    text-align: center;
    transition: all 0.25s ease;
}
.step-card:hover {
    transform: translateY(-4px);
    box-shadow: 0 12px 30px var(--shadow-lg);
}

.step-icon {
    width: 64px;
    height: 64px;
    border-radius: 16px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
    margin-bottom: 1rem;
}

.step-card h5 {
    font-weight: 700;
    font-size: 1.05rem;
    color: var(--text-primary);
    margin-bottom: 0.5rem;
}

.step-card p {
    color: var(--text-secondary);
    font-size: 0.875rem;
    line-height: 1.6;
    margin: 0;
}

.feature-card {
    display: flex;
    gap: 1rem;
    padding: 1.25rem;
    border-radius: 12px;
    border: 1px solid transparent;
    transition: all 0.25s ease;
}
.feature-card:hover {
    background: var(--bg-hover);
    border-color: var(--border);
}

.feature-icon {
    flex-shrink: 0;
    width: 42px;
    height: 42px;
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.15rem;
    background: rgba(99, 102, 241, 0.1);
    color: var(--accent);
}

.feature-card h6 {
    font-weight: 600;
    font-size: 0.925rem;
    color: var(--text-primary);
    margin-bottom: 0.25rem;
}

.feature-card p {
    color: var(--text-secondary);
    font-size: 0.825rem;
    line-height: 1.5;
    margin: 0;
}

/* ===== CTA Section (always dark) ===== */
.cta-section {
    background: linear-gradient(135deg, #0f0c29, #302b63, #1a1045);
    background-size: 400% 400%;
    animation: gradientShift 8s ease infinite;
    color: #ffffff;
    padding: 5rem 0;
    text-align: center;
}

/* ===== Footer ===== */
.footer-theme {
    background: var(--bg-secondary);
    color: var(--text-muted);
    text-align: center;
    padding: 1rem;
    font-size: 0.8rem;
    border-top: 1px solid var(--border);
}
.footer-theme a {
    color: var(--text-secondary);
    text-decoration: none;
}
.footer-theme a:hover {
    color: var(--accent);
}

/* ===== Dashboard Cards ===== */
.page-card {
    background: var(--bg-card);
    border: 1px solid var(--border);
    border-radius: 16px;
    overflow: hidden;
    transition: all 0.25s ease;
}
.page-card:hover {
    box-shadow: 0 8px 25px var(--shadow-lg);
    transform: translateY(-2px);
}

.page-card-body {
    padding: 1.25rem;
}

.page-card-footer {
    padding: 0.75rem 1.25rem;
    border-top: 1px solid var(--border);
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
}

.page-card-footer .btn-ghost {
    flex-grow: 0;
}

/* ===== Editor ===== */
.editor-toolbar {
    position: sticky;
    top: 0;
    z-index: 40;
    backdrop-filter: blur(12px);
    -webkit-backdrop-filter: blur(12px);
    background: var(--navbar-bg);
    border-bottom: 1px solid var(--border);
    padding: 0.625rem 1rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.editor-panel {
    background: var(--bg-primary);
    overflow-y: auto;
    padding: 1.25rem;
}

.preview-panel {
    background: var(--bg-secondary);
    background-image: radial-gradient(circle, var(--border) 1px, transparent 1px);
    background-size: 20px 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow-y: auto;
    padding: 2rem;
}

.editor-card {
    background: var(--bg-card);
    border: 1px solid var(--border);
    border-radius: 12px;
    margin-bottom: 1rem;
}

.editor-card-header {
    padding: 0.875rem 1rem;
    font-weight: 600;
    font-size: 0.9rem;
    color: var(--text-primary);
    border-bottom: 1px solid var(--border);
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.editor-card-body {
    padding: 1rem;
}

.link-item {
    display: flex;
    align-items: center;
    gap: 0.625rem;
    padding: 0.75rem 1rem;
    border-bottom: 1px solid var(--border);
    transition: background 0.15s ease;
}
.link-item:last-child {
    border-bottom: none;
}
.link-item:hover {
    background: var(--bg-hover);
}

/* ===== Phone Frame (Preview) ===== */
.phone-frame {
    width: 320px;
    min-height: 560px;
    border-radius: 32px;
    overflow: hidden;
    box-shadow: 0 8px 40px var(--shadow-lg), 0 0 0 1px var(--border);
    position: relative;
}

.phone-frame::before {
    content: '';
    position: absolute;
    top: 8px;
    left: 50%;
    transform: translateX(-50%);
    width: 80px;
    height: 4px;
    background: rgba(128, 128, 128, 0.3);
    border-radius: 4px;
    z-index: 10;
}

/* ===== Alert Theme ===== */
.alert-theme {
    background: rgba(99, 102, 241, 0.08);
    border: 1px solid rgba(99, 102, 241, 0.2);
    border-radius: 12px;
    padding: 0.75rem 1rem;
    color: var(--text-primary);
    font-size: 0.85rem;
    display: flex;
    align-items: center;
    gap: 0.625rem;
}

/* ===== Scrollbar ===== */
::-webkit-scrollbar {
    width: 6px;
}
::-webkit-scrollbar-track {
    background: transparent;
}
::-webkit-scrollbar-thumb {
    background: var(--border);
    border-radius: 3px;
}
::-webkit-scrollbar-thumb:hover {
    background: var(--text-muted);
}

/* ===== Blazor overrides ===== */
#blazor-error-ui {
    background: lightyellow;
    bottom: 0;
    box-shadow: 0 -1px 2px rgba(0, 0, 0, 0.2);
    display: none;
    left: 0;
    padding: 0.6rem 1.25rem 0.7rem 1.25rem;
    position: fixed;
    width: 100%;
    z-index: 1000;
}

#blazor-error-ui .dismiss {
    cursor: pointer;
    position: absolute;
    right: 0.75rem;
    top: 0.5rem;
}

.blazor-error-boundary {
    background: url(data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNTYiIGhlaWdodD0iNDkiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIG92ZXJmbG93PSJoaWRkZW4iPjxkZWZzPjxjbGlwUGF0aCBpZD0iY2xpcDAiPjxyZWN0IHg9IjIzNSIgeT0iNTEiIHdpZHRoPSI1NiIgaGVpZ2h0PSI0OSIvPjwvY2xpcFBhdGg+PC9kZWZzPjxnIGNsaXAtcGF0aD0idXJsKCNjbGlwMCkiIHRyYW5zZm9ybT0idHJhbnNsYXRlKC0yMzUgLTUxKSI+PHBhdGggZD0iTTI2My41MDYgNTFDMjY0LjcxNyA1MSAyNjUuODEzIDUxLjQ4MzcgMjY2LjYwNiA1Mi4yNjU4TDI2Ny4wNTIgNTIuNzk4NyAyNjcuNTM5IDUzLjYyODMgMjkwLjE4NSA5Mi4xODMxIDI5MC41NDUgOTIuNzk1IDI5MC42NTYgOTIuOTk2QzI5MC44NzcgOTMuNTEzIDI5MSA5NC4wODE1IDI5MSA5NC42NzgyIDI5MSA5Ny4wNjUxIDI4OS4wMzggOTkgMjg2LjYxNyA5OUwyNDAuMzgzIDk5QzIzNy45NjMgOTkgMjM2IDk3LjA2NTEgMjM2IDk0LjY3ODIgMjM2IDk0LjM3OTkgMjM2LjAzMSA5NC4wODg2IDIzNi4wODkgOTMuODA3MkwyMzYuMzM4IDkzLjAxNjIgMjM2Ljg1OCA5Mi4xMzE0IDI1OS40NzMgNTMuNjI5NCAyNTkuOTYxIDUyLjc5ODUgMjYwLjQwNyA1Mi4yNjU4QzI2MS4yIDUxLjQ4MzcgMjYyLjI5NiA1MSAyNjMuNTA2IDUxWk0yNjMuNTg2IDY2LjAxODNDMjYwLjczNyA2Ni4wMTgzIDI1OS4zMTMgNjcuMTI0NSAyNTkuMzEzIDY5LjMzNyAyNTkuMzEzIDY5LjYxMDIgMjU5LjMzMiA2OS44NjA4IDI1OS4zNzEgNzAuMDg4N0wyNjEuNzk1IDg0LjAxNjEgMjY1LjM4IDg0LjAxNjEgMjY3LjgyMSA2OS43NDc1QzI2Ny44NiA2OS43MzA5IDI2Ny44NzkgNjkuNTg3NyAyNjcuODc5IDY5LjMxNzkgMjY3Ljg3OSA2Ny4xMTgyIDI2Ni40NDggNjYuMDE4MyAyNjMuNTg2IDY2LjAxODNaTTI2My41NzYgODYuMDU0N0MyNjEuMDQ5IDg2LjA1NDcgMjU5Ljc4NiA4Ny4zMDA1IDI1OS43ODYgODkuNzkyMSAyNTkuNzg2IDkyLjI4MzcgMjYxLjA0OSA5My41Mjk1IDI2My41NzYgOTMuNTI5NSAyNjYuMTE2IDkzLjUyOTUgMjY3LjM4NyA5Mi4yODM3IDI2Ny4zODcgODkuNzkyMSAyNjcuMzg3IDg3LjMwMDUgMjY2LjExNiA4Ni4wNTQ3IDI2My41NzYgODYuMDU0N1oiIGZpbGw9IiNGRkU1MDAiIGZpbGwtcnVsZT0iZXZlbm9kZCIvPjwvZz48L3N2Zz4=) no-repeat 1rem/1.8rem, #b32121;
    padding: 1rem 1rem 1rem 3.7rem;
    color: white;
}

.blazor-error-boundary::after {
    content: "An error has occurred."
}

/* ===== Responsive ===== */
@media (max-width: 768px) {
    .hero-nav {
        padding: 1rem;
    }
    .hero-content {
        padding: 1.5rem 1rem 3rem;
    }
    .navbar-theme {
        padding: 0.625rem 1rem;
    }
}
```

- [ ] **Step 2: Verify the file was saved correctly**

Run: `wc -l "src/UmbLink.Web/wwwroot/css/site.css"`
Expected: approximately 550-600 lines

- [ ] **Step 3: Commit**

```bash
git add src/UmbLink.Web/wwwroot/css/site.css
git commit -m "feat: CSS theme system with light/dark variables, animations, and utility classes"
```

---

### Task 2: Theme Toggle JS + App.razor Setup

**Files:**
- Modify: `src/UmbLink.Web/wwwroot/js/app.js`
- Modify: `src/UmbLink.Web/App.razor`

- [ ] **Step 1: Add theme functions to app.js**

Add these three functions to the `window.umblink` object in `app.js`, after the existing `scrollToTop` function:

```js
getTheme: () => localStorage.getItem('umblink-theme') || 'light',
setTheme: (theme) => {
    localStorage.setItem('umblink-theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
},
initTheme: () => {
    const saved = localStorage.getItem('umblink-theme') || 'light';
    document.documentElement.setAttribute('data-theme', saved);
}
```

- [ ] **Step 2: Update App.razor**

Replace the entire `App.razor` with:

```html
<!DOCTYPE html>
<html lang="pt-BR" data-theme="light">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <title>UmbLink</title>

    <!-- Theme init (must run before paint to avoid flash) -->
    <script>
        (function() {
            var t = localStorage.getItem('umblink-theme') || 'light';
            document.documentElement.setAttribute('data-theme', t);
        })();
    </script>

    <!-- Fonts -->
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet" />

    <!-- Styles -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />
    <link rel="stylesheet" href="css/site.css" />

    <HeadOutlet @rendermode="@(new InteractiveServerRenderMode(prerender: false))" />
</head>
<body>
    <Routes @rendermode="@(new InteractiveServerRenderMode(prerender: false))" />
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sortablejs@1.15.2/Sortable.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.2/dist/chart.umd.min.js"></script>
    <script src="js/app.js"></script>
    <script src="_framework/blazor.server.js"></script>
</body>
</html>
```

Key changes:
- `data-theme="light"` on `<html>` tag
- Inline `<script>` in `<head>` that reads localStorage before paint
- Google Fonts `<link>` for Inter (weights 400, 500, 600, 700, 800)
- `css/site.css` loaded after Bootstrap (so our variables override)

- [ ] **Step 3: Commit**

```bash
git add src/UmbLink.Web/wwwroot/js/app.js src/UmbLink.Web/App.razor
git commit -m "feat: theme toggle JS functions and App.razor with Inter font + theme init"
```

---

### Task 3: MainLayout Redesign

**Files:**
- Rewrite: `src/UmbLink.Web/Layouts/MainLayout.razor`

- [ ] **Step 1: Rewrite MainLayout.razor**

Replace the entire contents with:

```razor
@inherits LayoutComponentBase
@inject IJSRuntime JS

<div class="min-vh-100 d-flex flex-column" style="background:var(--bg-primary)">
    <nav class="navbar-theme d-flex align-items-center">
        <a class="nav-brand" href="/dashboard">UmbLink</a>

        <div class="ms-auto d-flex align-items-center gap-2">
            <AuthorizeView>
                <Authorized>
                    <span class="d-none d-md-inline text-theme-secondary" style="font-size:.8rem">
                        <i class="bi bi-person-circle me-1"></i>@context.User.FindFirst("display_name")?.Value
                    </span>
                    <a href="/dashboard" class="nav-link-theme">Dashboard</a>
                    <a href="/plans" class="nav-link-theme">Planos</a>
                    <a href="/my-plan" class="nav-link-theme">Meu Plano</a>
                    <button class="theme-toggle" @onclick="ToggleTheme" title="Alternar tema">
                        <i class="bi @(_isDark ? "bi-sun" : "bi-moon-stars")"></i>
                    </button>
                    <form action="/auth/logout" method="post">
                        <AntiforgeryToken />
                        <button type="submit" class="btn-ghost" style="color:var(--danger)">
                            <i class="bi bi-box-arrow-right"></i> Sair
                        </button>
                    </form>
                </Authorized>
                <NotAuthorized>
                    <button class="theme-toggle" @onclick="ToggleTheme" title="Alternar tema">
                        <i class="bi @(_isDark ? "bi-sun" : "bi-moon-stars")"></i>
                    </button>
                    <a href="/auth/login" class="btn-ghost">Entrar</a>
                </NotAuthorized>
            </AuthorizeView>
        </div>
    </nav>

    <main class="flex-grow-1">
        @Body
    </main>

    <footer class="footer-theme">
        &copy; @DateTime.Now.Year UmbLink by Umbler
    </footer>
</div>

@code {
    bool _isDark;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var theme = await JS.InvokeAsync<string>("umblink.getTheme");
            _isDark = theme == "dark";
            StateHasChanged();
        }
    }

    async Task ToggleTheme()
    {
        _isDark = !_isDark;
        await JS.InvokeVoidAsync("umblink.setTheme", _isDark ? "dark" : "light");
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add src/UmbLink.Web/Layouts/MainLayout.razor
git commit -m "feat: redesigned MainLayout with theme toggle, glass navbar, sticky positioning"
```

---

### Task 4: Landing Page Redesign

**Files:**
- Rewrite: `src/UmbLink.Web/Pages/Index.razor`

- [ ] **Step 1: Rewrite Index.razor**

Replace the entire contents with:

```razor
@page "/"
@layout PublicLayout

<!-- Hero Section (always dark) -->
<div class="hero-section">
    <div class="hero-nav">
        <a href="/" class="nav-brand" style="color:#fff;font-size:1.35rem">UmbLink</a>
        <div class="d-flex gap-2 align-items-center">
            <a href="/auth/login" class="hero-cta-secondary" style="padding:.5rem 1.25rem;font-size:.875rem">Entrar</a>
            <a href="/auth/register" class="hero-cta-primary" style="padding:.5rem 1.25rem;font-size:.875rem">Criar grátis</a>
        </div>
    </div>

    <div class="hero-content">
        <div class="animate-fade-in-up">
            <div class="hero-badge animate-fade-in-up delay-1">
                <i class="bi bi-stars"></i>
                Novo — planos a partir de R$19/mês
            </div>
            <h1 class="hero-title animate-fade-in-up delay-2">
                Todos os seus links<br/>em <span class="highlight">um só lugar</span>
            </h1>
            <p class="hero-subtitle animate-fade-in-up delay-3">
                Crie sua página personalizada em segundos e compartilhe com o mundo.
                Ideal para criadores, profissionais e marcas.
            </p>
            <div class="d-flex gap-3 justify-content-center flex-wrap animate-fade-in-up delay-4">
                <a href="/auth/register" class="hero-cta-primary">
                    <i class="bi bi-rocket-takeoff"></i>Começar grátis
                </a>
                <a href="/p/joao-silva" class="hero-cta-secondary" target="_blank">
                    <i class="bi bi-eye"></i>Ver exemplo
                </a>
            </div>
            <p class="animate-fade-in-up delay-5" style="color:rgba(255,255,255,0.4);font-size:.85rem;margin-top:1rem">
                Sem cartão de crédito. Plano gratuito para sempre.
            </p>
        </div>
    </div>
</div>

<!-- Como funciona -->
<div class="section-steps section-padding bg-theme-secondary">
    <div class="container">
        <div class="text-center">
            <h2 class="section-title animate-fade-in-up">Como funciona</h2>
            <p class="section-subtitle animate-fade-in-up delay-1">Em menos de 3 minutos você tem sua página pronta</p>
        </div>
        <div class="row g-4 justify-content-center">
            <div class="col-md-4">
                <div class="step-card animate-fade-in-up delay-2">
                    <div class="step-icon" style="background:rgba(99,102,241,0.1);color:var(--accent)">
                        <i class="bi bi-person-plus"></i>
                    </div>
                    <div class="badge-accent badge-theme mb-2">Passo 1</div>
                    <h5>Crie sua conta</h5>
                    <p>Cadastre-se gratuitamente em segundos. Sem cartão de crédito.</p>
                </div>
            </div>
            <div class="col-md-4">
                <div class="step-card animate-fade-in-up delay-3">
                    <div class="step-icon" style="background:rgba(245,158,11,0.1);color:var(--warning)">
                        <i class="bi bi-palette"></i>
                    </div>
                    <div class="badge-theme" style="background:rgba(245,158,11,0.1);color:var(--warning)">Passo 2</div>
                    <h5>Personalize</h5>
                    <p>Escolha um tema, adicione seus links e customize as cores da sua página.</p>
                </div>
            </div>
            <div class="col-md-4">
                <div class="step-card animate-fade-in-up delay-4">
                    <div class="step-icon" style="background:rgba(16,185,129,0.1);color:var(--success)">
                        <i class="bi bi-share"></i>
                    </div>
                    <div class="badge-theme" style="background:rgba(16,185,129,0.1);color:var(--success)">Passo 3</div>
                    <h5>Compartilhe</h5>
                    <p>Um único link para todas as suas redes, projetos e contatos.</p>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Recursos -->
<div class="section-features section-padding bg-theme-primary">
    <div class="container">
        <div class="text-center">
            <h2 class="section-title animate-fade-in-up">Tudo que você precisa</h2>
            <p class="section-subtitle animate-fade-in-up delay-1">Gratuito para sempre, com planos premium para quem quer mais</p>
        </div>
        <div class="row g-3">
            <div class="col-md-4 animate-fade-in-up delay-2">
                <div class="feature-card">
                    <div class="feature-icon"><i class="bi bi-bar-chart-fill"></i></div>
                    <div>
                        <h6>Analytics em tempo real</h6>
                        <p>Veja quantas pessoas visitam sua página e quais links clicam mais.</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4 animate-fade-in-up delay-2">
                <div class="feature-card">
                    <div class="feature-icon"><i class="bi bi-palette2"></i></div>
                    <div>
                        <h6>Temas personalizáveis</h6>
                        <p>Escolha entre temas prontos ou customize cores, fontes e botões.</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4 animate-fade-in-up delay-3">
                <div class="feature-card">
                    <div class="feature-icon"><i class="bi bi-globe2"></i></div>
                    <div>
                        <h6>Domínio próprio</h6>
                        <p>Vincule seu domínio personalizado e deixe sua página ainda mais profissional.</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4 animate-fade-in-up delay-3">
                <div class="feature-card">
                    <div class="feature-icon"><i class="bi bi-phone"></i></div>
                    <div>
                        <h6>100% responsivo</h6>
                        <p>Perfeito em qualquer dispositivo — celular, tablet ou computador.</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4 animate-fade-in-up delay-4">
                <div class="feature-card">
                    <div class="feature-icon"><i class="bi bi-arrow-up-down"></i></div>
                    <div>
                        <h6>Reordenação fácil</h6>
                        <p>Arraste e solte para organizar seus links na ordem que quiser.</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4 animate-fade-in-up delay-4">
                <div class="feature-card">
                    <div class="feature-icon"><i class="bi bi-eye-slash"></i></div>
                    <div>
                        <h6>Ativar / desativar links</h6>
                        <p>Esconda links temporariamente sem precisar excluir.</p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- CTA Final -->
<div class="cta-section">
    <div class="container animate-fade-in-up">
        <h2 style="font-weight:700;font-size:1.875rem;margin-bottom:0.75rem">Pronto para começar?</h2>
        <p style="color:rgba(255,255,255,0.5);margin-bottom:1.5rem">Crie sua página gratuita agora. Sem cartão de crédito.</p>
        <a href="/auth/register" class="hero-cta-primary btn-accent-lg">
            <i class="bi bi-rocket-takeoff"></i>Criar minha página grátis
        </a>
    </div>
</div>

<!-- Footer -->
<footer class="footer-theme">
    &copy; @DateTime.Now.Year UmbLink by Umbler — <a href="/auth/login">Entrar</a>
</footer>
```

- [ ] **Step 2: Commit**

```bash
git add src/UmbLink.Web/Pages/Index.razor
git commit -m "feat: landing page redesign with animated gradient hero, glassmorphism cards, CSS animations"
```

---

### Task 5: Dashboard Index Redesign

**Files:**
- Modify: `src/UmbLink.Web/Pages/Dashboard/Index.razor`

- [ ] **Step 1: Replace the template section (lines 8-136) of Dashboard/Index.razor**

Keep the `@page`, `@attribute`, `@inject` directives (lines 1-5) and `@code` block (lines 138+) unchanged. Replace only the template (everything between the directives and `@code`):

```razor
<PageTitle>Dashboard — UmbLink</PageTitle>

<TrialBanner TrialEndsAt="_sub?.TrialEndsAt" />

<div class="container py-4" style="max-width:960px">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 style="font-weight:700;color:var(--text-primary);margin:0">Minhas Páginas</h4>
            @if (_sub is not null)
            {
                <span class="text-theme-secondary" style="font-size:.8rem">Plano <PlanBadge PlanName="@_sub.PlanName" /></span>
            }
        </div>
        <button class="btn-accent" @onclick="ShowCreateModal">
            <i class="bi bi-plus-lg"></i>Nova página
        </button>
    </div>

    @if (_sub?.PlanName == "Free" && _pages.Count >= 1)
    {
        <div class="alert-theme mb-3">
            <i class="bi bi-stars text-accent flex-shrink-0"></i>
            <span class="flex-grow-1">Você está no plano gratuito — <strong>@_pages.Count/1</strong> página usada.
                <a href="/plans" style="color:var(--accent);font-weight:600">Fazer upgrade</a> para até 3 páginas e links ilimitados.</span>
        </div>
    }

    <LoadingSpinner Loading="_loading">
        @if (_pages.Count == 0)
        {
            <div class="text-center py-5">
                <div class="animate-fade-in-up">
                    <i class="bi bi-stars" style="font-size:3rem;color:var(--accent);display:block;margin-bottom:1rem"></i>
                    <h5 style="font-weight:700;color:var(--text-primary)">Crie sua primeira página!</h5>
                    <p class="text-theme-secondary" style="max-width:360px;margin:0 auto 1.5rem">
                        Junte todos os seus links em um só lugar e compartilhe com o mundo.
                    </p>
                    <button class="btn-accent btn-accent-lg" @onclick="ShowCreateModal">
                        <i class="bi bi-rocket-takeoff"></i>Criar minha página
                    </button>
                    <p class="text-theme-muted" style="font-size:.8rem;margin-top:0.75rem">Grátis, sem cartão. Pronto em minutos.</p>
                </div>
            </div>
        }
        else
        {
            <div class="row g-3">
                @foreach (var pg in _pages)
                {
                    <div class="col-md-6 col-lg-4">
                        <div class="page-card">
                            <div class="page-card-body">
                                <div class="d-flex justify-content-between align-items-start mb-2">
                                    <h6 style="font-weight:600;color:var(--text-primary);margin:0">@pg.Title</h6>
                                    <span class="badge-theme @(pg.Status == PageStatus.Published ? "badge-success" : "badge-muted") ms-2">
                                        @(pg.Status == PageStatus.Published ? "Publicada" : "Rascunho")
                                    </span>
                                </div>
                                <p class="text-theme-muted" style="font-size:.8rem;margin-bottom:.25rem">umblink.com/p/@pg.Slug</p>
                                <p class="text-theme-secondary" style="font-size:.8rem;margin:0">@pg.LinkCount link(s)</p>
                            </div>
                            <div class="page-card-footer">
                                <a href="/editor/@pg.Id" class="btn-accent" style="flex-grow:1;justify-content:center">
                                    <i class="bi bi-pencil"></i>Editar
                                </a>
                                <a href="/dashboard/metrics/@pg.Id" class="btn-ghost" title="Métricas">
                                    <i class="bi bi-bar-chart"></i>
                                </a>
                                <button class="btn-ghost" @onclick="() => CopyLink(pg.Slug)" title="Copiar link">
                                    <i class="bi bi-link-45deg"></i>
                                </button>
                                <button class="btn-ghost" @onclick="() => TogglePublish(pg)"
                                        title="@(pg.Status == PageStatus.Published ? "Despublicar" : "Publicar")"
                                        style="color:@(pg.Status == PageStatus.Published ? "var(--warning)" : "var(--success)")">
                                    <i class="bi @(pg.Status == PageStatus.Published ? "bi-eye-slash" : "bi-eye")"></i>
                                </button>
                                <button class="btn-ghost" style="color:var(--danger)" @onclick="() => ConfirmDelete(pg)" title="Excluir">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                }
            </div>
        }
    </LoadingSpinner>
</div>

<!-- Create modal -->
@if (_showCreate)
{
    <div class="modal-theme-backdrop">
        <div class="modal-theme">
            <div class="modal-theme-header">
                <h5>Nova Página</h5>
                <button class="btn-ghost" @onclick="() => _showCreate = false" style="padding:.25rem .5rem">
                    <i class="bi bi-x-lg"></i>
                </button>
            </div>
            <div class="modal-theme-body">
                @if (!string.IsNullOrEmpty(_createError))
                {
                    <div class="alert-theme" style="margin-bottom:1rem;border-color:rgba(239,68,68,0.3);background:rgba(239,68,68,0.08)">
                        <i class="bi bi-exclamation-circle" style="color:var(--danger)"></i>
                        @_createError
                    </div>
                }
                <div style="margin-bottom:1rem">
                    <label class="form-label-theme">Título</label>
                    <input value="@_createTitle" @oninput="OnCreateTitleInput" class="form-control-theme w-100" placeholder="Minha Página" />
                </div>
                <div style="margin-bottom:1rem">
                    <label class="form-label-theme">Endereço (slug)</label>
                    <div class="d-flex">
                        <span class="form-control-theme d-flex align-items-center text-theme-muted"
                              style="border-radius:10px 0 0 10px;border-right:none;font-size:.8rem;padding:.5rem .625rem;white-space:nowrap">umblink.com/p/</span>
                        <input value="@_createSlug" @oninput="OnCreateSlugInput" class="form-control-theme w-100"
                               style="border-radius:0 10px 10px 0" placeholder="meu-slug" />
                    </div>
                </div>
                <div>
                    <label class="form-label-theme">Bio (opcional)</label>
                    <textarea @bind="_createBio" class="form-control-theme w-100" rows="2"></textarea>
                </div>
            </div>
            <div class="modal-theme-footer">
                <button class="btn-ghost" @onclick="() => _showCreate = false">Cancelar</button>
                <button class="btn-accent" @onclick="CreatePage">Criar</button>
            </div>
        </div>
    </div>
}

<UpgradeModal @bind-Visible="_showUpgrade" Message="@_upgradeMessage" />
<ConfirmModal @bind-Visible="_showConfirm" Title="Excluir página"
    Message="Tem certeza? Todos os links e dados serão removidos."
    OnConfirm="DeleteConfirmed" />
<ToastContainer @ref="_toast" />
```

- [ ] **Step 2: Commit**

```bash
git add src/UmbLink.Web/Pages/Dashboard/Index.razor
git commit -m "feat: dashboard index redesign with theme-aware cards, modal, and alert styles"
```

---

### Task 6: Dashboard Editor Redesign

**Files:**
- Modify: `src/UmbLink.Web/Pages/Dashboard/Editor.razor`

- [ ] **Step 1: Replace the template section of Editor.razor**

Keep the `@page`, `@attribute`, `@inject`, `@implements` directives (lines 1-8) and the full `@code` block (lines 249-444) unchanged. Replace only the template between them:

```razor
<PageTitle>Editor — UmbLink</PageTitle>

@if (_page is null && !_loading)
{
    <div class="d-flex align-items-center justify-content-center" style="height:100vh;color:var(--text-muted)">
        Página não encontrada.
    </div>
}
else
{
    <div class="d-flex flex-column" style="height:100vh;background:var(--bg-primary)">
        <!-- Toolbar -->
        <div class="editor-toolbar">
            <a href="/dashboard" class="btn-ghost" style="padding:.375rem .625rem">
                <i class="bi bi-arrow-left"></i>
            </a>
            <span style="font-weight:600;font-size:.9rem;color:var(--text-primary)">@_page?.Title</span>
            <div class="ms-auto d-flex gap-2 align-items-center">
                @if (_page is not null)
                {
                    <a href="/p/@_editSlug" target="_blank" class="btn-ghost" title="Ver página">
                        <i class="bi bi-eye"></i><span class="d-none d-md-inline">Ver</span>
                    </a>
                    <a href="/dashboard/metrics/@PageId" class="btn-ghost" title="Métricas">
                        <i class="bi bi-bar-chart"></i>
                    </a>
                    <a href="/editor/@PageId/domain" class="btn-ghost" title="Domínio">
                        <i class="bi bi-globe"></i>
                    </a>
                }
                <button class="btn-accent" @onclick="SavePage" disabled="@_saving" style="padding:.375rem 1rem">
                    @if (_saving) { <span class="spinner-border spinner-border-sm"></span> }
                    else { <i class="bi bi-check-lg"></i> }
                    Salvar
                </button>
            </div>
        </div>

        <div class="row g-0 flex-grow-1 overflow-hidden">
            <!-- Editor panel -->
            <div class="col-lg-7 editor-panel">
                <LoadingSpinner Loading="_loading">
                    @if (!string.IsNullOrEmpty(_error))
                    {
                        <div class="alert-theme" style="margin-bottom:1rem;border-color:rgba(239,68,68,0.3);background:rgba(239,68,68,0.08)">
                            <i class="bi bi-exclamation-circle" style="color:var(--danger)"></i>
                            @_error
                        </div>
                    }

                    <!-- Page settings -->
                    <div class="editor-card">
                        <div class="editor-card-header">Informações da página</div>
                        <div class="editor-card-body">
                            <div style="margin-bottom:.75rem">
                                <label class="form-label-theme">Título</label>
                                <input value="@_editTitle" @oninput="OnTitleInput" class="form-control-theme w-100" />
                            </div>
                            <div style="margin-bottom:.75rem">
                                <label class="form-label-theme">Endereço</label>
                                <div class="d-flex">
                                    <span class="form-control-theme d-flex align-items-center text-theme-muted"
                                          style="border-radius:10px 0 0 10px;border-right:none;font-size:.75rem;padding:.4rem .5rem;white-space:nowrap">umblink.com/p/</span>
                                    <input @bind="_editSlug" class="form-control-theme w-100" style="border-radius:0 10px 10px 0" />
                                </div>
                            </div>
                            <div style="margin-bottom:.75rem">
                                <label class="form-label-theme">Bio</label>
                                <textarea value="@_editBio" @oninput="OnBioInput" class="form-control-theme w-100" rows="2"></textarea>
                            </div>
                            <div>
                                <label class="form-label-theme">Avatar (URL da imagem)</label>
                                <input value="@_editAvatarUrl" @oninput="OnAvatarInput" class="form-control-theme w-100" placeholder="https://..." />
                            </div>
                        </div>
                    </div>

                    <!-- Links -->
                    <div class="editor-card">
                        <div class="editor-card-header">
                            <span>Links</span>
                            <button class="btn-accent" style="padding:.25rem .625rem;font-size:.75rem" @onclick="() => _showAddLink = true">
                                <i class="bi bi-plus-lg"></i>
                            </button>
                        </div>
                        @if (_links.Count == 0)
                        {
                            <div class="text-center" style="padding:2rem 1rem">
                                <i class="bi bi-link-45deg" style="font-size:2rem;color:var(--text-muted);display:block;margin-bottom:.5rem"></i>
                                <p class="text-theme-muted" style="font-size:.85rem;margin-bottom:.75rem">Você ainda não tem links.</p>
                                <button class="btn-accent" style="font-size:.8rem" @onclick="() => _showAddLink = true">
                                    <i class="bi bi-plus-lg"></i>Adicionar meu primeiro link
                                </button>
                            </div>
                        }
                        else
                        {
                            <div @ref="_sortableEl">
                                @foreach (var link in _links)
                                {
                                    <div class="link-item" style="@(link.IsActive ? "" : "opacity:.5")">
                                        <span class="drag-handle text-theme-muted" style="cursor:grab"><i class="bi bi-grip-vertical"></i></span>
                                        <div class="flex-grow-1" style="min-width:0">
                                            <div style="font-weight:600;font-size:.85rem;color:var(--text-primary)">@link.Title</div>
                                            <div class="text-theme-muted" style="font-size:.7rem;overflow:hidden;text-overflow:ellipsis;white-space:nowrap">@link.Url</div>
                                        </div>
                                        <button class="btn-ghost" style="padding:.25rem .5rem" @onclick="() => ToggleLink(link)"
                                                title="@(link.IsActive ? "Desativar" : "Ativar")">
                                            <i class="bi @(link.IsActive ? "bi-eye" : "bi-eye-slash")"></i>
                                        </button>
                                        <button class="btn-ghost" style="padding:.25rem .5rem;color:var(--danger)" @onclick="() => DeleteLink(link)">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </div>
                                }
                            </div>
                        }
                    </div>

                    <!-- Appearance -->
                    <div class="editor-card">
                        <div class="editor-card-header" style="cursor:pointer" @onclick="() => _showThemePanel = !_showThemePanel">
                            <span>Aparência</span>
                            <i class="bi @(_showThemePanel ? "bi-chevron-up" : "bi-chevron-down")" style="color:var(--text-muted)"></i>
                        </div>
                        @if (_showThemePanel)
                        {
                            <div class="editor-card-body">
                                <label class="form-label-theme" style="font-weight:600">Tema</label>
                                <div class="row g-2 mb-3">
                                    @foreach (var theme in Themes.All)
                                    {
                                        var isLocked = theme.IsPremium && !(_sub?.PlanName is "Pro" or "Business");
                                        <div class="col-6">
                                            <div style="border:1px solid @(_selectedThemeId == theme.Id ? "var(--accent)" : "var(--border)");
                                                        border-radius:10px;padding:.5rem .75rem;cursor:pointer;display:flex;align-items:center;gap:.5rem;
                                                        background:@(_selectedThemeId == theme.Id ? "rgba(99,102,241,0.08)" : "transparent");
                                                        opacity:@(isLocked ? ".6" : "1");transition:all .2s ease"
                                                 @onclick="() => SelectThemeOrUpgrade(theme)">
                                                <div style="width:20px;height:20px;border-radius:50%;background:@theme.DefaultBg;border:1px solid var(--border);flex-shrink:0"></div>
                                                <span style="font-size:.8rem;color:var(--text-primary)">@theme.Name</span>
                                                @if (isLocked)
                                                {
                                                    <span class="ms-auto"><i class="bi bi-lock-fill" style="font-size:.65rem;color:var(--warning)"></i></span>
                                                }
                                                else if (theme.IsPremium)
                                                {
                                                    <span class="badge-theme ms-auto" style="background:rgba(245,158,11,0.1);color:var(--warning);font-size:.55rem">Pro</span>
                                                }
                                            </div>
                                        </div>
                                    }
                                </div>

                                <label class="form-label-theme" style="font-weight:600">Estilo do botão</label>
                                <div class="d-flex gap-1 mb-3 flex-wrap">
                                    @foreach (var (key, label) in new[] { ("rounded","Redondo"), ("pill","Pílula"), ("square","Quadrado"), ("outline","Contorno") })
                                    {
                                        <button type="button"
                                                class="@(_btnStyleKey == key ? "btn-accent" : "btn-ghost")"
                                                style="font-size:.75rem;padding:.3rem .75rem"
                                                @onclick="() => { _btnStyleKey = key; ApplyTheme(); }">@label</button>
                                    }
                                </div>

                                <div class="row g-2 mb-3">
                                    <div class="col-6">
                                        <label class="form-label-theme">Cor do botão</label>
                                        <input type="color" class="form-control form-control-color w-100" value="@_btnColor"
                                               style="border-radius:10px;border:1px solid var(--border)"
                                               @oninput="e => { _btnColor = e.Value?.ToString() ?? _btnColor; ApplyTheme(); }" />
                                    </div>
                                    <div class="col-6">
                                        <label class="form-label-theme">Cor do texto</label>
                                        <input type="color" class="form-control form-control-color w-100" value="@_textColor"
                                               style="border-radius:10px;border:1px solid var(--border)"
                                               @oninput="e => { _textColor = e.Value?.ToString() ?? _textColor; ApplyTheme(); }" />
                                    </div>
                                </div>

                                <label class="form-label-theme" style="font-weight:600">Formato do avatar</label>
                                <div class="d-flex gap-1">
                                    <button type="button"
                                            class="@(_avatarShape == "circle" ? "btn-accent" : "btn-ghost")"
                                            style="font-size:.75rem;padding:.3rem .75rem"
                                            @onclick='() => { _avatarShape = "circle"; ApplyTheme(); }'>
                                        <i class="bi bi-circle me-1"></i>Círculo
                                    </button>
                                    <button type="button"
                                            class="@(_avatarShape == "square" ? "btn-accent" : "btn-ghost")"
                                            style="font-size:.75rem;padding:.3rem .75rem"
                                            @onclick='() => { _avatarShape = "square"; ApplyTheme(); }'>
                                        <i class="bi bi-square me-1"></i>Quadrado
                                    </button>
                                </div>
                            </div>
                        }
                    </div>
                </LoadingSpinner>
            </div>

            <!-- Preview panel -->
            <div class="col-lg-5 d-none d-lg-flex preview-panel">
                @if (_page is not null)
                {
                    <PagePreview Page="_page" Links="_links" />
                }
            </div>
        </div>
    </div>
}

<!-- Add link modal -->
@if (_showAddLink)
{
    <div class="modal-theme-backdrop">
        <div class="modal-theme">
            <div class="modal-theme-header">
                <h5>Adicionar Link</h5>
                <button class="btn-ghost" @onclick="() => _showAddLink = false" style="padding:.25rem .5rem">
                    <i class="bi bi-x-lg"></i>
                </button>
            </div>
            <div class="modal-theme-body">
                @if (!string.IsNullOrEmpty(_linkError))
                {
                    <div class="alert-theme" style="margin-bottom:1rem;border-color:rgba(239,68,68,0.3);background:rgba(239,68,68,0.08)">
                        <i class="bi bi-exclamation-circle" style="color:var(--danger)"></i>
                        @_linkError
                    </div>
                }
                <div style="margin-bottom:1rem">
                    <label class="form-label-theme">Título</label>
                    <input @bind="_newLinkTitle" class="form-control-theme w-100" placeholder="Meu Instagram" />
                </div>
                <div>
                    <label class="form-label-theme">URL</label>
                    <input @bind="_newLinkUrl" type="url" class="form-control-theme w-100" placeholder="https://..." />
                </div>
            </div>
            <div class="modal-theme-footer">
                <button class="btn-ghost" @onclick="() => _showAddLink = false">Cancelar</button>
                <button class="btn-accent" @onclick="AddLink">Adicionar</button>
            </div>
        </div>
    </div>
}

<UpgradeModal @bind-Visible="_showUpgrade" Message="@_upgradeMessage" />
<ToastContainer @ref="_toast" />
```

- [ ] **Step 2: Commit**

```bash
git add src/UmbLink.Web/Pages/Dashboard/Editor.razor
git commit -m "feat: editor redesign with glass toolbar, themed cards, dot-grid preview panel"
```

---

### Task 7: PagePreview Phone Frame

**Files:**
- Modify: `src/UmbLink.Web/Components/PagePreview.razor`

- [ ] **Step 1: Update the outer container div**

In `PagePreview.razor`, replace the outer container div (line 4) and its style to use the `.phone-frame` class. Change the opening tag:

```razor
<div class="phone-frame">
```

Remove the inline style from line 4 (`style="width:320px;min-height:560px;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.15);@_containerStyle"`). The `_containerStyle` field is no longer needed on the outer div.

The inner `<div>` on line 5 keeps its existing styles — they handle the user's theme colors.

- [ ] **Step 2: Commit**

```bash
git add src/UmbLink.Web/Components/PagePreview.razor
git commit -m "feat: phone-frame container for page preview with notch indicator"
```

---

### Task 8: Build Verification

- [ ] **Step 1: Run dotnet build**

Run: `dotnet build`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)` (or only pre-existing warnings)

- [ ] **Step 2: Fix any build errors if they exist**

If there are build errors, read the error messages, locate the file and line, and fix the issue.

- [ ] **Step 3: Commit any fixes**

```bash
git add -A
git commit -m "fix: resolve build errors from redesign"
```

(Skip this step if the build succeeded with no changes needed.)
