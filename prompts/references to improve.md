Analise todos os arquivos do projeto atual (HTML, CSS, JS) e faça um redesign completo da página de links no estilo Linktree, tornando-a moderna, animada e memorável.

## Diagnóstico primeiro
Antes de alterar qualquer coisa, leia todos os arquivos relevantes e liste:
- Quais bibliotecas estão sendo usadas atualmente (ex: Bootstrap)
- Quais elementos existem (foto, nome, links, ícones, etc.)
- O que está limitando o visual atual

## Objetivo
Criar uma página visualmente deslumbrante, com identidade própria, que pareça viva. Inspiração: Linktree, Bento, Lnk.bio — mas com personalidade única.

## Requisitos técnicos obrigatórios

### Remover / reduzir Bootstrap
- Remova a dependência de Bootstrap para layout e visual
- Use CSS puro com Flexbox/Grid para estrutura
- Mantenha apenas se houver algo essencial que não vale reescrever

### Bibliotecas externas a importar via CDN
Adicione via `<link>` ou `<script>` no `<head>`:
- **GSAP** (https://cdnjs.cloudflare.com/ajax/libs/gsap/3.12.5/gsap.min.js) — para animações de entrada e micro-interações
- **Google Fonts** — importe uma fonte moderna (ex: Inter, Plus Jakarta Sans ou Outfit) via https://fonts.googleapis.com

### Fundo animado (obrigatório)
Implemente um dos seguintes com CSS puro ou Canvas:
- Gradiente animado com `@keyframes` que transita suavemente entre 3-4 cores (ex: roxo → azul → índigo → violeta)
- Use `background-size: 400% 400%` com `animation: gradientShift 8s ease infinite`
- O fundo deve ser rico, mas não distrair do conteúdo

### Cartões de link (obrigatório)
Cada link deve ser um `<a>` estilizado com:
- `backdrop-filter: blur(12px)` + fundo semi-transparente (glass morphism)
- `border: 1px solid rgba(255,255,255,0.2)`
- `border-radius: 16px`
- `transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1)`
- No hover: `transform: translateY(-4px) scale(1.02)` + `box-shadow` colorido + borda mais brilhante
- Cursor pointer e `user-select: none`

### Animações de entrada com GSAP (obrigatório)
Após carregar a página:
```js
gsap.from(".profile", { opacity: 0, y: -30, duration: 0.8, ease: "power3.out" })
gsap.from(".link-card", { 
  opacity: 0, y: 40, duration: 0.6, 
  stagger: 0.1, ease: "back.out(1.4)", delay: 0.3 
})
```

### Micro-interações nos botões
- Ao clicar: efeito de "press" com `transform: scale(0.96)`
- Ripple effect no clique (círculo que expande e desaparece)
- Ícone com pequena rotação ou pulse no hover se houver ícones

### Avatar/foto de perfil
- Borda circular com gradiente animado girando (CSS `@keyframes rotate`)
- Sutil `box-shadow` colorido pulsando

### Tipografia
- Use a fonte importada via Google Fonts em todo o projeto
- Nome/título: bold, grande, com `letter-spacing` ligeiramente negativo
- Subtítulo/bio: levemente opaco (rgba branco ~0.7) para contraste suave

## Estilo geral
- Fundo: gradiente animado (tons escuros: roxo, azul, índigo)
- Texto: branco / branco semi-transparente
- Cartões: glass morphism (blur + transparência)
- Sem cantos quadrados — tudo `border-radius` generoso
- Máximo 480px de largura centralizado, com padding lateral
- Visual mobile-first mas bonito no desktop também

## Entrega
- Edite os arquivos existentes no projeto (não crie arquivos novos desnecessariamente)
- Certifique-se que o resultado abre sem erros no navegador
- Preserve todos os links, textos e imagens existentes — só mude o visual
- Se usar inline styles por necessidade, tudo bem — priorize o resultado visual