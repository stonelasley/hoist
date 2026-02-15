using Hoist.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Hoist.Application.ExerciseTemplates.Commands.UploadExerciseImage;

public record UploadExerciseImageCommand : IRequest<string>
{
    public int Id { get; init; }

    public Stream FileStream { get; init; } = null!;

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;
}

public class UploadExerciseImageCommandHandler : IRequestHandler<UploadExerciseImageCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IWebHostEnvironment _environment;

    public UploadExerciseImageCommandHandler(IApplicationDbContext context, IUser user, IWebHostEnvironment environment)
    {
        _context = context;
        _user = user;
        _environment = environment;
    }

    public async Task<string> Handle(UploadExerciseImageCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        var entity = await _context.ExerciseTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        var extension = Path.GetExtension(request.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "exercises");

        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await request.FileStream.CopyToAsync(fileStream, cancellationToken);
        }

        var relativePath = $"/uploads/exercises/{uniqueFileName}";

        entity.ImagePath = relativePath;

        await _context.SaveChangesAsync(cancellationToken);

        return relativePath;
    }
}
