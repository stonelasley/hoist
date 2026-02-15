using Hoist.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Hoist.Application.ExerciseTemplates.Commands.DeleteExerciseImage;

public record DeleteExerciseImageCommand(int Id) : IRequest;

public class DeleteExerciseImageCommandHandler : IRequestHandler<DeleteExerciseImageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IWebHostEnvironment _environment;

    public DeleteExerciseImageCommandHandler(IApplicationDbContext context, IUser user, IWebHostEnvironment environment)
    {
        _context = context;
        _user = user;
        _environment = environment;
    }

    public async Task Handle(DeleteExerciseImageCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var entity = await _context.ExerciseTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        if (!string.IsNullOrWhiteSpace(entity.ImagePath))
        {
            var filePath = Path.Combine(_environment.WebRootPath, entity.ImagePath.TrimStart('/'));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            entity.ImagePath = null;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
