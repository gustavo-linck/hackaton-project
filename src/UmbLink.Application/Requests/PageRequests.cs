using FluentValidation;
namespace UmbLink.Application.Requests;

public class CreatePageRequest
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Bio { get; set; }
}

public class UpdatePageRequest
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? ThemeConfig { get; set; }
}

public class CreatePageRequestValidator : AbstractValidator<CreatePageRequest>
{
    public CreatePageRequestValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(30)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("O endereço deve conter apenas letras minúsculas, números e hífens.");
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class UpdatePageRequestValidator : AbstractValidator<UpdatePageRequest>
{
    public UpdatePageRequestValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(30)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("O endereço deve conter apenas letras minúsculas, números e hífens.");
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);
    }
}
