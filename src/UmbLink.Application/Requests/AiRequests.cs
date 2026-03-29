using System.Text.Json.Serialization;

namespace UmbLink.Application.Requests;

public record GenerateProfileRequest(string UserDescription, string ProfileType);
public record GenerateProfileResponse(
    string Title,
    string Bio,
    [property: JsonPropertyName("slug")] string SlugSuggestion);
