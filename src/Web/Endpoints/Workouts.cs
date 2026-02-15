using Hoist.Application.Workouts.Commands.CompleteWorkout;
using Hoist.Application.Workouts.Commands.CreateWorkoutSet;
using Hoist.Application.Workouts.Commands.DeleteWorkoutSet;
using Hoist.Application.Workouts.Commands.DiscardWorkout;
using Hoist.Application.Workouts.Commands.StartWorkout;
using Hoist.Application.Workouts.Commands.UpdateWorkout;
using Hoist.Application.Workouts.Commands.UpdateWorkoutExercises;
using Hoist.Application.Workouts.Commands.UpdateWorkoutSet;
using Hoist.Application.Workouts.Queries.GetInProgressWorkout;
using Hoist.Application.Workouts.Queries.GetRecentWorkouts;
using Hoist.Application.Workouts.Queries.GetWorkout;
using Hoist.Application.Workouts.Queries.GetWorkoutHistory;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hoist.Web.Endpoints;

public class Workouts : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetWorkoutHistory).RequireAuthorization();
        groupBuilder.MapPost(StartWorkout).RequireAuthorization();
        groupBuilder.MapGet(GetInProgressWorkout, "InProgress").RequireAuthorization();
        groupBuilder.MapGet(GetRecentWorkouts, "Recent").RequireAuthorization();
        groupBuilder.MapGet(GetWorkout, "{id}").RequireAuthorization();
        groupBuilder.MapDelete(DiscardWorkout, "{id}").RequireAuthorization();
        groupBuilder.MapPut(CompleteWorkout, "{id}/Complete").RequireAuthorization();
        groupBuilder.MapPut(UpdateWorkout, "{id}").RequireAuthorization();
        groupBuilder.MapPut(UpdateWorkoutExercises, "{id}/Exercises").RequireAuthorization();
        groupBuilder.MapPost(CreateWorkoutSet, "{workoutId}/Exercises/{exerciseId}/Sets").RequireAuthorization();
        groupBuilder.MapPut(UpdateWorkoutSet, "{workoutId}/Exercises/{exerciseId}/Sets/{setId}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteWorkoutSet, "{workoutId}/Exercises/{exerciseId}/Sets/{setId}").RequireAuthorization();
    }

    public async Task<Ok<PaginatedWorkoutList>> GetWorkoutHistory(ISender sender, [AsParameters] GetWorkoutHistoryQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> StartWorkout(ISender sender, StartWorkoutCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Workouts)}/{id}", id);
    }

    public async Task<Ok<WorkoutDetailDto>> GetInProgressWorkout(ISender sender)
    {
        var result = await sender.Send(new GetInProgressWorkoutQuery());

        return TypedResults.Ok(result)!;
    }

    public async Task<Ok<List<WorkoutBriefDto>>> GetRecentWorkouts(ISender sender)
    {
        var result = await sender.Send(new GetRecentWorkoutsQuery());

        return TypedResults.Ok(result);
    }

    public async Task<Ok<WorkoutDetailDto>> GetWorkout(ISender sender, int id)
    {
        var result = await sender.Send(new GetWorkoutQuery { Id = id });

        return TypedResults.Ok(result);
    }

    public async Task<NoContent> DiscardWorkout(ISender sender, int id)
    {
        await sender.Send(new DiscardWorkoutCommand { Id = id });

        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, BadRequest>> CompleteWorkout(ISender sender, int id, CompleteWorkoutCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, BadRequest>> UpdateWorkout(ISender sender, int id, UpdateWorkoutCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, BadRequest>> UpdateWorkoutExercises(ISender sender, int id, UpdateWorkoutExercisesCommand command)
    {
        if (id != command.WorkoutId) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<Created<int>> CreateWorkoutSet(ISender sender, int workoutId, int exerciseId, CreateWorkoutSetCommand command)
    {
        if (workoutId != command.WorkoutId || exerciseId != command.WorkoutExerciseId)
            return TypedResults.Created("", 0);

        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Workouts)}/{workoutId}/Exercises/{exerciseId}/Sets/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateWorkoutSet(ISender sender, int workoutId, int exerciseId, int setId, UpdateWorkoutSetCommand command)
    {
        if (workoutId != command.WorkoutId || exerciseId != command.WorkoutExerciseId || setId != command.SetId)
            return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteWorkoutSet(ISender sender, int workoutId, int exerciseId, int setId)
    {
        await sender.Send(new DeleteWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = exerciseId,
            SetId = setId
        });

        return TypedResults.NoContent();
    }
}
