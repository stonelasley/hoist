using Hoist.Application.WorkoutTemplates.Commands.CreateWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Commands.DeleteWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplateExercises;
using Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplates;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hoist.Web.Endpoints;

public class WorkoutTemplates : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetWorkoutTemplates).RequireAuthorization();
        groupBuilder.MapGet(GetWorkoutTemplate, "{id}").RequireAuthorization();
        groupBuilder.MapPost(CreateWorkoutTemplate).RequireAuthorization();
        groupBuilder.MapPut(UpdateWorkoutTemplate, "{id}").RequireAuthorization();
        groupBuilder.MapPut(UpdateWorkoutTemplateExercises, "{id}/Exercises").RequireAuthorization();
        groupBuilder.MapDelete(DeleteWorkoutTemplate, "{id}").RequireAuthorization();
    }

    public async Task<Ok<List<WorkoutTemplateBriefDto>>> GetWorkoutTemplates(ISender sender,
        [AsParameters] GetWorkoutTemplatesQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<WorkoutTemplateDetailDto>> GetWorkoutTemplate(ISender sender, int id)
    {
        var result = await sender.Send(new GetWorkoutTemplateQuery(id));

        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateWorkoutTemplate(ISender sender, CreateWorkoutTemplateCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(WorkoutTemplates)}/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateWorkoutTemplate(ISender sender, int id, UpdateWorkoutTemplateCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, BadRequest>> UpdateWorkoutTemplateExercises(ISender sender, int id, UpdateWorkoutTemplateExercisesCommand command)
    {
        if (id != command.WorkoutTemplateId) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteWorkoutTemplate(ISender sender, int id)
    {
        await sender.Send(new DeleteWorkoutTemplateCommand(id));

        return TypedResults.NoContent();
    }
}
