using Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Commands.DeleteExerciseImage;
using Hoist.Application.ExerciseTemplates.Commands.DeleteExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Commands.UpdateExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Commands.UploadExerciseImage;
using Hoist.Application.ExerciseTemplates.Queries.GetExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Queries.GetExerciseTemplates;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hoist.Web.Endpoints;

public class ExerciseTemplates : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetExerciseTemplates).RequireAuthorization();
        groupBuilder.MapGet(GetExerciseTemplate, "{id}").RequireAuthorization();
        groupBuilder.MapPost(CreateExerciseTemplate).RequireAuthorization();
        groupBuilder.MapPut(UpdateExerciseTemplate, "{id}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteExerciseTemplate, "{id}").RequireAuthorization();
        groupBuilder.MapPost(UploadExerciseImage, "{id}/Image").RequireAuthorization().DisableAntiforgery();
        groupBuilder.MapDelete(DeleteExerciseImage, "{id}/Image").RequireAuthorization();
    }

    public async Task<Ok<List<ExerciseTemplateBriefDto>>> GetExerciseTemplates(ISender sender,
        [AsParameters] GetExerciseTemplatesQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<ExerciseTemplateDetailDto>> GetExerciseTemplate(ISender sender, int id)
    {
        var result = await sender.Send(new GetExerciseTemplateQuery(id));

        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateExerciseTemplate(ISender sender, CreateExerciseTemplateCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(ExerciseTemplates)}/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateExerciseTemplate(ISender sender, int id, UpdateExerciseTemplateCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteExerciseTemplate(ISender sender, int id)
    {
        await sender.Send(new DeleteExerciseTemplateCommand(id));

        return TypedResults.NoContent();
    }

    public async Task<Ok<string>> UploadExerciseImage(ISender sender, int id, IFormFile file)
    {
        var command = new UploadExerciseImageCommand
        {
            Id = id,
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var imagePath = await sender.Send(command);

        return TypedResults.Ok(imagePath);
    }

    public async Task<NoContent> DeleteExerciseImage(ISender sender, int id)
    {
        await sender.Send(new DeleteExerciseImageCommand(id));

        return TypedResults.NoContent();
    }
}
