using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UmbLink.Application.Requests;

namespace UmbLink.Application.Services;

public class GroqService(HttpClient http, IConfiguration config, ILogger<GroqService> logger)
{
    public async Task<GenerateProfileResponse?> GenerateProfileAsync(string userDescription, string profileType)
    {
        var apiKey = config["Groq:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogWarning("Groq:ApiKey not configured. AI profile generation disabled.");
            return null;
        }

        var prompt = $$"""
            O usuário quer criar uma página de links estilo Linktree.
            Tipo de perfil: {{profileType}}
            Descrição do usuário: {{userDescription}}

            Retorne APENAS um JSON válido, sem markdown, sem explicação:
            {
              "title": "nome/título da página (máx 60 chars, pode ser nome + profissão)",
              "bio": "bio curta e engajante (máx 200 chars, primeira pessoa)",
              "slug": "slug-sem-espacos-e-sem-acentos (máx 30 chars, lowercase, hifens)"
            }
            """;

        var body = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 200,
            temperature = 0.7
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post,
                "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = JsonContent.Create(body);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var response = await http.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Groq API returned {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<GroqApiResponse>();
            var json = result?.Choices?[0]?.Message?.Content;
            if (string.IsNullOrEmpty(json))
            {
                logger.LogWarning("Groq API returned empty content in choices.");
                return null;
            }

            // Strip markdown code fences if present
            json = json.Trim();
            if (json.StartsWith("```"))
            {
                var lines = json.Split('\n');
                json = string.Join("\n", lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
            }

            try
            {
                var profile = JsonSerializer.Deserialize<GenerateProfileResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return profile;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to deserialize Groq response. Raw content: {Json}", json);
                return null;
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Groq API request timed out after 15 seconds.");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error calling Groq API.");
            return null;
        }
    }
}

// Internal Groq API response shape
internal class GroqApiResponse
{
    [JsonPropertyName("choices")]
    public GroqChoice[]? Choices { get; set; }
}
internal class GroqChoice
{
    [JsonPropertyName("message")]
    public GroqMessage? Message { get; set; }
}
internal class GroqMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
