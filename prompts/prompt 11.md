# 🤖 Prompt de Execução — Melhorias de UX e Formulários

> Execute na raiz do projeto com o Claude Code CLI.

---

## Antes de Começar — Reescreva Este Prompt

**Antes de executar qualquer tarefa**, leia todo este prompt e o reescreva com suas próprias palavras, da forma que você achar melhor para entender e executar o que foi pedido. Reorganize, detalhe, simplifique ou expanda o que precisar — o objetivo é que você internalize as tarefas no formato que te permita trabalhar com mais precisão e qualidade. Só comece a executar após ter feito essa reescrita.

---

## Contexto Geral

Melhorias pontuais de UX e comportamento de formulários. Analise o projeto completo antes de começar para entender a estrutura e os padrões existentes.

---

## Tarefas Obrigatórias

### 1. 🔗 Campo de URL — Prefixo HTTPS Automático

Nas telas de criação e edição de página, o campo de URL deve sempre exibir `https://` de forma fixa e automática, sem depender do usuário digitá-lo:

- O prefixo `https://` deve aparecer fixo e visualmente destacado antes do input (ex: como texto prefixado fora do campo, ou como adorno interno não editável), de forma que o usuário só precise digitar o restante da URL (ex: `meusite.com`).
- **Se o usuário colar uma URL completa** que já contenha `http://` ou `https://`, remova automaticamente esse prefixo do valor colado antes de exibi-lo no campo — evitando duplicação como `https://https://meusite.com`.
- Trate também colagens com `www.` isolado ou outros formatos comuns, normalizando o valor exibido.
- O valor salvo no banco deve sempre incluir o `https://` completo — a separação é apenas visual no frontend.
- Aplique esse comportamento de forma consistente em **todas** as telas que tenham campo de URL no projeto.

---

### 2. ✅ Validação Client-Side com Preview em Tempo Real

Em todas as telas do projeto que possuem formulários, adicione validação no lado do cliente com feedback visual em tempo real:

- **Enquanto o usuário digita** (ou ao sair do campo), exiba indicações visuais de validade: borda verde/ícone de check para campo válido, borda vermelha/mensagem de erro para campo inválido.
- As mensagens de erro devem ser **específicas e úteis** — não apenas "campo inválido", mas "O e-mail deve conter @", "A senha deve ter ao menos 8 caracteres", etc.
- Para campos que têm **preview de resultado** (ex: nome da página, slug, URL, foto de perfil, cores, textos de bio), adicione um preview em tempo real logo abaixo ou ao lado do campo, mostrando como ficará o resultado final à medida que o usuário digita.
- O preview deve ser atualizado de forma suave — use debounce de ~300ms para campos que disparam transformações mais pesadas.
- Use as bibliotecas de validação já presentes no projeto (ex: `react-hook-form`, `zod`, `yup`). Se não houver nenhuma, implemente com estado local de forma simples e direta.
- Não quebre nenhum comportamento de submit existente — a validação client-side é uma camada adicional, não substitui a validação do backend.

---

### 3. 🔐 Login — Preservar Campos ao Errar

Atualmente, quando o usuário erra a senha ou as credenciais no login, os campos são apagados, forçando o usuário a redigitar tudo. Corrija esse comportamento:

- **Ao ocorrer erro de autenticação** (senha incorreta, usuário não encontrado, etc.), preserve o valor digitado no campo de e-mail/usuário — o usuário não deve precisar redigitá-lo.
- O campo de senha pode ser limpo (é o comportamento esperado por segurança), mas o e-mail deve permanecer.
- Exiba a mensagem de erro de forma clara e próxima ao formulário, sem redirecionar ou recarregar a página desnecessariamente.
- Verifique se esse mesmo problema ocorre em outras telas de autenticação (ex: recuperação de senha, troca de senha, cadastro) e aplique o mesmo cuidado — nunca apagar o que o usuário já preencheu por causa de um erro evitável.

---

### 4. 🧭 Botão "Outros Produtos Umbler" — Visibilidade

Há um botão ou link que direciona para outros produtos da Umbler que está pouco visível ou escondido. Melhore sua presença:

- Localize onde esse elemento está no projeto (menu, sidebar, footer, dashboard, etc.).
- Substitua o elemento atual (ícone, link discreto ou botão sem texto) por um botão ou link com o texto **"Outros produtos Umbler"** (ou variação próxima como "Conheça outros produtos Umbler"), tornando-o explícito e fácil de encontrar.
- O estilo deve ser compatível com o design do restante da interface — não precisa ser chamativo, mas deve ser legível e clicável sem esforço.
- Mantenha o comportamento de navegação original (mesma URL de destino, mesmo target de abertura).

---

### 5. 🔖 Favicon — Logo da Umbler na Aba do Navegador

O ícone que aparece na aba do navegador (favicon) precisa ser substituído pela logo da Umbler em formato SVG:

- Localize o favicon atual no projeto (geralmente em `public/`, `src/assets/` ou referenciado no `index.html` / `_document.tsx`).
- Substitua pelo SVG oficial da logo da Umbler. Busque o arquivo SVG já presente no projeto (verifique `src/assets`, `public/` ou imports de logo existentes no código) — não crie um SVG do zero, use o que já existe.
- Configure o `<link rel="icon">` no HTML principal apontando para o novo arquivo SVG, com `type="image/svg+xml"`.
- Adicione também um fallback em `.png` ou `.ico` para navegadores mais antigos que não suportam SVG como favicon, se já houver um arquivo desse tipo disponível no projeto.
- Verifique se o projeto usa algum gerador de metadados (ex: `next/head`, `react-helmet`, `vite-plugin-pwa`) e atualize o favicon nesses locais também, garantindo consistência.
- Confirme que a mudança aparece corretamente em aba normal e aba anônima após hard refresh.

---

## Regras de Execução

- **Leia antes de agir**: entenda o projeto completo antes de qualquer modificação.
- **Não quebre o que funciona**: avalie o impacto de cada mudança antes de aplicar.
- **Siga os padrões do projeto**: mesmas bibliotecas, convenções de nome, estrutura de pastas.
- **Commits atômicos** se usar git: um commit por tarefa principal.
- **Entrega funcionando**: ao final, sem erros críticos de console e todas as funcionalidades operacionais.

---

## Ordem de Execução Sugerida

1. Favicon com logo da Umbler (Tarefa 5) — rápida, sem risco
2. Botão "Outros produtos Umbler" (Tarefa 4)
3. Preservar campos no login ao errar (Tarefa 3)
4. Campo de URL com prefixo automático (Tarefa 1)
5. Validação client-side com preview em tempo real (Tarefa 2)

---
