using FluentValidation;
namespace UmbLink.Application.Requests;

public class CreateLinkRequest
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? IconName { get; set; }
}

public class UpdateLinkRequest
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? IconName { get; set; }
    public bool IsActive { get; set; }
}

public class CreateLinkRequestValidator : AbstractValidator<CreateLinkRequest>
{
    public CreateLinkRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("O título deve ter no máximo 50 caracteres.");
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var u)
                && (u.Scheme == "https" || u.Scheme == "http"))
            .WithMessage("URL inválida. Use o formato https://...");
    }
}

public class UpdateLinkRequestValidator : AbstractValidator<UpdateLinkRequest>
{
    public UpdateLinkRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var u)
                && (u.Scheme == "https" || u.Scheme == "http"))
            .WithMessage("URL inválida. Use o formato https://...");
    }
}
