using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.ExerciseTemplates.Commands.UpdateExerciseTemplate;

public class UpdateExerciseTemplateCommandValidator : AbstractValidator<UpdateExerciseTemplateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateExerciseTemplateCommandValidator(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200)
            .MustAsync(BeUniqueName)
                .WithMessage("'{PropertyName}' must be unique.")
                .WithErrorCode("Unique");

        RuleFor(v => v.ImplementType)
            .IsInEnum();

        RuleFor(v => v.ExerciseType)
            .IsInEnum();

        RuleFor(v => v.Model)
            .MaximumLength(500);
    }

    private async Task<bool> BeUniqueName(UpdateExerciseTemplateCommand command, string? name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            return true;

        var userId = _user.Id;

        return !await _context.ExerciseTemplates
            .AnyAsync(x => x.UserId == userId && x.Name == name && x.Id != command.Id, cancellationToken);
    }
}
