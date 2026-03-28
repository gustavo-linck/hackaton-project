Você receberá todas as informações necessárias abaixo para planejar e iniciar
um sistema do zero. NÃO faça perguntas. NÃO pare para pedir esclarecimentos.
Processe tudo com o que foi fornecido, documente as assunções feitas e liste
ao final o que está ambíguo ou faltando — sem interromper o fluxo.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
CONTEXTO DE EXECUÇÃO: HACKATHON UMBLER
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Este sistema será desenvolvido em um hackathon com prazo curto.
O contexto é competitivo: o sistema será avaliado por jurados via link público.

FILOSOFIA DE ENTREGA:
- Qualidade e completude são prioritárias — o código deve ser funcional,
  seguro e bem estruturado o suficiente para impressionar quem for lê-lo
- A arquitetura deve suportar crescimento real — o sistema foi pensado para
  escalar e gerar receita, não apenas para demo
- O escopo base descrito é obrigatório e não negociável
- Funcionalidades além do escopo base são bem-vindas SE agregarem valor real
  ao produto e não comprometerem a entrega do núcleo
- Um fluxo completo e polido impressiona mais do que muitos fluxos pela metade
- A experiência do jurado (primeiro acesso, clareza, velocidade, mobile-first)
  é critério de avaliação — trate como requisito, não como detalhe

EXCEÇÃO DE MOCK — APENAS PARA PAGAMENTO:
O sistema possui um modelo de planos e monetização real. A cobrança em si
é a única parte mockada: o fluxo de upgrade (tela de planos, seleção,
formulário de pagamento com campos reais de cartão, confirmação) deve ser
completamente implementado e parecer real, porém ao confirmar o pagamento,
o sistema simplesmente ativa o plano sem processar cobrança real.
Tudo mais — limites de plano, bloqueios, trial, downgrade — é 100% funcional.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
INFORMAÇÕES DO SISTEMA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

NOME DO PRODUTO: UmbLink

DESCRIÇÃO GERAL:
Criador de páginas de links para bio de redes sociais. O usuário monta de forma
simples e rápida uma página personalizada que reúne todos os seus links
importantes (site, redes sociais, WhatsApp, portfólio, etc.) em uma única URL
compartilhável. A ferramenta oferece um editor intuitivo com personalização
visual detalhada e gera uma página leve e otimizada para mobile.
O produto opera em modelo freemium com planos pagos, gerando receita recorrente
e servindo como porta de entrada para o ecossistema Umbler.

PROBLEMA QUE RESOLVE:
Quem divulga seu trabalho nas redes sociais só pode colocar um único link na
bio — o que força a escolher entre site, WhatsApp, portfólio ou loja, perdendo
audiência para tudo que ficou de fora.

INSPIRAÇÕES:
- Linktree: referência global, mais de 70M usuários. Criação rápida, analytics
  de cliques, integrações com e-commerce, templates personalizáveis.
- Hopp by Wix: vai além de links — venda de produtos, captação de leads,
  agendamento de serviços direto pela página (mini-site completa).

CONTEXTO DE NEGÓCIO:
Produto da Umbler. Objetivo: gerar receita recorrente via planos pagos e
aumentar superfície de contato com potenciais clientes, criando um ponto de
entrada no ecossistema que evolua para adoção de outros produtos (upsell/cross-sell).

USUÁRIOS:
- Usuário free: acessa funcionalidades básicas sem custo
- Usuário pago (Pro/Business): acessa funcionalidades avançadas mediante plano
- Admin Umbler: acesso ao painel administrativo global

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
MODELO DE PLANOS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

ESTRUTURA:
- Free: gratuito, sem trial, funcionalidades básicas permanentes
- Pro: pago, com trial gratuito de 7 dias antes da cobrança
- Business: pago, com trial gratuito de 7 dias antes da cobrança
  (defina os valores sugeridos — ex: Pro R$19/mês, Business R$49/mês)

LIMITES POR PLANO (o sistema deve enforcar esses limites em toda a lógica):

| Funcionalidade                        | Free     | Pro       | Business   |
|---------------------------------------|----------|-----------|------------|
| Páginas de links                      | 1        | 3         | ilimitado  |
| Links por página                      | 3        | 5         | ilimitado  |
| Personalização visual                 | básica   | completa  | completa   |
| Temas disponíveis                     | 2 básicos| todos     | todos      |
| Fontes disponíveis                    | 2        | todas     | todas      |
| Métricas                              | 7 dias   | 1 ano     | total      |
| Origens de acesso (referrer)          | não      | sim       | sim        |
| Domínio customizado                   | não      | sim       | sim        |
| Remoção do branding Umbler            | não      | não       | sim        |
| Suporte                               | comunidade| e-mail   | prioritário|

OBS: A partir do Pro, tem que ter como escolher entre 7 dias, 3 meses, 6 meses e um ano, no Business, aí coloca a opção de todo o tempo

TRIAL:
- Ao iniciar trial de Pro ou Business, o usuário tem acesso completo ao plano
  por 7 dias informando o cartão, mas cobrança apenas no último dia
- No 7º dia, o sistema exibe aviso de que o trial encerra em 24h
- Ao encerrar o trial sem assinar(falha no pagamento), o usuário é rebaixado para Free
- Conteúdo criado durante o trial é preservado mas fica inacessível/bloqueado
  até o usuário assinar ou excluir o excedente manualmente

DOWNGRADE:
- Se o usuário cancelar plano pago e voltar para Free:
  * Páginas excedentes ficam desativadas (não excluídas)
  * Links excedentes ficam inativos (não excluídos)
  * O usuário vê aviso claro sobre o que está bloqueado e por quê
  * O usuário pode escolher quais manter dentro dos limites do Free

FLUXO DE PAGAMENTO (MOCKADO — experiência completa, cobrança não real):
- Tela de planos com comparativo visual claro entre Free / Pro / Business
- Ao clicar em assinar: formulário com campos reais (número do cartão,
  nome, validade, CVV, CPF) — campos com máscara e validação de formato
- Botão "Assinar agora" — ao confirmar, o sistema ativa o plano imediatamente
- E-mail de confirmação simulado (exibir mensagem de sucesso com detalhes)
- O usuário deve sentir que fez uma compra real — a experiência é o produto

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
FUNCIONALIDADES OBRIGATÓRIAS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

AUTENTICAÇÃO:
- Cadastro e login com e-mail + senha
- Login social com Google (OAuth2)
- Recuperação de senha por e-mail ou SMS

EDITOR DE PÁGINA:
- Criação e edição de página de links com URL única: UmbLink/{username}
- Adição, remoção, reordenação de links (drag-and-drop)
- Cada link: título (obrigatório, máx 50 chars), URL (obrigatória, validada),
  ícone opcional (lista predefinida de ícones de redes sociais e serviços),
  toggle ativo/inativo
- Links inativos não aparecem na página pública mas ficam salvos
- Preview em tempo real (split-view: editor à esquerda, preview à direita)
- Preview mobile/desktop alternável no editor

PERSONALIZAÇÃO VISUAL:
- Temas prontos: mínimo 4 temas (2 no Free, todos nos planos pagos)
- Ajustes sobre o tema (disponibilidade varia por plano):
  * Cor de fundo (sólida ou gradiente) — Pro+
  * Cor e estilo dos botões (arredondado, quadrado, outline, sólido) — Pro+
  * Cor do texto e títulos — Pro+
  * Fonte do título e links (ao menos 6 opções, 2 no Free) — Pro+
  * Imagem de fundo (upload ou URL) — Pro+
  * Avatar: foto de perfil circular ou quadrada — todos
  * Espaçamento entre links (compacto, normal, espaçado) — Pro+
- Funcionalidades bloqueadas exibem cadeado com CTA de upgrade contextual

PÁGINA PÚBLICA:
- Acessível sem login em /{username}
- Leve, sem chrome do sistema, 100% mobile-first
- Branding Umbler no rodapé (removível no Business)
- Links abrem em nova aba
- Cliques e acessos registrados de forma assíncrona

MÉTRICAS (dashboard do usuário):
- Total de acessos à página (por dia — janela conforme plano)
- Total de cliques por link
- Taxa de clique por link (cliques / acessos)
- Origem do acesso (referrer) — Pro+
- Gráfico de evolução temporal

GESTÃO DE PLANOS (área do usuário):
- Tela "Meu Plano" com plano atual, data de renovação, uso vs limites
- Tela de upgrade com comparativo de planos e CTA de assinar/fazer trial
- Formulário de pagamento mockado (experiência completa)
- Histórico de faturas (mockado com dados simulados)
- Cancelamento de plano com confirmação e aviso de downgrade

DOMÍNIO CUSTOMIZADO (Business):
- Campo para informar domínio próprio
- Instruções de configuração DNS exibidas (CNAME)
- O sistema aceita a configuração mas não precisa validar DNS real no hackathon

PAINEL ADMIN:
- Lista de todos os usuários com plano, status, data de cadastro
- Métricas globais: total de páginas, total de cliques, receita simulada
- Capacidade de visualizar qualquer página como admin
- Capacidade de suspender/reativar usuários
- Capacidade de alterar plano de qualquer usuário manualmente

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
REGRAS DE NEGÓCIO
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

- Limites de plano são enforçados na camada de serviço — nunca só na UI
- Ao atingir limite, exibir mensagem humana + CTA de upgrade (nunca erro técnico)
- Username/slug: único, alfanumérico com hífens, 3-30 caracteres
- Slug pode ser alterado — URL antiga para de funcionar imediatamente
- Ao trocar slug: exibir aviso "Seu link antigo vai parar de funcionar.
  Atualize sua bio nas redes sociais."
- Trial só pode ser iniciado uma vez por plano por usuário
- Usuário não pode ter dois trials do mesmo plano
- Downgrade preserva dados, bloqueia excedentes, nunca deleta silenciosamente
- Métricas são registradas de forma assíncrona — nunca bloqueiam a resposta
  da página pública
- Admin não pode ser acessado por usuários comuns por design de arquitetura,
  não apenas por ocultação de UI
- URLs de links são validadas (formato) antes de salvar — links com URL
  inválida não podem ser ativados
- Registros de clique devem incluir: timestamp, link_id, user_agent resumido,
  referrer (se disponível) — sem armazenar IP completo (LGPD)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
CONTEXTO DE APRESENTAÇÃO
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Sistema avaliado por jurados via link público em hackathon Umbler.
Jurados acessam pelo navegador, sem instalação.
Páginas públicas acessíveis sem login.
Cadastro necessário apenas para criar/editar página.
O seed de dados deve garantir que o jurado veja algo funcional e bonito
ao abrir o sistema pela primeira vez — sem precisar cadastrar nada.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
STACK OBRIGATÓRIA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

- Backend: .NET 8+ com C#
- Frontend: Blazor Server (padrão) ou Blazor WebAssembly — avalie e justifique
  considerando preview em tempo real, performance mobile e deploy gratuito
- ORM: Entity Framework Core com Migrations
- Banco: SQLite (padrão para deploy gratuito simples) — justifique se outro
- Estilo: exclusivamente Bootstrap utilities e components
  CSS customizado, inline styles e bibliotecas externas são proibidos
- Deploy: gratuito, acessível via link público sem cadastro do visitante
  Avalie Railway, Render ou Fly.io — escolha e justifique para este sistema

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
FASE 1 — DOMÍNIO E REGRAS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Com base nas informações fornecidas:
- Reescreva o objetivo real do sistema em 2-3 frases
- Liste todas as entidades do domínio com atributos completos e relacionamentos
  Atenção especial para: User, Page, Link, Plan, Subscription, Trial,
  ClickEvent, PageView, CustomDomain
- Classifique funcionalidades: Deve ter / Pode ter / Fora do escopo
  Para "Pode ter": avalie ativamente o que faria sentido para um produto
  freemium de link bio — funcionalidades que agregariam valor real
- Liste todas as regras de negócio, validações e edge cases críticos
- Mapeie os fluxos de estado:
  * Subscription: free / trial / active / cancelled / expired
  * Page: draft / published / suspended
  * Link: active / inactive
  * Trial: not_started / active / expired / converted

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
FASE 2 — UX/UI E USABILIDADE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Considerando que o usuário final não tem conhecimento técnico:
- Mapeie as jornadas principais:
  * Onboarding: cadastro → criar primeira página → publicar (menos de 3 minutos)
  * Edição: adicionar link, reordenar, trocar tema
  * Upgrade: atingir limite → ver CTA → tela de planos → trial ou pagamento
  * Downgrade: cancelar plano → entender o que vai ser bloqueado → confirmar
  * Visualização de métricas
  * Admin: visão global, gerenciar usuário
- Defina o layout do editor: split-view com preview em tempo real
  (editor à esquerda, preview de celular simulado à direita)
- Especifique os estados de UI obrigatórios para cada tela:
  vazio, carregando, erro, sucesso, sem permissão, limite atingido
- Estado "limite atingido": nunca mostrar erro técnico — mostrar o que está
  bloqueado, por que, e CTA de upgrade contextual e amigável
- Defina padrão de feedback (toasts, modais, inline alerts)
- Defina padrão de confirmação para ações destrutivas
- Avisos de consequência obrigatórios: troca de slug, downgrade, cancelamento
- Liste os componentes Blazor reutilizáveis necessários
  Incluir: PlanLimitGuard (bloqueia ação e exibe CTA), PlanBadge,
  UpgradeModal, TrialBanner, UsageMeter
- Mensagens de erro em linguagem humana — nunca mensagens técnicas
- Ações mais frequentes acessíveis em no máximo 1-2 cliques
- A página pública gerada é produto final: rápida, bonita, mobile-first

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
FASE 3 — ARQUITETURA (AVALIAÇÃO CONTEXTUAL)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Compare ao menos 2 abordagens arquiteturais para este sistema específico.
Para cada uma, avalie objetivamente:
  - Complexidade de implementação para o tamanho real deste sistema
  - Manutenção por outra pessoa em 6 meses
  - Facilidade de integração futura (gateway de pagamento real, APIs Umbler)
  - Testabilidade (unit, integration)
  - Compatibilidade com deploy gratuito escolhido
  - Capacidade de lidar com: preview em tempo real, registro assíncrono de
    métricas, enforcement de limites de plano em toda a lógica

Escolha a arquitetura com melhor custo-benefício. Não a mais sofisticada —
a mais adequada para este contexto com potencial de crescimento real.

Documente:
- Estrutura de pastas e projetos na solution
- Direção das dependências entre camadas
- Convenções de nomenclatura (definidas uma vez, seguidas sempre)
- Padrão de validação (FluentValidation ou DataAnnotations — justifique)
- Padrão de tratamento de erros e logging
- Como o enforcement de limites de plano é centralizado (não duplicado)
- Como o registro assíncrono de métricas será implementado
- Pontos de extensão: onde conectar pagamento real, domínio customizado real,
  e-mail transacional real, integração com outros produtos Umbler

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
FASE 4 — SEGURANÇA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Especifique apenas o que é relevante para este sistema:
- Autenticação: ASP.NET Identity com Cookie + OAuth2 Google — confirme ou ajuste
- Perfis de acesso: usuário free, usuário pago, admin — permissões de cada um
- O painel admin deve ser inacessível a usuários comuns por design de
  arquitetura, não apenas por ocultação de UI — especifique como
- Proteção de rotas no Blazor (AuthorizeView, [Authorize], políticas por perfil)
- Endpoint de registro de cliques é público e pode ser abusado:
  defina rate limiting ou estratégia de proteção
- Dados sensíveis: e-mail, tokens OAuth, dados de cartão (mockados mas
  campos reais — não armazenar dados de cartão, nem mockados)
- Proteções OWASP aplicáveis: XSS (links maliciosos na página pública),
  CSRF, validação de URLs antes de salvar
- LGPD: registro de cliques não deve armazenar IP completo
- Configurações que não podem estar hardcoded (secrets OAuth, connection string)
- Log de auditoria: criação de conta, upgrade/downgrade, troca de slug,
  suspensão de usuário pelo admin

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
FASE 5 — DEPLOY E ACESSO PARA JURADOS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

O sistema precisa ser acessível via link público sem custo.
Defina:
- Plataforma de deploy escolhida e justificativa
- Dockerfile funcional para o deploy
- Variáveis de ambiente necessárias e como configurá-las na plataforma
- Estratégia de banco: SQLite persistido em volume — justifique configuração
- URL gerada automaticamente pela plataforma (sem domínio customizado)
- Configuração do OAuth Google para funcionar no domínio gerado
- Instruções simples para o jurado: apenas abrir o link no navegador,
  sem instalação, sem cadastro para visualizar páginas públicas

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
FASE 6 — BOILERPLATE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Defina o boilerplate completo da solution. O boilerplate deve compilar e subir
sem erro — essa é a validação de que a base está correta.
Nenhuma feature começa antes do boilerplate estar de pé e verificado.

Especifique com precisão:
- Estrutura completa de pastas e arquivos a criar
- Program.cs: DI, EF Core, ASP.NET Identity, OAuth Google, middlewares,
  rota especial para páginas públicas (/{username}), rota protegida para admin
- AppDbContext com todos os DbSets das entidades do domínio
- Tabelas de planos e limites configuradas como dados de referência (seed),
  não hardcoded no código de negócio
- Migration inicial com toda a estrutura de tabelas
- Seed de dados obrigatório:
  * 3 planos configurados (Free, Pro, Business) com todos os limites
  * 1 usuário admin
  * 3 usuários de exemplo com planos diferentes (Free, Pro em trial, Business)
  * Cada usuário com página publicada, links variados e temas diferentes
  * Dados de métricas simulados para os últimos 30 dias
  (O jurado deve ver algo funcional e bonito ao abrir o sistema)
- MainLayout.razor, NavMenu.razor, layout separado para página pública
- TrialBanner component (exibe aviso quando trial está ativo ou prestes a expirar)
- PlanLimitGuard component (bloqueia ação e exibe CTA de upgrade)
- Página de erro genérica e componente de loading reutilizável
- appsettings.json e appsettings.Development.json
- Dockerfile funcional para o deploy escolhido
- Checklist de validação: o que deve funcionar antes de iniciar qualquer feature

REGRA: nenhuma lógica de negócio entra no boilerplate.
O boilerplate é infraestrutura pura — se quebrar, quebra tudo.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
FASE 7 — PRD CONSOLIDADO
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Gere o PRD final com:
1.  Visão geral e objetivo do produto
2.  Contexto de negócio, modelo de receita e alinhamento com ecossistema Umbler
3.  Usuários e personas (free, Pro, Business, admin)
4.  Jornadas principais do usuário (onboarding, upgrade, downgrade, métricas)
5.  Funcionalidades (Deve ter / Pode ter / Fora do escopo)
    — inclua funcionalidades extras identificadas que façam sentido para o produto
6.  Modelo de planos completo: limites, trial, downgrade, fluxo de pagamento
7.  Modelo de dados completo (entidades, atributos, relacionamentos, índices)
8.  Regras de negócio e validações
9.  Decisões de UX/UI (layout editor, split-view, preview, página pública,
    métricas, upgrade flow, componentes, estados de UI)
10. Sistema de temas e personalização visual (estrutura de dados, como temas
    são armazenados, como limites de plano são aplicados visualmente)
11. Enforcement de limites de plano: onde e como é centralizado na arquitetura
12. Arquitetura escolhida e justificativa
13. Estrutura de pastas da solution
14. Boilerplate: checklist do que deve estar de pé antes das features
15. Seed de dados para demo ao jurado
16. Setup de deploy: plataforma, Dockerfile, variáveis de ambiente
17. Como o jurado acessa: passos simples
18. Decisões técnicas e trade-offs
19. Pontos de extensão: pagamento real, domínio customizado, e-mail, Umbler
20. Riscos e mitigações

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
FASE 8 — AUDITORIA FINAL (EXECUTE SEMPRE)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Sem interromper o fluxo, liste ao final:

ASSUNÇÕES FEITAS (informação ausente que foi assumida):
- [liste cada uma]

AMBIGUIDADES DE RISCO (podem gerar retrabalho):
- [liste cada uma]

INCONSISTÊNCIAS DETECTADAS:
- [liste se houver]

FUNCIONALIDADES EXTRAS IDENTIFICADAS (além do escopo base, com justificativa):
- [liste o que faria sentido natural para um produto freemium de link bio]

PERGUNTAS PRIORITÁRIAS (só as que realmente impactam arquitetura ou escopo):
- [liste no máximo 5, em ordem de impacto]

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
REGRAS INVIOLÁVEIS NA IMPLEMENTAÇÃO
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

- Lógica de negócio nunca em componentes Blazor ou controllers
- Nunca vaze responsabilidades entre camadas
- Nunca duplique lógica existente
- Enforcement de limites de plano centralizado em um único lugar na camada
  de serviço — nunca verificado em múltiplos pontos de forma duplicada
- Limites de plano configurados como dados (seed), nunca hardcoded no código
- Migrations via EF Core — nunca DDL manual
- Configurações sensíveis: User Secrets (dev) e variáveis de ambiente (prod)
- Dados de cartão nunca armazenados — nem os mockados
- Todos os estados de UI implementados: vazio, carregando, erro, sucesso,
  limite atingido, sem permissão
- Estado "limite atingido" sempre com CTA de upgrade — nunca mensagem técnica
- Mensagens de erro em linguagem humana — nunca mensagens técnicas
- Ações destrutivas sempre com confirmação explícita e aviso de consequência
- O boilerplate deve compilar e rodar antes de qualquer feature ser iniciada
- O seed deve garantir que o jurado veja algo funcional e bonito ao abrir
  o sistema pela primeira vez, sem precisar cadastrar nada
- A página pública é produto final: performática, bonita e mobile-first
- Registro de métricas é assíncrono e nunca bloqueia o carregamento da página
- O painel admin deve ser inacessível a usuários comuns por design de
  arquitetura, não apenas por ocultação de UI
- Trial só pode ser iniciado uma vez por plano por usuário — enforçado no banco
- Downgrade nunca deleta dados silenciosamente — sempre preserva e bloqueia
- Antes de implementar qualquer feature: confirme que o PRD cobre aquele caso
- Se uma decisão conflitar com a arquitetura do PRD: sinalize, não assuma