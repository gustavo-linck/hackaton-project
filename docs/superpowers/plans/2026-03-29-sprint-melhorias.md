# Sprint Melhorias UmbLink — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar 5 melhorias: preview de upload, admin nav ativa, wizard unificado, dashboard animado e auditoria de UX.

**Architecture:** Cada tarefa é independente e toca arquivos distintos. T5 e T1 compartilham `site.css` mas em seções separadas. Ordem: T4 → T2 → T3 → T5 → T1.

**Tech Stack:** .NET 8, Blazor Server, Bootstrap 5.3, CSS variables, JavaScript (app.js), SortableJS, IJSRuntime

---

## Task 1 (T4): Preview imediato de upload de imagem — Avatar

**Files:**
- Modify: `src/UmbLink.Web/Pages/Dashboard/Editor.razor` — seção avatar upload (linhas ~83–111 e método `HandleAvatarUpload` ~747–791)

### Contexto
Atualmente `HandleAvatarUpload` envia o arquivo direto sem mostrar preview. O usuário só vê o resultado após o upload completar. `_editAvatarUrl` só é preenchido após resposta do servidor.

- [ ] **Step 1: Adicionar campo de preview local no @code**

No bloco `@code`, logo após a declaração de `_avatarUploadError = ""` (~linha 479), adicionar:
```csharp
string _avatarLocalPreview = ""; // base64 data URL para preview antes do upload
```

- [ ] **Step 2: Atualizar HandleAvatarUpload para gerar preview antes de enviar**

Substituir o método `HandleAvatarUpload` (linhas ~747–791) por:
```csharp
async Task HandleAvatarUpload(InputFileChangeEventArgs e)
{
    _avatarUploading = true;
    _avatarUploadError = "";
    _avatarLocalPreview = "";
    StateHasChanged();

    try
    {
        var file = e.File;
        if (file.Size > 2 * 1024 * 1024)
        {
            _avatarUploadError = "Arquivo muito grande (máx. 2 MB)";
            return;
        }

        // Gerar preview local imediato (base64, max 400KB para o preview)
        using var previewStream = file.OpenReadStream(maxAllowedSize: 2 * 1024 * 1024);
        var buffer = new byte[file.Size];
        await previewStream.ReadExactlyAsync(buffer);
        _avatarLocalPreview = $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer)}";
        StateHasChanged();

        // Upload para o servidor
        using var content = new MultipartFormDataContent();
        using var uploadStream = new MemoryStream(buffer);
        content.Add(new StreamContent(uploadStream), "file", file.Name);

        var http = HttpFactory.CreateClient();
        http.BaseAddress = new Uri(GetBaseUrl());
        var cookies = HttpCtxAccessor.HttpContext?.Request.Headers.Cookie.ToString();
        if (!string.IsNullOrEmpty(cookies)) http.DefaultRequestHeaders.Add("Cookie", cookies);

        var response = await http.PostAsync("/api/upload/avatar", content);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadFromJsonAsync<UploadResult>();
            if (json?.Url is not null)
            {
                _editAvatarUrl = json.Url;
                if (_page is not null) _page = _page with { AvatarUrl = _editAvatarUrl };
                _avatarLocalPreview = ""; // limpa preview local — usa a URL definitiva
            }
        }
        else
        {
            _avatarUploadError = response.StatusCode == System.Net.HttpStatusCode.RequestEntityTooLarge
                ? "Arquivo muito grande (máx. 2 MB)"
                : "Erro ao enviar arquivo. Tente novamente.";
            _avatarLocalPreview = "";
        }
    }
    catch (Exception)
    {
        _avatarUploadError = "Não foi possível enviar. Verifique sua conexão e tente novamente.";
        _avatarLocalPreview = "";
    }
    finally
    {
        _avatarUploading = false;
        StateHasChanged();
    }
}
```

- [ ] **Step 3: Atualizar o markup de avatar upload para exibir preview**

Localizar o bloco de avatar upload no `@if (_avatarMode == "upload")` (~linhas 95–111). Substituir por:
```razor
<InputFile OnChange="HandleAvatarUpload" accept="image/jpeg,image/png,image/webp"
           class="form-control-theme w-100" MaxFileSize="2097152" />

@if (!string.IsNullOrEmpty(_avatarLocalPreview) || (!string.IsNullOrEmpty(_editAvatarUrl) && _avatarMode == "upload"))
{
    <div style="margin-top:.5rem;display:flex;align-items:center;gap:.75rem">
        <img src="@(_avatarLocalPreview != "" ? _avatarLocalPreview : _editAvatarUrl)"
             alt="preview"
             style="width:56px;height:56px;object-fit:cover;border-radius:50%;border:2px solid var(--border)" />
        <span style="font-size:.72rem;color:var(--text-muted)">
            @(_avatarLocalPreview != "" ? "Preview — enviando..." : "Avatar atual")
        </span>
    </div>
}
@if (_avatarUploading)
{
    <div class="d-flex align-items-center gap-2 mt-1" style="font-size:.75rem;color:var(--text-muted)">
        <span class="spinner-border spinner-border-sm"></span> Enviando...
    </div>
}
@if (!string.IsNullOrEmpty(_avatarUploadError))
{
    <div style="font-size:.75rem;color:var(--danger);margin-top:.25rem">
        <i class="bi bi-exclamation-circle me-1"></i>@_avatarUploadError
    </div>
}
```

- [ ] **Step 4: Verificar e rodar o projeto**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Esperado: build sem erros. Abrir editor de uma página, aba Avatar → "Fazer upload", selecionar imagem → deve aparecer preview circular imediatamente, spinner durante envio.

- [ ] **Step 5: Commit**

```bash
git add src/UmbLink.Web/Pages/Dashboard/Editor.razor
git commit -m "feat: show immediate avatar preview before upload completes"
```

---

## Task 2 (T4b): Preview imediato de upload — Imagem de fundo

**Files:**
- Modify: `src/UmbLink.Web/Pages/Dashboard/Editor.razor` — seção background upload e método `HandleBgUpload` (~793–840)

### Contexto
Mesmo problema do avatar: `HandleBgUpload` não mostra preview antes do upload. Adicionar campo `_bgLocalPreview` e mesmo padrão do Task 1.

- [ ] **Step 1: Adicionar _bgLocalPreview no @code**

Logo após `_bgUploadError = ""` (~linha 495):
```csharp
string _bgLocalPreview = ""; // base64 preview para fundo antes do upload
```

- [ ] **Step 2: Atualizar HandleBgUpload**

Localizar `HandleBgUpload` (~linha 793). Substituir pelo mesmo padrão do avatar (adaptado para background):
```csharp
async Task HandleBgUpload(InputFileChangeEventArgs e)
{
    _bgUploading = true;
    _bgUploadError = "";
    _bgLocalPreview = "";
    StateHasChanged();

    try
    {
        var file = e.File;
        if (file.Size > 2 * 1024 * 1024)
        {
            _bgUploadError = "Arquivo muito grande (máx. 2 MB)";
            return;
        }

        using var previewStream = file.OpenReadStream(maxAllowedSize: 2 * 1024 * 1024);
        var buffer = new byte[file.Size];
        await previewStream.ReadExactlyAsync(buffer);
        _bgLocalPreview = $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer)}";
        StateHasChanged();

        using var content = new MultipartFormDataContent();
        using var uploadStream = new MemoryStream(buffer);
        content.Add(new StreamContent(uploadStream), "file", file.Name);

        var http = HttpFactory.CreateClient();
        http.BaseAddress = new Uri(GetBaseUrl());
        var cookies = HttpCtxAccessor.HttpContext?.Request.Headers.Cookie.ToString();
        if (!string.IsNullOrEmpty(cookies)) http.DefaultRequestHeaders.Add("Cookie", cookies);

        var response = await http.PostAsync($"/api/upload/background?pageId={PageId}", content);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadFromJsonAsync<UploadResult>();
            if (json?.Url is not null)
            {
                _bgImageUrl = json.Url;
                _bgLocalPreview = "";
                UpdatePreviewTheme();
            }
        }
        else
        {
            _bgUploadError = "Erro ao enviar imagem de fundo. Tente novamente.";
            _bgLocalPreview = "";
        }
    }
    catch (Exception)
    {
        _bgUploadError = "Não foi possível enviar. Verifique sua conexão e tente novamente.";
        _bgLocalPreview = "";
    }
    finally
    {
        _bgUploading = false;
        StateHasChanged();
    }
}
```

- [ ] **Step 3: Exibir preview de fundo no markup**

Localizar onde `_bgUploading` e `_bgUploadError` são exibidos na seção de background (~linhas 330–360). Adicionar preview logo após o `InputFile` de background:
```razor
@if (!string.IsNullOrEmpty(_bgLocalPreview) || (!string.IsNullOrEmpty(_bgImageUrl) && _bgMode == "upload"))
{
    var previewSrc = _bgLocalPreview != "" ? _bgLocalPreview : _bgImageUrl;
    <div style="margin-top:.5rem;border-radius:8px;overflow:hidden;height:60px;position:relative">
        <img src="@previewSrc" alt="preview fundo"
             style="width:100%;height:100%;object-fit:cover" />
        @if (_bgLocalPreview != "")
        {
            <div style="position:absolute;inset:0;background:rgba(0,0,0,0.4);display:flex;align-items:center;justify-content:center">
                <span class="spinner-border spinner-border-sm text-white"></span>
            </div>
        }
    </div>
}
@if (!string.IsNullOrEmpty(_bgUploadError))
{
    <div style="font-size:.75rem;color:var(--danger);margin-top:.25rem">
        <i class="bi bi-exclamation-circle me-1"></i>@_bgUploadError
    </div>
}
```

- [ ] **Step 4: Build e verificar**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Esperado: sem erros. No editor, seção Aparência → Fundo → Upload → selecionar imagem → preview em thumbnail aparece com spinner, depois mostra a imagem após upload.

- [ ] **Step 5: Commit**

```bash
git add src/UmbLink.Web/Pages/Dashboard/Editor.razor
git commit -m "feat: show immediate background image preview before upload completes"
```

---

## Task 3 (T2): Admin sidebar — active state e tema consistente

**Files:**
- Modify: `src/UmbLink.Web/Layouts/AdminLayout.razor`

### Contexto
`AdminLayout.razor` usa Bootstrap `bg-dark`/`text-white` que não respeita o tema do app. Os links do sidebar não destacam o item ativo. Solução: injetar `NavigationManager`, comparar URI atual, e migrar para CSS variables.

- [ ] **Step 1: Adicionar injeções e lógica de rota ativa**

No topo do arquivo, após `@inherits LayoutComponentBase`, adicionar:
```razor
@inject NavigationManager Nav
```

No bloco `@code` (criar se não existir ou adicionar ao existente):
```razor
@code {
    bool IsActive(string href) =>
        Nav.Uri.TrimEnd('/').EndsWith(href.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
}
```

- [ ] **Step 2: Substituir o AdminLayout completo**

Substituir todo o conteúdo de `AdminLayout.razor` por:
```razor
@inherits LayoutComponentBase
@inject NavigationManager Nav
@attribute [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]

@* Mobile top bar *@
<div class="d-flex d-md-none align-items-center px-3 py-2 sticky-top"
     style="background:var(--bg-card);border-bottom:1px solid var(--border);z-index:1045">
    <button class="btn-ghost me-2" type="button"
            data-bs-toggle="offcanvas"
            data-bs-target="#adminSidebar"
            aria-controls="adminSidebar">
        <i class="bi bi-list fs-5"></i>
    </button>
    <span style="font-weight:700;font-size:.95rem;color:var(--text-primary)">Admin</span>
</div>

@* Offcanvas sidebar mobile *@
<div class="offcanvas offcanvas-start" tabindex="-1" id="adminSidebar"
     style="width:220px;background:var(--bg-card);border-right:1px solid var(--border)">
    <div class="offcanvas-header" style="border-bottom:1px solid var(--border)">
        <span style="font-weight:700;font-size:.85rem;color:var(--text-secondary);text-transform:uppercase;letter-spacing:.05em">Admin</span>
        <button type="button" class="btn-ghost" data-bs-dismiss="offcanvas" aria-label="Fechar">
            <i class="bi bi-x-lg"></i>
        </button>
    </div>
    <div class="offcanvas-body p-3 d-flex flex-column">
        <nav class="nav flex-column gap-1">
            <a href="/admin" class="admin-nav-link @(IsActive("/admin") && !Nav.Uri.Contains("/admin/") ? "active" : "")">
                <i class="bi bi-bar-chart me-2"></i>Dashboard
            </a>
            <a href="/admin/users" class="admin-nav-link @(IsActive("/admin/users") ? "active" : "")">
                <i class="bi bi-people me-2"></i>Usuários
            </a>
        </nav>
        <div class="mt-auto pt-3" style="border-top:1px solid var(--border)">
            <a href="/dashboard" class="admin-nav-link" style="font-size:.82rem">
                <i class="bi bi-arrow-left me-1"></i>Voltar ao app
            </a>
        </div>
    </div>
</div>

<div class="d-flex min-vh-100">
    @* Desktop sidebar *@
    <aside class="d-none d-md-flex flex-column flex-shrink-0 p-3"
           style="width:220px;background:var(--bg-card);border-right:1px solid var(--border)">
        <span style="font-weight:700;font-size:.85rem;color:var(--text-secondary);text-transform:uppercase;letter-spacing:.05em;margin-bottom:.75rem;display:block">Admin</span>
        <nav class="nav flex-column gap-1">
            <a href="/admin" class="admin-nav-link @(IsActive("/admin") && !Nav.Uri.Contains("/admin/") ? "active" : "")">
                <i class="bi bi-bar-chart me-2"></i>Dashboard
            </a>
            <a href="/admin/users" class="admin-nav-link @(IsActive("/admin/users") ? "active" : "")">
                <i class="bi bi-people me-2"></i>Usuários
            </a>
        </nav>
        <div class="mt-auto pt-3" style="border-top:1px solid var(--border)">
            <a href="/dashboard" class="admin-nav-link" style="font-size:.82rem">
                <i class="bi bi-arrow-left me-1"></i>Voltar ao app
            </a>
        </div>
    </aside>
    <main class="flex-grow-1 p-3 p-md-4" style="background:var(--bg-primary)">@Body</main>
</div>

@code {
    bool IsActive(string href) =>
        Nav.Uri.TrimEnd('/').EndsWith(href.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
}
```

- [ ] **Step 3: Adicionar classe .admin-nav-link em site.css**

Abrir `src/UmbLink.Web/wwwroot/css/site.css`. Adicionar ao final do arquivo:
```css
/* ===== Admin Sidebar ===== */
.admin-nav-link {
    display: flex;
    align-items: center;
    padding: .5rem .75rem;
    border-radius: 8px;
    font-size: .875rem;
    font-weight: 500;
    color: var(--text-secondary);
    text-decoration: none;
    transition: background .15s, color .15s;
}

.admin-nav-link:hover {
    background: rgba(99, 102, 241, 0.08);
    color: var(--text-primary);
}

.admin-nav-link.active {
    background: rgba(99, 102, 241, 0.12);
    color: var(--accent);
    font-weight: 600;
}
```

- [ ] **Step 4: Build e verificar**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Esperado: sem erros. Acessar `/admin` → sidebar usa cores do tema, link "Dashboard" fica destacado. Acessar `/admin/users` → "Usuários" fica destacado. Funciona no tema claro e escuro.

- [ ] **Step 5: Commit**

```bash
git add src/UmbLink.Web/Layouts/AdminLayout.razor src/UmbLink.Web/wwwroot/css/site.css
git commit -m "feat: admin sidebar with active nav state and theme-consistent styling"
```

---

## Task 4 (T3): Wizard — fundir step 3 + 4 em step unificado (perfil + links + preview)

**Files:**
- Modify: `src/UmbLink.Web/Pages/Onboarding/Wizard.razor`

### Contexto
Atualmente: step 3 = perfil (título/bio/slug), step 4 = links, step 5 = sucesso.
Meta: step 3 = perfil + links + preview ao vivo; step 4 = sucesso. Dois cards agrupados (Perfil / Links), preview `PagePreview` à direita. No mobile o preview colapsa com toggle.

- [ ] **Step 1: Atualizar constantes e mapeamentos**

No bloco `@code`, alterar:
```csharp
// Antes:
string StepMaxWidth => _step switch { 2 => "780px", 3 or 4 => "900px", _ => "640px" };
int VisualStep => _step switch { 4 => 3, 5 => 4, _ => _step };
int TotalVisualSteps => 4;

// Depois:
string StepMaxWidth => _step switch { 2 => "780px", 3 => "960px", _ => "640px" };
int VisualStep => _step switch { 4 => 3, _ => _step };
int TotalVisualSteps => 3;
```

- [ ] **Step 2: Adicionar estado de preview mobile no @code**

No bloco `@code`, após `bool _creatingLinks;`, adicionar:
```csharp
bool _showMobilePreview;
```

- [ ] **Step 3: Criar helper para links do preview**

No bloco `@code`, adicionar propriedade:
```csharp
List<LinkDto> PreviewLinks => _initialLinks
    .Where(l => !string.IsNullOrWhiteSpace(l.Title))
    .Select((l, i) => new LinkDto(Guid.Empty, Guid.Empty, l.Title, l.Url, null, true, i + 1, DateTime.UtcNow))
    .ToList();
```

- [ ] **Step 4: Atualizar CreatePageAsync — vai direto para step 4 (sucesso)**

Localizar `_step = 4;` dentro de `CreatePageAsync`. Substituir por:
```csharp
// Salvar links opcionais
var validLinks = _initialLinks
    .Where(l => !string.IsNullOrWhiteSpace(l.Title) && !string.IsNullOrWhiteSpace(l.Url))
    .ToList();

foreach (var link in validLinks)
{
    await LinkSvc.AddAsync(_userId, _createdPageId, new()
    {
        Title = link.Title,
        Url = link.Url
    });
}

_creating = false;
_step = 4; // step 4 = sucesso (antigo step 5)
```

Remover também o `_creating = false;` que havia antes do `_step = 4` original.

- [ ] **Step 5: Substituir o bloco @if (_step == 3) no markup**

Localizar o bloco `@* ───── Step 3: Page details ───── *@` e substituir completamente por:

```razor
@* ───── Step 3: Perfil + Links + Preview ───── *@
@if (_step == 3)
{
    <div class="animate-fade-in-up">
        <div class="row g-4">
            @* Coluna esquerda: formulário *@
            <div class="col-12 col-lg-7">

                @if (!string.IsNullOrEmpty(_error))
                {
                    <div class="alert-theme mb-3" style="border-color:rgba(239,68,68,0.3);background:rgba(239,68,68,0.08)">
                        <i class="bi bi-exclamation-circle" style="color:var(--danger)"></i> @_error
                    </div>
                }

                @* Card: Perfil *@
                <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:14px;padding:1.25rem;margin-bottom:1rem">
                    <div style="font-weight:700;font-size:.85rem;color:var(--accent);margin-bottom:1rem;display:flex;align-items:center;gap:.5rem">
                        <i class="bi bi-person-circle"></i> Perfil
                    </div>

                    <div style="margin-bottom:.875rem">
                        <label class="form-label-theme" style="font-weight:600">Seu nome ou marca</label>
                        <input class="form-control-theme w-100" placeholder="Meu Nome"
                               value="@_pageTitle" @oninput="OnTitleInput" maxlength="60" />
                        <div style="font-size:.7rem;text-align:right;color:@(_pageTitle.Length >= 54 ? "var(--danger)" : "var(--text-muted)")">@_pageTitle.Length / 60</div>
                    </div>

                    <div style="margin-bottom:.875rem">
                        <label class="form-label-theme" style="font-weight:600">Endereço da página</label>
                        <div class="d-flex">
                            <span class="form-control-theme d-flex align-items-center text-theme-muted"
                                  style="border-radius:10px 0 0 10px;border-right:none;font-size:.8rem;padding:.5rem .625rem;white-space:nowrap">@PublicPagePrefix</span>
                            <input class="form-control-theme w-100" style="border-radius:0 10px 10px 0"
                                   placeholder="meu-slug" value="@_pageSlug" @oninput="OnSlugInput" maxlength="30" />
                        </div>
                        <div style="font-size:.7rem;text-align:right;color:@(_pageSlug.Length >= 27 ? "var(--danger)" : "var(--text-muted)")">@_pageSlug.Length / 30</div>
                    </div>

                    <div style="margin-bottom:.875rem">
                        <label class="form-label-theme" style="font-weight:600">Bio (opcional)</label>
                        <textarea class="form-control-theme w-100" rows="2" maxlength="200"
                                  value="@_pageBio" @oninput="e => _pageBio = e.Value?.ToString() ?? string.Empty"
                                  placeholder="Uma breve descrição sobre você..."></textarea>
                        <div style="font-size:.7rem;text-align:right;color:@(_pageBio.Length >= 180 ? "var(--danger)" : "var(--text-muted)")">@_pageBio.Length / 200</div>
                    </div>

                    @* AI helper *@
                    <div style="border:1px solid var(--border);border-radius:10px;padding:.875rem;background:rgba(99,102,241,0.04)">
                        <div style="display:flex;align-items:center;gap:.5rem;margin-bottom:.625rem">
                            <i class="bi bi-stars" style="color:var(--accent)"></i>
                            <span style="font-weight:600;font-size:.82rem;color:var(--text-primary)">Gerar com IA</span>
                        </div>
                        <textarea class="form-control-theme w-100" rows="2"
                                  placeholder="Descreva quem você é em uma frase..."
                                  @bind="_aiDescription" style="margin-bottom:.5rem" />
                        @if (!string.IsNullOrEmpty(_aiError))
                        {
                            <div style="font-size:.75rem;color:var(--danger);margin-bottom:.375rem">@_aiError</div>
                        }
                        @if (!string.IsNullOrEmpty(_aiSuccessMsg))
                        {
                            <div style="font-size:.75rem;color:var(--success);margin-bottom:.375rem">@_aiSuccessMsg</div>
                        }
                        <button class="btn-accent" style="font-size:.8rem;padding:.3rem .75rem"
                                @onclick="GenerateWithAI" disabled="@(_aiLoading || _aiCallCount >= 3)">
                            @if (_aiLoading) { <span class="spinner-border spinner-border-sm me-1"></span> }
                            else { <i class="bi bi-stars me-1"></i> }
                            @(_aiCallCount >= 3 ? "Limite atingido" : "Gerar com IA")
                        </button>
                        @if (_aiCallCount > 0)
                        {
                            <span style="font-size:.7rem;color:var(--text-muted);margin-left:.5rem">@_aiCallCount / 3</span>
                        }
                    </div>
                </div>

                @* Card: Links *@
                <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:14px;padding:1.25rem;margin-bottom:1rem">
                    <div style="font-weight:700;font-size:.85rem;color:var(--accent);margin-bottom:1rem;display:flex;align-items:center;gap:.5rem">
                        <i class="bi bi-link-45deg"></i> Links
                        <span style="font-weight:400;font-size:.75rem;color:var(--text-muted)">(opcional)</span>
                    </div>

                    @if (!string.IsNullOrEmpty(_linkError))
                    {
                        <div class="alert-theme mb-3" style="border-color:rgba(239,68,68,0.3);background:rgba(239,68,68,0.08)">
                            <i class="bi bi-exclamation-circle" style="color:var(--danger)"></i> @_linkError
                        </div>
                    }

                    @for (int i = 0; i < _initialLinks.Count; i++)
                    {
                        var idx = i;
                        <div style="background:var(--bg-primary);border:1px solid var(--border);border-radius:10px;padding:.75rem;margin-bottom:.625rem">
                            <div style="display:flex;gap:.625rem;align-items:center">
                                <div style="width:26px;height:26px;border-radius:50%;background:rgba(99,102,241,0.1);display:flex;align-items:center;justify-content:center;flex-shrink:0">
                                    <span style="font-weight:700;font-size:.75rem;color:var(--accent)">@(idx + 1)</span>
                                </div>
                                <div style="flex:1;display:flex;gap:.375rem;flex-direction:column">
                                    <input class="form-control-theme w-100"
                                           placeholder="Título (ex: Instagram)"
                                           value="@_initialLinks[idx].Title"
                                           @oninput="e => UpdateLinkTitle(idx, e.Value?.ToString() ?? string.Empty)"
                                           maxlength="80" />
                                    <input class="form-control-theme w-100"
                                           placeholder="URL (ex: https://instagram.com/seuperfil)"
                                           value="@_initialLinks[idx].Url"
                                           @oninput="e => UpdateLinkUrl(idx, e.Value?.ToString() ?? string.Empty)"
                                           maxlength="2048" />
                                </div>
                            </div>
                        </div>
                    }
                </div>

                @* Botão preview mobile *@
                <button class="btn-ghost d-lg-none w-100 mb-3" @onclick="() => _showMobilePreview = !_showMobilePreview">
                    <i class="bi bi-eye me-1"></i>@(_showMobilePreview ? "Ocultar" : "Ver") preview
                </button>

                @* Preview mobile colapsível *@
                @if (_showMobilePreview)
                {
                    <div class="d-lg-none d-flex flex-column align-items-center mb-4">
                        <PagePreview Page="GetPreviewDto()" Links="PreviewLinks" />
                        <small class="text-theme-muted mt-2">Preview ao vivo</small>
                    </div>
                }

                <div class="d-flex justify-content-between mt-2">
                    <button class="btn-ghost" @onclick="() => _step = 2">
                        <i class="bi bi-arrow-left me-1"></i> Voltar
                    </button>
                    <button class="btn-accent" @onclick="CreatePageAsync" disabled="@_creating">
                        @if (_creating) { <span class="spinner-border spinner-border-sm me-1"></span> }
                        Criar minha página <i class="bi bi-arrow-right ms-1"></i>
                    </button>
                </div>
            </div>

            @* Coluna direita: preview desktop *@
            <div class="col-lg-5 d-none d-lg-flex flex-column align-items-center justify-content-start pt-2">
                <PagePreview Page="GetPreviewDto()" Links="PreviewLinks" />
                <small class="text-theme-muted mt-2">Preview ao vivo</small>
            </div>
        </div>
    </div>
}
```

- [ ] **Step 6: Remover bloco do step 4 (links separado) e renomear step 5 → step 4**

Localizar `@* ───── Step 4: Add initial links ───── *@` e remover o bloco inteiro (`@if (_step == 4) { ... }`).

Localizar `@* ───── Step 5: Success ───── *@` e alterar a condição de `@if (_step == 5)` para `@if (_step == 4)`.

- [ ] **Step 7: Remover métodos SaveLinksAsync e SkipLinks (incorporados em CreatePageAsync)**

No bloco `@code`, remover os métodos `SaveLinksAsync()` e `SkipLinks()` — a lógica de salvar links agora está dentro de `CreatePageAsync`.

- [ ] **Step 8: Build e verificar**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Esperado: sem erros. Acessar `/onboarding` → step 3 mostra cards "Perfil" e "Links" + preview ao vivo à direita. Digitar título → preview atualiza. Preencher link → aparece no preview. No mobile: botão "Ver preview" colapsa/abre o preview.

- [ ] **Step 9: Commit**

```bash
git add src/UmbLink.Web/Pages/Onboarding/Wizard.razor
git commit -m "feat: merge wizard steps 3+4 into unified profile+links step with live preview"
```

---

## Task 5 (T5): Dashboard — remover blocos e adicionar fundo animado

**Files:**
- Modify: `src/UmbLink.Web/Pages/Dashboard/Index.razor`
- Modify: `src/UmbLink.Web/wwwroot/css/site.css`

### Contexto
Remover seção "Atividade Recente" + "Próximos Passos" (linhas ~136–231). Adicionar fundo animado gradiente escuro + glass morphism nos cards (opção A aprovada). Reutiliza `initHeroGradient` do `app.js`.

- [ ] **Step 1: Adicionar CSS para dashboard animado em site.css**

Adicionar ao final de `site.css` (após a seção Admin Sidebar adicionada em Task 3):
```css
/* ===== Dashboard Animated Background ===== */
.dashboard-animated-bg {
    --gx: 50%;
    --gy: 50%;
    position: relative;
    min-height: 100%;
    background: #080d1a;
    overflow: hidden;
}

[data-theme="light"] .dashboard-animated-bg {
    background: #f0f2ff;
}

.dashboard-animated-bg::before {
    content: '';
    position: absolute;
    inset: 0;
    background:
        radial-gradient(ellipse 700px 700px at var(--gx) var(--gy), rgba(85, 124, 242, 0.28), transparent 70%),
        radial-gradient(ellipse 500px 500px at calc(100% - var(--gx)) calc(100% - var(--gy)), rgba(168, 85, 247, 0.18), transparent 70%);
    pointer-events: none;
    z-index: 0;
}

[data-theme="light"] .dashboard-animated-bg::before {
    background:
        radial-gradient(ellipse 700px 700px at var(--gx) var(--gy), rgba(85, 124, 242, 0.10), transparent 70%),
        radial-gradient(ellipse 500px 500px at calc(100% - var(--gx)) calc(100% - var(--gy)), rgba(168, 85, 247, 0.07), transparent 70%);
}

.dashboard-animated-bg > .dashboard-content {
    position: relative;
    z-index: 1;
}

/* Glass morphism card para o dashboard */
.dashboard-glass-card {
    background: rgba(255, 255, 255, 0.06);
    backdrop-filter: blur(14px);
    -webkit-backdrop-filter: blur(14px);
    border: 1px solid rgba(255, 255, 255, 0.10);
    border-radius: 12px;
    padding: 1rem;
    color: #fff;
}

[data-theme="light"] .dashboard-glass-card {
    background: rgba(255, 255, 255, 0.75);
    border: 1px solid rgba(99, 102, 241, 0.15);
    color: var(--text-primary);
}

.dashboard-glass-card .metric-label {
    font-size: .75rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: .04em;
    margin-bottom: .25rem;
    color: rgba(255, 255, 255, 0.55);
}

[data-theme="light"] .dashboard-glass-card .metric-label {
    color: var(--text-secondary);
}

.dashboard-glass-card .metric-value {
    font-size: 1.75rem;
    font-weight: 700;
    color: #fff;
}

[data-theme="light"] .dashboard-glass-card .metric-value {
    color: var(--text-primary);
}

.dashboard-glass-card .metric-sub {
    font-size: .75rem;
    color: rgba(255, 255, 255, 0.45);
}

[data-theme="light"] .dashboard-glass-card .metric-sub {
    color: var(--text-muted);
}
```

- [ ] **Step 2: Remover ActivitySvc, _activityLogs e blocos HTML no Dashboard/Index.razor**

Em `Dashboard/Index.razor`:

**a)** Remover a linha de injeção:
```razor
@inject IUserActivityLogService ActivitySvc
```

**b)** Remover o campo no `@code`:
```csharp
List<UserActivityLogDto> _activityLogs = [];
```

**c)** Remover esta linha em `OnInitializedAsync`:
```csharp
_activityLogs = await ActivitySvc.GetLogsAsync(userId, 8);
```

**d)** Remover todo o bloco HTML entre os comentários `@* ─── Recent Activity + Next Steps ─── *@` e o `<!-- Create modal` (linhas ~136–231 inclusive).

**e)** Remover os métodos `GetActivityIcon` e `GetActivityLabel` do `@code`.

- [ ] **Step 3: Envolver o conteúdo do dashboard no wrapper animado**

O arquivo atual começa com `<TrialBanner .../>` seguido de `<div class="container py-4" ...>`.

Envolver o conteúdo total em:
```razor
<div class="dashboard-animated-bg" @ref="_dashboardBgRef">
    <div class="dashboard-content container py-4" style="max-width:960px">
        @* ... todo o conteúdo existente do container ... *@
    </div>
</div>
```

O `<TrialBanner>` fica **fora** do wrapper (mantém estilo próprio).

- [ ] **Step 4: Atualizar os metric cards para usar classes glass**

Localizar os 4 `<div>` de métricas (col-6 col-md-3) que usam `style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1rem"`.

Substituir cada um pela classe `dashboard-glass-card`:
```razor
<div class="dashboard-glass-card">
    <div class="metric-label">Views (30d)</div>
    <div class="metric-value">@_totalViews30d</div>
    <div class="metric-sub"><i class="bi bi-eye me-1"></i>visualizações</div>
</div>
```
(Repetir para os 4 cards com seus respectivos valores: Views, Cliques, Páginas, Top página)

- [ ] **Step 5: Adicionar ElementReference e chamar initHeroGradient**

No bloco `@code`, adicionar campo:
```csharp
ElementReference _dashboardBgRef;
```

Em `OnAfterRenderAsync`:
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
        await JS.InvokeVoidAsync("umblink.initHeroGradient", _dashboardBgRef);
}
```

Certificar que `[Inject] IJSRuntime JS { get; set; } = default!;` está declarado (se não estiver).

- [ ] **Step 6: Build e verificar**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Esperado: sem erros. Dashboard exibe fundo escuro com gradiente animado que segue o mouse. Cards com glass morphism. No tema claro, fundo azul-lavanda suave com cards brancos semitransparentes. Sem os blocos de atividade/próximos passos.

- [ ] **Step 7: Commit**

```bash
git add src/UmbLink.Web/Pages/Dashboard/Index.razor src/UmbLink.Web/wwwroot/css/site.css
git commit -m "feat: dashboard animated gradient bg with glass morphism cards, remove activity blocks"
```

---

## Task 6 (T1): UX/UI — dark mode na tabela admin e touch targets

**Files:**
- Modify: `src/UmbLink.Web/wwwroot/css/site.css`
- Modify: `src/UmbLink.Web/Pages/Admin/Users.razor`

### Contexto
A tabela Bootstrap em `Admin/Users.razor` não respeita o tema escuro do app (fundo branco, texto preto). Os botões de ação no `page-card-footer` podem ter touch target menor que 44px em mobile.

- [ ] **Step 1: Adicionar estilos de tabela dark mode em site.css**

Adicionar após a seção `/* ===== Admin Sidebar =====*/`:
```css
/* ===== Table dark mode ===== */
[data-theme="dark"] .table {
    color: var(--text-primary);
    border-color: var(--border);
}

[data-theme="dark"] .table thead th {
    color: var(--text-secondary);
    border-color: var(--border);
    background: var(--bg-card);
}

[data-theme="dark"] .table tbody tr td {
    border-color: var(--border);
    background: transparent;
}

[data-theme="dark"] .table tbody tr:hover td {
    background: rgba(99, 102, 241, 0.06);
}

[data-theme="dark"] .table-secondary {
    background: rgba(255, 255, 255, 0.04) !important;
}
```

- [ ] **Step 2: Garantir touch targets nos page-card-footer buttons**

Em `site.css`, localizar ou adicionar:
```css
/* Garantir touch target mínimo nos botões de ação do page card */
.page-card-footer .btn-ghost {
    min-width: 44px;
    min-height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
}
```

- [ ] **Step 3: Build e verificar**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Esperado: sem erros. No tema escuro, acessar `/admin/users` → tabela com fundo escuro, texto legível, linhas separadas por bordas temáticas. Em mobile, botões de ação no dashboard têm área de toque confortável.

- [ ] **Step 4: Commit final**

```bash
git add src/UmbLink.Web/wwwroot/css/site.css
git commit -m "fix: table dark mode styles and minimum touch target for page card action buttons"
```

---

## Self-Review

**Spec coverage:**
- T4 upload preview ✅ (Tasks 1 e 2)
- T2 admin nav ✅ (Task 3)
- T3 wizard unificado ✅ (Task 4)
- T5 dashboard animado ✅ (Task 5)
- T1 UX audit ✅ (Task 6)

**Placeholders:** nenhum TBD ou "implement later" encontrado.

**Consistência de tipos:**
- `PreviewLinks` retorna `List<LinkDto>` — mesmo tipo que `PagePreview` recebe no parâmetro `Links`
- `InitialLink` record já existe no Wizard — reutilizado
- `_dashboardBgRef` é `ElementReference` — compatível com `umblink.initHeroGradient(el)` que espera elemento DOM
- `_bgLocalPreview` e `_avatarLocalPreview` são `string` — compatíveis com `src` de `<img>`
